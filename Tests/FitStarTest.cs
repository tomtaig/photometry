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

            Image = new ushort[ImageHeight, ImageWidth];

            for (var y=0; y<ImageHeight; y++)
            {
                for (var x=0; x<ImageWidth; x++)
                {
                    this.Image[y, x] = mat.At<ushort>(y, x);
                }
            }
        }

        public Func<StarInfo, string> Name { get; set; }
        public Func<StarInfo, GaussianFitOptions> Options { get; set; }
        public ushort[,] Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }
}
