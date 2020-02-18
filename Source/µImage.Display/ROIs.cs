using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace µ.Display
{
	public enum ROItype{
		None,
		Line
	}

	public enum EventType
		{
			NoEvent,
			Click,
			Draw,
		}

		public enum EventTool
		{
			None,
			Cursor,
			ROI,
			Pan
		}

	public class ROIDescriptor{
		public class Contour{
			public ROItype roiType;
			public List<Point> points;
			public Contour()
			{
				points = new List<Point>();
			}

			public bool IsChanged(Contour other)
			{
				if (other != null &&  roiType.Equals(other.roiType)){
					return points.SequenceEqual(other.points);
				}
				return false;
			}
		}
		
		public class LastEventData
		{
			public EventType type;
			public EventTool tool;
			public ROItype roi;
			public List<Point> coordinates;
			public List<double> otherParameters;
			public bool IsChanged(LastEventData other)
			{
				if (other != null && type.Equals(other.type) && tool.Equals(other.tool) && coordinates.SequenceEqual(other.coordinates)){
					return otherParameters.SequenceEqual(other.otherParameters);
				}
				return false;
			}
		}

		public class LastEventArgs : EventArgs
		{
			public LastEventData data;

			public LastEventArgs(LastEventData data)
			{
				this.data = data;
			}
		}

		public List<double> boundingBox;

		public List<Contour> contours;

		public ROIDescriptor()
		{
			boundingBox = new List<double>();
			contours = new List<Contour>();
		}

  		public bool IsChanged(ROIDescriptor other)
  		{
  			if (other == null) return false;
  			if (boundingBox != null && !boundingBox.SequenceEqual(other.boundingBox)) return false;
  			if (boundingBox == null && other.boundingBox != null) return false;
  			if (contours.Count != other.contours.Count) return false;
  			for (int i = 0; i < contours.Count; i++) if (!contours[i].IsChanged(other.contours[i])) return false;
  			return true;
  		}
	}// class ROIDescriptor

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
