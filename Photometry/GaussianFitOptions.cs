using System;

namespace Tests
{
    public class GaussianFitOptions
    {
        public GaussianFitOptions()
        {
            MaxIterations = 1000;
            MinimumChangeThreshold = 0.0;
            RadiusToSample = 3;
            IterationStepSize = 0.1;
            Desc = star => $"{RadiusToSample + 1}x{RadiusToSample + 1} sample: step {IterationStepSize}";
        }

        public int MaxIterations { get; set; }
        public double MinimumChangeThreshold { get; set; }
        public int RadiusToSample { get; set; }
        public double IterationStepSize { get; set; }
        public Func<StarInfo, string> Desc { get; set; }

        public static readonly GaussianFitOptions Default = new GaussianFitOptions();
    }
}
