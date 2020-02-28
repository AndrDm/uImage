using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using µ.Vision;
using µ.Structures;
using µ.Core;

//at this momemnt I would like to keep everything in single file, later will be splitted
namespace µ.Display
{

	public class Anchor : FrameworkElement
	{
	
		public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState", typeof(State), 
		typeof(Anchor), new FrameworkPropertyMetadata(State.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

		public State CurrentState{
			get{return (State)GetValue(CurrentStateProperty);}
			set{SetValue(CurrentStateProperty, value);}
		}

		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(Point), 
		typeof(Anchor), new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

		public Point Position{
			get{return (Point)GetValue(PositionProperty);}
			set{SetValue(PositionProperty, value);}
		}

		public static readonly DependencyProperty MagnificationProperty = DependencyProperty.Register("Magnification", typeof(double), 
		typeof(Anchor), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public double Magnification{
			get{return (double)GetValue(MagnificationProperty);}
			set{SetValue(MagnificationProperty, value);}
		}

		public Anchor(){}
	}


	public class CrossAnchor : Anchor
	{ //MoveAnchor appeared in the middle of the ROI - for whole movement
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			Point center = new Point(base.Position.X, base.Position.Y);
			Point top = new Point(center.X, center.Y - 5);
			Point bottom = new Point(center.X, center.Y + 5);
			Point left = new Point(center.X - 5, center.Y);
			Point right = new Point(center.X + 5, center.Y);
			Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification);
			//10x10 cross
			drawingContext.DrawLine(pen, top, bottom);
			drawingContext.DrawLine(pen, left, right);
		}
	}

