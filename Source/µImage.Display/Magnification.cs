using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace µ.Display
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

		public void PerformZoomToFit()
		{
			if (part_µImage != null && part_µImage.ActualWidth != 0.0 && part_µImage.ActualHeight != 0.0 && 
            part_µScrollViewer != null && part_µScrollViewer.ViewportHeight != 0.0 && part_µScrollViewer.ViewportWidth != 0.0)
			{
				double zoom = ComputeZoomToFitRatio();
				Magnification = (zoom > 0.0)?zoom:1.0;
			}
		}

		private void PerformZoomToFit(int imgWidth, int imgHeight)
		{
			if (part_µScrollViewer == null || part_µScrollViewer.ViewportWidth == 0.0 || part_µScrollViewer.ViewportHeight == 0.0 || 
                 imgWidth == 0 || imgHeight == 0) return;

			double ratioWidth = part_µScrollViewer.ViewportWidth / (double)imgWidth;
			double ratioHeight = part_µScrollViewer.ViewportHeight / (double)imgHeight;
			double zoom = Math.Min(ratioWidth, ratioHeight);
			Magnification = zoom;			
		}

		private double ComputeZoomToFitRatio()
		{
			if (part_µScrollViewer == null || part_µScrollViewer.ViewportWidth == 0.0 || part_µScrollViewer.ViewportHeight == 0.0 || 
                part_µImage == null || part_µImage.ActualWidth == 0.0 || part_µImage.ActualHeight == 0.0){
				return 1.0;
			}
			double RatioWidth = part_µScrollViewer.ViewportWidth / part_µImage.ActualWidth;
			double RatioHeight = part_µScrollViewer.ViewportHeight / part_µImage.ActualHeight;
			return Math.Min(RatioWidth, Height);
		}
    }
}
