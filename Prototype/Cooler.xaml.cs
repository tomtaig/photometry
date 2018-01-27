using OxyPlot.Axes;
using Prototype.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prototype
{
    /// <summary>
    /// Interaction logic for Cooler.xaml
    /// </summary>
    public partial class Cooler : UserControl
    {
        public Cooler()
        {
            InitializeComponent();

            DataContextChanged += CoolerDataContextChanged;
        }
        
        private void CoolerDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as Session;
            var newModel = e.NewValue as Session;

            if(oldModel != null)
            {
                oldModel.Cooler.ReadingsUpdated -= OnReadingsUpdated;
            }

            if(newModel != null)
            {
                newModel.Cooler.ReadingsUpdated += OnReadingsUpdated;
            }
        }

        private void OnReadingsUpdated(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var model = (Session)DataContext;
                var cooler = model.Cooler;
                var count = cooler.TemperatureReadings.Count;

                if(cooler.LastReadingTime.Subtract(cooler.FirstReadingTime) > TimeSpan.FromMinutes(5))
                {
                    xAxis.Minimum = DateTimeAxis.ToDouble(cooler.LastReadingTime.Subtract(TimeSpan.FromMinutes(5)));
                    xAxis.Maximum = DateTimeAxis.ToDouble(cooler.LastReadingTime);
                }
                else
                {
                    xAxis.Minimum = DateTimeAxis.ToDouble(cooler.FirstReadingTime);
                    xAxis.Maximum = DateTimeAxis.ToDouble(cooler.FirstReadingTime.Add(TimeSpan.FromMinutes(5)));
                }
                
                readings.InvalidatePlot(true);
            });
        }

        private void TurnOnClick(object sender, RoutedEventArgs e)
        {
            var model = (Session)DataContext;

            model.Cooler.TurnOn();
        }

        private void TurnOffClick(object sender, RoutedEventArgs e)
        {
            var model = (Session)DataContext;

            model.Cooler.TurnOff();
        }

        private void TargetTemperatureClick(object sender, RoutedEventArgs e)
        {
            var model = (Session)DataContext;

            model.Cooler.SetTargetTemperature(targetTemperature.Value);
        }
    }
}
