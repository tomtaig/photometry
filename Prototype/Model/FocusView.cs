using OxyPlot;
using OxyPlot.Series;
using Prototype.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System;
using Tests;
using System.Linq;
using System.Collections.Generic;

namespace Prototype.Model
{
    public class FocusView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DataPoint> ProfileSamples { get; set; }
        public ObservableCollection<ColumnItem> Measurements { get; set; }
        public ObservableCollection<string> MeasurementCategories { get; set; }

        public bool IsStarSelected { get; set; }

        public bool UsePhdBackgroundFormula { get; set; } = false;
        public bool UseMedianFilter { get; set; } = false;

        public Visibility SelectedStarIndicatorVisibility => IsStarSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StarSelectionImageVisibility => IsStarSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StarSelectionStatsVisibility => IsStarSelected ? Visibility.Visible : Visibility.Hidden;
        public Visibility StarFittingGraphVisibility => IsStarSelected ? Visibility.Visible : Visibility.Hidden;
        public Visibility StarFittingStatsVisibility => IsStarSelected ? Visibility.Visible : Visibility.Hidden;
        public Visibility StarFocusGraphVisibility => IsStarSelected ? Visibility.Visible : Visibility.Hidden;

        public Visibility CrosshairReticleVisibility => SelectedReticle == ReticleType.Crosshair ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SubFrameSelectVisibility => IsSubFrameActive ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StartCaptureLoopVisibility => !IsLoopingCapture ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StopCaptureLoopVisibility => IsLoopingCapture && IsCapturing ? Visibility.Visible : Visibility.Collapsed;
        
        public bool IsCaptureEnabled => !IsCapturing;

        public double Exposure { get; set; }
        public double ExposureMinute { get; set; }
        public double ExposureSecond { get; set; }
        public double ExposureMillisecond { get; set; }

        public bool IsCapturing { get; set; }
        public bool IsLoopingCapture { get; set; }
        public bool IsSubFrameSelecting { get; set; }
        public double Boost { get; set; } = 1;
        public double BoostSlider { get; set; } = 1;
        public double FittedOffset { get; set; }
        public double FittedAmplitude { get; set; }
        public double FittedSigma { get; set; }
        public double FittedCenter { get; set; }
        public int SubFrameX { get; set; }
        public int SubFrameY { get; set; }
        public int SubFrameWidth { get; set; }
        public int SubFrameHeight { get; set; }
        public double SubFrameSelectWidth { get; set; }
        public double SubFrameSelectHeight { get; set; }
        public double SubFrameSelectAreaX { get; set; }
        public double SubFrameSelectAreaY { get; set; }
        public double SubFrameSelectAreaWidth { get; set; }
        public double SubFrameSelectAreaHeight { get; set; }
        public double Zoom { get; set; } = 1;
        public double ZoomSlider { get; set; } = 100;
        public double ZoomedWidth { get; set; }
        public double ZoomedHeight { get; set; }
        public double SubFrameSelectionThickness { get; set; }
        public bool IsSubFrameActive { get; set; }
        public ReticleType SelectedReticle { get; set; }
        public CaptureView Capture { get; set; }
        public RegionView Region { get; set; }
        public StarView Star { get; set; }

        public Action FrameChanged { get; set; }
        public Action RegionChanged { get; set; }

        CameraView _camera;

