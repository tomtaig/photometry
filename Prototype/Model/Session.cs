using Prototype.Model;
using Prototype.Services;
using Prototype.Services.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using OpenCvSharp;
using System.Windows.Media.Imaging;
using Tests;

namespace Prototype
{
    public class Session
    {
        Task _coolingMonitor;
        Task _filterWheelMonitor;
        CancellationTokenSource _cancelCoolingMonitor;
        CancellationTokenSource _cancelFilterWheelMonitor;

        public Session()
        {
            Camera = new CameraView();
            Cooler = new CoolerView();
            Filter = new FilterView();
            Focus = new FocusView();

            FilterWheelService = new AscomFilterWheelService();
            FilterWheelService.Initialize(this);
        }

        public ICameraService CameraService { get; set; }
        public IFilterWheelService FilterWheelService { get; set; }
        public CameraView Camera { get; set; }
        public CoolerView Cooler { get; set; }
        public FilterView Filter { get; set; }
        public FocusView Focus { get; set; }

        public OperationResult SetCameraInterface()
        {
            switch (Camera.Interface?.Id)
            {
                case "ASCOM":
                    CameraService = new AscomCameraService();
                    break;
                default:
                    Camera.Cameras.Clear();
                    break;
            }

            if (CameraService != null)
            {
                var result = CameraService.Initialize(this);

                if (!result.IsError)
                {
                    Camera.ChangeInterface();
                }

                return result;
            }


            return OperationResult.Ok;
        }
        
        public void SetDiscreteGain(double value)
        {
            CameraService.SetDiscreteGain((short)value);

            Camera.ChangeDiscreteGain(value);
        }
        
        public void SelectFocusStar(int x1, int y1, int x2, int y2)
        {
            Focus.SetStarSelection(x1, y1, x2, y2);

            if(Focus.Capture != null)
            {
                SelectStar();
            }
        }

        public void ChangeBinX(int x)
        {
            CameraService.SetBinX(x);

            Camera.ChangeBinX(x);
        }

        public void ChangeBinY(int y)
        {
            CameraService.SetBinY(y);

            Camera.ChangeBinY(y); 
        }

        public void ExposureChange(double min, double sec, double msec)
        {
            var seconds = min * 60 + sec + (msec / 1000.0);

            if(seconds < Camera.ExposureMin)
            {
                min = 0;
                sec = 0;
                msec = 0;

                if(Camera.ExposureMin < 1)
                {
                    msec = Camera.ExposureMin * 1000;
                }
                else if (Camera.ExposureMin < 60)
                {
                    sec = Camera.ExposureMin;
                }
                else
                {
                    min = Camera.ExposureMin / 60.0;
                }
            }
            else if (seconds > Camera.ExposureMax)
            {
                min = Camera.ExposureMax / 60.0;
                sec = (min - (int)min) * 60.0;
                msec = (sec / 60.0) - (int)(sec / 60.0) * 1000;
            }

            Focus.ExposureChange(min, sec, msec);
        }
        
