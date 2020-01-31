using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System;
using System.Collections.Generic;
//using OpenCvSharp;

namespace µImage.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static WriteableBitmap writeableBitmap;
        static OpenCvSharp.Mat src;
        static OpenCvSharp.Mat dst;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Init()
        {
            var µtype = new OpenCvSharp.MatType(); µtype = OpenCvSharp.MatType.CV_8U;
            var µsize = new OpenCvSharp.Size(); µsize.Width = µsize.Height = 512;
            dst = new OpenCvSharp.Mat(µsize, µtype);
            src = new OpenCvSharp.Mat("Zippo.jpg", OpenCvSharp.ImreadModes.Grayscale);
            OpenCvSharp.Cv2.CopyTo(src, dst);

            writeableBitmap = new WriteableBitmap(
                        dst.Width, dst.Height,
                        96, 96,
                        PixelFormats.Gray8, null);
            ToWriteableBitmap(dst, writeableBitmap);
            MyµImage.ApplyWriteableBitmap(writeableBitmap);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Original_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenCvSharp.Cv2.CopyTo(src, dst);
            ToWriteableBitmap(dst, writeableBitmap);
            MyµImage.ApplyWriteableBitmap(writeableBitmap);
        }

        private void Median_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenCvSharp.Cv2.MedianBlur(src, dst, 15);
            ToWriteableBitmap(dst, writeableBitmap);
            MyµImage.ApplyWriteableBitmap(writeableBitmap);
        }

        private void Dilate_Button_Click(object sender, RoutedEventArgs e)
        {
            byte[] kernelValues = { 0, 0, 1, 0, 0,
                                    0, 0, 1, 0, 0,
                                    1, 1, 1, 1, 1,
                                    0, 0, 1, 0, 0,
                                    0, 0, 1, 0, 0}; // cross (+)
            OpenCvSharp.Mat kernel = new OpenCvSharp.Mat(5, 5, OpenCvSharp.MatType.CV_8UC1, kernelValues);

            OpenCvSharp.Cv2.Dilate(src, dst, kernel);

            ToWriteableBitmap(dst, writeableBitmap);
            MyµImage.ApplyWriteableBitmap(writeableBitmap);
        }

        private void Threshold_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenCvSharp.Cv2.Threshold(src, dst, 100, 255, OpenCvSharp.ThresholdTypes.Binary);

            ToWriteableBitmap(dst, writeableBitmap);
            MyµImage.ApplyWriteableBitmap(writeableBitmap);
        }
        
        //Helper
        //from https://github.com/shimat/opencvsharp/blob/master/src/OpenCvSharp.Extensions/WriteableBitmapConverter.cs
        public static void ToWriteableBitmap(OpenCvSharp.Mat src, WriteableBitmap dst)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dst == null) throw new ArgumentNullException(nameof(dst));
            if (src.Width != dst.PixelWidth || src.Height != dst.PixelHeight) throw new ArgumentException("size of src must be equal to size of dst");
            //if (src.Depth != BitDepth.U8) throw new ArgumentException("bit depth of src must be BitDepth.U8", "src");
            if (src.Dims > 2) throw new ArgumentException("Mat dimensions must be 2");
            int w = src.Width;
            int h = src.Height;
            int bpp = dst.Format.BitsPerPixel;
            //int channels = GetOptimumChannels(dst.Format);

            int channels = 1; //single channel only - just for test
            if (src.Channels() != channels) { throw new ArgumentException("channels of dst != channels of PixelFormat", nameof(dst)); }

            bool submat = src.IsSubmatrix();
            bool continuous = src.IsContinuous();

            unsafe{
                byte* pSrc = (byte*)(src.Data);
                int sstep = (int)src.Step();
                //1 bit bpp completely removed - I haven't plan to support such images. The 8 bit will be used instead.
                // Copy            
                if (!submat && continuous){
                    long imageSize = src.DataEnd.ToInt64() - src.Data.ToInt64();
                    if (imageSize < 0) throw new OpenCvSharp.OpenCvSharpException("The mat has invalid data pointer");
                    if (imageSize > int.MaxValue) throw new OpenCvSharp.OpenCvSharpException("Too big mat data");
                    dst.WritePixels(new Int32Rect(0, 0, w, h), src.Data, (int)imageSize, sstep);
                    return;
                }
                // row by row copy if not continuous
                try{
                    dst.Lock();
                    dst.AddDirtyRect(new Int32Rect(0, 0, dst.PixelWidth, dst.PixelHeight));
                    int dstep = dst.BackBufferStride;
                    byte* pDst = (byte*)dst.BackBuffer;
                    for (int y = 0; y < h; y++){
                        long offsetSrc = (y * sstep);
                        long offsetDst = (y * dstep);
                        OpenCvSharp.Util.MemoryHelper.CopyMemory(pDst + offsetDst, pSrc + offsetSrc, w * channels);
                    }
                }
                finally{
                    dst.Unlock();
                }
            }
        }
    }
}
