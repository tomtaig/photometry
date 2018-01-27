using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Photometry
    {
        public static List<StarInfo> FindStars(ushort[] pixels, int width, int height, double peakMin)
        {
            var results = new List<StarInfo>();

            results.AddRange(FindStarCenters(pixels, width, height, peakMin));
                
            foreach (var star in results)
            {
                var sampleX = star.X + 1;
                var sampleY = star.Y + 1;
                var sampleValue = pixels[sampleY * width + sampleX];
                var distanceSquared = 1.0;

                star.Width = GaussianSigmaFromSample(star.Peak, distanceSquared, sampleValue);                
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="stars"></param>
        /// <returns></returns>
        public static List<GaussianFit> FindStarSigmaWidths(ushort[] image, int iWidth, int iHeight, List<StarInfo> stars)
        {
            return FindStarGaussianPSF(image, iWidth, iHeight, stars, GaussianFitOptions.Default);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="stars"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static List<GaussianFit> FindStarGaussianPSF(ushort[] image, int iWidth, int iHeight, List<StarInfo> stars, GaussianFitOptions options)
        {
            var results = new List<GaussianFit>();

            foreach (var star in stars)
            {
                results.Add(FindStarGaussianPSF(image, iWidth, iHeight, star, stars.IndexOf(star), options));
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        /// <param name="star"></param>
        /// <param name="starReference"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static GaussianFit FindStarGaussianPSF(ushort[] image, int iWidth, int iHeight, StarInfo star, int starReference, GaussianFitOptions options)
        {
            return FindGaussianPSF(image, iWidth, iHeight, star, starReference, options);
        }

        /// <summary>
        /// Find a sigma value for the Gaussian Point Distribution Function that produces less error
        /// </summary>
        /// <param name="image"></param>
        /// <param name="star"></param>
        /// <param name="starReference"></param>
        /// <param name="options"></param>
        static GaussianFit FindGaussianPSF(ushort[] image, int iWidth, int iHeight, StarInfo star, int starReference, GaussianFitOptions options)
        {
            var peak = star.Peak;
            var width = GaussianSigmaFromSample(star.Peak, 2, image[(star.Y + 1) * iWidth + star.X + 1]);
            var widthStep = 1.0;
            var iterations = 0;

            // happens when the star is saturated
            if(double.IsInfinity(width) || width == 0)
            {
                width = 5.0;
            }

            var x1 = star.X - options.Radius;
            var x2 = star.X + options.Radius;
            var y1 = star.Y - options.Radius;
            var y2 = star.Y + options.Radius;

            if(x1 < 0 || y1 < 0 || x2 > iWidth || y2 > iHeight)
            {
                return new GaussianFit { Result = GaussianFitResult.Clipped };
            }
                        
            for (iterations = 0; iterations < options.MaxIterations && Math.Abs(widthStep) > options.MinimumChangeThreshold; iterations++)
            {
                var direction = GradientTowardsMinimalErrorForGaussian(peak, width, image, iWidth, iHeight, star, options);
                widthStep = options.IterationStepSize * direction[0];
                width = width + widthStep;

                if(width < 0)
                {
                    width = 1.0;
                }
            }

            return new GaussianFit
            {
                Width = width,
                Peak = peak,
                Iterations = iterations,
                StarReference = starReference,
                Result = iterations == options.MaxIterations ? GaussianFitResult.IterationsMaxed : GaussianFitResult.StepMinimumReached
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="radius"></param>
        /// <param name="peakAdu"></param>
        /// <param name="mass"></param>
        /// <param name="peak"></param>
        /// <param name="peakX"></param>
        /// <param name="peakY"></param>
        public static void FindStarMassAndPeak(ushort[] image, int width, int height, float xc, float yc, float radius, int peakAdu, out int mass, out int peak, out int peakX, out int peakY)
        {
            var r = (int)Math.Ceiling(radius);
            var rsq = r * r;
            var same = 0;
            var mx = 0;
            var my = 0;

            mass = 0;
            peak = 0;

            peakX = -1;
            peakY = -1;

            for (var y = yc - r; y < yc + r; y++)
            {
                for (var x = xc - r; x < xc + r; x++)
                {
                    var d = (y - yc) * (y - yc) + (x - xc) * (x - xc);

                    if(d <= rsq)
                    {
                        int xi = (int)x;
                        int yi = (int)y;

                        var value = image[yi * width + xi];

                        mass += value;

                        if(peak <= value)
                        {
                            mx += xi;
                            my += yi;

                            same++;

                            peak = value;

                            peakX = xi;
                            peakY = yi;
                        }
                    }
                }
            }

            if(same > 1)
            {
                peakX = (mx / same);
                peakY = (my / same);
            }
        }

        /// <summary>
        /// Calculate the slope towards Gaussian function parameters that produce less error
        /// </summary>
        /// <param name="peak"></param>
        /// <param name="sigma"></param>
        /// <param name="image"></param>
        /// <param name="star"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] GradientTowardsMinimalErrorForGaussian(double peak, double sigma, ushort[] pixels, int iWidth, int iHeight, StarInfo star, GaussianFitOptions options)
        {
            var vec = new double[1];
            var samples = (options.Radius * 2.0) * (options.Radius * 2.0) - 1;
            var x1 = star.X - options.Radius;
            var x2 = star.X + options.Radius;
            var y1 = star.Y - options.Radius;
            var y2 = star.Y + options.Radius;
            
            for (var y = y1; y < y2; y++)
            {
                for (var x = x1; x < x2; x++)
                {
                    if (x == star.X && y == star.Y) continue;

                    var distanceSquared = (x - star.X) * (x - star.X) + (y - star.Y) * (y - star.Y);

                    if (distanceSquared <= options.Radius * 2)
                    {
                        var sample = pixels[y * iWidth + x];
                        var sigmaSample = GaussianSigmaFromSample(peak, distanceSquared, sample);

                        var sChange = GaussianSigmaErrorSlopeFunction(distanceSquared, peak, sigmaSample, sigma);
                        
                        vec[0] += (-1 * sChange);

                        samples++;
                    }
                }
            }

            vec[0] /= samples;
            
            return vec;
        }
        
        public static double GaussianFitError(double peak, double width, ushort[] pixels, int iWidth, int iHeight, StarInfo star, GaussianFitOptions options)
        {
            var samples = (options.Radius * 2.0) * (options.Radius * 2.0) - 1;
            var x1 = star.X - options.Radius;
            var x2 = star.X + options.Radius;
            var y1 = star.Y - options.Radius;
            var y2 = star.Y + options.Radius;
            var error = 0.0;

            for (var y = y1; y < y2; y++)
            {
                for (var x = x1; x < x2; x++)
                {
                    if (x == star.X && y == star.Y) continue;

                    var distanceSquared = (x - star.X) * (x - star.X) + (y - star.Y) * (y - star.Y);

                    if (distanceSquared <= options.Radius * 2)
                    {
                        var prediction = GaussianAmplitudeFromPSF(distanceSquared, peak, width);

                        error += (prediction - pixels[y * iWidth + x]) * (prediction - pixels[y * iWidth + x]);
                    }
                }
            }
            
            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GaussianPeakFromSample(int distanceSquared, double sigma, double sample)
        {
            var xb2 = distanceSquared;
            var sigma2 = sigma * sigma;

            return sample / Math.Exp(-1 * xb2 / (2.0 * sigma2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussianSigmaSlopeFromLeastSquares(double distanceSquared, double modelPeak, double modelSigma, double sample)
        {
            var a = modelPeak;
            var c = modelSigma;
            var s = sample;
            var c2 = c * c;
            var c3 = c2 * c;
            var b2 = distanceSquared;

            var lhs = 2 * a * b2 * Math.Exp(-1 * (b2 / c2));
            var rhs = a - s * Math.Exp(b2 / (2 * c2));

            return (lhs * rhs) / c3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussianPeakSlopeFromLeastSquares(double distanceSquared, double modelPeak, double modelSigma, double sample)
        {
            var a = modelPeak;
            var c = modelSigma;
            var s = sample;
            var c2 = c * c;
            var b2 = distanceSquared;

            var lhs = 2 * Math.Exp(-1 * (b2 / c2));
            var rhs = a - s * Math.Exp(b2 / (2 * c2));

            return lhs * rhs;
        }

        /// <summary>
        /// Amplitude at a point on a Gaussian curve
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="peak"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussianAmplitudeFromPSF(double distanceSquared, double peak, double sigma)
        {
            var xb2 = distanceSquared;
            var sigma2 = sigma * sigma;

            return peak * Math.Exp(-1 * xb2 / (2.0 * sigma2));
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

            return xb2 / (amplitude * (xb2 / (2.0 * sigmaDist2)));
        }

        /// <summary>
        /// Slopes down the error function curve to the lowest point - where the sigma produces the least error
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="peak"></param>
        /// <param name="sigmaSample"></param>
        /// <param name="sigmaPrediction"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussianSigmaErrorSlopeFunction(double distanceSquared, double peak, double sigmaSample, double sigmaPrediction)
        {
            var xb2 = distanceSquared;
            var top = 4 * (sigmaPrediction - sigmaSample);

            return top / (peak * xb2);
        }

        /// <summary>
        /// Trends to zero as peak goes towards sampled peak
        /// Trends to infinity as peak goes away from sampled peak
        /// </summary>
        public static double GaussianPeakErrorFunction(double peakSample, double peakPrediction, double sigma)
        {
            var xm2 = (peakPrediction - peakSample) * (peakPrediction - peakSample);
            var s2 = (sigma * sigma);

            return xm2 * (1.0 / (2 * s2));
        }

        /// <summary>
        /// Slopes down the error function curve to the lowest point -where the peak produces the least error
        /// </summary>
        /// <param name="peakSample"></param>
        /// <param name="peakPrediction"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GaussianPeakErrorSlopeFunction(double peakSample, double peakPrediction, double sigma)
        {
            var xm2 = (peakPrediction - peakSample);
            var s2 = (sigma * sigma);

            return xm2 / s2;
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
        /// <param name="sample"></param>
        /// <returns></returns>
        public static double GaussianSigmaFromSample(double amplitude, double distanceSquared, double sample)
        {
            var xb2 = distanceSquared;

            return 2.0 * Math.Sqrt(Math.Abs((xb2 / 2F) / Math.Log(sample / amplitude)));
        }
        
        /// <summary>
        /// Find the centers of stars in an image
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        static List<StarInfo> FindStarCenters(ushort[] image, int width, int height, double peakMin)
        {
            var results = new List<StarInfo>();

            for (var y = 0; y < height - 3; y++)
            {
                for (var x = 0; x < width - 3; x++)
                {
                    var center = image[(y + 1) * width + x + 1];

                    if (center < peakMin)
                    {
                        continue;
                    }

                    var surrounds = new float[]
                    {
                        image[(y + 0) * width + x + 0],
                        image[(y + 0) * width + x + 1],
                        image[(y + 0) * width + x + 2],
                        image[(y + 1) * width + x + 0],
                        image[(y + 1) * width + x + 2],
                        image[(y + 2) * width + x + 0],
                        image[(y + 2) * width + x + 1],
                        image[(y + 2) * width + x + 2]
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
        public static double FindSkyBackgroundIntensity(ushort[] image)
        {
            var background = KappaSigmaClip(image, 3.0, 5);
            var mean = 0.0;

            for (var y = 0; y < image.Length; y++)
            {
                mean += image[y];
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