        public void Capture()
        {
            Focus.StartCapturing();

            if (Focus.IsSubFrameActive)
            {
                var frame = Focus.GetSubFrame();

                CameraService.SetRegion(frame.ChipX1, frame.ChipY1, frame.ChipX2 - frame.ChipX1, frame.ChipY2 - frame.ChipY1);
                Camera.SetRegion(frame.ChipX1, frame.ChipY1, frame.ChipX2 - frame.ChipX1, frame.ChipY2 - frame.ChipY1);
            }
            else
            {
                CameraService.SetRegion(0, 0, Camera.BinnedX, Camera.BinnedY);
                Camera.SetRegion(0, 0, Camera.BinnedX, Camera.BinnedY);
            }

            var task = CameraService.Capture(Focus.Exposure);

            task.ContinueWith(x =>
            {
                if(x.Status == TaskStatus.RanToCompletion)
                {
                    var result = x.Result;

                    var capture = new CaptureView
                    {
                        BinnedX = Camera.BinnedX,
                        BinnedY = Camera.BinnedY,
                        ChipX = Camera.UnbinnedX,
                        ChipY = Camera.UnbinnedY,
                        XSize = result.XSize,
                        YSize = result.YSize
                    };

                    if (Camera.IsSubFrameActive)
                    {
                        capture.Frame = new SubFrame
                        {
                            ChipX1 = Camera.SubFrameX,
                            ChipY1 = Camera.SubFrameY,
                            ChipX2 = Camera.SubFrameX + Camera.SubFrameWidth,
                            ChipY2 = Camera.SubFrameY + Camera.SubFrameHeight
                        };
                    }

                    capture.ImageArray = result.Image;
                    capture.Image = new Mat(capture.XSize, capture.YSize, MatType.CV_16UC1, result.Image);

                    Focus.SetCapture(capture);

                    Camera.ChangeActualSize(capture.GetWidth(), capture.GetHeight());

                    if (Focus.IsStarSelected)
                    {
                        SelectStar();
                    }

                    Focus.FrameChanged();

                    if (Focus.IsLoopingCapture)
                    {
                        Capture();
                    }
                    else
                    {
                        Focus.StopCapturing();
                    }
                }
                else
                {
                    Focus.StopCapturing();
                }
            });

            task.Start();
        }

        void SelectStar()
        {
            if (Focus.Capture != null)
            {
                var x1 = Focus.Region.VisibleX;
                var y1 = Focus.Region.VisibleY;
                var x2 = x1 + Focus.Region.VisibleWidth;
                var y2 = y1 + Focus.Region.VisibleHeight;
                
                var xSize = Focus.Region.VisibleWidth;
                var ySize = Focus.Region.VisibleHeight;

                if (xSize > 0)
                {
                    SetStar(Focus.Capture, Focus.Region);
                }
            }
        }

        public void ChangeBinXY(int xy)
        {
            CameraService.SetBinXY(xy);
            Camera.ChangeBinXY(xy);
        }

        public void SetSymmetricBins(List<string> bins)
        {
            Camera.ChangeAsymmetricBinning(false);

            Camera.BinValues.Clear();

            foreach (var bin in bins)
            {
                Camera.BinValues.Add(bin);
            }
        }
        
        public void SetAsymmetricBins(List<string> x, List<string> y)
        {
            Camera.ChangeAsymmetricBinning(true);

            Camera.BinXValues.Clear();

            foreach (var bin in x)
            {
                Camera.BinXValues.Add(bin);
            }

            Camera.BinYValues.Clear();

            foreach (var bin in y)
            {
                Camera.BinYValues.Add(bin);
            }
        }

        public void ApplyFocusRegion(int x, int y, int width, int height)
        {
            CameraService.SetRegion(x, y, width, height);

            Camera.SetRegion(x, y, width, height);
        }

        public void ResetRegion()
        {
            CameraService.ClearRegion();

            Camera.ClearRegion();
        }

        public void SetGainOptions(IEnumerable<GainOption> options)
        {
            Camera.Gains.Clear();

            foreach (var option in options)
            {
                Camera.Gains.Add(option);
            }
        }

        public OperationResult SetGainSettings(bool discrete, double min, double max)
        {
            Camera.ChangeGainSettings(discrete, min, max);

            return OperationResult.Ok;
        }

        public void EnableSubFrame()
        {
            Focus.SetSubFrameActive(true);
        }

        public void DisableSubFrame()
        {
            Focus.SetSubFrameActive(false);
        }

        public OperationResult SetGainMode(string value)
        {
            if(Camera.IsGainDiscrete)
            {
                throw new ArgumentException();
            }

            CameraService.SetGainMode(value);

            Camera.ChangeGainMode(Camera.Gains.IndexOf(Camera.Gains.Single(x => x.Value == value)));
            
            return OperationResult.Ok;
        }

