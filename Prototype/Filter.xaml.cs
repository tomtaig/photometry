using Prototype.Model;
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
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : UserControl
    {
        public Filter()
        {
            InitializeComponent();
        }

        private void FilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.Filter.SelectFilterSlot();
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            var result = session.ConnectWheel();

            if(result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Filter Wheel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            var result = session.DisconnectWheel();
            
            if (result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Filter Wheel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WheelSelectionChanged(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.Filter.SelectFilterWheel();
        }

        private void SaveLabelsClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.Filter.SaveLabels();
        }

        private void SetFilterClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;
            
            var result = session.MoveFilterWheel();

            if (result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Filter Wheel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
