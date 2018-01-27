using Prototype.Model;
using System.Windows;
using System.Windows.Controls;

namespace Prototype
{
    /// <summary>
    /// Interaction logic for Camera.xaml
    /// </summary>
    public partial class Camera : UserControl
    {
        public Camera()
        {
            InitializeComponent();
        }

        private void InterfaceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            var result = session.SetCameraInterface();

            if (result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CameraSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.Camera.ChangeCamera();
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            var result = session.ConnectCamera();

            if(result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            var result = session.DisconnectCamera();
            
            if (result.IsError)
            {
                MessageBox.Show(result.ErrorMessage, "Connect Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
