﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Tests
{
    public class PhotoSimulator
    {
        Random _rand;

        public readonly FakePhoto Photo = new FakePhoto();
        public readonly ushort[] ImageArray;
        public readonly Mat Image;

        public PhotoSimulator(Random random)
        {
            _rand = random;

            ImageArray = new ushort[800 * 600];

            Image = new Mat(600, 800, MatType.CV_16UC1, ImageArray);

            Image.SetTo(new Scalar(0));
        }
        
        public void AddPRNUNoise()
        {
            var precision = 10000;
            
            for (var y = 0; y < 600; y++)
            {
                for (var x = 0; x < 800; x++)
                {
                    var value = Image.At<ushort>(y, x);

                    if (value > 0)
                    {
                        var performance = 1.0 + ((_rand.Next(0, precision) / ((double)precision)) - 0.5) / 100;
                        var noised = value * performance;

                        if (noised > ushort.MaxValue)
                        {
                            noised = ushort.MaxValue;
                        }
                        else if (noised < 0)
                        {
                            noised = 0;
                        }

                        Image.Set(y, x, (ushort)noised);
                    }
                }
            }
        }

        public void AddStars(double minPeak, double maxPeak, double minWidth, double maxWidth, int starCount)
        {
            var drawThreshold = 5.0; // stop drawing the star when its this dim
            var stars = new List<StarInfo>();

            for (var s = 0; s < starCount; s++)
            {
                var star = new StarInfo();

                star.Peak = _rand.Next((int)(minPeak * 1000), (int)(maxPeak * 1000)) / 1000.0;
                star.Width = _rand.Next((int)(minWidth * 1000), (int)(maxWidth * 1000)) / 1000.0;

                var distance = 0;
                var value = 0.0;

                do
                {
                    value = Photometry.GaussianAmplitudeFromPSF(distance * distance, star.Peak, star.Width);
                    distance++;
                }
                while (value > drawThreshold);

                star.PixelWidth = distance;

                var isOk = false;

                while (!isOk)
                {
                    star.X = _rand.Next(0, Image.Width - 1);
                    star.Y = _rand.Next(0, Image.Height - 1);

                    isOk = true;

                    foreach (var existingStar in Photo.Stars)
                    {
                        var existingWidthSqr = (existingStar.PixelWidth * existingStar.PixelWidth);
                        var starWidthSqr = (star.PixelWidth * star.PixelWidth);
                        var xDistanceSqr = (existingStar.X - star.X) * (existingStar.X - star.X);
                        var yDistanceSqr = (existingStar.Y - star.Y) * (existingStar.Y - star.Y);

                        if (xDistanceSqr + yDistanceSqr < existingWidthSqr + starWidthSqr)
                        {
                            isOk = false;
                        }
                    }
                }

                AddStar(star);

                stars.Add(star);
            }

            Photo.Stars.AddRange(stars);
        }

        public void AddStar(StarInfo star)
        {
            var distance = star.PixelWidth;
            var value = star.Peak;

            var x1 = star.X - distance;
            var y1 = star.Y - distance;
            var x2 = star.X + distance;
            var y2 = star.Y + distance;

            if (x1 < 0) x1 = 0;
            if (x2 >= Image.Width) x2 = Image.Width - 1;
            if (y1 < 0) y1 = 0;
            if (y2 >= Image.Height) y2 = Image.Height - 1;

            for (var py = y1; py < y2; py++)
            {
                for (var px = x1; px < x2; px++)
                {
                    var distanceSquared = (px - star.X) * (px - star.X) + (py - star.Y) * (py - star.Y);
                    var amplitude = Photometry.GaussianAmplitudeFromPSF(distanceSquared, star.Peak, star.Width);
                    var pixel = (amplitude > ushort.MaxValue) ? ushort.MaxValue : (ushort)amplitude;
                    Image.Set(py, px, pixel);
                }
            }
        }

        public void AddHotPixels(int count)
        {
            var result = new List<BadPixel>();

            for (var i = 0; i < count; i++)
            {
                var x = _rand.Next(0, Image.Width - 1);
                var y = _rand.Next(0, Image.Height - 1);

                var pixel = new BadPixel { Hot = true, X = x, Y = y };

                result.Add(pixel);

                Image.Set(pixel.Y, pixel.X, ushort.MaxValue);
            }

            Photo.BadPixels.AddRange(result);
        }

        public Bitmap To16BitBitmap()
        {
            var result = new Bitmap(Image.Width, Image.Height, sizeof(ushort) * Image.Width, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale, Image.Data);

            return result;
        }
    }
}
