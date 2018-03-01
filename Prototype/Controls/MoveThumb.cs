using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Prototype.Controls
{
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(OnDrag);
        }

        T FindChild<T>(Visual parent) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T)
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

            double left = Canvas.GetLeft(item);
            double top = Canvas.GetTop(item);

            var newLeft = left + e.HorizontalChange;
            var newTop = top + e.VerticalChange;

            if (newLeft < 0) newLeft = 0;
            if (newTop < 0) newTop = 0;

            var maxLeft = canvas.Width - item.Width;
            var maxTop = canvas.Height - item.Height;

            if (newLeft > maxLeft) newLeft = maxLeft;
            if (newTop > maxTop) newTop = maxTop;

            Canvas.SetLeft(item, newLeft);
            Canvas.SetTop(item, newTop);
        }
    }
}
