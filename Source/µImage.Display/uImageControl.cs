using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace µImage.Display
{
   
    [TemplatePart(Name = "PART_µImage", Type = typeof(Image))]
    [TemplatePart(Name = "PART_µMouseHandler", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_µScrollViewer", Type = typeof(Grid))]
    
    public partial class uImageControl : Control
    {
        private Image part_µImage;
        private Grid part_µMouseHandler;
        private ScrollViewer part_µScrollViewer;
        public static readonly DependencyProperty MagnificationProperty;
        public static readonly DependencyProperty MousePositionProperty;

        static uImageControl()
        {
            MagnificationProperty = DependencyProperty.Register("Magnification", typeof(double), typeof(uImageControl), 
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnMagnificationChanged));
            MousePositionProperty = DependencyProperty.Register("MousePosition", typeof(Point), typeof(uImageControl), 
                new PropertyMetadata(new Point(0.0, 0.0)));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(uImageControl), new FrameworkPropertyMetadata(typeof(uImageControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            part_µImage = GetTemplateChild("PART_µImage") as Image;
            if (null == part_µImage) throw new NullReferenceException("Template Part µImage is not available");
            part_µMouseHandler = GetTemplateChild("PART_µMouseHandler") as Grid;
            if (null == part_µMouseHandler) throw new NullReferenceException("Template Part µMouseHandler is not available");
            part_µScrollViewer = GetTemplateChild("PART_µScrollViewer") as ScrollViewer;
            if (null == part_µScrollViewer) throw new NullReferenceException("Template Part µScrollViewer is not available");

            part_µImage.LayoutTransform = new ScaleTransform();
            part_µMouseHandler.MouseWheel += OnDisplayControlMouseWheel;
        }

        private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
        {    
            SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

            double zoom_delta = e.Delta > 0 ? .1 : -.1;            
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
            CenterViewerAroundMouse(MousePosition);
    
            e.Handled = true;
        }    
    }
}
