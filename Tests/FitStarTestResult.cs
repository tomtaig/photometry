using System;

namespace Tests
{
    public class FitStarTestResult
    {
        public const string Header = "Name,Peak,Width,Saturated,Estimated Width,Estimation Error,Iterations,Fit Result,Sample Radius,Iteration Step Size,Minimum Change Threshold,Max Iterations";

        public string Name { get; set; }
        public double Peak { get; set; }
        public double Width { get; set; }
        public double Estimated { get; set; }
        public double Error { get; set; }
        public int Iterations { get; set; }
        public string FitResult { get; set; }
        public int MaxIterations { get; set; }
        public double MinimumChangeThreshold { get; set; }
        public double IterationStepSize { get; set; }
        public int SampleRadius { get; set; }
        public bool Saturated { get; set; }

        string SaturatedVerbose => Saturated ? "Yes" : "No";

        public override string ToString()
        {
            return $"{Name},{Peak},{Width},{SaturatedVerbose},{Estimated},{Error},{Iterations},{FitResult},{SampleRadius},{IterationStepSize},{MinimumChangeThreshold},{MaxIterations}";
        }
    }
}
