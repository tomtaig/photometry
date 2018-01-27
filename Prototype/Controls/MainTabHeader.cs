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

namespace Prototype.Controls
{
    public class MainTabHeader : ContentControl
    {
        public static readonly DependencyProperty IsConnectedProperty = DependencyProperty.Register("IsConnected", typeof(bool),
                         typeof(MainTabHeader), new FrameworkPropertyMetadata(false));

        public bool IsConnected
        {
            get { return (bool)GetValue(IsConnectedProperty); }
            set { SetValue(IsConnectedProperty, value); }
        }

        static MainTabHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainTabHeader), new FrameworkPropertyMetadata(typeof(MainTabHeader)));
        }
    }
}
