using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using µ.Vision;
using µ.Structures;

namespace µ.Display
{


	
	public abstract class ROI : ItemsControl
	{
		public enum State{
			DrawingInProgress,
			Normal
		}
    	public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState", 
		typeof(State), typeof(ROI), new FrameworkPropertyMetadata(State.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

     	public State CurrentState{
			get{ return (State)GetValue(CurrentStateProperty); }
			set{ SetValue(CurrentStateProperty, value); }
		}

		public event EventHandler<ROIDescriptor.LastEventArgs> LastROIDrawEvent;

		protected void UpdateLastROIDrawEvent(ROIDescriptor.LastEventArgs e)
		{
			this.LastROIDrawEvent?.Invoke(this, e);
		}

		public abstract ROIDescriptor.LastEventData GetLastDrawEventData();
		public abstract ROIDescriptor.Contour GetROIDescriptorContour();
    }//class ROI

    public class ROILine : ROI
	{
        //https://wpf.2000things.com/tag/affectsrender/
		public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint", 
        typeof(Point), typeof(ROILine), new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
            FrameworkPropertyMetadataOptions.AffectsRender));
		public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", 
        typeof(Point), typeof(ROILine), new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
            FrameworkPropertyMetadataOptions.AffectsRender));

		public Point StartPoint{
			get{ return (Point)GetValue(StartPointProperty); }
			set{ SetValue(StartPointProperty, value); }
		}

		public Point EndPoint{
			get{ return (Point)GetValue(EndPointProperty); }
			set{ SetValue(EndPointProperty, value); }
		}

		public ROILine()
		{
			base.MouseLeftButtonDown += OnLineROIMouseLeftButtonDown;
			base.MouseLeftButtonUp += OnLineROIMouseLeftButtonUp;
			base.MouseMove += OnLineROIMouseMove;
		}

        //Down Move Up:
		private void OnLineROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CaptureMouse();
        }

        private void OnLineROIMouseMove(object sender, MouseEventArgs e)
		{
        	if (base.CurrentState == State.DrawingInProgress) {
				EndPoint = e.GetPosition(this);
				UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
			}
		}

		private void OnLineROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0); //ToDo: scale with magnification factor!
			dc.DrawLine(pen, StartPoint, EndPoint);	
		}

		public override ROIDescriptor.LastEventData GetLastDrawEventData()
		{
			double width = Math.Abs(EndPoint.X - StartPoint.X);
			double height = Math.Abs(EndPoint.Y - StartPoint.Y);
			double diagonal = Math.Sqrt(width * width + height * height);
			double angle = (Math.Atan2(0.0 - (EndPoint.Y - StartPoint.Y), EndPoint.X - StartPoint.X) * 180.0 / Math.PI);
			return new ROIDescriptor.LastEventData{
				type = EventType.Draw,
				tool = EventTool.ROI,
				roi = ROItype.Line,
				coordinates = new List<Point>{
					StartPoint,
					EndPoint
				},
				otherParameters = new List<double>{
					width,
					height,
					diagonal,
					angle
				}
			};
		}

		public override ROIDescriptor.Contour GetROIDescriptorContour()
		{
			return new ROIDescriptor.Contour{
				roiType = ROItype.Line,
				points = new List<Point>{
					StartPoint,
					EndPoint
				}
			};
		}
    }//class ROILine : ROI

	public class ROIRect : ROI
	{

		public static readonly DependencyProperty 
		TopLeftProperty = DependencyProperty.Register("TopLeft", 
		typeof(Point), typeof(ROIRect), 
		new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
		FrameworkPropertyMetadataOptions.AffectsRender)); //ToDo:OnChanged
		public static readonly DependencyProperty BottomRightProperty = 
		DependencyProperty.Register("BottomRight", 
		typeof(Point), typeof(ROIRect), 
		new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
		FrameworkPropertyMetadataOptions.AffectsRender)); //ToDo:OnChanged

		public Point TopLeftPoint{
			get{return (Point)GetValue(TopLeftProperty);}
			set{SetValue(TopLeftProperty, value);}
		}

		public Point BottomRightPoint{
			get{return (Point)GetValue(BottomRightProperty);}
			set{SetValue(BottomRightProperty, value);}
		}

		public ROIRect()
		{
			base.MouseLeftButtonDown += OnRectROIMouseLeftButtonDown;
			base.MouseLeftButtonUp += OnRectROIMouseLeftButtonUp;
			base.MouseMove += OnRectROIMouseMove;
		}
		private void OnRectROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CaptureMouse();
        }

        private void OnRectROIMouseMove(object sender, MouseEventArgs e)
		{
        	if (base.CurrentState == State.DrawingInProgress) {
				BottomRightPoint = e.GetPosition(this);
				UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
			}
		}

		private void OnRectROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0); //ToDo: scale with magnification factor!
			Rect rect;

			rect.X = Math.Min(TopLeftPoint.X, BottomRightPoint.X);
			rect.Y = Math.Min(TopLeftPoint.Y, BottomRightPoint.Y);
			rect.Width = Math.Abs(BottomRightPoint.X - TopLeftPoint.X);
			rect.Height = Math.Abs(BottomRightPoint.Y - TopLeftPoint.Y);
	
			dc.DrawRectangle(Brushes.Transparent, pen, rect);
		}

		public override ROIDescriptor.LastEventData GetLastDrawEventData()
		{
			double width = Math.Abs(BottomRightPoint.X - TopLeftPoint.X);
			double height = Math.Abs(BottomRightPoint.Y - TopLeftPoint.Y);
			double diagonal = Math.Sqrt(width * width + height * height);
			return new ROIDescriptor.LastEventData{
				type = EventType.Draw,
				tool = EventTool.ROI,
				roi = ROItype.Rectangle,
				coordinates = new List<Point>{
					TopLeftPoint,
					BottomRightPoint
				},
				otherParameters = new List<double>{
					width,
					height,
					diagonal,
				}
			};
		}

		public override ROIDescriptor.Contour GetROIDescriptorContour()
		{
			return new ROIDescriptor.Contour{
				roiType = ROItype.Rectangle,
				points = new List<Point>{
					TopLeftPoint,
					BottomRightPoint
				}
			};
		}

	} //RectROI


	public class ROIValueChangedEventArgs : EventArgs
	{
		public ROIDescriptor.LastEventData lastEventData;
		public ROIDescriptor ROI;
		public ROIValueChangedEventArgs(ROIDescriptor.LastEventData lastEventData, ROIDescriptor oldROI, ROIDescriptor newROI)
		{
			this.lastEventData = lastEventData;
			this.ROI = newROI;
		}
	}//class ROIValueChangedEventArgs

    public partial class uImageControl : Control
    {
		private ROIDescriptor _lastROIDescriptor = new ROIDescriptor();
		private static readonly DependencyPropertyKey GetLastEventDataPropertyKey;
		public static readonly DependencyProperty GetLastEventDataProperty;
		public ROIDescriptor.LastEventData GetLastEventData
		{
			get{return (ROIDescriptor.LastEventData)GetValue(GetLastEventDataProperty); }
			protected set {SetValue(GetLastEventDataPropertyKey, value); }
		}

        private void StartDrawingLineROI()
        {
			ROILine lineROI = new ROILine();
			ROIList.Add(lineROI);	
			lineROI.EndPoint = lineROI.StartPoint = MousePosition;
			lineROI.CaptureMouse();
			lineROI.CurrentState = ROI.State.DrawingInProgress;
			lineROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
        }

        private void StartDrawingRectROI()
        {
			ROIRect rectROI = new ROIRect();
			ROIList.Add(rectROI);
			rectROI.TopLeftPoint = rectROI.BottomRightPoint = MousePosition;
			rectROI.CaptureMouse();
			rectROI.CurrentState = ROI.State.DrawingInProgress;
			rectROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
        }


		//https://docs.microsoft.com/en-us/dotnet/api/system.windows.dependencypropertychangedeventargs?view=netcore-3.1
		private static void OnGetLastEventDataChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			uImageControl imageControl = (uImageControl)obj;
			ROIDescriptor.LastEventData lastEventData = (ROIDescriptor.LastEventData)args.NewValue;
			ROIDescriptor.LastEventData other = (ROIDescriptor.LastEventData)args.OldValue;
			if (lastEventData.type == EventType.Draw){
				ROIDescriptor lastROIDescriptor = imageControl._lastROIDescriptor;
				ROIDescriptor previousROIDescriptor = imageControl.GetROIDescriptor();
				if (!lastEventData.IsChanged(other) || !previousROIDescriptor.IsChanged(lastROIDescriptor)){
					imageControl._lastROIDescriptor = previousROIDescriptor;
					imageControl.OnROIValueChanged(new ROIValueChangedEventArgs(lastEventData, lastROIDescriptor, previousROIDescriptor));
				}
			}
		}

		public ROIDescriptor GetROIDescriptor()
		{
			ROIDescriptor roiDescriptor = new ROIDescriptor();
			if (ROIList.Count == 0) return roiDescriptor;
			foreach (ROI item in ROIList) roiDescriptor.contours.Add(item.GetROIDescriptorContour());
			return roiDescriptor;
		}

		public event EventHandler<ROIValueChangedEventArgs> ROIValueChanged;
		private void OnGetLastDrawEventUpdated(object sender, ROIDescriptor.LastEventArgs lastEventArgs)
		{
			GetLastEventData = lastEventArgs.data;
		}

		private void OnROIValueChanged(ROIValueChangedEventArgs args)
		{
			this.ROIValueChanged?.Invoke(this, args);
		}
    }//partial class uImageControl
}//namespace µ.Display
