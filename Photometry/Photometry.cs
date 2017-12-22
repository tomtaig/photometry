using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Photometry
    {
        public static List<StarInfo> FindStars(Mat image, double peakMin)
        {
            var results = new List<StarInfo>();

            results.AddRange(FindStarCenters(image, peakMin));
                
            foreach (var star in results)
            {
                var sampleX = star.X + 1;
                var sampleY = star.Y + 1;
                var sampleValue = image.At<ushort>(sampleY, sampleX);
                var distanceSquared = 1.0;

                star.Width = SigmaFromAmplitude(star.Peak, distanceSquared, sampleValue);                
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="stars"></param>
        /// <returns></returns>
        public static List<GaussianFit> FindStarSigmaWidths(ushort[,] image, int iWidth, int iHeight, List<StarInfo> stars)
        {
            return FindStarSigmaWidths(image, iWidth, iHeight, stars, GaussianFitOptions.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="stars"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static List<GaussianFit> FindStarSigmaWidths(ushort[,] image, int iWidth, int iHeight, List<StarInfo> stars, GaussianFitOptions options)
        {
            var results = new List<GaussianFit>();

            foreach (var star in stars)
            {
                results.Add(FindStarSigmaWidth(image, iWidth, iHeight, star, stars.IndexOf(star), options));
            }

            return results;
        }

        public static GaussianFit FindStarSigmaWidth(ushort[,] image, int iWidth, int iHeight, StarInfo star, int starReference, GaussianFitOptions options)
        {
            return FindSigmaForPDF(image, iWidth, iHeight, star, starReference, options);
        }

        /// <summary>
        /// Find a sigma value for the Gaussian Point Distribution Function that produces less error
        /// </summary>
        /// <param name="image"></param>
        /// <param name="star"></param>
        /// <param name="starReference"></param>
        /// <param name="options"></param>
        static GaussianFit FindSigmaForPDF(ushort[,] image, int iWidth, int iHeight, StarInfo star, int starReference, GaussianFitOptions options)
        {
            var peak = star.Peak;
            var width = SigmaFromAmplitude(star.Peak, 2, (double)image[star.Y + 1, star.X + 1]);
            var widthStep = 1.0;
            var iterations = 0;

            var x1 = star.X - options.RadiusToSample;
            var x2 = star.X + options.RadiusToSample;
            var y1 = star.Y - options.RadiusToSample;
            var y2 = star.Y + options.RadiusToSample;

            if(x1 < 0 || y1 < 0 || x2 > iWidth || y2 > iHeight)
            {
                return new GaussianFit { Result = GaussianFitResult.Clipped };
            }

            for (iterations = 0; iterations < options.MaxIterations && Math.Abs(widthStep) > options.MinimumChangeThreshold; iterations++)
            {
                var direction = GradientTowardsMinimalErrorForGaussian(peak, width, image, iWidth, iHeight, star, options);
                widthStep = options.IterationStepSize * direction[0];
                width = width + widthStep;
            }

            return new GaussianFit
            {
                Width = width,
                Iterations = iterations,
                StarReference = starReference,
                Result = iterations == options.MaxIterations ? GaussianFitResult.IterationsMaxed : GaussianFitResult.StepMinimumReached
            };
        }
        
        /// <summary>
        /// Calculate the slope towards Gaussian function parameters that produce less error
        /// </summary>
        /// <param name="peak"></param>
        /// <param name="width"></param>
        /// <param name="image"></param>
        /// <param name="star"></param>
        /// <returns></returns>
        public static double[] GradientTowardsMinimalErrorForGaussian(double peak, double width, ushort[,] pixels, int iWidth, int iHeight, StarInfo star, GaussianFitOptions options)
        {
            var vec = new double[1];
            var samples = (options.RadiusToSample * 2.0) * (options.RadiusToSample * 2.0) - 1;
            var x1 = star.X - options.RadiusToSample;
            var x2 = star.X + options.RadiusToSample;
            var y1 = star.Y - options.RadiusToSample;
            var y2 = star.Y + options.RadiusToSample;
            
            for (var y = y1; y < y2; y++)
            {
                for (var x = x1; x < x2; x++)
                {
                    if (x == star.X && y == star.Y) continue;
                    
                    var sample = pixels[y, x];                        

                    var distanceSquared = (x - star.X) * (x- star.X) + (y - star.Y) * (y - star.Y);
                    var prediction = GaussianAmplitude(distanceSquared, peak, width);
                    var slope = GaussianSigmaErrorSlopeFunction(distanceSquared, star.Peak, sample, prediction);

                    vec[0] += (-1 * slope) / samples;
                }
            }
            
            return vec;
        }

        /// <summary>
        /// Amplitude at a point on a Gaussian curve
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="amplitude"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double GaussianAmplitude(double distanceSquared, double amplitude, double sigma)
        {
            var xb2 = distanceSquared;
            var sigma2 = sigma * sigma;

            return amplitude * Math.Exp(-1 * xb2 / (2.0 * sigma2));
        }

        /// <summary>
        /// Trends to zero as signal goes towards peak amplitude
        /// Trends to infinity as signal goes away from peak amplitude
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="amplitude"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double InvertedGaussianAmplitude(double distanceSquared, double amplitude, double sigma)
        {
            var xb2 = distanceSquared;
            var sigma2 = sigma * sigma;

            return amplitude * (xb2 / (2.0 * sigma2));
        }

        /// <summary>
        /// Trends to zero as sigma goes towards sampled sigma
        /// Trends to infinity as sigma goes away from sampled sigma
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="amplitude"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double GaussianSigmaErrorFunction(double distanceSquared, double amplitude, double sigma, double sigmaSample)
        {
            var xb2 = distanceSquared;
            var sigmaDist = sigma - sigmaSample;
            var sigmaDist2 = sigmaDist * sigmaDist;

            return 1.0 / (amplitude * (xb2 / (2.0 * sigmaDist2)));
        }

        /// <summary>
        /// Slopes down the error function curve to the lowest point - where the sigma produces the least error
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="amplitude"></param>
        /// <param name="sigmaSample"></param>
        /// <param name="sigmaPrediction"></param>
        /// <returns></returns>
        public static double GaussianSigmaErrorSlopeFunction(double distanceSquared, double amplitude, double sigmaSample, double sigmaPrediction)
        {
            var xb2 = distanceSquared;
            var top = 4 * (sigmaPrediction - sigmaSample);

            return top / (amplitude * xb2);
        }

        /// <summary>
        /// Return the FWHM of a given sigma
        /// </summary>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static double GetFullWidthHalfMaximum(double sigma)
        {
            return sigma * 2.3548;
        }

        /// <summary>
        /// Get a sigma value from an amplitude at position x on the curve of a gaussian
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="xAmplitude"></param>
        /// <returns></returns>
        public static double SigmaFromAmplitude(double amplitude, double distanceSquared, double xAmplitude)
        {
            var xb2 = distanceSquared;

            return Math.Sqrt(Math.Abs((xb2 / 2F) / Math.Log(xAmplitude / amplitude)));
        }
        
        /// <summary>
        /// Find the centers of stars in an image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        static List<StarInfo> FindStarCenters(Mat image, double peakMin)
        {
            var results = new List<StarInfo>();

            for (var y = 0; y < image.Height - 3; y++)
            {
                for (var x = 0; x < image.Width - 3; x++)
                {
                    var center = image.At<ushort>(y + 1, x + 1);

                    if (center < peakMin)
                    {
                        continue;
                    }

                    var surrounds = new float[]
                    {
                        image.At<ushort>(y+0, x+0),
                        image.At<ushort>(y+0, x+1),
                        image.At<ushort>(y+0, x+2),
                        image.At<ushort>(y+1, x+0),
                        image.At<ushort>(y+1, x+2),
                        image.At<ushort>(y+2, x+0),
                        image.At<ushort>(y+2, x+1),
                        image.At<ushort>(y+2, x+2)
                    };

                    if (!surrounds.Any(v => v >= center || v == 0))
                    {
                        results.Add(new StarInfo { X = x + 1, Y = y + 1, Peak = center });
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Subtract a whole image by a given value
        /// </summary>
        /// <param name="image"></param>
        /// <param name="offset"></param>
        public static void Subtract(Mat image, ushort offset)
        {
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var value = image.At<ushort>(y, x) - offset;

                    image.Set(y, x, value < 0 ? (ushort)0 : (ushort)value);
                }
            }
        }

        /// <summary>
        /// Find the mean background value for an image
        /// </summary>
        /// <param name="image">The image</param>
        /// <returns>Mean background value</returns>
        public static double FindSkyBackgroundIntensity(Mat image)
        {
            var pixels = new ushort[image.Width * image.Height];

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    pixels[y * image.Width + x] = image.At<ushort>(y, x);
                }
            }

            var background = KappaSigmaClip(pixels, 3.0, 5);
            var mean = 0.0;

            foreach (var index in background)
            {
                mean += pixels[index];
            }

            return mean / background.Count;
        }

        /// <summary>
        /// Kappa sigma iterative rejection
        /// </summary>
        /// <param name="pixels">Flat list of pixel values</param>
        /// <param name="sigma">Reject pixels more than sigma standard deviations from the mean</param>
        /// <param name="iterations">Maximum iterations to perform</param>
        /// <returns>Surviving pixels</returns>
        public static List<int> KappaSigmaClip(ushort[] pixels, double sigma, int iterations)
        {
            var result = new List<int>();

            for (var i = 0; i < pixels.Length; i++)
            {
                result.Add(i);
            }

            for (var i = 0; i < iterations && result.Count > pixels.Length / 2; i++)
            {
                var mean = 0.0;

                foreach (var index in result)
                {
                    mean += pixels[index];
                }

                mean /= result.Count;

                var deviation = new List<double>();

                foreach (var index in result)
                {
                    deviation.Add((pixels[index] - mean) * (pixels[index] - mean));
                }

                var std = Math.Sqrt(deviation.Sum() / (result.Count - 1));
                var rejected = 0;

                for (var n = 0; n < deviation.Count; n++)
                {
                    if (Math.Sqrt(deviation[n]) > std * sigma)
                    {
                        deviation.RemoveAt(n);
                        result.RemoveAt(n--);
                        rejected++;
                    }
                }

                if (rejected == 0)
                {
                    break;
                }
            }

            return result;
        }
    }
}
