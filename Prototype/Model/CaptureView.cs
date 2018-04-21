using OpenCvSharp;

namespace Prototype.Model
{
    public class CaptureView
    {
        public SubFrame Frame { get; set; }
        public int ChipX { get; set; }
        public int ChipY { get; set; }
        public int BinnedX { get; set; }
        public int BinnedY { get; set; }
        public int XSize { get; set; }
        public int YSize { get; set; }
        public Mat Image { get; set; }
        public ushort[] ImageArray { get; set; }
        
        public Point2i GetVisiblePositionFromChipPosition(double x, double y)
        {
            if(Frame != null)
            {
                x -= Frame.ChipX1;
                y -= Frame.ChipY1;
            }
            
            var xRatio = ChipX / (double)BinnedX;
            var yRatio = ChipY / (double)BinnedY;

            return new Point2i((int)(x * xRatio), (int)(y * yRatio));
        }

        public Point2i GetChipPositionFromVisiblePosition(double x, double y)
        {
            if (Frame != null)
            {
                x += Frame.ChipX1;
                y += Frame.ChipY1;
            }

            var xRatio = BinnedX / (double)ChipX;
            var yRatio = BinnedY / (double)ChipY;

            return new Point2i((int)(x * xRatio), (int)(y * yRatio));
        }

        public int GetWidth()
        {
            if(Frame != null)
            {
                return Frame.ChipX2 - Frame.ChipX1;
            }

            return BinnedX;
        }

        public int GetHeight()
        {
            if (Frame != null)
            {
                return Frame.ChipY2 - Frame.ChipY1;
            }

            return BinnedY;
        }
    }
}
