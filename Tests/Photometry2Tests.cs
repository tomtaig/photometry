using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class Photometry2Tests
    {
        [TestMethod]
        public void ShouldDecreaseSigma()
        {
            var slopes = new double[]
            {
                Photometry2.SigmaSlopeFromDerivative(381.640843293, 1, 6, 3.0, 1500),
                Photometry2.SigmaSlopeFromDerivative(381.640843293, 1, 6, 3.022, 1500),
                Photometry2.SigmaSlopeFromDerivative(381.640843293, 1, 6, 3.2, 1500)
            };

            var residuals = new double[]
            {
                Photometry2.GetResidualFromEstimate(381.640843293, 1, 6, 3.022, 1500)
            };
        }
    }
}
