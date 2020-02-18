# µImage
Learn C#/WPF by building micro Image Viewer from scratch.

Just my "learning by doing" pet project, which could be useful for someone else.

## Getting started:
- Download and install latest .NET Core SDK (https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.101-windows-x64-installer)
- Obtain latest code snapshot (https://github.com/AndrDm/uImage/archive/master.zip)
- Build project 
````console
>dotnet build µImage.sln
````
- or run it 
````console
>dotnet run µImage.Viewer.csproj
````
v.1.10.0.14 (18-FEB-2020)
Line Profile and new Palettes (Inverted gray and Gammas) added.

v.1.9.0.13 (14-FEB-2020)
Rubber Line ROI added, tools switch between Pan and Line

v.1.8.0.12 (09-FEB-2020)
Histogram added (with LiveCharts.Wpf). Currently 256 bins only.

v.1.7.0.11 (07-FEB-2020)
Mapping 16 to 8 bit added + Color Palettes 

v.1.6.0.10 (05-FEB-2020)
Pixel Info, Zoom to Fit, Open File (8 and 16 bit grays)
  
v.1.5.0.9 (29-JAN-2020)
Pan functionality was added to the Display Control

v.1.4.0.8 (29-JAN-2020)
Zoom centered around mouse pointer

v.1.3.0.7 (29-JAN-2020)
Magnification handled as DependencyProperty

v.1.3.0.6 (29-JAN-2020)
Scroll Bars added, Scroll Event received from Grid instead of image

v.1.2.0.5 (28-JAN-2020)
Image and Zoom moved to "µImage.Display" control

v.1.2.0.4 (28-JAN-2020)
Custom User Control "µImage.Display" added

v.1.2.0.3 (28-JAN-2020)
Zoom functionality added

v.1.1.0.2 (28-JAN-2020)
Demo Image added to XAML

v.1.0.0.1 (27-JAN-2020)
>dotnet new wpf --name µImage.Viewer
