using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.Model
{
    public class StarView
    {
        public int SelectedStarVisibleX { get; set; }
        public int SelectedStarVisibleY { get; set; }
        public int SelectedStarVisibleWidth { get; set; }
        public int SelectedStarVisibleHeight { get; set; }
        public double SelectedStarZoomedX { get; set; }
        public double SelectedStarZoomedY { get; set; }
        public double SelectedStarZoomedWidth { get; set; }
        public double SelectedStarZoomedHeight { get; set; }
        public double SelectedStarCenterX { get; set; }
        public double SelectedStarCenterY { get; set; }
        public double SelectedStarBrightestX { get; set; }
        public double SelectedStarBrightestY { get; set; }
        public int SelectedStarChipX1 { get; set; }
        public int SelectedStarChipY1 { get; set; }
        public int SelectedStarChipX2 { get; set; }
        public int SelectedStarChipY2 { get; set; }
        public double SelectedStarSNR { get; set; }
        public ushort SelectedStarPeak { get; set; }
        public double SelectedStarBackgroundMean { get; set; }
        public double SelectedStarBackgroundSigma { get; set; }
        public int SelectedStarSignalPixels { get; set; }
    }
}