        public FocusView()
        {
            Region = new RegionView();

            Region.RefreshZoom(Zoom);

            Star = new StarView();

            var samples = new ObservableCollection<DataPoint>();
            
            ProfileSamples = samples;

            MeasurementCategories = new ObservableCollection<string>();
            
            for (var i=0; i<20; i++)
            {
                MeasurementCategories.Add(i.ToString());
            }

            Measurements = new ObservableCollection<ColumnItem>();

            Measurements.Add(new ColumnItem(5, 0));
            Measurements.Add(new ColumnItem(5, 1));
            Measurements.Add(new ColumnItem(5, 2));
            Measurements.Add(new ColumnItem(5, 3));
            Measurements.Add(new ColumnItem(5, 4));
            Measurements.Add(new ColumnItem(5, 5));
            Measurements.Add(new ColumnItem(5, 6));
            Measurements.Add(new ColumnItem(5, 7));
            Measurements.Add(new ColumnItem(5, 8));
            Measurements.Add(new ColumnItem(5, 9));
            Measurements.Add(new ColumnItem(5, 10));
            Measurements.Add(new ColumnItem(5, 11));
            Measurements.Add(new ColumnItem(10, 12));
            Measurements.Add(new ColumnItem(9, 13));
            Measurements.Add(new ColumnItem(7, 14));
            Measurements.Add(new ColumnItem(8, 15));
            Measurements.Add(new ColumnItem(6.2, 16));
            Measurements.Add(new ColumnItem(5, 17));
            Measurements.Add(new ColumnItem(4.2, 18));
            Measurements.Add(new ColumnItem(2, 19));
            Measurements.Add(new ColumnItem(2, 20));
        }

