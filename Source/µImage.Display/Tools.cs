using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace µ.Display
{
	public enum Tool
	{
		None,
		Pan,
		ROILine,
	}

    public partial class uImageControl : Control
    {
		public static DependencyProperty SelectedToolProperty;

    	public Tool SelectedTool
		{
			get{ return (Tool)GetValue(SelectedToolProperty); }
			set{ SetValue(SelectedToolProperty, value); }
		}

		private static void ToolRegisterProperty()
		{
			SelectedToolProperty = DependencyProperty.Register("SelectedTool", typeof(Tool), typeof(uImageControl), 
            new PropertyMetadata(Tool.None, OnSelectedToolChanged));
        }
		private static void OnSelectedToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            //MessageBox.Show("Tool changed");
		}
    }
}


