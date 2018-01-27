using OpenCvSharp;
using System;
using System.IO;

namespace Tests
{
    public class FitStarTest
    {
        public FitStarTest(string name, string imagePath, GaussianFitOptions options)
        {
            this.Options = star => options;
            this.Name = star => $"{name} - {options.Desc(star)}";
            LoadImage(imagePath);
        }

        public FitStarTest(string name, string imagePath, Func<StarInfo, GaussianFitOptions> options)
        {
            this.Options = options;
            this.Name = star => $"{name} - {options(star).Desc(star)}";
            LoadImage(imagePath);
        }

        void LoadImage(string imagePath)
        {
            var mat = new Mat(imagePath, ImreadModes.Unchanged);

            ImageHeight = mat.Height;
            ImageWidth = mat.Width;

            Image = new ushort[ImageHeight * ImageWidth];

            var image2 = new Mat(ImageHeight, ImageWidth, MatType.CV_16SC1, Image);

            mat.ConvertTo(image2, MatType.CV_16SC1);
        }

        public Func<StarInfo, string> Name { get; set; }
        public Func<StarInfo, GaussianFitOptions> Options { get; set; }
        public ushort[] Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }
}
