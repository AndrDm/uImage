using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Globalization;
using OpenCvSharp;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace µ.Vision 
{

    public enum ImageType 
    {
        U8 = 0,
        U16 = 1,
    }

       public enum CalibrationUnit
    {
        Undefined = 0,
        Millimeter = 1,
        Inch = 2,
    }

    //[Serializable]
    public sealed class µImage  // : IDisposable, ISerializable ToDo: later
    {
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public IntPtr _image = IntPtr.Zero;
        public OpenCvSharp.Mat _image = null;

        /// <summary>
        /// Creates a new image.
        /// </summary>
        public µImage() : this(ImageType.U8)
        {
        }

        /// <summary>
        /// Creates a new image.
        /// </summary>
        /// <param name="type">The <see cref="µImage.Vision.ImageType"/> of the image.</param>
        public µImage(ImageType type) : this(type, 0, 0)
        {
        }
        public µImage(ImageType type, Int32 width, Int32 height)
        {
            var µsize = new OpenCvSharp.Size(); 
            µsize.Width = width;
            µsize.Height = height;

            switch (type) {
                case ImageType.U8:
                    _image = new OpenCvSharp.Mat(µsize, OpenCvSharp.MatType.CV_8U);
                    break;
                case ImageType.U16:
                    _image = new OpenCvSharp.Mat(µsize, OpenCvSharp.MatType.CV_16U);
                    break;
                default:
                    Debug.Fail("Unknown image type!");
                    break;
            }
        }

        public ImageType imageType
        {
            get {
                if (OpenCvSharp.MatType.CV_8U == _image.Type()) return ImageType.U8;
                if (OpenCvSharp.MatType.CV_16U == _image.Type()) return ImageType.U16;
                return ImageType.U8; //ToDo : add unknown type here
            }
        }

        //==========================================================================================
        /// <summary>
        /// Gets the height of the image in pixels.</summary>
        /// <value>
        /// The height of the image in pixels.
        /// </value>
        public Int32 Height
        {
            get {   //ThrowIfDisposed();
                Int32 height;
                height = _image.Height;
                return height;
            }
        }
        public Int32 Width
        {
            get {   //ThrowIfDisposed();
                Int32 width;
                width = _image.Width;
                return width;
            }
        }

        /// <summary>
        /// Reads an image file.
        /// </summary>
        /// <param name="fileName">The path of the file to read.</param>
        public static void µReadFile(µImage destination, string fileName)
        {
            destination._image = new OpenCvSharp.Mat(fileName, OpenCvSharp.ImreadModes.AnyDepth);
        }

        public static void ToWriteableBitmap(µImage source, WriteableBitmap dst)
        {
            OpenCvSharp.Mat src = source._image;

            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dst == null) throw new ArgumentNullException(nameof(dst));
            if (src.Width != dst.PixelWidth || src.Height != dst.PixelHeight) throw new ArgumentException("size of src must be equal to size of dst");
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

        public static Int32 GetPixelValue(µImage image, Int32 column, Int32 row)
        {
            if (image.imageType == ImageType.U8){
                return image._image.Get<byte>(row, column); //y, x
            }
            if (image.imageType == ImageType.U16){
                return image._image.Get<UInt16>(row, column); //y, x
            }
            
            return -1; //unsupported type exception will be better

        }
        public static void µCopy(µImage source, µImage destination)
        {
            if (null == source) { throw new ArgumentNullException("source"); }
            if (null == destination) { throw new ArgumentNullException("destination"); }

            OpenCvSharp.Cv2.CopyTo(source._image, destination._image);
        }

        public static void Median_Demo(µImage source, µImage destination)
        {
           OpenCvSharp.Cv2.MedianBlur(source._image, destination._image, 5);
        }

        public static void Dilate_Demo(µImage source, µImage destination)
        {
            byte[] kernelValues = { 0, 0, 1, 0, 0,
                                    0, 0, 1, 0, 0,
                                    1, 1, 1, 1, 1,
                                    0, 0, 1, 0, 0,
                                    0, 0, 1, 0, 0}; // cross (+)
            OpenCvSharp.Mat kernel = new OpenCvSharp.Mat(5, 5, OpenCvSharp.MatType.CV_8UC1, kernelValues);

            OpenCvSharp.Cv2.Dilate(source._image, destination._image, kernel);
        }

        public static void Threshold_Demo(µImage source, µImage destination)
        {
            OpenCvSharp.Cv2.Threshold(source._image, destination._image, 100, 255, OpenCvSharp.ThresholdTypes.Binary);
        }

    }
}

