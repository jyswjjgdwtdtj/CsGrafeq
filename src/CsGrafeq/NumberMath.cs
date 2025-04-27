using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public static class NumberMath
    {
        public static double New(double num)
        {
            return num;
        }
        public static double Add(double n1, double n2)
        {
            return n1 + n2;
        }
        public static double Subtract(double n1, double n2)
        {
            return n1 - n2;
        }
        public static double Multiply(double n1, double n2)
        {
            return n1 * n2;
        }
        public static double Divide(double n1, double n2)
        {
            return n1 / n2;
        }
        public static double Pow(double n1,double n2)
        {
            return Math.Pow(n1,n2);
        }
        public static double Sin(double n)
        {
            return Math.Sin(n);
        }
        public static double Cos(double n)
        {
            return Math.Cos(n);
        }
        public static double Tan(double n)
        {
            return Math.Tan(n);
        }
        public static double Exp(double n)
        {
            return Math.Exp(n);
        }
        public static double Ln(double n)
        {
            return Math.Log(n);
        }
        public static double Log(double n,double n1)
        {
            return Math.Log(n,n1);
        }
        public static double Lg(double n)
        {
            return Math.Log10(n);
        }
        public static double Arccos(double n)
        {
            return Math.Acos(n);
        }
        public static double Arcsin(double n)
        {
            return Math.Asin(n);
        }
        public static double Arctan(double n)
        {
            return Math.Atan(n);
        }
        public static double Sinh(double n)
        {
            return Math.Sinh(n);
        }
        public static double Cosh(double n)
        {
            return Math.Cosh(n);
        }
        public static double Tanh(double n)
        {
            return Math.Tanh(n);
        }
        public static double Sqrt(double n)
        {
            return Math.Sqrt(n);
        }
        public static double Min(double n,double n2)
        {
            return Math.Min(n,n2);
        }
        public static double Max(double n,double n2)
        {
            return Math.Max(n, n2);
        }
        public static double Floor(double n)
        {
            return Math.Floor(n);
        }
        public static double Ceil(double n)
        {
            return Math.Ceiling(n);
        }
        public static double Abs(double n)
        {
            return Math.Abs(n);
        }
        public static double Neg(double n)
        {
            return -n;
        }
        public static double Mod(double a,double b)
        {
            return a - Math.Floor(a / b) * b;
        }
        public static double Median(double n1,double n2,double n3)
        {
            double[] arr = new double[3] { n1,n2,n3 };
            Array.Sort(arr);
            return arr[1];
        }
        public static double LCM(double da, double db)
        {
            return da * db/GCD(da,db);
        }

        public static double GCD(double da, double db)
        {
            int b = (int)db;
            int a= (int)da;
            if (b != db || a != da)
                throw new Exception(a+" "+b);
            while (b != 0)
            {
                int tmp = a;
                a = b;
                b = tmp % b;
            }
            return a;
        }
        public static double Cot(double n)
        {
            return Math.Tan(Math.PI/2-n);
        }
        public static double Sgn(double n1)
        {
            return Math.Sign(n1);
        }
        public static double Root(double n1,double n2)
        {
            return Math.Pow(n1, 1 / n2);
        }
        public static double Factorial(double num1)
        {
            if(num1!=(int)num1)
                throw new Exception();
            int num = (int)num1;
            if (num < 0)
                return double.NaN;
            if (num >= FactorialValue.Values.Length)
                return double.MaxValue;
            return FactorialValue.Values[num];
        }
        public static int Equal(double x, double y)
        {
            if (x == y)
                return 0;
            return (x > y) ? 10 : 1;
        }
        public static int Less(double x, double y)
        {
            return Equal(x, y);
        }
        public static int Greater(double x, double y)
        {
            return Equal(x, y);
        }
        public static int LessEqual(double x, double y)
        {
            return Equal(x, y);
        }
        public static int GreaterEqual(double x, double y)
        {
            return Equal(x, y);
        }
        public static bool IsCrossZero(int n1, int n2, int n3, int n4)
        {
            int a = n1 + n2 + n3 + n4;
            return !(a>0&&(a<5||a%10==0));
        }
        public static bool IsAllGeThanZero(int n1, int n2, int n3, int n4)
        {
            return (n1 + n2 + n3 + n4) % 10 == 0;
        }
        public static bool IsAllLeThanZero(int n1, int n2, int n3, int n4)
        {
            return (n1 + n2 + n3 + n4 < 5);
        }
    }
}
