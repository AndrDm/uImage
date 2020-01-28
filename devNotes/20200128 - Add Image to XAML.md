The output moved to the \Build subdirectory

In the µImage.Viewer.csproj:
````xml
  <PropertyGroup Condition="'$(Configuration)'=='Release'">
    <OutputPath>..\..\Build\Release\</OutputPath>
  </PropertyGroup>
  
  <PropertyGroup Condition="'$(Configuration)'=='Debug'">
    <OutputPath>..\..\Build\Debug\</OutputPath>
  </PropertyGroup>
  ````

By the way - Night Owl theme is not so bad for night coding:
https://github.com/sdras/night-owl-vscode-theme

Now we will add image to the XAML file

````xml
    <Grid>
        <Image Height="512" Width="512" Source="Zippo.jpg" />
    </Grid>
````

Important point - add according Resource to the project file:

````xml
  <ItemGroup>
    <Resource Include="..\..\Images\Zippo.jpg" />
  </ItemGroup>
````

now
````console
>dotnet run
````

