using ASCOM.DriverAccess;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        System.Windows.Point? _clicked;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _camera = new Camera("ASCOM.Simulator.Camera");
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

        double backgroundLevel = 0;

        unsafe void ProcessImage()
        {
            var width = _camera.CameraXSize;
            var height = _camera.CameraYSize;
            var max = 0;
            var starPeakAdu = 0;
            var pixels = GetCameraAs16Bits(width, height, out max);
            var scale = (double)ushort.MaxValue / max;
            var maxAdu = _camera.MaxADU;
            var peakAdu = maxAdu; // maxAdu * scale;

            var mat = new Mat(height, width, MatType.CV_16UC1, pixels);
            
            var mat2 = mat.GaussianBlur(new OpenCvSharp.Size(5, 5), 0, 0, BorderTypes.Default);

            if (backgroundLevel == 0)
            {
                backgroundLevel = Math.Round(Photometry.FindSkyBackgroundIntensity(pixels) * 1.5);

                Dispatcher.Invoke(() =>
                {
                    blackLevel.Text = backgroundLevel.ToString();
                });
            }
            
            mat2 = mat2.Threshold(backgroundLevel, ushort.MaxValue, ThresholdTypes.Binary);

            var mat3 = new Mat();
            
            mat2.ConvertTo(mat3, MatType.CV_8UC1);

            var contours = mat3.FindContoursAsMat(RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            
            //mat.DrawContours(contours, -1, new Scalar(ushort.MaxValue), thickness: 2);

            var found = contours.Length;
            var sat = 0;
            var w = 0.0;
            var error = 0.0;
            StarInfo star = null;
            GaussianFitOptions fitOptions = GaussianFitOptions.Default;

            var graph = new Mat(256, 256, MatType.CV_8UC1);

            foreach (var contour in contours)
            {
                Point2f center;
                float radius;

                contour.MinEnclosingCircle(out center, out radius);
                
                graph.SetTo(new Scalar(0));

                if (_clicked != null)
                {
                    var clickDistance = (center.X - _clicked.Value.X) * (center.X - _clicked.Value.X) + (center.Y - _clicked.Value.Y) * (center.Y - _clicked.Value.Y);

                    if (clickDistance < radius * radius)
                    {
                        var mass = 0;
                        var peakX = 0;
                        var peakY = 0;

                        Photometry.FindStarMassAndPeak(pixels, width, height, center.X, center.Y, radius, peakAdu, out mass, out starPeakAdu, out peakX, out peakY);

                        fitOptions.Radius = (int)radius * 2;

                        peakX = (int)center.X;
                        peakY = (int)center.Y;
                        
                        if (starPeakAdu == peakAdu)
                        {
                            sat++;
                        }
                       
                        star = new StarInfo { Peak = starPeakAdu, X = peakX, Y = peakY };
                        var fit = Photometry.FindStarGaussianPSF(pixels, width, height, star, -1, GaussianFitOptions.Default);
                        w = Photometry.GetFullWidthHalfMaximum(fit.Width);
                        error = Photometry.GaussianFitError(star.Peak, fit.Width, pixels, width, height, star, GaussianFitOptions.Default);
                        
                        var samples = new List<Tuple<double, double>>();

                        for (var x=star.X-fitOptions.Radius; x<star.X+fitOptions.Radius; x++)
                        {
                            for (var y=star.Y-fitOptions.Radius; y<star.Y+fitOptions.Radius; y++)
                            {
                                var distance = (double)((x - star.X) * (x - star.X) + (y - star.Y) * (y - star.Y));
                                var r = fitOptions.Radius / 2 * fitOptions.Radius / 2;

                                if(distance < r)
                                {
                                    distance = Math.Sqrt(distance);

                                    if (x < star.X) distance *= -1;

                                    samples.Add(new Tuple<double, double>(distance, pixels[y * width + x]));
                                }
                            }
                        }

                        var distances = samples.Select(x => x.Item1);
                        var minX = distances.Min();
                        var maxX = distances.Max();

                        var amplitudes = samples.Select(x => x.Item2);
                        var minY = amplitudes.Min();
                        var maxY = amplitudes.Max();

                        var scaleX = 160.0 / (maxX - minX);
                        var scaleY = 200.0 / (maxY - minY);

                        foreach (var sample in samples)
                        {
                            var x = 49 + (sample.Item1 - minX) * scaleX;
                            var y = 29 + (sample.Item2 - minY) * scaleY;

                            graph.Set<byte>(256 - (int)y, (int)x, byte.MaxValue);
                        }

                        var lastY = -1;

                        for (var x2 = minX; x2 < maxX; x2+=.01)
                        {
                            var distance = x2;
                            var y2 = Photometry.GaussianAmplitudeFromPSF(distance * distance, starPeakAdu, fit.Width);

                            //if (lastY == -1)
                            //{
                                lastY = (int)y2;
                            //}

                            var x = 49 + (x2 - minX) * scaleX;
                            var y = 29 + (y2 - minY) * scaleY;
                            var y3 = 29 + (lastY - minY) * scaleY;

                            if (lastY > y)
                            {
                                for (var yi = (int)y; yi < y3; yi++)
                                {
                                    graph.Set(256 - yi, (int)x, byte.MaxValue);
                                }
                            }
                            else
                            {
                                for (var yi = (int)y3; yi < y; yi++)
                                {
                                    graph.Set(256 - yi, (int)x, byte.MaxValue);
                                }
                            }

                            lastY = (int)y2;
                        }
                        
                        break;
                    }
                }

                // draw saturated pixels only
                //mat = mat.Threshold(peakAdu-1, peakAdu, ThresholdTypes.Binary);
                //mat.Circle(center, (int)radius, new Scalar(ushort.MaxValue), thickness: 2);                
            }

            Dispatcher.Invoke(() =>
            {
                stars.Content = found;
                fwhm.Content = Math.Round(w, 2);
                starPeak.Content = starPeakAdu;
                fitError.Content = error;
                mat *= scale;

                if(star != null)
                {
                    var rect = new OpenCvSharp.Rect(star.X - fitOptions.Radius, star.Y - fitOptions.Radius, fitOptions.Radius * 2, fitOptions.Radius * 2);
                    var sub2 = new Mat(fitOptions.Radius, fitOptions.Radius, MatType.CV_16SC1);
                    var sub = new Mat(mat, rect);
                    sub.CopyTo(sub2);
                    sub2 = sub2.Resize(new OpenCvSharp.Size(256, 256), 0, 0, InterpolationFlags.Area);
                    sub2 -= backgroundLevel * scale;
                    saturated.Content = sat > 0 ? "Yes" : "No";
                    focus.Source = sub2.ToWriteableBitmap();
                    plot.Source = graph.ToWriteableBitmap();
                }

                preview.Source = mat.ToWriteableBitmap();
            });
        }

        ushort[] GetCameraAs16Bits(int width, int height, out int max)
        {
            var camera = (int[,])_camera.ImageArray;
            var pixels = new ushort[width * height];
            
            var maxAdu = _camera.MaxADU;
            var offset = 0;

            max = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    pixels[offset] = (ushort)camera[x, y];
                    max = pixels[offset] > max ? pixels[offset] : max;
                    offset++;
                }
            }

            return pixels;
        }
        
        ushort Stretch(ushort[] pixels, int max, int scaleTo)
        {
            var scale = (ushort)((double)scaleTo / max);

            for (var i=0; i<pixels.Length; i++)
            {
                pixels[i] *= scale;
            }

            return scale;
        }

        List<OpenCvSharp.Rect> FindStars(Mat binary, Mat image, double backgroundLevel)
        {
            var results = new List<OpenCvSharp.Rect>();
            
            var contours = binary.FindContoursAsMat(RetrievalModes.List, ContourApproximationModes.ApproxNone);
            
            image.DrawContours(contours, -1, Scalar.White, thickness: 1);

            return results;
        }

        private void blackLevel_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            double.TryParse(blackLevel.Text, out backgroundLevel);
        }

        private void preview_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _clicked = e.GetPosition(preview);
        }
    }
}
