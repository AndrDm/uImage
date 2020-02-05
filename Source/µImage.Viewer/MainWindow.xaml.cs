using System.Windows;
using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Collections.Generic;
using µ.Vision;
using static µ.Vision.µImage;

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
        private void Init()
        {           
            µsrc = new µImage();
            µdst = new µImage();
            µReadFile(µsrc, "Zippo.jpg");
            µCopy(µsrc, µdst);
            MyµImage.DisplayImage(µsrc);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }
	
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            Open_Button_Click(sender, e);
        }
        private void menuQuit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.tif)|*.png;*.jpg;*.tif|All files (*.*)|*.*";
			if(openFileDialog.ShowDialog() == true){
                µReadFile(µsrc, openFileDialog.FileName);
                µCopy(µsrc, µdst);
                MyµImage.DisplayImage(µdst);            
            };
        }
        private void Original_Button_Click(object sender, RoutedEventArgs e)
        {
            µCopy(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
        }

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
        }

        private void Dilate_Button_Click(object sender, RoutedEventArgs e)
        {
            Dilate_Demo(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
        }

        private void Threshold_Button_Click(object sender, RoutedEventArgs e)
        {
            Threshold_Demo(µsrc, µdst);
            MyµImage.DisplayImage(µdst);
        }        
    }
}
