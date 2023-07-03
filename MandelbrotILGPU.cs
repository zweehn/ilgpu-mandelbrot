using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ILGPU;
using ILGPU.Runtime;
using Windows.ApplicationModel.Background;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Streams;

namespace ilgputest
{
    public class MandelbrotILGPU : IDisposable, IImageGenerator
    {
        private bool disposedValue;

        Context context;
        Accelerator accelerator;
        private Action<Index1D, ComplexDouble, int, int, double, ArrayView<int>, int, ArrayView<uint>> _GenerateImage;

        public MandelbrotILGPU()
        {
            // Builds a context with only OpenCL and Cuda acclerators.
            context = Context.CreateDefault();
            accelerator = context.GetPreferredDevice(false).CreateAccelerator(context);
            StringWriter infoString = new StringWriter();
            accelerator.Device.PrintInformation(infoString);
            Console.WriteLine(infoString.ToString());

            // load / precompile the kernel
            _GenerateImage =
                accelerator.LoadAutoGroupedStreamKernel<Index1D, ComplexDouble, int, int, double, ArrayView<int>, int, ArrayView<uint>>(Kernel);

        }



        public ImageSource GenerateImage(int width, int height, ComplexDouble centerPoint, double step, int[] palette, int limit)
        {
            PixelFormat pf = PixelFormats.Bgr32;


            var rawImage = new uint[width * height];
            using var dev_out = accelerator.Allocate1D<uint>(rawImage.Length);

            using var palette_buf = accelerator.Allocate1D<int>(palette.Length);
            palette_buf.CopyFromCPU(palette);

            _GenerateImage(width * height, centerPoint, width, height, step, palette_buf.View, limit, dev_out.View);

            accelerator.Synchronize();

            dev_out.CopyToCPU(rawImage);
            return BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, rawImage, width * pf.BitsPerPixel / 8);
        }

        static void Kernel(Index1D position, ComplexDouble centerPoint,int height,int width, double step, ArrayView<int> palette, int limit, ArrayView<uint> output)
        {
            int x = position % width;
            int y = position / width;

            ComplexDouble start = centerPoint + new ComplexDouble(-width * step / 2, -height * step / 2);

            var pixelValue = IteratePixel(new ComplexDouble(x * step, y * step) + start, limit);

            output[x + y * width] = ValueToColor(pixelValue, limit, palette);

        }

        private static int IteratePixel(ComplexDouble c, int limit)
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

        private static uint ValueToColor(int value, int limit, ArrayView<int> palette)
        {
            uint color = 0x00;
            color |= (uint)((double)value / limit * 255);
            //color |= (uint)((double)value / limit * 255) << 8;
            color |= (uint)((double)value / limit * 255) << 16;
            return color;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                accelerator.Dispose();
                context.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MandelbrotILGPU()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
