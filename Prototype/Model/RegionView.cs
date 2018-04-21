using OpenCvSharp;
using System;
using System.ComponentModel;
using System.Windows;

namespace Prototype.Model
{
    public class RegionView : INotifyPropertyChanged
    {
        double zoom;
        bool isSelected;
        bool isCaptureSet;
        bool isZoomSet;
        bool isClipped;

        public int VisibleX { get; set; }
        public int VisibleY { get; set; }
        public int VisibleWidth { get; set; }
        public int VisibleHeight { get; set; }
        public double ZoomedX { get; set; }
        public double ZoomedY { get; set; }
        public double ZoomedWidth { get; set; }
        public double ZoomedHeight { get; set; }
        public int ChipX1 { get; set; }
        public int ChipY1 { get; set; }
        public int ChipX2 { get; set; }
        public int ChipY2 { get; set; }
        public CaptureView Capture { get; set; }
        public CaptureView Parent { get; set; }
        public double CaptureWidth { get; set; }
        public double CaptureHeight { get; set; }

        public Visibility RegionVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;
        public bool IsActive => isSelected && isCaptureSet && isZoomSet && !isClipped;

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetParentCapture(CaptureView parent)
        {
            Parent = parent;
            
            isCaptureSet = true;

            var star1 = Parent.GetVisiblePositionFromChipPosition(ChipX1, ChipY1);
            var star2 = Parent.GetVisiblePositionFromChipPosition(ChipX2, ChipY2);

            VisibleX = star1.X;
            VisibleY = star1.Y;
            VisibleWidth = star2.X - star1.X;
            VisibleHeight = star2.Y - star1.Y;

            var x2 = VisibleX + VisibleWidth;
            var y2 = VisibleY + VisibleHeight;

            var maxX = Parent.GetWidth();
            var maxY = Parent.GetHeight();

            CaptureWidth = maxX;
            CaptureHeight = maxY;
            
            if (VisibleWidth == 0 || VisibleHeight == 0 || VisibleX < 0 || VisibleY < 0 || x2 >= maxX || y2 >= maxY)
            {
                isClipped = true;
            }
            else
            {
                isClipped = false;
            }

            var xSize = VisibleWidth;
            var ySize = VisibleHeight;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegionVisibility)));

            RefreshZoom(zoom);
        }

        public void RefreshZoom(double zoom)
        {
            isZoomSet = true;

            this.zoom = zoom;

            ZoomedX = VisibleX * this.zoom;
            ZoomedY = VisibleY * this.zoom;
            ZoomedWidth = VisibleWidth * this.zoom;
            ZoomedHeight = VisibleHeight * this.zoom;
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedX)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedY)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedWidth)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ZoomedHeight)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegionVisibility)));
        }
        
        public void SetPosition(int chipX1, int chipY1, int chipX2, int chipY2)
        {
            isSelected = true;

            ChipX1 = chipX1;
            ChipY1 = chipY1;
            ChipX2 = chipX2;
            ChipY2 = chipY2;

            if (Parent != null)
            {
                SetParentCapture(Parent);
            }
        }

        public void ClearSelection()
        {
            isSelected = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RegionVisibility)));
        }
    }
}
