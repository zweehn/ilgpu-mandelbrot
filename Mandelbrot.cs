using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ilgputest
{
    public class Mandelbrot: IImageGenerator
    {

        public ImageSource GenerateImage(int width, int height, ComplexDouble centerPoint, double step, uint[] palette, int limit)
        {
            PixelFormat pf = PixelFormats.Bgr32;
            ComplexDouble start = centerPoint + new ComplexDouble(-width * step / 2, -height * step / 2);

            uint[] rawImage = new uint[width * height];
            Parallel.For(0, width, x =>
            {
                Parallel.For(0, height, y =>
                {
                    var pixelValue = IteratePixel(new ComplexDouble(x * step, y * step) + start, limit);

                    rawImage[x + y * width] = ValueToColor(pixelValue, limit, palette);
                });
            });

            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, rawImage, width * pf.BitsPerPixel / 8);

        }

        private int IteratePixel(ComplexDouble c, int limit)
        {
            var z = new ComplexDouble(0, 0);
            for (var i = 0; i < limit; i++)
            {
                z = z * z + c;
                if (z.ModolusSquared > 4)
                    return i;
            }
            return limit;
        }

        private uint ValueToColor(int value, int limit, uint[] palette)
        {
            uint color = 0x00;
            color |= (uint)((double)value / limit * 255);
            color |= (uint)((double)value / limit * 255) << 8;
            color |= (uint)((double)value / limit * 255) << 16;
            return color;
        }
    }
}
