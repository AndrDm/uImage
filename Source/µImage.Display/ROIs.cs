using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace µ.Display
{
	public enum ROItype
	{
		None,
		Line
	}

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
    }

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
        	if (base.CurrentState == State.DrawingInProgress) EndPoint = e.GetPosition(this);
		}

		private void OnLineROIMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ReleaseMouseCapture();
			base.CurrentState = State.Normal;
		}
		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);
			Pen pen = new Pen(Brushes.Red, 2.0);
			dc.DrawLine(pen, StartPoint, EndPoint);	
		}
    }
    public partial class uImageControl : Control
    {
        private void StartDrawingLineROI()
        {
			ROILine lineROI = new ROILine();
			ROIList.Add(lineROI);	
			lineROI.EndPoint = lineROI.StartPoint = MousePosition;
			lineROI.CaptureMouse();
			lineROI.CurrentState = ROI.State.DrawingInProgress;
        }
    }
}
