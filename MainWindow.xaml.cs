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
        [ObservableProperty]
        ImageSource image;

        [ObservableProperty]
        double step = 0.005;

        [ObservableProperty]
        int limit = 15;

        [ObservableProperty]
        string rendertime = "";

        [ObservableProperty]
        ComplexDouble centerPoint = new ComplexDouble(0, 0);

        IImageGenerator mandelbrot = new MandelbrotILGPU();

        private bool isProcessing = false;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Image = mandelbrot.GenerateImage((int)Width, (int)Height, CenterPoint, Step, new int[] { 0x00000000, 0x00ffffff }, Limit);
            MouseWheel += MainWindow_MouseWheel;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isProcessing)
                return;

            isProcessing = true;
            var stopwatch = Stopwatch.StartNew();

            if (e.Delta > 0)
            {
                Step = Step * 0.9;
            }
            else
            {
                Step = Step * 1.1;
            }

            var mousePosition = e.GetPosition(this);
            var x = mousePosition.X - Width / 2;
            var y = mousePosition.Y - Height / 2;

            CenterPoint = CenterPoint + new ComplexDouble(x * Step / 2, y * Step / 2);

            Image = mandelbrot.GenerateImage((int)Width, (int)Height, CenterPoint, Step, new int[] { 0x00000000, 0x00ffffff }, Limit);
            OnPropertyChanged(nameof(Image));
            stopwatch.Stop();

            Rendertime = stopwatch.Elapsed.ToString("fff");
            isProcessing = false;
        }
    }

}
