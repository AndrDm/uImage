Now I need scrollbars.
I will place Scroll elenmenth to the themes.xaml

Hints
Optimizing performance: Controls
https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/optimizing-performance-controls?redirectedfrom=MSDN

https://www.codeproject.com/Questions/138653/Scrolling-image-with-WPF

so my template looks like this now:
````xml
<ControlTemplate TargetType="{x:Type local:uImageControl}">
    <ScrollViewer Name="PART_µScrollViewer"> 
        <Grid>
            <Border Background="{TemplateBinding Background}"
                    BorderBrush="{TemplateBinding BorderBrush}"
                    BorderThickness="{TemplateBinding BorderThickness}">
            </Border>
            <Image Name="PART_µImage" Height="512" Width="512" Source="Zippo.jpg" />
            <Grid Name="PART_µMouseHandler" Background="#00FFFFFF"/>
        </Grid>
    </ScrollViewer>
</ControlTemplate>
````

Can someone explain to me why the only vertical scroller appeared and not horizontal?!
No problem, will add property:
````xml
<ScrollViewer Name="PART_µScrollViewer"  HorizontalScrollBarVisibility="Visible">
````
Now much better.