	public class RoundAnchor : Anchor
	{
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			Point center = new Point(base.Position.X, base.Position.Y);
			Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification);
			drawingContext.DrawEllipse(Brushes.Transparent, pen, center, 5.0 / base.Magnification, 5.0 / base.Magnification);
		}
	}

	internal static class AnchorsFactory
	{
		public static Anchor Create(AnchorType type, ROI roi)
		{
			Anchor anchor = null;
			switch (type){
				case AnchorType.Move: anchor = new CrossAnchor(); break;
				case AnchorType.Resize: anchor = new RoundAnchor(); break;
				default: return null;
			}

			Binding binding = new Binding(Anchor.MagnificationProperty.Name);
			binding.Source = roi;
			anchor.SetBinding(Anchor.MagnificationProperty, binding);

			return anchor;

		}
	}

	public abstract class ROI : ItemsControl
	{
		public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState", 
		typeof(State), typeof(ROI), new FrameworkPropertyMetadata(State.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

	 	public State CurrentState{
			get{ return (State)GetValue(CurrentStateProperty); }
			set{ SetValue(CurrentStateProperty, value); }
		}
		public static readonly DependencyProperty MagnificationProperty = DependencyProperty.Register("Magnification", 
		typeof(double), typeof(ROI), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public double Magnification{
			get{return (double)GetValue(MagnificationProperty);}
			set{SetValue(MagnificationProperty, value);}
		}


		public Point actPos = new Point(0.0, 0.0);
		public Point prevPos = new Point(0.0, 0.0);
		public object actElt; //can keep in private scope, but here pretty convenient

		public event EventHandler<ROIDescriptor.LastEventArgs> LastROIDrawEvent;

		protected void UpdateLastROIDrawEvent(ROIDescriptor.LastEventArgs e)
		{
			this.LastROIDrawEvent?.Invoke(this, e);
		}

		public abstract ROIDescriptor.LastEventData GetLastDrawEventData();
		public abstract ROIDescriptor.Contour GetROIDescriptorContour();


		/* ANCHORS using System.Collections.ObjectModel; */
		private static readonly DependencyPropertyKey AnchorsPropertyKey = DependencyProperty.RegisterReadOnly("Anchors", 
			typeof(ObservableCollection<Anchor>), typeof(ROI), new PropertyMetadata(null));

		public static readonly DependencyProperty AnchorsProperty = AnchorsPropertyKey.DependencyProperty;

		public ObservableCollection<Anchor> Anchors{
			get{return (ObservableCollection<Anchor>)GetValue(AnchorsProperty);}
			protected set{SetValue(AnchorsPropertyKey, value);}
		}

		public ROI()
		{
			Anchors = new ObservableCollection<Anchor>();
			base.ItemsSource = Anchors;
		}

	}//class ROI

	public class ROILine : ROI
	{
		//https://wpf.2000things.com/tag/affectsrender/
		public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register("StartPoint", 
		typeof(Point), typeof(ROILine), new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
			FrameworkPropertyMetadataOptions.AffectsRender, OnStartPointChanged)); //.AD. OnChange added 21FEB2020
		public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register("EndPoint", 
		typeof(Point), typeof(ROILine), new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
			FrameworkPropertyMetadataOptions.AffectsRender, OnEndPointChanged));

		public Point StartPoint{
			get{ return (Point)GetValue(StartPointProperty); }
			set{ SetValue(StartPointProperty, value); }
		}

		public Point EndPoint{
			get{ return (Point)GetValue(EndPointProperty); }
			set{ SetValue(EndPointProperty, value); }
		}

		const int START=0, END=1;
		public ROILine()
		{
			base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));
			base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));

			base.MouseLeftButtonDown += OnLineROIMouseLeftButtonDown;
			base.MouseLeftButtonUp += OnLineROIMouseLeftButtonUp;
			base.MouseMove += OnLineROIMouseMove;
		}

		//Down Move Up:
		private void OnLineROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			CaptureMouse();
			if (base.CurrentState == State.Normal){
				base.CurrentState = State.Selected;
				actPos = prevPos = e.GetPosition(this);
			}
			actElt = e.OriginalSource;
		}

		private void OnLineROIMouseMove(object sender, MouseEventArgs e)
		{
			actPos = e.GetPosition(this);
			Point diff = new Point(prevPos.X - actPos.X, prevPos.Y - actPos.Y);
			prevPos = actPos;

			switch (base.CurrentState) {
				case State.DrawingInProgress:
					EndPoint = actPos;
				break;
				case State.Selected:
					//ToDo: avoid collapse here!
					if (actElt == base.Anchors[START]) StartPoint = new Point(StartPoint.X - diff.X, StartPoint.Y - diff.Y);
					else if (actElt == base.Anchors[END]) EndPoint = new Point(EndPoint.X - diff.X, EndPoint.Y - diff.Y);
					else if (actElt == this){
						StartPoint = new Point(StartPoint.X - diff.X, StartPoint.Y - diff.Y);
						EndPoint = new Point(EndPoint.X - diff.X, EndPoint.Y - diff.Y);
					}
				break;
			}
			UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
		}

		private void OnLineROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification); //ToDo: scale with magnification factor!
			dc.DrawLine(pen, StartPoint, EndPoint);	
		}

		private static void OnStartPointChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			((ROILine)obj).Anchors[START].Position = (Point)e.NewValue;
		}

		private static void OnEndPointChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			((ROILine)obj).Anchors[END].Position = (Point)e.NewValue;
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

		public static readonly DependencyProperty TopLeftProperty = DependencyProperty.Register("TopLeft", typeof(Point), typeof(ROIRect), 
		new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
		FrameworkPropertyMetadataOptions.AffectsRender, OnTopLeftChanged));
		public static readonly DependencyProperty BottomRightProperty = 
		DependencyProperty.Register("BottomRight", typeof(Point), typeof(ROIRect), 
		new FrameworkPropertyMetadata(new Point(0.0, 0.0), 
		FrameworkPropertyMetadataOptions.AffectsRender, OnBottomRightChanged));

		public Point TopLeftPoint{
			get{return (Point)GetValue(TopLeftProperty);}
			set{SetValue(TopLeftProperty, value);}
		}

		public Point BottomRightPoint{
			get{return (Point)GetValue(BottomRightProperty);}
			set{SetValue(BottomRightProperty, value);}
		}

		//Anchors
		const int TOP_LEFT=0, TOP_RIGHT=1, BOTTOM_LEFT=2, BOTTOM_RIGHT=3, CENTER=4;

		public ROIRect()
		{
			//four anchor points and one center point
			for (int i = 0; i < 4; i++) base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));
			base.Anchors.Add(AnchorsFactory.Create(AnchorType.Move, this));
			base.MouseLeftButtonDown += OnRectROIMouseLeftButtonDown;
			base.MouseLeftButtonUp += OnRectROIMouseLeftButtonUp;
			base.MouseMove += OnRectROIMouseMove;
		}
		private void OnRectROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (base.CurrentState == State.Normal){
				actPos = prevPos = e.GetPosition(this);
				base.CurrentState = State.Selected;
			}
			CaptureMouse();
			actElt = e.OriginalSource;
		}

		private void OnRectROIMouseMove(object sender, MouseEventArgs e)
		{
			actPos = e.GetPosition(this);
			Point diff = new Point(prevPos.X - actPos.X, prevPos.Y - actPos.Y);
			prevPos = actPos;

			switch (base.CurrentState) {
				case State.DrawingInProgress:
					BottomRightPoint = e.GetPosition(this);
				break;
				case State.Selected:
					if (actElt == base.Anchors[TOP_LEFT]){ //trivial case
						TopLeftPoint = new Point(TopLeftPoint.X - diff.X, TopLeftPoint.Y - diff.Y);
					}
					else if (actElt == base.Anchors[BOTTOM_RIGHT]){ //trivial case
						BottomRightPoint = new Point(BottomRightPoint.X - diff.X, BottomRightPoint.Y - diff.Y);
					}
					else if (actElt == base.Anchors[TOP_RIGHT]){
						Point newAnchor = new Point(base.Anchors[TOP_RIGHT].Position.X - diff.X, base.Anchors[TOP_RIGHT].Position.Y - diff.Y);
						TopLeftPoint = new Point (TopLeftPoint.X, newAnchor.Y);
						BottomRightPoint = new Point (newAnchor.X, BottomRightPoint.Y);
					}
					else if (actElt == base.Anchors[BOTTOM_LEFT]){
						Point newAnchor = new Point(base.Anchors[BOTTOM_LEFT].Position.X - diff.X, base.Anchors[BOTTOM_LEFT].Position.Y - diff.Y);
						TopLeftPoint = new Point (newAnchor.X, TopLeftPoint.Y);
						BottomRightPoint = new Point (BottomRightPoint.X, newAnchor.Y);
					}
					else if (actElt == this || actElt == base.Anchors[CENTER]){
							TopLeftPoint = new Point(TopLeftPoint.X - diff.X, TopLeftPoint.Y - diff.Y);
							BottomRightPoint = new Point(BottomRightPoint.X - diff.X, BottomRightPoint.Y - diff.Y);	
					}
				break;
			}
			UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
		}

		private void OnRectROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}

		private static void OnTopLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ROIRect obj = (ROIRect)d;
			Point topLeft = (Point)e.NewValue;
			Point topRight = obj.Anchors[TOP_RIGHT].Position;
			topRight.Y = topLeft.Y;
			Point bottomLeft = obj.Anchors[BOTTOM_LEFT].Position;
			bottomLeft.X = topLeft.X;
			Point center = obj.Anchors[CENTER].Position;
			center.X = (topLeft.X + topRight.X) / 2.0;
			center.Y = (topLeft.Y + bottomLeft.Y) / 2.0;
			obj.Anchors[TOP_LEFT].Position = topLeft;
			obj.Anchors[TOP_RIGHT].Position = topRight;
			obj.Anchors[BOTTOM_LEFT].Position = bottomLeft;
			obj.Anchors[CENTER].Position = center;
		}

		private static void OnBottomRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ROIRect obj = (ROIRect)d;
			Point bottomRight = (Point)e.NewValue;
			Point topRight = obj.Anchors[TOP_RIGHT].Position;
			topRight.X = bottomRight.X;
			Point bottomLeft = obj.Anchors[BOTTOM_LEFT].Position;
			bottomLeft.Y = bottomRight.Y;
			Point center = obj.Anchors[CENTER].Position;
			center.X = (bottomRight.X + bottomLeft.X) / 2.0;
			center.Y = (bottomRight.Y + topRight.Y) / 2.0;
			obj.Anchors[TOP_RIGHT].Position = topRight;
			obj.Anchors[BOTTOM_LEFT].Position = bottomLeft;
			obj.Anchors[BOTTOM_RIGHT].Position = bottomRight;
			obj.Anchors[CENTER].Position = center;
		}


		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification);
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

	public class ROIOval : ROI
	{

		public static readonly DependencyProperty RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), 
			typeof(ROIOval), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRadiusXChanged));

		public static readonly DependencyProperty RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), 
			typeof(ROIOval), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRadiusYChanged));

		public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(Point), 
			typeof(ROIOval), new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender, OnCenterChanged));


		public double RadiusX{
			get{return (double)GetValue(RadiusXProperty);}
			set{SetValue(RadiusXProperty, value);}
		}

		public double RadiusY{
			get{return (double)GetValue(RadiusYProperty);}
			set{SetValue(RadiusYProperty, value);}
		}

		public Point Center{
			get{return (Point)GetValue(CenterProperty);}
			set{SetValue(CenterProperty, value);}
		}

		const int LEFT=2, RIGHT=3, TOP=0, BOTTOM=1, CENTER=4;

		public ROIOval()
		{
			µCore.OutputDebugString("create - begin");
			//four anchor points and one center point
			for (int i = 0; i < 4; i++) base.Anchors.Add(AnchorsFactory.Create(AnchorType.Resize, this));
			base.Anchors.Add(AnchorsFactory.Create(AnchorType.Move, this));
			
			base.MouseLeftButtonDown += OnOvalROIMouseLeftButtonDown;
			base.MouseLeftButtonUp += OnOvalROIMouseLeftButtonUp;
			base.MouseMove += OnOvalROIMouseMove;
			µCore.OutputDebugString("create - end");
		}	

		private void OnOvalROIMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (base.CurrentState == State.Normal){
				base.CurrentState = State.Selected;
				actPos = prevPos = e.GetPosition(this);
			}
			CaptureMouse();
			actElt = e.OriginalSource;
		}

		private void OnOvalROIMouseMove(object sender, MouseEventArgs e)
		{
			actPos = e.GetPosition(this);
			Point diff = new Point(prevPos.X - actPos.X, prevPos.Y - actPos.Y);
			prevPos = actPos;

			switch (base.CurrentState) {
				case State.DrawingInProgress:
					RadiusX = Math.Abs(actPos.X - Center.X);
					RadiusY = Math.Abs(actPos.Y - Center.Y);
				break;
				case State.Selected:
					if (actElt == base.Anchors[LEFT] ) RadiusX = RadiusX + diff.X;
					else if (actElt == base.Anchors[RIGHT] ) RadiusX = RadiusX - diff.X;
					else if (actElt == base.Anchors[TOP] ) RadiusY = RadiusY + diff.Y;
					else if (actElt == base.Anchors[BOTTOM] ) RadiusY = RadiusY - diff.Y;
					else if (actElt == base.Anchors[CENTER] || actElt == this){
						Center = new Point(Center.X - diff.X, Center.Y - diff.Y);
					}
				break;
			}
			UpdateLastROIDrawEvent(new ROIDescriptor.LastEventArgs(GetLastDrawEventData()));
		}

		private void OnOvalROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}

		private static void OnRadiusXChanged(DependencyObject d, DependencyPropertyChangedEventArgs r)
		{
			ROIOval obj = (ROIOval)d;
			double radius = (double)r.NewValue;
			Point center = obj.Anchors[CENTER].Position;
			obj.Anchors[LEFT].Position = new Point(center.X - radius, center.Y);
			obj.Anchors[RIGHT].Position = new Point(center.X + radius, center.Y);
		}

		private static void OnRadiusYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ROIOval obj = (ROIOval)d;
			double radius = (double)e.NewValue;
			Point center = obj.Anchors[CENTER].Position;
			obj.Anchors[TOP].Position = new Point(center.X, center.Y - radius);;
			obj.Anchors[BOTTOM].Position = new Point(center.X, center.Y + radius);;
		}

		private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ROIOval roiOval = (ROIOval)d;
			Point center = (Point)e.NewValue;
			roiOval.Anchors[TOP].Position = new Point(center.X, center.Y - roiOval.RadiusY);
			roiOval.Anchors[BOTTOM].Position = new Point(center.X, center.Y + roiOval.RadiusY);
			roiOval.Anchors[LEFT].Position = new Point(center.X - roiOval.RadiusX, center.Y);
			roiOval.Anchors[RIGHT].Position = new Point(center.X + roiOval.RadiusX, center.Y);;
			roiOval.Anchors[CENTER].Position = center;
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0 / base.Magnification); //ToDo: scale with magnification factor!
			dc.DrawEllipse(Brushes.Transparent, pen, Center, RadiusX, RadiusY);
		}

		public override ROIDescriptor.LastEventData GetLastDrawEventData()
		{
			return new ROIDescriptor.LastEventData{
				type = EventType.Draw,
				tool = EventTool.ROI,
				roi = ROItype.Oval,
				coordinates = new List<Point>{
					Center,
				},
				otherParameters = new List<double>{
					RadiusX,
					RadiusY,
				}
			};
		}

		public override ROIDescriptor.Contour GetROIDescriptorContour()
		{
			return new ROIDescriptor.Contour{
				roiType = ROItype.Oval,
				points = new List<Point>{
					Center,
				}
			};
		}

	} //OvalROI

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

		private void SetBinding(string sourcePathName, DependencyObject targetObject, DependencyProperty targetDp)
		{
			Binding binding = new Binding(sourcePathName);
			binding.Source = this;
			BindingOperations.SetBinding(targetObject, targetDp, binding);
		}
			
		private void StartDrawingLineROI()
		{
			ROILine lineROI = new ROILine();
			ROIList.Add(lineROI);	
			lineROI.EndPoint = lineROI.StartPoint = MousePosition;
			lineROI.CaptureMouse();
			lineROI.CurrentState = State.DrawingInProgress;
			lineROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
			SetBinding(MagnificationProperty.Name, lineROI, ROI.MagnificationProperty);

		}

		private void StartDrawingRectROI()
		{
			ROIRect rectROI = new ROIRect();
			ROIList.Add(rectROI);
			rectROI.TopLeftPoint = rectROI.BottomRightPoint = MousePosition;
			rectROI.CaptureMouse();
			rectROI.CurrentState = State.DrawingInProgress;
			rectROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
			SetBinding(MagnificationProperty.Name, rectROI, ROI.MagnificationProperty);
		}

	   private void StartDrawingOvalROI()
		{
			ROIOval ovalROI = new ROIOval();
			ROIList.Add(ovalROI);
			ovalROI.Center = MousePosition;
			ovalROI.CaptureMouse();
			ovalROI.CurrentState = State.DrawingInProgress;
			ovalROI.LastROIDrawEvent += OnGetLastDrawEventUpdated;
			SetBinding(MagnificationProperty.Name, ovalROI, ROI.MagnificationProperty);
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
