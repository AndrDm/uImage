Now I will zoom image around mouse's position, otherwise it scrolled to 0,0, which is pretty uncomfortable

The same logic as with magnification:

````csharp
public static readonly DependencyProperty MousePositionProperty;

        static uImageControl()
        {
            MousePositionProperty = DependencyProperty.Register("MousePosition", typeof(Point), typeof(uImageControl), 
                new PropertyMetadata(new Point(0.0, 0.0)));
        }

        private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
        {    
            SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

            double zoom_delta = e.Delta > 0 ? .1 : -.1;            
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
            CenterViewerAroundMouse(MousePosition);
    
        }

        private void CenterViewerAroundMouse(Point center)
		{
			if (part_µScrollViewer != null){
				part_µScrollViewer.ScrollToHorizontalOffset(center.X * Magnification - Mouse.GetPosition(part_µScrollViewer).X);
				part_µScrollViewer.ScrollToVerticalOffset(center.Y * Magnification - Mouse.GetPosition(part_µScrollViewer).Y);
			}
		}
````

and it works
