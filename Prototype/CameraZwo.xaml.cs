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
    /// Interaction logic for CameraZwo.xaml
    /// </summary>
    public partial class CameraZwo : UserControl
    {
        public CameraZwo()
        {
            InitializeComponent();
        }

        public void GainChanged(object sender, RoutedEventArgs e)
        {
            var model = (CameraView)DataContext;

            model.ChangeGain(gain.Value);
        }

        public void OffsetChanged(object sender, RoutedEventArgs e)
        {
            var model = (CameraView)DataContext;

            model.ChangeOffset(offset.Value);
        }

        public void GammaChanged(object sender, RoutedEventArgs e)
        {
            var model = (CameraView)DataContext;

            model.ChangeGamma(gamma.Value);
        }
    }
}
