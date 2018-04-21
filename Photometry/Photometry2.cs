using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tests;
using System.Diagnostics;

namespace Tests
{
    public static class Photometry2
    {
        public static GaussianFit FindGaussianPSF(ushort[] samples, double peak, double center, GaussianFitOptions2 options)
        {
            var width = options.StartSigma;
            var widthStep = 1.0;
            var peakStep = 1.0;
            var iterations = 0;
            
            for (iterations = 0; iterations < options.MaxIterations && Math.Abs(widthStep) > options.MinimumChangeThreshold; iterations++)
            {
                var direction = GradientTowardsMinimalErrorForGaussian(peak, width, samples, center, options);

                widthStep = options.IterationStepSize * direction[0];
                width = width + widthStep;

                //peakStep = options.IterationStepSize * direction[1];
                //peak = peak + peakStep;
                
                if (width < .1)
                {
                    Debug.WriteLine($"Error: sigma got too small");

                    return new GaussianFit { Result = GaussianFitResult.Error };
                }
                else if (width > 20)
                {
                    Debug.WriteLine($"Error: sigma got too large");

                    return new GaussianFit { Result = GaussianFitResult.Error };
                }

                if (peak < 0)
                {
                    Debug.WriteLine($"Error: peak got too small");

                    return new GaussianFit { Result = GaussianFitResult.Error };
                }

                PrintResiduals(samples, peak, center, width);
            }
            
            return new GaussianFit
            {
                Width = width,
                Peak = peak,
                Iterations = iterations,
                Result = iterations == options.MaxIterations ? GaussianFitResult.IterationsMaxed : GaussianFitResult.StepMinimumReached
            };
        }

        static void PrintResiduals(ushort[] samples, double peak, double center, double width)
        {
            var debug = new StringBuilder();

            var prefix = $"Sigma: {width.ToString("N5")} - ";
            debug.Append(prefix.PadLeft(60 - prefix.Length));

            for (var i = 0; i < samples.Length; i++)
            {
                var res = Math.Sqrt(Math.Abs(GetResidualFromEstimate(samples[i], i, center, width, peak)));

                var text = $"s[{(i - center).ToString("N2")}]={res.ToString("N2")};";
                text = text.PadLeft(60 - text.Length);
                debug.Append(text);
            }

            Debug.WriteLine(debug.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double[] GradientTowardsMinimalErrorForGaussian(double peak, double sigma, ushort[] samples, double center, GaussianFitOptions2 options)
        {
            var vec = new double[2];
            var used = 0;

            for (var x = 0; x < samples.Length; x++)
            {
                var distance = (x - center);
                
                if(distance <= 5)
                {
                    continue;
                }

                var distanceSquared = distance * distance;
                
                var sample = samples[x];
                
                var sChange = Photometry2.SigmaSlopeFromDerivative((double)sample, (double)x, center, sigma, peak);
                var pChange = Photometry2.PeakSlopeFromDerivative((double)sample, (double)x, center, sigma, peak);

                if (double.IsPositiveInfinity(sChange) || double.IsNegativeInfinity(sChange))
                {
                    throw new ArgumentException();
                }

                if (double.IsPositiveInfinity(pChange) || double.IsNegativeInfinity(pChange))
                {
                    throw new ArgumentException();
                }

                vec[0] += (1 * sChange);
                vec[1] += (1 * pChange);
                
                used++;
            }

            vec[0] /= used;
            vec[1] /= used;

            return vec;
        }

        public static double GetResidualFromEstimate(double sample, double x, double xc, double sigma, double amplitude)
        {
            var estimate = amplitude * (Math.Exp(-1 * ((x - xc) * (x - xc)) / (2 * (sigma * sigma))));

            return sample - estimate;
        }

        public static double SigmaSlopeFromDerivative(double sample, double x, double xc, double sigma, double a)
        {
            var d2 = (x - xc) * (x - xc);
            var c2 = sigma * sigma;
            var c3 = c2 * sigma;

            var m = (a * Math.Exp(-1 * (d2 / (2 * c2))));

            var num2 = sample - m;
            var numerator = 2 * a * d2 * Math.Exp(-1 * (d2 / (2 * c2))) * (num2);

            return numerator / (c3 * Math.Sqrt(m * m));
        }

        public static double PeakSlopeFromDerivative(double sample, double x, double xc, double sigma, double a)
        {
            var d2 = (x - xc) * (x - xc);
            var c2 = sigma * sigma;
            var a2 = a * a;

            var numerator = a * Math.Exp(-1 * (d2 / c2));
            var denominator = Math.Sqrt(Math.Abs(sample - a2 * Math.Exp(-1 * (d2 / c2))));

            return -1 * numerator / denominator;
        }
    }
}
