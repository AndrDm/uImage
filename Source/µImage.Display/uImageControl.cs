using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;
using µ.Vision;
using µ.Structures;
using static µ.Vision.µImage;
//using OpenCvSharp;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;


namespace µ.Display
{
	public enum DisplayMappingOption{
		Default = 0,
		FullDynamic = 1,
		GivenRange = 2,
	}
	
	[TemplatePart(Name = "PART_µImage", Type = typeof(Image))]
	[TemplatePart(Name = "PART_µMouseHandler", Type = typeof(Grid))]
	[TemplatePart(Name = "PART_µScrollViewer", Type = typeof(Grid))]
	[TemplatePart(Name = "PART_µZoom", Type = typeof(TextBlock))]
	[TemplatePart(Name = "PART_µInfo", Type = typeof(TextBlock))]
	[TemplatePart(Name = "PART_µPixelInfo", Type = typeof(TextBlock))]
	[TemplatePart(Name = "PART_µROI", Type = typeof(ItemsControl))]
	
	public partial class uImageControl : Control
	{
		//All parts here
		private Image part_µImage;
		private Grid part_µMouseHandler;
		private ScrollViewer part_µScrollViewer;
		private TextBox part_µZoom;
		private TextBox part_µInfo;
		private TextBox part_µPixelInfo;
		private ItemsControl part_µROI;

		private µ.Vision.µImage µimage;
		private WriteableBitmap writeableBitmap;

		private Point _previousPanPoint = new Point(0.0, 0.0);
		private PixelFormat _previousPixelFormat;
		private bool _mouseDown = false;

		public Point MousePosition
		{
			get{ return (Point)GetValue(MousePositionProperty); }
			set{ SetValue(MousePositionProperty, value); }
		}

		public int Test;
		public struct DisplayMapping
		{
			public DisplayMappingOption displayMappingOption;
			public double min;
			public double max;
		} 
		
		DisplayMapping displayMapping;

		public static readonly DependencyProperty MousePositionProperty;
		public static readonly DependencyProperty MagnificationProperty;
		public static readonly DependencyProperty PaletteProperty;

		private static readonly DependencyPropertyKey ROIListPropertyKey;
		public static readonly DependencyProperty ROIListProperty;

		public ObservableCollection<ROI> ROIList
		{
			get{ return (ObservableCollection<ROI>)GetValue(ROIListProperty); }
			protected set{ SetValue(ROIListPropertyKey, value); }
		}

		static uImageControl()
		{
			MagnificationProperty = DependencyProperty.Register("Magnification", typeof(double), typeof(uImageControl), 
				new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnMagnificationChanged));
			MousePositionProperty = DependencyProperty.Register("MousePosition", typeof(Point), typeof(uImageControl), 
				new PropertyMetadata(new Point(0.0, 0.0)));
			PaletteProperty = DependencyProperty.Register("Palette", typeof(PaletteType), typeof(uImageControl), 
				new PropertyMetadata(PaletteType.Default));
			ROIListPropertyKey = DependencyProperty.RegisterReadOnly("ROIList", 
				typeof(ObservableCollection<ROI>), typeof(uImageControl), 
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
			ROIListProperty = ROIListPropertyKey.DependencyProperty;
			
			GetLastEventDataPropertyKey = DependencyProperty.RegisterReadOnly("GetLastEventData", 
				typeof(ROIDescriptor.LastEventData), typeof(uImageControl), new PropertyMetadata(null, OnGetLastEventDataChanged));
			GetLastEventDataProperty = GetLastEventDataPropertyKey.DependencyProperty;

			ToolRegisterProperty();    


			DefaultStyleKeyProperty.OverrideMetadata(typeof(uImageControl), new FrameworkPropertyMetadata(typeof(uImageControl)));
		}

		public uImageControl()
		{
			ROIList= new ObservableCollection<ROI>();
		}

		public void ApplyBitmap(BitmapImage bitmap)
		{
			part_µImage.Source = bitmap;
		}

		public void ApplyWriteableBitmap(WriteableBitmap bitmap)
		{
			part_µImage.Source = bitmap;
		}