        public void Configure(CameraView camera)
        {
            var whr = camera.BinnedY / (double)camera.BinnedX;

            SubFrameSelectWidth = 210;
            SubFrameSelectHeight = SubFrameSelectWidth * whr;

            SubFrameSelectionThickness = 4;

            SubFrameWidth = camera.SubFrameWidth;
            SubFrameHeight = camera.SubFrameHeight;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectHeight)));

            _camera = camera;

            var xratio = (SubFrameSelectWidth / (double)SubFrameWidth);
            var yratio = (SubFrameSelectHeight / (double)SubFrameHeight);

            SetSubFrame(
                camera.SubFrameX * xratio, 
                camera.SubFrameY * yratio, 
                camera.SubFrameWidth * xratio, 
                camera.SubFrameHeight * yratio);
        }

        public void SetSelectedStar(StarView star)
        {
            Star = star;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Star)));
        }
        
        public void StartCapturing()
        {
            IsCapturing = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCapturing)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoopingCapture)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCaptureEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopCaptureLoopVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartCaptureLoopVisibility)));
        }
        
        public void StopCapturing()
        {
            IsCapturing = false;
            IsLoopingCapture = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCapturing)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoopingCapture)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCaptureEnabled)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopCaptureLoopVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartCaptureLoopVisibility)));
        }
        
        public SubFrame GetSubFrame()
        {
            var result = new SubFrame();

            var xratio = SubFrameWidth / SubFrameSelectWidth;
            var yratio = SubFrameHeight / SubFrameSelectHeight;

            result.ChipX1 = (int)(SubFrameSelectAreaX * xratio);
            result.ChipY1 = (int)(SubFrameSelectAreaY * yratio);
            result.ChipX2 = (int)((SubFrameSelectAreaX + SubFrameSelectAreaWidth) * xratio);
            result.ChipY2 = (int)((SubFrameSelectAreaY + SubFrameSelectAreaHeight) * yratio);

            return result;
        }

        public void SetLoopCapture(bool loop)
        {
            IsLoopingCapture = loop;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoopingCapture)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopCaptureLoopVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartCaptureLoopVisibility)));
        }

        public void SetReticle(ReticleType reticle)
        {
            SelectedReticle = reticle;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedReticle)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CrosshairReticleVisibility)));
        }
        
        public void SetCapture(CaptureView capture)
        {
            Capture = capture;                       

            Region.SetParentCapture(capture);
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Capture)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Region)));
        }

        public void SetZoom(double? zoom)
        {
            if (zoom.HasValue)
            {
                Zoom = zoom.Value;
                ZoomSlider = Zoom * 100.0;
            }
            else
            {
                Zoom = ZoomSlider / 100.0;
            }

            Region.RefreshZoom(Zoom);

            ZoomedWidth = (Capture?.GetWidth() ?? 0) * Zoom;
            ZoomedHeight = (Capture?.GetHeight() ?? 0) * Zoom;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomSlider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomSlider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomPercentage)));
        }

        public void ClearStarSelection()
        {
            Star.SetStarNotFound();
            Region.ClearSelection();

            IsStarSelected = false;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStarSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarIndicatorVisibility)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingStatsVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFocusGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionImageVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionStatsVisibility)));
        }

        public void SetStarSelection(int x1, int y1, int x2, int y2)
        {
            if (Capture == null)
            {
                return;
            }
            
            Region.SetPosition(x1, y1, x2, y2);
            
            IsStarSelected = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Region)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStarSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarIndicatorVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingStatsVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFocusGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionImageVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionStatsVisibility)));
        }

        GaussianFitOptions2 _fitOptions = GaussianFitOptions2.Default;

        public void SetProfileSamples(ushort[] samples)
        {
            var plotted = new ObservableCollection<DataPoint>();
            var signals = new List<ushort>();
            var start = 0;
            var end = 39;

            //for (var i = 0; i < 40; i++)
            //{
            //    if (samples[i] > 0)
            //    {
            //        start = i;
            //        break;
            //    }
            //}

            //for (var i = 39; i >= 0; i--)
            //{
            //    if (samples[i] > 0)
            //    {
            //        end = i;
            //        break;
            //    }
            //}

            if(end - start <= 0)
            {
                ProfileSamples.Clear();
                return;
            }

            samples = samples.Skip(start).Take(end - start).ToArray();

            for (var i=0; i<end-start; i++)
            {
                plotted.Add(new DataPoint(i - ((end - start) / 2.0), samples[i]));
            }

            ProfileSamples = plotted;

            var max = 0;
            var center = 0.0;
            var mass = 0.0;
            
            for (var i = 0; i < samples.Length; i++)
            {
                if (samples[i] > max) max = samples[i];

                center += samples[i] * i;
                mass += samples[i];
            }

            center /= mass;
            
            var result = Photometry2.FindGaussianPSF(samples, max, center, _fitOptions);

            //if(result.Result != GaussianFitResult.Error)
            //{
            //    if (!double.IsNaN(result.Width))
            //    {
            //        _fitOptions.StartSigma = result.Width;
            //    }
            //}

            FittedAmplitude = result.Peak;
            FittedCenter = center - ((end - start) / 2.0);
            FittedSigma = result.Width;
            FittedOffset = 0;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FittedAmplitude)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FittedCenter)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FittedSigma)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FittedOffset)));                        
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProfileSamples)));
        }

        public void SetSubFrameSelectionThickness(double thickness)
        {
            SubFrameSelectionThickness = thickness;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectionThickness)));
        }

        double PlotGaussian(double x)
        {
            return GaussianSeries.PlotGaussian(FittedOffset, FittedAmplitude, FittedCenter, FittedSigma, x);
        }
        
        private void SetSubFrame(double startX, double startY, double numX, double numY)
        {
            var xratio = (SubFrameWidth / (double)SubFrameSelectWidth);
            var yratio = (SubFrameHeight / (double)SubFrameSelectHeight);

            SubFrameSelectAreaX = startX;
            SubFrameSelectAreaY = startY;
            SubFrameSelectAreaWidth = numX;
            SubFrameSelectAreaHeight = numY;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectAreaX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectAreaY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectAreaWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectAreaHeight)));

            SubFrameX = (int)(SubFrameSelectAreaX * xratio);
            SubFrameY = (int)(SubFrameSelectAreaY * yratio);
            SubFrameWidth = (int)(SubFrameSelectAreaWidth * xratio);
            SubFrameHeight = (int)(SubFrameSelectAreaHeight * yratio);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameHeight)));
        }

        public void SetSubFrameActive(bool enabled)
        {
            IsSubFrameActive = enabled;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSubFrameActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubFrameSelectVisibility)));
        }
        
        public void ExposureChange(double min, double sec, double msec)
        {
            Exposure = min * 60 + sec + (msec / 1000.0);
            ExposureMinute = min;
            ExposureSecond = sec;
            ExposureMillisecond = msec;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exposure)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureMinute)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureSecond)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ExposureMillisecond)));
        }

        public void BoostChange(double? boost)
        {
            if (boost.HasValue)
            {
                BoostSlider = boost.Value;
            }

            Boost = BoostSlider;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Boost)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BoostSlider)));
        }
        
        public string ZoomPercentage
        {
            get { return $"{(Zoom).ToString("P0", CultureInfo.InvariantCulture)}"; }
        }
    }
}
