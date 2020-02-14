using System.Windows;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using µ.Vision;
using µ.Display;
using static µ.Vision.µImage;
using LiveCharts;
using LiveCharts.Wpf;


namespace µ.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static µImage µsrc, µdst;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            µsrc = new µImage();
            µdst = new µImage();
            µReadFile(µsrc, "Zippo.jpg");
            µCopy(µsrc, µdst);
            MyµImage.DisplayImage(µsrc);
            DisplayHistogram(µsrc);
        }
	
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            Open_Button_Click(sender, e);
        }
        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.tif)|*.png;*.jpg;*.tif|All files (*.*)|*.*";
			if(openFileDialog.ShowDialog() == true){
                µReadFile(µsrc, openFileDialog.FileName);
                µCopy(µsrc, µdst);
                if (µdst.imageType == ImageType.U8) SliderMin.Maximum = SliderMax.Maximum = SliderMax.Value = 255;
                if (µdst.imageType == ImageType.U16){
                    SliderMin.Minimum = SliderMax.Minimum = SliderMin.Value = µGetMin(µdst);                
                    SliderMin.Maximum = SliderMax.Maximum = SliderMax.Value = µGetMax(µdst);
                }                
                MyµImage.SetDisplayMappingData((int)µGetMin(µdst), (int)µGetMax(µdst), µ.Display.DisplayMappingOption.Default);
                MyµImage.DisplayImage(µdst); 
                MyµImage.PerformZoomToFit();
                DisplayHistogram(µdst);            }
        }
        private void Original_Button_Click(object sender, RoutedEventArgs e)
        {
            µCopy(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
            DisplayHistogram(µdst);        }

        private void ZoomFit_Button_Click(object sender, RoutedEventArgs e)
        {
            MyµImage.PerformZoomToFit();
        }

        private void Zoom100_Button_Click(object sender, RoutedEventArgs e)
        {
            MyµImage.Magnification = 1.0;
        }
        private void Median_Button_Click(object sender, RoutedEventArgs e)
        {
            Median_Demo(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
            DisplayHistogram(µdst);
        }

        private void Dilate_Button_Click(object sender, RoutedEventArgs e)
        {
            Dilate_Demo(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
            DisplayHistogram(µdst);
        }

        private void Threshold_Button_Click(object sender, RoutedEventArgs e)
        {
            Threshold_Demo(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
            DisplayHistogram(µdst);
        }

        private async void Sliders_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (null == µdst) return;
            double min = SliderMin.Value;
            double max = SliderMax.Value;
            await Task.Run(() => ApplyMinMax (min, max));
        }

        internal void ApplyMinMax(double min, double max)
        {
            MyµImage.SetDisplayMappingData(min, max, µ.Display.DisplayMappingOption.GivenRange); 
            MyµImage.Dispatcher.Invoke( () => MyµImage.DisplayImage(µdst) );    
            System.Threading.Thread.Sleep(1);
        }
        
        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void PaletteComboBox_Selected(object sender, RoutedEventArgs e)
        {
            if (null == µdst) return; //not loaded yet
            switch (PalettesList.SelectedIndex){ 
                case 0: MyµImage.DisplayImage(µdst, PaletteType.Gray); break;
                case 1: MyµImage.DisplayImage(µdst, PaletteType.Binary); break;
                case 2: MyµImage.DisplayImage(µdst, PaletteType.Gradient); break;
                case 3: MyµImage.DisplayImage(µdst, PaletteType.Rainbow); break;
                case 4: MyµImage.DisplayImage(µdst, PaletteType.Temperature); break;
            }
        }

        public SeriesCollection SeriesCollection { get; set; }
        private void DisplayHistogram(µImage image)
        {
            if(null == image) return;

            double[] hist = new double[256]; //Currently 256 bins only ToDo: make flexible

            µHistogram(image, hist);
        
            DataContext = null;
            SeriesCollection = new SeriesCollection{
                new ColumnSeries{
                    Title = "Histogram",
                    Values = new ChartValues<double>(hist),
					MaxColumnWidth = double.PositiveInfinity,
					ColumnPadding = 0
                }
            };
            DataContext = this;
        }

        private void Pan_Button_Click(object sender, RoutedEventArgs e)
        {
            MyµImage.SelectedTool = Tool.Pan;
        }
        private void Line_Button_Click(object sender, RoutedEventArgs e)
        {
            MyµImage.SelectedTool = Tool.ROILine;
        }
    }
}
