From common architecture point of view we will need reusable control, which should include scroll bars, some tools, etc.

So, I will create new custom control called "µImage.Display":
````console
µImage\Source\µImage.Display>dotnet new wpfcustomcontrollib
````
Technically its also possible to create in in Visual Studio, but I would like to use CLI - just for exersices.

many users frustrated - what's the Difference Between a Custom Control and User Control?

Two explanations below:

https://www.codeproject.com/Articles/179442/So-Whats-the-Difference-Between-a-Custom-Control-a

https://stackoverflow.com/questions/11247708/custom-vs-user-control

Now I have two projects, therefore I'll create new solution
````console
µImage\Source>dotnet new sln --name µImage
````

And add both projects to the solution
````console
µImage\Source>dotnet sln µImage.sln add µImage.Viewer/µImage.Viewer.csproj
µImage\Source>dotnet sln µImage.sln add µImage.Display/µImage.Display.csproj
````

Don't forget to add custom path to the output - both project will be builded to the same directory:
````xml
  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <OutputPath>..\..\Build\Release\</OutputPath>
  </PropertyGroup>
  
  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <OutputPath>..\..\Build\Debug\</OutputPath>
  </PropertyGroup>
````
And run the build

````console
µImage\Source>dotnet new
`````
Output:

````console
Microsoft (R)-Build-Engine, Version 16.4.0+e901037fe für .NET Core
Copyright (C) Microsoft Corporation. Alle Rechte vorbehalten.

  Wiederherstellung in "40,32 ms" für "µImage\Source\µImage.Viewer\µImage.Viewer.csproj" abgeschlossen.
  Wiederherstellung in "40,32 ms" für "µImage\Source\µImage.Display\µImage.Display.csproj" abgeschlossen.
  µImage.Display -> µImage\Build\Debug\netcoreapp3.1\µImage.Display.dll
  µImage.Viewer  -> µImage\Build\Debug\netcoreapp3.1\µImage.Viewer.dll

Der Buildvorgang wurde erfolgreich ausgef�hrt.
    0 Warnung(en)
    0 Fehler

Verstrichene Zeit 00:00:03.61
````

refer to
https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-sln

Back to the Custom Control

The following two files was created:

### "CustomControl1.cs"
````csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace µImage.Display
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:µImage.Display"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:µImage.Display;assembly=µImage.Display"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class CustomControl1 : Control
    {
        static CustomControl1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomControl1), new FrameworkPropertyMetadata(typeof(CustomControl1)));
        }
    }
}
````
and another one
### "Generic.xaml" in \Theme subfolder:
````xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="clr-namespace:µImage.Display">
    <Style TargetType="{x:Type local:CustomControl1}">
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:CustomControl1}">
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}">
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
`````
