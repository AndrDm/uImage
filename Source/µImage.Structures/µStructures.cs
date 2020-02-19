using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Linq;


namespace µ.Structures 
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

public enum ROItype{
		None,
		Line,
		Rectangle
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

}