		public void ApplyµImage(µImage image)
		{
			µimage = image;
			part_µImage.Width = image.Width;
			part_µImage.Height = image.Height;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			part_µImage = GetTemplateChild("PART_µImage") as Image;
			if (null == part_µImage) throw new NullReferenceException("Template Part µImage is not available");
			part_µMouseHandler = GetTemplateChild("PART_µMouseHandler") as Grid;
			if (null == part_µMouseHandler) throw new NullReferenceException("Template Part µMouseHandler is not available");
			part_µScrollViewer = GetTemplateChild("PART_µScrollViewer") as ScrollViewer;
			if (null == part_µScrollViewer) throw new NullReferenceException("Template Part µScrollViewer is not available");
			part_µZoom = GetTemplateChild("PART_µZoom") as TextBox;
			if (null == part_µZoom) throw new NullReferenceException("Template Part µZoom is not available");
			part_µInfo = GetTemplateChild("PART_µInfo") as TextBox;
			if (null == part_µInfo) throw new NullReferenceException("Template Part µInfo is not available");
			part_µPixelInfo = GetTemplateChild("PART_µPixelInfo") as TextBox;
			if (null == part_µPixelInfo) throw new NullReferenceException("Template Part µPixelInfo is not available");
			part_µROI = GetTemplateChild("PART_µROI") as ItemsControl;
			if (null == part_µROI) throw new NullReferenceException("Template Part µROI is not available");

			part_µImage.LayoutTransform = new ScaleTransform();
			part_µROI.LayoutTransform = new ScaleTransform();
			
			part_µMouseHandler.MouseWheel += OnµImageControlMouseWheel;
			part_µMouseHandler.MouseLeftButtonDown += OnµImageControlMouseLeftButtonDown;
			part_µMouseHandler.MouseMove += OnµImageControlMouseMove;
			part_µMouseHandler.MouseLeftButtonUp += OnµImageControlMouseLeftButtonUp;

			SetDisplayMappingData(0, 255, DisplayMappingOption.FullDynamic); 
		}

		private void OnµImageControlMouseWheel(object sender, MouseWheelEventArgs e)
		{    
			SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

			double zoom_delta = e.Delta > 0 ? .1 : -.1;            
			Magnification = (Magnification += zoom_delta).LimitToRange(.1, 10);
			CenterViewerAroundMouse(MousePosition);
			ShowMousePosition();
			e.Handled = true;
		}    

