using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;

namespace Prototype.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(OnDrag);
        }

        T FindChild<T>(Visual parent) where T : DependencyObject
        {
            for (var i=0; i<VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if(child is T)
                {
                    return (T)child;
                }
            }

            return default(T);
        }

        T FindAncestor<T>(Visual child) where T : class
        {
            var parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !typeof(T).IsInstanceOfType(parent))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return (parent as T);
        }

        void OnDrag(object sender, DragDeltaEventArgs e)
        {
            var canvas = FindAncestor<Canvas>(this);
            var item = FindChild<ContentControl>(canvas);

            double deltaVertical, deltaHorizontal;
            double width = 0, height = 0;
                
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    deltaVertical = Math.Min(-e.VerticalChange, item.ActualHeight - item.MinHeight);
                    height = item.Height - deltaVertical;
                    break;
                case VerticalAlignment.Top:
                    deltaVertical = Math.Min(e.VerticalChange, item.ActualHeight - item.MinHeight);
                    Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                    height = item.Height - deltaVertical;
                    break;
                default:
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    deltaHorizontal = Math.Min(e.HorizontalChange, item.ActualWidth - item.MinWidth);
                    Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                    width = item.Width - deltaHorizontal;
                    break;
                case HorizontalAlignment.Right:
                    deltaHorizontal = Math.Min(-e.HorizontalChange, item.ActualWidth - item.MinWidth);
                    width = item.Width - deltaHorizontal;
                    break;
                default:
                    break;
            }

            if(width < MinResizeWidth)
            {
                width = MinResizeWidth;
            }

            if(height < MinResizeHeight)
            {
                height = MinResizeHeight;
            }

            double left = Canvas.GetLeft(item);
            double top = Canvas.GetTop(item);
                
            var newLeft = left + width;
            var newTop = top + height;
                
            var maxLeft = canvas.Width;
            var maxTop = canvas.Height;

            if (newLeft > maxLeft) width = maxLeft - left;
            if (newTop > maxTop) height = maxTop - top;

            if(width < MinResizeWidth)
            {
                width = MinResizeWidth;
            }

            if(height < MinResizeHeight)
            {
                height = MinResizeHeight;
            }

            item.Width = width;
            item.Height = height;
            
            e.Handled = true;
        }

        public int MinResizeWidth { get; set; }
        public int MinResizeHeight { get; set; }
    }
}
