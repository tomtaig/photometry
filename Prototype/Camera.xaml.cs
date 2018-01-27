using Prototype.ViewModel;
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
            var model = (CameraView)DataContext;

            model.ChangeInterface();
        }

        private void CameraSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var model = (CameraView)DataContext;

            model.ChangeCamera();
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.ConnectCamera();
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            var session = Application.Current.Resources["session"] as Session;

            session.DisconnectCamera();
        }
    }
}