		private void OnµImageControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource == part_µMouseHandler){ //user has clcked on the free space and on ROI
				switch (SelectedTool){
					case Tool.Pan:
					case Tool.None: //none als pan
						_previousPanPoint = Mouse.GetPosition(part_µScrollViewer);
						part_µMouseHandler.CaptureMouse();
						_mouseDown = true;
						break;
					case Tool.ROILine:
						ROIList.Clear();
						StartDrawingLineROI();
						break;
					case Tool.ROIRect:    
						ROIList.Clear();
						StartDrawingRectROI();
						break;
					case Tool.ROIOval:
						ROIList.Clear();
						StartDrawingOvalROI();
						break;

				}
			}            
		}//OnµImageControlMouseLeftButtonDown - new ROI types add here

		private void OnµImageControlMouseMove(object sender, MouseEventArgs e)
		{
			SetCurrentValue(MousePositionProperty, Mouse.GetPosition(part_µImage));

			if (_mouseDown){
				Point previousPanPoint = _previousPanPoint;
				Point position = Mouse.GetPosition(part_µScrollViewer);
				double x_diff = position.X - previousPanPoint.X;
				double y_diff = position.Y - previousPanPoint.Y;
				part_µScrollViewer.ScrollToHorizontalOffset(part_µScrollViewer.HorizontalOffset - x_diff);
				part_µScrollViewer.ScrollToVerticalOffset(part_µScrollViewer.VerticalOffset - y_diff);
				_previousPanPoint = position;
			}
			ShowMousePosition();
			ShowImagePixelValue();            
		}

		private void OnµImageControlMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			part_µMouseHandler.ReleaseMouseCapture();
			_mouseDown = false;
		}

		private void ShowMousePosition()
		{
			Point pos = Mouse.GetPosition(part_µImage);
			if ((pos.X>=0) && (pos.X < part_µImage.Width) &&(pos.Y>=0) && (pos.Y < part_µImage.Height)){
				part_µInfo.Text = $"x = {Mouse.GetPosition(part_µImage).X:N0}; y = {Mouse.GetPosition(part_µImage).Y:N0}";
			}
		}

		private int ShowImagePixelValue()
		{
			if (part_µImage == null) return 0;

			int result = 0;
			if (MousePosition.X < part_µImage.ActualWidth && MousePosition.X >= 0.0 
				&& MousePosition.Y < part_µImage.ActualHeight && MousePosition.Y >= 0.0){
				result = µ.Vision.µImage.GetPixelValue(µimage, (int)MousePosition.X, (int)MousePosition.Y);
				if (µimage.imageType == ImageType.U8) part_µPixelInfo.Text = $"8-bit image, {result:N0}";                
				if (µimage.imageType == ImageType.U16) part_µPixelInfo.Text = $"16-bit image, {result:N0}";                
			}
			return result;
		}

		public void DisplayImage(µImage image)
		{
			DisplayImage(image, _previousPaletteType);
		}

		public void DisplayImage(µImage image, PaletteType palette)
		{
			ReCreateBitmap(image, palette, ref writeableBitmap);
		}

		internal void ReCreateBitmap(µImage image, PaletteType paletteType, ref WriteableBitmap writeableBitmap)
		{
			PixelFormat pixelFormat;
			BitmapPalette palette;

			switch (paletteType){
				case PaletteType.Default:
					palette = null;
					pixelFormat = PixelFormats.Gray8;
					break;
				default:
					palette = µPalette.GetPalette(paletteType);
					pixelFormat = PixelFormats.Indexed8;
					break;
			}
		
			if ((null == writeableBitmap) || 
				(image.Width != writeableBitmap.Width) || (image.Height != writeableBitmap.Height) ||
				(paletteType != _previousPaletteType) || (pixelFormat != _previousPixelFormat) ){
				writeableBitmap = new WriteableBitmap(image.Width, image.Height, 96, 96, pixelFormat, palette);
			}
			ToWriteableBitmap(image, writeableBitmap, displayMapping);
			this.ApplyWriteableBitmap(writeableBitmap);
			this.ApplyµImage(image);
			_previousPaletteType = paletteType;
			_previousPixelFormat = pixelFormat;
		}

		public void SetDisplayMappingData(double min, double max, DisplayMappingOption displayMappingOption)
		{
			displayMapping.min = min;
			displayMapping.max = max;
			displayMapping.displayMappingOption = displayMappingOption;
		}

		[DllImport("µImage.Unmanaged.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Mapping16to8( IntPtr src_ptr_16bit, IntPtr dst_ptr_8bit,
												int src_bytesPerLine, int dst_bytesPerLine,
												int width, int height, int min, int max, DisplayMappingOption mode);
		
		[DllImport("µImage.Unmanaged.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Mapping8to8( IntPtr src_ptr_8bit, IntPtr dst_ptr_8bit,
												int src_bytesPerLine, int dst_bytesPerLine,
												int width, int height, int min, int max, DisplayMappingOption mode);

		public static void ToWriteableBitmap(µImage source, WriteableBitmap dst, DisplayMapping displayMapping)
		{
			OpenCvSharp.Mat src = source._image;

			if (src == null) throw new ArgumentNullException(nameof(src));
			if (dst == null) throw new ArgumentNullException(nameof(dst));
			if (src.Width != dst.PixelWidth || src.Height != dst.PixelHeight) throw new ArgumentException("size of src must be equal to size of dst");
			if (src.Dims > 2) throw new ArgumentException("Mat dimensions must be 2");
			int width = src.Width;
			int height = src.Height;
			int bpp = dst.Format.BitsPerPixel;
			//int channels = GetOptimumChannels(dst.Format);

			int channels = 1; //single channel only - just for test
			if (src.Channels() != channels) { throw new ArgumentException("channels of dst != channels of PixelFormat", nameof(dst)); }

			unsafe{
				byte* pSrc = (byte*)(src.Data);
				int sstep = (int)src.Step();
				int dstep = dst.BackBufferStride;
				//1 bit bpp completely removed - I haven't plan to support such images. The 8 bit will be used instead.
				// Copy            
				long imageSize = src.DataEnd.ToInt64() - src.Data.ToInt64();
				if (imageSize < 0) throw new OpenCvSharp.OpenCvSharpException("The mat has invalid data pointer");
				if (imageSize > int.MaxValue) throw new OpenCvSharp.OpenCvSharpException("Too big mat data");

				dst.Lock();
				IntPtr backBuffer = dst.BackBuffer;
				IntPtr srcPointer = src.Data;
				switch (source.imageType){
					case ImageType.U8:
						Mapping8to8(srcPointer, backBuffer, sstep, dstep, width, height,  
							(int)displayMapping.min, (int)displayMapping.max, displayMapping.displayMappingOption);
						break;
					case ImageType.U16:
						Mapping16to8(srcPointer, backBuffer, sstep, dstep, width, height,
							(int)displayMapping.min, (int)displayMapping.max, displayMapping.displayMappingOption);
						break;
				}
				dst.AddDirtyRect(new Int32Rect(0, 0, width, height));                    
				dst.Unlock();
			}
		}
	}
}
