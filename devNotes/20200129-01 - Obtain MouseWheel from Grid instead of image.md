Now th problem is that mosuse scroll working only when mouse over the image and not over the control, which is not good for zoome out images.
I will placew Grid over the image and will get MosueScroll event from Grid instead of the image

````xml
<ControlTemplate TargetType="{x:Type local:uImageControl}">
    <Grid>
        <Border Background="{TemplateBinding Background}"
                BorderBrush="{TemplateBinding BorderBrush}"
                BorderThickness="{TemplateBinding BorderThickness}">
        </Border>
        <Image Name="PART_µImage" Height="512" Width="512" Source="Zippo.jpg" />
        <Grid Name="PART_µMouseHandler" Background="#00FFFFFF"/>
    </Grid>
</ControlTemplate>
````
Hint: 
Indent one space Extension
https://marketplace.visualstudio.com/items?itemName=usernamehw.indent-one-space
Shift block right with Space or to the left with Shift+Space

One more hint:
How to access Control Template parts from Code Behind
https://www.codeproject.com/Articles/179105/How-to-access-Control-Template-parts-from-Code-Beh