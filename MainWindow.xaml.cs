using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace ilgputest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MainWindow : Window
    {
        public ImageSource Image => getImage();

        [ObservableProperty]
        double step = 0.005;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Image))]
        int limit = 150;

        [ObservableProperty]
        string rendertime = "";

        [ObservableProperty]
        ComplexDouble centerPoint = new ComplexDouble(0, 0);

        IImageGenerator mandelbrot = new MandelbrotILGPU();

        private bool isProcessing = false;
        private Point? initmousepoint = null;

        private uint[] palette = new uint[] { 0x000000ff, 0x0000ff00, 0x00ffff00, 0x00ffffff };

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            MouseWheel += MainWindow_MouseWheel;
            SizeChanged += MainWindow_SizeChanged;
            MouseDown += MainWindow_MouseDown;
            MouseUp += MainWindow_MouseUp;
            MouseMove += MainWindow_MouseMove;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!initmousepoint.HasValue)
                return;

            var currmousepoint = e.GetPosition(this);
            Vector delta = (Vector)(currmousepoint - initmousepoint!);
            initmousepoint = currmousepoint;

            delta = delta * Step;

            CenterPoint = new ComplexDouble(CenterPoint.r - delta.X, CenterPoint.i - delta.Y);
            OnPropertyChanged(nameof(Image));
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            initmousepoint = null;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            initmousepoint = e.GetPosition(this);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Image));
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var oldStep = Step;
            if (e.Delta > 0)
            {
                Step = Step * 0.8;
            }
            else
            {
                Step = Step * 1.2;
            }

            // Increase Limit while zooming
            Limit = (int)(-Math.Log2(Step)*10);

            var stepchange = Step - oldStep;

            var mousePosition = e.GetPosition(this);
            var x = -((mousePosition.X - Width/2) * stepchange);
            var y = -((mousePosition.Y - Height/2) * stepchange);

            CenterPoint = CenterPoint + new ComplexDouble(x, y);
            
            OnPropertyChanged(nameof(Image));
        }

        private ImageSource getImage()
        {
            if (isProcessing)
                return (ImageSource)Binding.DoNothing;

            isProcessing = true;
            var stopwatch = Stopwatch.StartNew();

            

            var Image = mandelbrot.GenerateImage((int)Width, (int)Height, CenterPoint, Step, palette, Limit);
            //OnPropertyChanged(nameof(Image));
            stopwatch.Stop();

            Rendertime = stopwatch.Elapsed.ToString("fff");
            isProcessing = false;

            return Image;
        }
    }

}
