namespace Tests
{
    public class GaussianFit
    {
        public int StarReference { get; set; }
        public double Width { get; set; }
        public int Iterations { get; set; }
        public GaussianFitResult Result { get; internal set; }
    }
}
