using OxyPlot;
using OxyPlot.Series;
using Prototype.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System;
using static Prototype.Model.CaptureView;

namespace Prototype.Model
{
    public class FocusView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DataPoint> ProfileSamples { get; set; }
        public ObservableCollection<ColumnItem> Measurements { get; set; }
        public ObservableCollection<string> MeasurementCategories { get; set; }

        public bool IsStarSelected { get; set; }

        public Visibility SelectedStarIndicatorVisibility => IsStarSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility StarSelectionPromptVisibility => !IsStarSelected ? Visibility.Visible : Visibility.Collapsed;
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
        public double FittedOffset { get; set; } = 500;
        public double FittedAmplitude { get; set; } = 45000;
        public double FittedSigma { get; set; } = 1;
        public double FittedCenter { get; set; } = 0;
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
        public double SubFrameSelectionThickness { get; set; }
        public bool IsSubFrameActive { get; set; }
        public ReticleType SelectedReticle { get; set; }
        public CaptureView Capture { get; set; }
        public CaptureView CaptureROI { get; set; }
        public double ZoomedWidth { get; set; }
        public double ZoomedHeight { get; set; }
        public int SelectedStarVisibleX { get; set; }
        public int SelectedStarVisibleY { get; set; }
        public int SelectedStarVisibleWidth { get; set; }
        public int SelectedStarVisibleHeight { get; set; }
        public double SelectedStarZoomedX { get; set; }
        public double SelectedStarZoomedY { get; set; }
        public double SelectedStarZoomedWidth { get; set; }
        public double SelectedStarZoomedHeight { get; set; }
        public double SelectedStarCenterX { get; set; }
        public double SelectedStarCenterY { get; set; }
        public double SelectedStarBrightestX { get; set; }
        public double SelectedStarBrightestY { get; set; }
        public int SelectedStarChipX1 { get; set; }
        public int SelectedStarChipY1 { get; set; }
        public int SelectedStarChipX2 { get; set; }
        public int SelectedStarChipY2 { get; set; }
        public double SelectedStarSNR { get; set; }
        public ushort SelectedStarPeak { get; set; }
        public double SelectedStarBackgroundMean { get; set; }
        public double SelectedStarBackgroundSigma { get; set; }
        public int SelectedStarSignalPixels { get; set; }

        public Action FrameChanged { get; set; }
        public Action RoiFrameChanged { get; set; }

        CameraView _camera;

        public FocusView()
        {   
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

        public void SetStarBackgroundMean(double background)
        {
            SelectedStarBackgroundMean = background;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarBackgroundMean)));
        }

        public void SetStarBackgroundSigma(double sigma)
        {
            SelectedStarBackgroundSigma = sigma;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarBackgroundSigma)));
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

        public void SetStarSignalPixels(int pixels)
        {
            SelectedStarSignalPixels = pixels;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarSignalPixels)));
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

        public void SetStarSNR(double snr)
        {
            SelectedStarSNR = snr;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarSNR)));
        }

        public void SetStarPeak(ushort peak)
        {
            SelectedStarPeak = peak;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarPeak)));
        }
        
        public void SetStarCenter(double x, double y)
        {
            SelectedStarCenterX = x;
            SelectedStarCenterY = y;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarCenterX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarCenterY)));
        }

        public void SetStarBrightestCenter(double x, double y)
        {
            SelectedStarBrightestX = x;
            SelectedStarBrightestY = y;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarBrightestX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarBrightestY)));
        }

        public void SetCapture(CaptureView capture)
        {
            Capture = capture;

            var star1 = capture.GetVisiblePositionFromChipPosition(SelectedStarChipX1, SelectedStarChipY1);
            var star2 = capture.GetVisiblePositionFromChipPosition(SelectedStarChipX2, SelectedStarChipY2);

            SelectedStarVisibleX = star1.X;
            SelectedStarVisibleY = star1.Y;
            SelectedStarVisibleWidth = star2.X - star1.X;
            SelectedStarVisibleHeight = star2.Y - star1.Y;

            SetZoomedProperties();

            var x2 = SelectedStarVisibleX + SelectedStarVisibleWidth;
            var y2 = SelectedStarVisibleY + SelectedStarVisibleHeight;

            var maxX = capture.GetWidth();
            var maxY = capture.GetHeight();

            if (SelectedStarVisibleX < 0 || SelectedStarVisibleY < 0 || x2 >= maxX || y2 >= maxY)
            {
                ClearStarSelection();
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleX)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleY)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleWidth)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleHeight)));
                
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedWidth)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedHeight)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Capture)));
        }

        private void SetZoomedProperties()
        {
            SelectedStarZoomedX = SelectedStarVisibleX * Zoom;
            SelectedStarZoomedY = SelectedStarVisibleY * Zoom;
            SelectedStarZoomedWidth = SelectedStarVisibleWidth * Zoom;
            SelectedStarZoomedHeight = SelectedStarVisibleHeight * Zoom;

            ZoomedWidth = (Capture?.GetWidth() ?? 0) * Zoom;
            ZoomedHeight = (Capture?.GetHeight() ?? 0) * Zoom;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarZoomedX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarZoomedY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarZoomedWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarZoomedHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedHeight)));
        }

        public void ClearStarSelection()
        {
            IsStarSelected = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStarSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarIndicatorVisibility)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingStatsVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFocusGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionImageVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionPromptVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionStatsVisibility)));
        }

        public void SetStarSelection(int x1, int y1, int x2, int y2)
        {
            if (Capture == null)
            {
                return;
            }

            var star1 = Capture.GetChipPositionFromVisiblePosition(x1, y1);
            var star2 = Capture.GetChipPositionFromVisiblePosition(x2, y2);

            SelectedStarChipX1 = star1.X;
            SelectedStarChipY1 = star1.Y;
        
            SelectedStarChipX2 = star2.X;
            SelectedStarChipY2 = star2.Y;
            
            SelectedStarVisibleX = x1;
            SelectedStarVisibleY = y1;

            SelectedStarVisibleWidth = x2 - x1;
            SelectedStarVisibleHeight = y2 - y1;

            IsStarSelected = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarVisibleHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarChipX1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarChipY1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarChipX2)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarChipY2)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStarSelected)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStarIndicatorVisibility)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFittingStatsVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarFocusGraphVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionImageVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionPromptVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StarSelectionStatsVisibility)));

            SetZoomedProperties();
        }
        
        public void SetProfileSamples(ushort[] samples)
        {
            var latest = new ObservableCollection<DataPoint>();

            for (var i=0; i<40; i++)
            {
                latest.Add(new DataPoint(i-20, samples[i]));
            }

            ProfileSamples = latest;

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

        public void ZoomChange(double? zoom)
        {
            if (zoom.HasValue)
            {
                ZoomSlider = zoom.Value;
            }

            Zoom = (ZoomSlider / 100.0);

            SetZoomedProperties();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zoom)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomSlider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomPercentage)));
        }

        public string ZoomPercentage
        {
            get { return $"{(Zoom).ToString("P0", CultureInfo.InvariantCulture)}"; }
        }
    }
}
