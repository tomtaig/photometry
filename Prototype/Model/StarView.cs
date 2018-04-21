using System.ComponentModel;
using System.Windows;

namespace Prototype.Model
{
    public class StarView : INotifyPropertyChanged
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double BrightestX { get; set; }
        public double BrightestY { get; set; }
        public double SignalNoiseRatio { get; set; }
        public ushort Peak { get; set; }
        public double BackgroundMean { get; set; }
        public double BackgroundSigma { get; set; }
        public int Signals { get; set; }
        public int Backgrounds { get; set; }
        public bool IsFound { get; set; }

        public Visibility MeasurementsVisibility => IsFound ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetMeasurements(double snr, ushort peak, double weightedCenterX, double weightedCenterY, double brightestPixelX, double brightestPixelY)
        {
            SignalNoiseRatio = snr;
            Peak = peak;
            BrightestX = brightestPixelX;
            BrightestY = brightestPixelY;
            CenterX = weightedCenterX;
            CenterY = weightedCenterY;

            IsFound = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFound)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SignalNoiseRatio)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Peak)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BrightestX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BrightestY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CenterX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CenterY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasurementsVisibility)));
        }

        public void SetStats(int signals, int backgrounds, double regionMean, double backgroundSigma, double backgroundMean)
        {
            Signals = signals;
            Backgrounds = backgrounds;
            BackgroundMean = backgroundMean;
            BackgroundSigma = backgroundSigma;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Signals)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Backgrounds)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundMean)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(backgroundSigma)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasurementsVisibility)));
        }

        public void SetStarNotFound()
        {
            IsFound = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFound)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MeasurementsVisibility)));
        }
    }
}
