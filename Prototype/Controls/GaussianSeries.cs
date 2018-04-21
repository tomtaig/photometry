using OxyPlot;
using OxyPlot.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Prototype.Controls
{
    public class GaussianSeries : LineSeries
    {
        public static DependencyProperty SigmaProperty = DependencyProperty.Register(nameof(Sigma), typeof(double), typeof(GaussianSeries));
        public static DependencyProperty CenterProperty = DependencyProperty.Register(nameof(Center), typeof(double), typeof(GaussianSeries));
        public static DependencyProperty AmplitudeProperty = DependencyProperty.Register(nameof(Amplitude), typeof(double), typeof(GaussianSeries));
        public static DependencyProperty OffsetProperty = DependencyProperty.Register(nameof(Offset), typeof(double), typeof(GaussianSeries));

        public double Sigma
        {
            get { return (double)this.GetValue(SigmaProperty); }
            set { this.SetValue(SigmaProperty, value); }
        }

        public double Center
        {
            get { return (double)this.GetValue(CenterProperty); }
            set { this.SetValue(CenterProperty, value); }
        }

        public double Amplitude
        {
            get { return (double)this.GetValue(AmplitudeProperty); }
            set { this.SetValue(AmplitudeProperty, value); }
        }

        public double Offset
        {
            get { return (double)this.GetValue(OffsetProperty); }
            set { this.SetValue(OffsetProperty, value); }
        }
        
        public override OxyPlot.Series.Series CreateModel()
        {
            var series = (OxyPlot.Series.LineSeries)base.CreateModel();
            var points = new ObservableCollection<DataPoint>();

            for(var i = -20.0; i < 20.0; i+=0.02)
            {
                points.Add(new DataPoint(i, PlotGaussian(i)));
            }

            series.ItemsSource = points;

            return series;
        }

        public double PlotGaussian(double x)
        {
            return Offset + Amplitude * Math.Exp(-1 * Math.Pow(x - Center, 2) / Math.Pow(2 * Sigma, 2));
        }

        public static double PlotGaussian(double offset, double amplitude, double center, double sigma, double x)
        {
            return offset + amplitude * Math.Exp(-1 * Math.Pow(x - center, 2) / Math.Pow(2 * sigma, 2));
        }
    }
}
