using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CsGrafeq
{
    public struct Complex
    {
        public double Re, Im;
        public Complex(double re,double im)
        {
            Re = re; Im = im;
        }
    } 
    public class ComplexMath
    {
        public Complex New(double n)
        {
            return new Complex(n,0);
        }
        public Complex Add(Complex c1, Complex c2)
        {
            return new Complex(c1.Re + c2.Re, c1.Im + c2.Im);
        }
        public Complex Subtract(Complex c1, Complex c2)
        {
            return new Complex(c1.Re - c2.Re, c1.Im * c2.Im);
        }
        public Complex Multiply(Complex c1, Complex c2)
        {
            return new Complex(c1.Re * c2.Re-c1.Im*c2.Im,c1.Re*c2.Im+c1.Im*c2.Re);
        }
        public Complex Divide(Complex c1, Complex c2)
        {
            return DivideNumber(Multiply(c1,Conj(c2)),c1.Re*c1.Re+c2.Re*c2.Re);
        }
        public Complex DivideNumber(Complex c1, double num)
        {
            c1.Re /= num;
            c1.Im /= num;
            return c1;
        }
        public Complex Conj(Complex c1)
        {
            c1.Im = -c1.Im;
            return c1;
        }
    }
}
