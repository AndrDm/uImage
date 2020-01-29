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
        public static Image part_µimage;
        public static Grid part_µMouseHandler;

        static uImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(uImageControl), new FrameworkPropertyMetadata(typeof(uImageControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            part_µimage = GetTemplateChild("PART_µImage") as Image;
            if (null == part_µimage) throw new NullReferenceException("Template Part µImage is not available");
            part_µMouseHandler = GetTemplateChild("PART_µMouseHandler") as Grid;
            if (null == part_µMouseHandler) throw new NullReferenceException("Template Part µMouseHandler is not available");

            part_µimage.LayoutTransform = new ScaleTransform();
            part_µMouseHandler.MouseWheel += OnDisplayControlMouseWheel;

        }

    private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
    {

        ScaleTransform obj = (ScaleTransform)part_µimage.LayoutTransform;

        double zoom = e.Delta > 0 ? .1 : -.1;
        obj.ScaleY = obj.ScaleX = (obj.ScaleX += zoom).LimitToRange(.1, 10);

        BitmapScalingMode mode = obj.ScaleX > 5 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;
        RenderOptions.SetBitmapScalingMode(part_µimage, mode);

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
