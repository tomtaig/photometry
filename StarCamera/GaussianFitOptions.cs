using System;

namespace Tests
{
    public class GaussianFitOptions
    {
        public GaussianFitOptions()
        {
            MaxIterations = 1000;
            MinimumChangeThreshold = 0.0;
            Radius = 3;
            IterationStepSize = 0.1;
            Desc = star => $"{Radius + 1}x{Radius + 1} sample: step {IterationStepSize}";
        }

        public int MaxIterations { get; set; }
        public double MinimumChangeThreshold { get; set; }
        public int Radius { get; set; }
        public double IterationStepSize { get; set; }
        public Func<StarInfo, string> Desc { get; set; }

        public static readonly GaussianFitOptions Default = new GaussianFitOptions();
    }
}
