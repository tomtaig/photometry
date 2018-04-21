using System;

namespace Tests
{
    public class GaussianFitOptions2
    {
        public GaussianFitOptions2()
        {
            MaxIterations = 500;
            MinimumChangeThreshold = 0.0;
            IterationStepSize = 0.0000015;
            StartSigma = 3.0;
        }

        public int MaxIterations { get; set; }
        public double MinimumChangeThreshold { get; set; }
        public double IterationStepSize { get; set; }
        public double StartSigma { get; set; }

        public static readonly GaussianFitOptions2 Default = new GaussianFitOptions2();
    }
}
