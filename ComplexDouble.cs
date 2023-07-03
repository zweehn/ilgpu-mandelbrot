using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ilgputest
{
    public struct ComplexDouble
    {
        public double r;
        public double i;
        
        public ComplexDouble(double real, double imaginary) {
            r = real;
            i = imaginary;
        }

        public static ComplexDouble operator +(ComplexDouble x, ComplexDouble y)
            => new ComplexDouble(x.r + y.r, x.i + y.i);

        public static ComplexDouble operator -(ComplexDouble x, ComplexDouble y)
            => new ComplexDouble(x.r - y.r, x.i - y.i);

        public static ComplexDouble operator *(ComplexDouble x, ComplexDouble y)
            => new ComplexDouble(x.r * y.r - x.i * y.i, x.r * y.i + x.i * y.r);

        public double ModolusSquared => r * r + i * i;
        public double Modolus => Math.Sqrt(ModolusSquared);
    }
}
