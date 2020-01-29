Now I would like to add Dependency Property, so my zoom will be triggered "OnChange", so I will be able to set up it from outside in simple form

Here is a little bit "magic" with DependencyProperty:

````csharp
public class uImageControl : Control
    {
        //Declaration:
        public static readonly DependencyProperty MagnificationProperty;

		public double Magnification
		{
			get { return (double)GetValue(MagnificationProperty); }
			set { SetValue(MagnificationProperty, value); }
		}

        static uImageControl()
        {
            //Registration:
            MagnificationProperty = DependencyProperty.Register("Magnification", typeof(double), typeof(uImageControl), 
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnMagnificationChanged));
        //...
        }
		
        private void OnDisplayControlMouseWheel(object sender, MouseWheelEventArgs e)
        {    
            double zoom_delta = e.Delta > 0 ? .1 : -.1;            
            //Change:
            Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
        //...
        }    

        private static void OnMagnificationChanged(DependencyObject image, DependencyPropertyChangedEventArgs magnification)
		{
            //Called OnChange
			ScaleTransform obj = (ScaleTransform)image.part_µImage.LayoutTransform;
  			obj.ScaleX = obj.ScaleY = magnification;
  		}

    }

````
refer to
https://docs.microsoft.com/en-us/dotnet/api/system.windows.dependencyproperty.register?view=netcore-3.1
https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/how-to-implement-a-dependency-property
https://www.codeproject.com/Articles/42203/How-to-Implement-a-DependencyProperty

and another important point - change access scope for parts from public static to private:

insted of
````csharp
public static Image part_µimage;
public static Grid part_µMouseHandler;
````

should be 
````csharp
private Image part_µImage;
private Grid part_µMouseHandler;
````
