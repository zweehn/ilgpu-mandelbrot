using System.Windows.Media;

namespace ilgputest
{
    public interface IImageGenerator
    {
        public ImageSource GenerateImage(int width, int height, ComplexDouble centerPoint, double step, int[] palette, int limit);
    }
}