# µImage
Learn C#/WPF by building micro Image Viewer from scratch.

Just my "learning by doing" pet project, which could be useful for someone else.

## Getting started:
- Download and install latest .NET 6.0.7 SDK (v6.0.302 released 12-JUL-2022):
  https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.302-windows-x64-installer
- Obtain latest code snapshot (https://github.com/AndrDm/uImage/archive/master.zip)
- Build project 
````console
>dotnet build µImage.sln
````
- or go to \Source\µImage.Viewer folder and run it 
````console
>dotnet run µImage.Viewer.csproj
````
### Project iterations:
v.1.17 (08-AUG-2022)
Targeted for .NET 6.0

v.1.16.0.21 (28-FEB-2020)
ROI's thickness and sizes of the anchors scaled with magnification factor

v.1.15.0.20 (26-FEB-2020)
Support for DICONDE File Format was added, bug fix in MinMax function.

v.1.14.1.19 (25-FEB-2020)
Zoom to fit bug - fixed (caused by added canvas for annotation).

v.1.14.0.18 (24-FEB-2020)
ROI's Anchors was added, Display is resizible now.

v.1.13.0.17 (23-FEB-2020)
Ability to move Line, Rect and Oval ROIs after creation was added.

v.1.12.0.16 (21-FEB-2020)
Oval (Ellipse) ROI type was added.

v.1.11.0.15 (19-FEB-2020)
Rectangular ROI added to display and Window function added to viewer.

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
