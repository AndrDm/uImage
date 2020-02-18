using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;

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

        public static double µGetMax(µImage source)
        {
            double min, max;
            OpenCvSharp.Cv2.MinMaxLoc(source._image,  out min, out max);
            return max;
        }

        public static double µGetMin(µImage source)
        {
            double min, max;
            OpenCvSharp.Cv2.MinMaxLoc(source._image,  out min, out max);
            return min;
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
            double min, max;
            OpenCvSharp.Cv2.MinMaxLoc(source._image,  out min, out max);
            OpenCvSharp.Cv2.Threshold(source._image, destination._image, (max-min)/2, max, OpenCvSharp.ThresholdTypes.Binary);
        }

        public static void µHistogram(µImage source, double[] hist_array)
        {
            OpenCvSharp.Mat hist = new OpenCvSharp.Mat();
            int[] hdims = {256}; // Histogram size - currently 256 bins only
            double min, max;
            OpenCvSharp.Cv2.MinMaxLoc(source._image,  out min, out max);
            OpenCvSharp.Rangef[] ranges = { new OpenCvSharp.Rangef((float)min, (float)max+1), }; // min/max 
            OpenCvSharp.Cv2.CalcHist(new OpenCvSharp.Mat[]{source._image}, new int[]{0}, null, hist, 1, hdims, ranges);
            for (int i = 0; i < hdims[0]; ++i) hist_array[i] = hist.Get<float>(i);
        }

        //==========================================================================================
        /// <summary>
        /// Computes the profile of a line of pixels.
        ///This method returns other information such as pixel averages.
        /// </summary>
        /// <param name="source">
        /// The image on which the method computes the line profile.
        /// </param>
        /// <param name="pt1">
        /// The start point the line along which the method computes the profile.
        /// </param>
        /// <param name="pt2">
        /// The end point the line along which the method computes the profile.
        /// </param>
        /// <returns>
        /// void
        /// </returns>
        /// <remarks>
        /// Use this method with U8 and U16 images.
        /// </remarks>
        public static void µLineProfile(µImage source, Point pt1, Point pt2, out List<double> Profile, out double Average)
        {
            // You can pass connectivity as constructor argument. Default is 8.
            OpenCvSharp.Point pt1cv;
            OpenCvSharp.Point pt2cv;
            pt1cv.X = (int)pt1.X; pt1cv.Y = (int)pt1.Y; 
            pt2cv.X = (int)pt2.X; pt2cv.Y = (int)pt2.Y; 
            int sum = 0;
            int count = 0;

            List<double> profile = new List<double>();
            
            switch (source.imageType){
                case ImageType.U8:
                    foreach (var lip in new OpenCvSharp.LineIterator(source._image, pt1cv, pt2cv)) {
                        byte value = lip.GetValue<byte>();
                        profile.Add(value);
                        sum += value; count++;
                    }
                break;
                case ImageType.U16:
                    foreach (var lip in new OpenCvSharp.LineIterator(source._image, pt1cv, pt2cv)) {
                        ushort value = lip.GetValue<ushort>();
                        profile.Add(value);
                        sum += value; count++;
                    }
                break;
            }
            
            Average = (double)sum / count;
            Profile = profile;
        }//µLineProfile (based on OpenCvSharp.LineIterator)
                    
            
    } //class µImage
} //namespace µ.Vision