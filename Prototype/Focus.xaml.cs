using OpenCvSharp;
using OpenCvSharp.Extensions;
using Prototype.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tests;

namespace Prototype
{
    /// <summary>
    /// Interaction logic for Focus.xaml
    /// </summary>
    public partial class Focus : UserControl
    {
        public Focus()
        {
            InitializeComponent();

            measurements.LabelFormatString = "{0:F2}";

            Loaded += OnLoaded;

            measureScroll.SizeChanged += MeasureScroll_SizeChanged;
        }

        private void MeasureScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            measureScroll.ScrollToRightEnd();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            measureScroll.ScrollToRightEnd();
        }

        private void ExposureSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var session = (Session)DataContext;

            if (session != null)
            {
                session.ExposureChange(minSlider.Value, secSlider.Value, msecSlider.Value);
            }
        }

        private void ExposureValueChanged(object sender, TextChangedEventArgs e)
        {
            var session = (Session)DataContext;

            if (session != null)
            {
                Func<string, int> toValue = s => s == string.Empty ? 0 : int.Parse(s);

                session.ExposureChange(toValue(minValue.Text), toValue(secValue.Text), toValue(msecValue.Text));
            }
        }

        private void ZoomSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var session = (Session)DataContext;

            if (session != null)
            {
                session.Focus.SetZoom(null);
            }
        }

        void CaptureClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.SetLoopCapture(false);

            Capture(session);
        }

        void StopLoopClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.SetLoopCapture(false);
        }

        void LoopClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.SetLoopCapture(true);

            Capture(session);
        }

        void Capture(Session session)
        {
            session.Focus.FrameChanged = (new Action(() => 
            {
                Dispatcher.Invoke(() => RenderCapture());
            }));

            session.Focus.RegionChanged = (new Action(() =>
            {
                Dispatcher.Invoke(() => RenderRegion());
            }));

            session.Capture();
        }
        
        void BoostSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var session = (Session)DataContext;

            if (session != null)
            {
                session.Focus.BoostChange(null);

                RenderCapture();
                RenderRegion();
            }
        }

        void RenderCapture()
        {
            Dispatcher.Invoke(() =>
            {
                var session = (Session)DataContext;

                if (session.Focus.Capture != null)
                {
                    var capture = session.Focus.Capture.Image;
                    
                    if (session.Focus.Boost != 1)
                    {
                        capture = capture.Mul(capture, session.Focus.Boost);
                    }
                    
                    image.Source = capture.ToWriteableBitmap();
                }
            });
        }

        void RenderRegion()
        {
            var session = (Session)DataContext;
            var region = session.Focus.Region;

            if (region.Capture != null)
            {
                var capture = region.Capture.Image;

                if (session.Focus.Boost != 1)
                {
                    capture = capture.Mul(capture, session.Focus.Boost);
                }

                selection.Source = capture.ToWriteableBitmap();
            }
            else
            {
                selection.Source = null;
            }
        }

        private void SubFrameCheckboxChecked(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            if (subframeCheckbox.IsChecked.HasValue && subframeCheckbox.IsChecked.Value)
            {
                session.EnableSubFrame();
            }
            else
            {
                session.DisableSubFrame();
            }
        }

        private void ImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            var session = (Session)DataContext;
            
            var c = e.GetPosition(image);
            var width = 40;
            var width2 = width / 2;

            var x1 = c.X - width2;
            var y1 = c.Y - width2;
            var x2 = c.X + width2;
            var y2 = c.Y + width2;
            
            session.SelectFocusStar((int)y1, (int)x1, (int)y2, (int)x2);
        }
        
        void ClearStarSelectionClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.ClearStarSelection();
        }

        void ResetStretchClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.BoostChange(1.0);
        }

        void ReticleCrosshairsClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.SetReticle(ReticleType.Crosshair);
        }

        void ReticleNoneClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.SetReticle(ReticleType.None);
        }
    }
}
