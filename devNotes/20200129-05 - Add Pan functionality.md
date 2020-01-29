Panning image with Mouse is quite simple - just need to handle Mouse Down/Move/Up events:

````csharp

        private Point _previousPanPoint = new Point(0.0, 0.0);
        private bool _mouseDown = false;

            part_µMouseHandler.MouseLeftButtonDown += OnµImageControlMouseLeftButtonDown;
            part_µMouseHandler.MouseMove += OnµImageControlMouseMove;
            part_µMouseHandler.MouseLeftButtonUp += OnµImageControlMouseLeftButtonUp;
````

and according handlers:

````csharp
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
        }

        private void OnµImageControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            part_µMouseHandler.ReleaseMouseCapture();
            _mouseDown = false;
        }
````

Some useful articles:
https://stackoverflow.com/questions/741956/pan-zoom-image
https://www.codeproject.com/Articles/85603/A-WPF-custom-control-for-zooming-and-panning
https://www.codeproject.com/Articles/1119476/An-Enhanced-WPF-Custom-Control-for-Zooming-and-Pan

