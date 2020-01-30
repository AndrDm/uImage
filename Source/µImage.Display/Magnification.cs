using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace µImage.Display
{
    public partial class uImageControl : Control
    {
        public double Magnification
        {
            get { return (double)GetValue(MagnificationProperty); }
            set { SetValue(MagnificationProperty, value); }
        }

        public Point MousePosition
        {
            get{ return (Point)GetValue(MousePositionProperty); }
            set{ SetValue(MousePositionProperty, value); }
        }

        
        private static void OnMagnificationChanged(DependencyObject image, DependencyPropertyChangedEventArgs magnification)
        {
            ApplyMagnification((uImageControl)image, (double)magnification.NewValue);
        }

        private static void ApplyMagnification(uImageControl image, double magnificationValue)
        {
            if (image.part_µImage != null){
                ScaleTransform obj = (ScaleTransform)image.part_µImage.LayoutTransform;
                obj.ScaleX = obj.ScaleY = magnificationValue;
                RenderOptions.SetBitmapScalingMode(image.part_µImage, BitmapScalingMode.HighQuality); //ToDo: depends on zoom or external settings
                image.part_µZoom.Text = $"{magnificationValue*100:N0}%";
            }
        }

        private void CenterViewerAroundMouse(Point center)
        {
            if (part_µScrollViewer != null){
                part_µScrollViewer.ScrollToHorizontalOffset(center.X * Magnification - Mouse.GetPosition(part_µScrollViewer).X);
                part_µScrollViewer.ScrollToVerticalOffset(center.Y * Magnification - Mouse.GetPosition(part_µScrollViewer).Y);
            }
        }
    }
}
