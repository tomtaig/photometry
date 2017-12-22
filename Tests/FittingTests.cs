using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class FittingTests
    {
        [TestMethod]
        public void GaussianFormulaCheck()
        {
            var expected = 2.0818004840828084;
            var actual = GaussianAmplitude(0.0, 8.0, 4.0, 7.0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GaussianInvertedFormulaCheck()
        {
            var expected = 1.0204081632653062;
            var actual = InvertedGaussianAmplitude(3.0, 8.0, 4.0, 7.0);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GaussianInvertedWidthEqualsZeroAtPeakAmplitude()
        {
            var actual = new double[] 
            {
                InvertedGaussianAmplitude(8.0, 8.0, 4.0, 7.0),
                GaussianAmplitude(8.0, 8.0, 4.0, 7.0)
            };

            Assert.AreEqual(0.0, actual[0]);
            Assert.AreEqual(4.0, actual[1]);
        }

        [TestMethod]
        public void GaussianSigmaErrorFunctionCheck()
        {
            var actual = new double[]
            {
                GaussianSigmaErrorFunction(3.0, 8.0, 4.0, 6.0, 7.0),
                GaussianSigmaErrorFunction(3.0, 8.0, 4.0, 8.0, 7.0)
            };

            Assert.AreEqual(0.02, actual[0]);
            Assert.AreEqual(0.02, actual[1]);
        }

        [TestMethod]
        public void GaussianSigmaFromSampleAmplitudeCheck()
        {
            const double epsilon = 0.00000001;

            var sigma = 7.0;
            var amplitude = GaussianAmplitude(6.0, 8.0, 4.0, sigma);
            var derivedSigma = SigmaFromAmplitude(4.0, 6.0, 8.0, amplitude);

            if(Math.Abs(derivedSigma - sigma) > epsilon)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GaussianSigmaErrorSlopesTowardsAnswer()
        {
            var actual = new double[]
            {
                GaussianSigmaErrorSlopeFunction(3.0, 8.0, 4.0, 7.0, 2.0),
                GaussianSigmaErrorSlopeFunction(3.0, 8.0, 4.0, 7.0, 6.0),
                GaussianSigmaErrorSlopeFunction(3.0, 8.0, 4.0, 7.0, 8.0),
                GaussianSigmaErrorSlopeFunction(3.0, 8.0, 4.0, 7.0, 12.0)
            };

            Assert.AreEqual(-0.2, actual[0]);
            Assert.AreEqual(-0.04, actual[1]);
            Assert.AreEqual(+0.04, actual[2]);
            Assert.AreEqual(+0.2, actual[3]);
        }

        /// <summary>
        /// Amplitude at a point on a Gaussian curve
        /// </summary>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="amplitude"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        double GaussianAmplitude(double x, double xCenter, double amplitude, double sigma)
        {
            var xb2 = (x - xCenter) * (x - xCenter);
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
        double InvertedGaussianAmplitude(double x, double xCenter, double amplitude, double sigma)
        {
            var xb2 = (x - xCenter) * (x - xCenter);
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
        double GaussianSigmaErrorFunction(double x, double xCenter, double amplitude, double sigma, double sigmaSample)
        {
            var xb2 = (x - xCenter) * (x - xCenter);
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
        double GaussianSigmaErrorSlopeFunction(double x, double xCenter, double amplitude, double sigmaSample, double sigmaPrediction)
        {
            var xb2 = (x - xCenter) * (x - xCenter);
            var top = 4 * (sigmaPrediction - sigmaSample);

            return top / (amplitude * xb2);
        }

        /// <summary>
        /// Get a sigma value from an amplitude at position x on the curve of a gaussian
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="x"></param>
        /// <param name="xCenter"></param>
        /// <param name="xAmplitude"></param>
        /// <returns></returns>
        double SigmaFromAmplitude(double amplitude, double x, double xCenter, double xAmplitude)
        {
            var xb2 = (x - xCenter) * (x - xCenter);

            return Math.Sqrt(Math.Abs((xb2 / 2F) / Math.Log(xAmplitude / amplitude)));
        }
    }
}
