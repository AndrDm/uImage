## Now zoomin with scroll placed to uImageControl.cs:

The image was added here:
````xml
    <Style TargetType="{x:Type local:uImageControl}">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:uImageControl}">
                    <Grid>
                        <Border Background="{TemplateBinding Background}"
                                BorderBrush="{TemplateBinding BorderBrush}"
                                BorderThickness="{TemplateBinding BorderThickness}">
                        </Border>
                        <Image Name="µImage" Height="512" Width="512" Source="Zippo.jpg" />
                    </Grid>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

````

and mouse handling here:

````csharp
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
````

This control was added then to µImage.Viewer (MainWindows.xaml):
````xml
        Title="µImage.Viewer" Height="600" Width="800">
    <Grid>
        <Display:uImageControl HorizontalAlignment="Center" VerticalAlignment="Center"/>
    </Grid>
````

and according reference was added to µImage.Viewer.csproj:

````xml
  <ItemGroup>
    <ProjectReference Include="..\µImage.Display\µImage.Display.csproj" />
  </ItemGroup>
````

Very nice thing is that I can't use "µ" symbol here:
        <Display:uImageControl 
Was possible everywhere except this place. I was too optimistic about Unicode. Still not everywhere.
So, replaced to "u" for this moment.