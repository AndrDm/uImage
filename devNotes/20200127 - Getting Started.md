## Create new wpf project:

````console
>dotnet new wpf
````

the same with specific name

````console
>dotnet new wpf --name µImage.Viewer
````

The following files will be created

### App.xaml:
````xml
<Application x:Class="µImage.Viewer.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:µImage.Viewer"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
         
    </Application.Resources>
</Application>
````
### App.xaml.cs:
````csharp
using System.Windows;

namespace µImage.Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }
}
````
### AssemblyInfo.cs:
````csharp
using System.Windows;

[assembly:ThemeInfo(ResourceDictionaryLocation.None, 
    ResourceDictionaryLocation.SourceAssembly
)]
/*
Location.None where theme specific resource dictionaries are located (used if a resource is not found in the page, or application resource dictionaries) 

Location.SourceAssembly where the generic resource dictionary is located (used if a resource is not found in the page, app, or any theme specific resource dictionaries)
*/
````
### MainWindow.xaml
````xml
<Window x:Class="µImage.Viewer.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:µImage.Viewer"
        mc:Ignorable="d"
        Title="µImage.Viewer" Height="600" Width="800">
    <Grid>

    </Grid>
</Window>
````
### MainWindow.xaml.cs
````csharp
using System.Windows;

namespace µImage.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
````
### and finally µImage.Viewer.csproj:
````xml
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

</Project>
````
## Build project:
````console
>dotnet build
````
## Run project
````console
>dotnet run
````
### Run specific project
````
>dotnet run --project µImage.Viewer\µImage.Viewer.csproj
````
[dotnet run](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-run?tabs=netcore30)

first result:

![first result](screenshots/2020-01-27%2023.38.29%20-%20%C2%B5Image.Viewer.png?raw=true)

