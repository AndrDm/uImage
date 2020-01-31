using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using OpenCvSharp;

namespace µImage.Display
{
   
    [TemplatePart(Name = "PART_µImage", Type = typeof(Image))]
    [TemplatePart(Name = "PART_µMouseHandler", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_µScrollViewer", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_µZoom", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_µInfo", Type = typeof(TextBlock))]

    
    public partial class uImageControl : Control
    {
        private Image part_µImage;
        private Grid part_µMouseHandler;
        private ScrollViewer part_µScrollViewer;
        private TextBox part_µZoom;
        private TextBox part_µInfo;


        private Point _previousPanPoint = new Point(0.0, 0.0);
        private bool _mouseDown = false;

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

        public void ApplyBitmap(BitmapImage bitmap)
        {
            part_µImage.Source = bitmap;
        }

        public void ApplyWriteableBitmap(WriteableBitmap bitmap)
        {
            part_µImage.Source = bitmap;
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
            part_µZoom = GetTemplateChild("PART_µZoom") as TextBox;
            if (null == part_µZoom) throw new NullReferenceException("Template Part µZoom is not available");
            part_µInfo = GetTemplateChild("PART_µInfo") as TextBox;
            if (null == part_µInfo) throw new NullReferenceException("Template Part µInfo is not available");


            part_µImage.LayoutTransform = new ScaleTransform();
            
            part_µMouseHandler.MouseWheel += OnµImageControlMouseWheel;
            part_µMouseHandler.MouseLeftButtonDown += OnµImageControlMouseLeftButtonDown;
            part_µMouseHandler.MouseMove += OnµImageControlMouseMove;
            part_µMouseHandler.MouseLeftButtonUp += OnµImageControlMouseLeftButtonUp;
        }

        private void OnµImageControlMouseWheel(object sender, MouseWheelEventArgs e)
        {    
            SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

            double zoom_delta = e.Delta > 0 ? .1 : -.1;            
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
            CenterViewerAroundMouse(MousePosition);
            ShowMousePosition();
            e.Handled = true;
        }    

        private void OnµImageControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _previousPanPoint = Mouse.GetPosition(part_µScrollViewer);
            part_µMouseHandler.CaptureMouse();
            _mouseDown = true;
        }

        private void OnµImageControlMouseMove(object sender, MouseEventArgs e)
        {
            SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

            if (_mouseDown){
                Point previousPanPoint = _previousPanPoint;
                Point position = Mouse.GetPosition(part_µScrollViewer);
                double x_diff = position.X - previousPanPoint.X;
                double y_diff = position.Y - previousPanPoint.Y;
                part_µScrollViewer.ScrollToHorizontalOffset(part_µScrollViewer.HorizontalOffset - x_diff);
                part_µScrollViewer.ScrollToVerticalOffset(part_µScrollViewer.VerticalOffset - y_diff);
                _previousPanPoint = position;
            }
            ShowMousePosition();            
        }

        private void OnµImageControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            part_µMouseHandler.ReleaseMouseCapture();
            _mouseDown = false;
        }

        private void ShowMousePosition()
        {
            Point pos = Mouse.GetPosition(part_µImage);
            if ((pos.X>=0) && (pos.X < part_µImage.Width) &&(pos.Y>=0) && (pos.Y < part_µImage.Height)){
                part_µInfo.Text = $"x = {Mouse.GetPosition(part_µImage).X:N0}; y = {Mouse.GetPosition(part_µImage).Y:N0}";
            }
        }
    }
}
