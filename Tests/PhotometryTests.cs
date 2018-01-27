using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using nom.tam.fits;

namespace Tests
{
    [TestClass]
    public class PhotometryTests
    {
        string _biasPath = "d:\\p\\noise.fits";
        Random _rand = new Random();
                
        [TestMethod]
        public void FindStars()
        {
            var underperformed = new List<string>();

            for (var i = 0; i < 10; i++)
            {
                var matches = 0;
                var image = new Mat($"stars-{i}-1.png", ImreadModes.Unchanged);
                var photo = JsonConvert.DeserializeObject<FakePhoto>(File.ReadAllText($"stars-{i}.json"));
                var unsaturatedStars = photo.Stars.Where(x => x.Peak < ushort.MaxValue).Count();

                var pixels = new ushort[image.Height * image.Width];
                var image2 = new Mat(image.Height, image.Width, MatType.CV_16SC1, pixels);

                image.ConvertTo(image2, MatType.CV_16SC1);
               
                var results = Photometry.FindStars(pixels, image.Width, image.Height, 10000);
             
                foreach (var result in results)
                {
                    foreach (var star in photo.Stars)
                    {
                        var distanceSq = (result.X - star.X) * (result.X - star.X) + (result.Y - star.Y) * (result.Y - star.Y);
                        if (distanceSq <= 1) matches++;
                    }
                }
                
                if(matches < unsaturatedStars / 3)
                {
                    underperformed.Add($"stars-{i}.png");
                }
            }

            if(underperformed.Count > 0)
            {
                Assert.Fail("Found less than 33% of stars in some images");
            }
        }

        [TestMethod]
        public void FitStars()
        {
            var starCount = 0;

            var results = new List<FitStarTestResult>();

            var options = new List<GaussianFitOptions>
            {
                new GaussianFitOptions { Radius = 2, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 3, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 4, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 5, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 6, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 7, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 },
                new GaussianFitOptions { Radius = 8, MaxIterations = 1000, IterationStepSize = 10, MinimumChangeThreshold = 0.000001 }
            };
            
            var sources = new List<List<FitStarTest>>();
            var metadatas = new List<FakePhoto>();

            for (var i = 0; i < 10; i++)
            {
                var inputs = new Dictionary<string, string>
                {
                    { "No noise", $"stars-{i}-1.png" },
                    { "Hot pixels", $"stars-{i}-2.png" },
                    { "Hot pixels/shot noise", $"stars-{i}-3.png" },
                    { "Hot pixels/shot noise/read noise", $"stars-{i}-4.png" },
                    { "Hot pixels/shot noise/read noise/PRNU", $"stars-{i}-5.png" },
                    { "Hot pixels/shot noise/read noise/PRNU - BG removed", $"stars-{i}-6.png" }
                };

                var source = new List<FitStarTest>();

                foreach (var input in inputs)
                {
                    foreach (var option in options)
                    {
                        source.Add(new FitStarTest(input.Key, input.Value, option));
                    }
                }

                sources.Add(source);

                metadatas.Add(JsonConvert.DeserializeObject<FakePhoto>(File.ReadAllText($"stars-{i}.json")));
            }
                        
            var start = DateTime.Now;

            for (var i = 0; i < 10; i++)
            {
                var metadata = metadatas[i];

                foreach (var source in sources[i])
                {
                    foreach (var star in metadata.Stars)
                    {
                        starCount++;

                        var fitOption = source.Options(star);
                        var fit = Photometry.FindStarGaussianPSF(source.Image, source.ImageWidth, source.ImageHeight, star, 0, fitOption);

                        if (fit.Result == GaussianFitResult.Clipped)
                        {
                            continue;
                        }

                        results.Add(new FitStarTestResult
                        {
                            Name = source.Name(star),
                            Peak = star.Peak,
                            Width = star.Width,
                            Saturated = star.Peak > ushort.MaxValue,
                            Estimated = fit.Width,
                            Error = Math.Abs(fit.Width - star.Width),
                            Iterations = fit.Iterations,
                            FitResult = fit.Result.ToString(),
                            SampleRadius = fitOption.Radius,
                            IterationStepSize = fitOption.IterationStepSize,
                            MinimumChangeThreshold = fitOption.MinimumChangeThreshold,
                            MaxIterations = fitOption.MaxIterations
                        });
                    }
                }
            }

            var end = DateTime.Now;

            var span = end - start;
            var starsPerSecond = starCount / span.TotalSeconds;

            var output = new StringBuilder();

            output.AppendLine(FitStarTestResult.Header);

            foreach (var result in results)
            {
                output.AppendLine(result.ToString());
            }

            File.WriteAllText("D:\\p\\fit-tests.csv", output.ToString());
        }

        [TestMethod]
        public void GenerateFakeStarImages()
        {
            var minWidth = 1.0;
            var maxWidth = 2.0;
                        
            for (var i = 0; i < 10; i++)
            {
                var simulator = new PhotoSimulator(_rand);

                simulator.AddStars(100, 500, minWidth, maxWidth, _rand.Next(1, 5));
                simulator.AddStars(500, 8000, minWidth, maxWidth, _rand.Next(1, 5));
                simulator.AddStars(8000, 60000, minWidth, maxWidth, _rand.Next(1, 5));
                simulator.AddStars(60000, 150000, minWidth, maxWidth, 3);
                
                CheckStars(simulator.Image, simulator.Photo.Stars);

                simulator.SaveAs16BitBitmap($"stars-{i}-1.png");

                simulator.AddHotPixels(_rand.Next(1, 10));
                simulator.SaveAs16BitBitmap($"stars-{i}-2.png");

                simulator.AddShotNoise();
                simulator.SaveAs16BitBitmap($"stars-{i}-3.png");

                simulator.AddReadNoise(_biasPath);
                simulator.SaveAs16BitBitmap($"stars-{i}-4.png");

                simulator.AddPRNUNoise();
                simulator.SaveAs16BitBitmap($"stars-{i}-5.png");

                var background = Photometry.FindSkyBackgroundIntensity(simulator.ImageArray);
                Photometry.Subtract(simulator.Image, (ushort)background);
                simulator.SaveAs16BitBitmap($"stars-{i}-6.png");

                File.WriteAllText($"stars-{i}.json", JsonConvert.SerializeObject(simulator.Photo));
            }
        }
        
        void CheckStars(Mat image, List<StarInfo> stars)
        {
            foreach (var star in stars)
            {
                CheckStar(image, star);
            }
        }

        void CheckStar(Mat image, StarInfo star)
        {
            var px = star.X;
            var py = star.Y;
            var distanceSquared = (px - star.X) * (px - star.X) + (py - star.Y) * (py - star.Y);
            var amplitude = Photometry.GaussianAmplitudeFromPSF(distanceSquared, star.Peak, star.Width);
            var pixel = (amplitude > ushort.MaxValue) ? ushort.MaxValue : (ushort)amplitude;
            
            if (Math.Abs(pixel - Math.Floor(star.Peak)) > 1 && star.Peak < ushort.MaxValue)
            {
                Assert.Fail("Peak star value from gaussian did not match predicted value");
            }
            
            var pixelValue = image.At<ushort>(star.Y, star.X);
            var peakOk = pixel == pixelValue;

            if (!peakOk)
            {
                Assert.Fail("Peak star value in image did not match predicted value");
            }
        }        
    }
}
