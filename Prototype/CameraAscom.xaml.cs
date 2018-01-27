using Prototype.Services;
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
    /// Interaction logic for CameraAscom.xaml
    /// </summary>
    public partial class CameraAscom : UserControl
    {
        public CameraAscom()
        {
            InitializeComponent();
        }

        private void AscomSettingsClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;
            var service = (AscomCameraService)session.CameraService;
            var result = service.OpenSetupDialog();

            if (result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        } 
    }
}
