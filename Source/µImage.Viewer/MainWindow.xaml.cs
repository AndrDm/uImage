using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace µImage.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            µImage.LayoutTransform = (ScaleTransform) (new ScaleTransform());
            µImage.MouseWheel += image_MouseWheel;
        }


        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleTransform obj = (ScaleTransform)µImage.LayoutTransform;

            double zoom = e.Delta > 0 ? .1 : -.1;
            obj.ScaleY = obj.ScaleX = (obj.ScaleX += zoom).LimitToRange(.1, 10);

            BitmapScalingMode mode = obj.ScaleX > 5 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;
            RenderOptions.SetBitmapScalingMode(µImage, mode);
            Title = "µImage.Viewer - Zoom = " + obj.ScaleX;
        }

    }

    public static class InputExtensions
    {
        public static double LimitToRange(this double value, double inclusiveMinimum, double inclusiveMaximum)
        {
            if (value < inclusiveMinimum) { return inclusiveMinimum; }
            if (value > inclusiveMaximum) { return inclusiveMaximum; }
            return value;
        }
    }
}
