using OpenCvSharp;
using OpenCvSharp.Extensions;
using Prototype.Model;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                session.Focus.ZoomChange(null);
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
            session.Capture(result =>
            {
                var capture = new CaptureView
                {
                    BinnedX = session.Camera.BinnedX,
                    BinnedY = session.Camera.BinnedY,
                    ChipX = session.Camera.UnbinnedX,
                    ChipY = session.Camera.UnbinnedY
                };

                if (session.Camera.IsSubFrameActive)
                {
                    capture.Frame = new CaptureView.SubFrame
                    {
                        ChipX1 = session.Camera.SubFrameX,
                        ChipY1 = session.Camera.SubFrameY,
                        ChipX2 = session.Camera.SubFrameX + session.Camera.SubFrameWidth,
                        ChipY2 = session.Camera.SubFrameY + session.Camera.SubFrameHeight
                    };
                }
                
                capture.Image = result;

                session.Focus.SetCapture(capture);
                
                RenderCapture(() => { });
            });
        }
        
        void BoostSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var session = (Session)DataContext;

            if (session != null)
            {
                session.Focus.BoostChange(null);

                RenderCapture(() => { });
            }
        }

        void RenderCapture(Action postRender)
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

                    session.Camera.ChangeActualSize(capture.Height, capture.Width);
                    image.Source = capture.ToWriteableBitmap();

                    RenderCaptureROI(session);

                    postRender();
                }
            });
        }

        void RenderCaptureROI(Session session)
        {
            if (session.Focus.Capture != null && session.Focus.IsStarSelected)
            {
                var focus = session.Focus;

                var x1 = focus.SelectedStarVisibleX;
                var y1 = focus.SelectedStarVisibleY;
                var x2 = x1 + focus.SelectedStarVisibleWidth;
                var y2 = y1 + focus.SelectedStarVisibleHeight;

                if (x1 < 0 || y1 < 0 || x2 > focus.Capture.Image.Height || y2 > focus.Capture.Image.Width)
                {
                    selection.Source = null;
                }
                else
                {
                    var rect = new OpenCvSharp.Rect((int)focus.SelectedStarVisibleY, (int)focus.SelectedStarVisibleX, (int)focus.SelectedStarVisibleHeight, (int)focus.SelectedStarVisibleWidth);
                    var roi = new Mat((int)focus.SelectedStarVisibleHeight, (int)focus.SelectedStarVisibleWidth, MatType.CV_16SC1);
                    var sub = new Mat(session.Focus.Capture.Image, rect);

                    sub.CopyTo(roi);

                    if (session.Focus.Boost != 1)
                    {
                        roi = roi.Mul(roi, session.Focus.Boost);
                    }

                    selection.Source = roi.ToWriteableBitmap();
                }
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
            var width = 20;
            var width2 = width / 2;

            var x1 = c.X - width2;
            var y1 = c.Y - width2;
            var x2 = c.X + width2;
            var y2 = c.Y + width2;
            
            session.SelectFocusStar((int)y1, (int)x1, (int)y2, (int)x2);

            RenderCaptureROI(session);
        }
        
        private void ReticleSelected(object sender, SelectionChangedEventArgs e)
        {
            var session = (Session)DataContext;

            var item = reticle.SelectedValue as ComboBoxItem;

            if(session != null && item != null)
            {
                var reticle = (ReticleType)Enum.Parse(typeof(ReticleType), item.Content.ToString());

                session.Focus.ShowReticle(reticle);
            }
        }

        void ClearStarSelectionClick(object sender, RoutedEventArgs e)
        {
            var session = (Session)DataContext;

            session.Focus.ClearStarSelection();
        }
    }
}
