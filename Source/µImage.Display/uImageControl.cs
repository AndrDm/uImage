using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace µImage.Display
{
   
    [TemplatePart(Name = "µImage", Type = typeof(Image))]
    public class uImageControl : Control
    {
        public static Image _µimage;

        static uImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(uImageControl), new FrameworkPropertyMetadata(typeof(uImageControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _µimage = GetTemplateChild("µImage") as Image;

            if (null == _µimage) throw new NullReferenceException("Template Part µImage is not available");
            else{
            _µimage.LayoutTransform = new ScaleTransform();
            _µimage.MouseWheel += OnDisplayControlMouseWheel;
        }
    }

    private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
    {

        ScaleTransform obj = (ScaleTransform)_µimage.LayoutTransform;

        double zoom = e.Delta > 0 ? .1 : -.1;
        obj.ScaleY = obj.ScaleX = (obj.ScaleX += zoom).LimitToRange(.1, 10);

        BitmapScalingMode mode = obj.ScaleX > 5 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;
        RenderOptions.SetBitmapScalingMode(_µimage, mode);

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
