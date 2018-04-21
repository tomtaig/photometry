using System;

namespace Tests
{
    public class GaussianFitOptions2
    {
        public GaussianFitOptions2()
        {
            MaxIterations = 10000;
            MinimumChangeThreshold = 0.0;
            IterationStepSize = 0.01;
        }

        public int MaxIterations { get; set; }
        public double MinimumChangeThreshold { get; set; }
        public double IterationStepSize { get; set; }

        public static readonly GaussianFitOptions2 Default = new GaussianFitOptions2();
    }
}
