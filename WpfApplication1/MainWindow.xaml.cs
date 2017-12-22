using ASCOM.DriverAccess;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tests;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        Camera _camera;
        Task _capture;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _camera = new Camera("ASCOM.AdvancedSimulator.Camera");

            _camera.Connected = true;            
        }

        void Capture(object sender, RoutedEventArgs e)
        {
            var seconds = double.Parse(exposure.Text);

            if (_capture == null)
            {
                _capture = new Task(() =>
                {
                    var start = DateTime.Now;
                    var frames = 0.0;

                    while (true)
                    {
                        _camera.StartExposure(seconds, true);

                        while (!_camera.ImageReady) ;

                        ProcessImage();

                        frames++;

                        var now = DateTime.Now;

                        if(now.Subtract(start).TotalSeconds > 1)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                fps.Content = Math.Round(frames / now.Subtract(start).TotalSeconds, 1);
                            });

                            start = DateTime.Now;
                            frames = 0.0;
                        }
                    }
                });

                _capture.Start();
            }
        }
        
        unsafe void ProcessImage()
        {
            var width = _camera.CameraXSize;
            var height = _camera.CameraYSize;
            var bpp = sizeof(byte);
            
            var camera = (int[,])_camera.ImageArray;
            var pixels = new byte[width * height];

            var offset = 0;
            var maxAdu = _camera.MaxADU;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    pixels[offset++] = (byte)((camera[x, y] * 255 ) / maxAdu);
                }
            }

            fixed (byte* p = &(pixels[0]))
            {
                var raw = new Mat(height, width, p);
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    raw.Set(y, x, (ushort)((camera[x, y] * ushort.MaxValue) / maxAdu));
                }
            }

            var found = Photometry.FindStars(raw, maxAdu * .75).Count;
            
            Dispatcher.Invoke(() =>
            {
                stars.Content = found;
                preview.Source = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, width * bpp * 1);
            });            
        }
        
        List<OpenCvSharp.Rect> FindStars(Mat image, double backgroundLevel)
        {
            var results = new List<OpenCvSharp.Rect>();
                        
            var formatted = new Mat();
            prepped.ConvertTo(formatted, MatType.CV_8UC1, byte.MaxValue);

            var contours = formatted.FindContoursAsMat(RetrievalModes.List, ContourApproximationModes.ApproxNone);

            image.DrawContours(contours, -1, Scalar.White, thickness: 1);
            
            return results;
        }
    }
}