        public OperationResult ConnectCamera()
        {
            var result = CameraService.Connect(this);

            if(!result.IsError)
            {
                Focus.Configure(Camera);
                StartMonitorCoolingTask();
                ExposureChange(Focus.ExposureMinute, Focus.ExposureSecond, Focus.ExposureMillisecond);
            }
            
            return result;
        }

        public OperationResult MoveFilterWheel()
        {
            var result = FilterWheelService.SetSlotPosition(Filter.SelectedFilter.Number);

            if(!result.IsError)
            {
                Filter.IsFilterWheelMoving = true;
            }

            return result;
        }

        public OperationResult DisconnectCamera()
        {
            if (Cooler.IsOn && !WarnAboutCooler())
            {
                return OperationResult.Ok;
            }

            var result = CameraService.Disconnect(this);

            StopMonitorCoolingTask();

            return result;
        }

        public bool WarnAboutCooler()
        {
            var result = MessageBox.Show("The cooler is on. Consider increasing the sensor temperature to protect equipment or press OK to continue anyway.", "Cooler On", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            return result == MessageBoxResult.OK;
        }

        public OperationResult ConnectWheel()
        {
            var result = FilterWheelService.Connect(this);

            if (!result.IsError)
            {
                StartMonitorFilterWheelTask();

                Filter.ConnectWheel();
            }

            return result;
        }

        public OperationResult DisconnectWheel()
        {
            var result = FilterWheelService.Disconnect(this);

            if (!result.IsError)
            {
                StopMonitorFilterWheelTask();

                Filter.DisconnectWheel();
            }

            return result;
        }
        
        void SetStar(CaptureView capture, RegionView region)
        {
            var xSize = region.VisibleWidth;
            var ySize = region.VisibleHeight;

            var sub = Photometry.GetSubImage(2,
                capture.ImageArray,
                capture.YSize,
                capture.XSize,
                region.VisibleY,
                region.VisibleX,
                region.VisibleY + ySize,
                region.VisibleX + xSize);

            var unclipped = Photometry.KappaSigmaClip(sub[0], 1.0, 3);

            var signal = new ushort[sub[0].Length - unclipped.Count];
            var signals = 0;

            var background = new ushort[unclipped.Count];
            var backgrounds = 0;

            for (var i = 0; i < sub[0].Length; i++)
            {
                if (unclipped.Contains(i))
                {
                    background[backgrounds++] = sub[0][i];
                }
                else
                {
                    signal[signals++] = sub[0][i];
                }
            }

            var regionMean = Photometry.FindMean(sub[0]);
            var backgroundSigma = Photometry.FindStandardDeviation(background);
            var backgroundMean = Photometry.FindMean(background);

            Focus.Star.SetStats(signals, backgrounds, regionMean, backgroundSigma, backgroundMean);

            var snr = regionMean / backgroundSigma;

            var bg = Focus.UsePhdBackgroundFormula ? regionMean + backgroundSigma * 2.0 : regionMean;

            Photometry.Subtract(sub[0], (ushort)bg);

            if (Focus.UseMedianFilter)
            {
                sub[0] = Photometry.MedianBoxFilter(sub[0], xSize, ySize);
            }

            Focus.Region.Capture = new CaptureView
            {
                XSize = xSize,
                YSize = ySize,
                ImageArray = sub[0],
                Image = new Mat(ySize, xSize, MatType.CV_16UC1, sub[0])
            };

            Focus.RegionChanged();

            if (signals < 5)
            {
                Focus.Star.SetStarNotFound();
            }
            else
            {
                var brightestX = 0;
                var brightestY = 0;
                var br = 0;

                for (var y = 0; y < ySize; y++)
                {
                    for (var x = 0; x < xSize; x++)
                    {
                        var brightest = brightestY * xSize + brightestX;

                        if (sub[0][brightest] < sub[0][y * xSize + x])
                        {
                            brightestX = x;
                            brightestY = y;
                            br = sub[0][brightest];
                        }
                    }
                }

                var center = Photometry.FindCenterOfMass(sub[0], xSize, ySize);

                ushort peak = 0;

                for (var i = 0; i < sub[0].Length; i++)
                {
                    if (sub[0][i] > peak) peak = sub[0][i];
                }

                var weightedCenterX = (center.X / (double)xSize) * 100.0;
                var weightedCenterY = (center.Y / (double)ySize) * 100.0;
                var brightestPixelX = (brightestX / (double)xSize) * 100.0;
                var brightestPixelY = (brightestY / (double)ySize) * 100.0;
                
                Focus.Star.SetMeasurements(
                    snr, 
                    peak, 
                    weightedCenterX, 
                    weightedCenterY, 
                    brightestPixelX, 
                    brightestPixelY);

                var samples = new ushort[40];
                var max = 0.0;

                for (var x = 0; x < 40; x++)
                {
                    samples[x] = sub[0][(int)center.Y * 40 + x];
                    max = samples[x] > max ? samples[x] : max;
                }

                Focus.SetProfileSamples(samples);
            }
        }

        void StartMonitorFilterWheelTask()
        {
            _cancelFilterWheelMonitor = new CancellationTokenSource();
            _filterWheelMonitor = new Task(() => MonitorFilterWheel(_cancelFilterWheelMonitor.Token), _cancelFilterWheelMonitor.Token);
            _filterWheelMonitor.Start();
        }

        void StartMonitorCoolingTask()
        {
            _cancelCoolingMonitor = new CancellationTokenSource();
            _coolingMonitor = new Task(() => MonitorCooling(_cancelCoolingMonitor.Token), _cancelCoolingMonitor.Token);
            _coolingMonitor.Start();
        }
        
        public OperationResult TurnOnCooler()
        {
            var result = CameraService.ToggleCooling(true);

            if(!result.IsError)
            {
                Cooler.TurnOn();

                result = CameraService.GetTargetTemperature(out double? target);

                if (!result.IsError)
                {
                    Cooler.SetTargetTemperature(target.Value);
                }
            }

            return result;
        }

        public OperationResult TurnOffCooler()
        {
            if (Cooler.IsOn && !WarnAboutCooler())
            {
                return OperationResult.Ok;
            }

            var result = CameraService.ToggleCooling(false);

            if (!result.IsError)
            {
                Cooler.TurnOff();
            }

            return result;
        }

        public OperationResult SetTargetTemperature(double celsius)
        {
            var result = CameraService.SetTargetTemperature(celsius);

            if (!result.IsError)
            {
                Cooler.SetTargetTemperature(celsius);
            }

            return result;
        }

        void StopMonitorCoolingTask()
        {
            if (_cancelCoolingMonitor != null)
            {
                _cancelCoolingMonitor.Cancel();
                _cancelCoolingMonitor = null;
                _coolingMonitor = null;
            }
        }

        void StopMonitorFilterWheelTask()
        {
            if (_cancelFilterWheelMonitor != null)
            {
                _cancelFilterWheelMonitor.Cancel();
                _cancelFilterWheelMonitor = null;
                _filterWheelMonitor = null;
            }
        }

        void MonitorCooling(CancellationToken token)
        {
            Cooler.FirstReadingTime = DateTime.Now;

            var readingTime = Cooler.FirstReadingTime;

            while (!token.IsCancellationRequested)
            {
                CameraService.GetObservedCelsius(out double? celsius);
                CameraService.GetObservedCoolingPower(out double? power);
                
                Cooler.AddReading(DateTime.Now, celsius ?? double.NaN, power);

                Thread.Sleep(1000);

                readingTime = DateTime.Now;
            }
        }

        void MonitorFilterWheel(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var result = FilterWheelService.GetSlotPosition(out short? position, out bool? moving);

                if (!result.IsError)
                {
                    if (moving.Value)
                    {
                        Filter.SetFilterWheelMoving(true);
                    }
                    else
                    {
                        Filter.SetActiveFilter(position.Value);
                    }
                }

                Thread.Sleep(1000);
            }
        }
    }
}
