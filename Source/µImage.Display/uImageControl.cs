using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace µImage.Display
{
   
    [TemplatePart(Name = "PART_µImage", Type = typeof(Image))]
    [TemplatePart(Name = "PART_µMouseHandler", Type = typeof(Grid))]
    public class uImageControl : Control
    {
        private Image part_µImage;
        private Grid part_µMouseHandler;
        public static readonly DependencyProperty MagnificationProperty;

		public double Magnification
		{
			get { return (double)GetValue(MagnificationProperty); }
			set { SetValue(MagnificationProperty, value); }
		}

        static uImageControl()
        {
            MagnificationProperty = DependencyProperty.Register("Magnification", typeof(double), typeof(uImageControl), 
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnMagnificationChanged));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(uImageControl), new FrameworkPropertyMetadata(typeof(uImageControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            part_µImage = GetTemplateChild("PART_µImage") as Image;
            if (null == part_µImage) throw new NullReferenceException("Template Part µImage is not available");
            part_µMouseHandler = GetTemplateChild("PART_µMouseHandler") as Grid;
            if (null == part_µMouseHandler) throw new NullReferenceException("Template Part µMouseHandler is not available");

            part_µImage.LayoutTransform = new ScaleTransform();
            part_µMouseHandler.MouseWheel += OnDisplayControlMouseWheel;

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
			}
		}


        private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
        {    
            //ScaleTransform obj = (ScaleTransform)part_µImage.LayoutTransform;
    
            double zoom_delta = e.Delta > 0 ? .1 : -.1;            
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
            
            //obj.ScaleY = obj.ScaleX = (obj.ScaleX += zoom).LimitToRange(.1, 10);
            //BitmapScalingMode mode = obj.ScaleX > 5 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;
            //RenderOptions.SetBitmapScalingMode(part_µImage, mode);
    
            e.Handled = true;
        }    
    }
    
    public static class InputExtensions
    {
        public static double LimitToRange(this double value, double inclusiveMinimum, double inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }
    }
}
