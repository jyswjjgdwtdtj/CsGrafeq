using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CsGrafeqApp
{
    internal static class InternalMath
    {
        internal static double Sgn(double num)
        {
            return double.IsNaN(num) ? num : Math.Sign(num);
        }
        internal static bool InRange(double num1, double num2, double numtest)
        {
            SwapIfNotLess(ref num1, ref num2);
            return num1 <= numtest && numtest <= num2;
        }
        internal static double Mod(this double num, double m)
        {
            return num - m * Math.Floor(num / m);
        }
        internal static void SwapIfNotLess(ref double num1, ref double num2)
        {
            if (num1 > num2)
            {
                double num3 = num1;
                num1 = num2;
                num2 = num3;
            }
        }
        public static float Round(float num, int fix)
        {
            return (float)System.Math.Round(num, fix);
        }
        public static double Round(double num, int fix)
        {
            if (fix < 0)
            {
                num /= System.Math.Pow(10, -fix);
                num = System.Math.Round(num, 0);
                num *= System.Math.Pow(10, -fix);
                return num;
            }
            return System.Math.Round(num, fix);
        }
        public static decimal Round(decimal num, int fix)
        {
            num /= Pow(10, -fix);
            num = System.Math.Round(num, 0);
            num *= Pow(10, -fix);
            return num;
        }
        private static decimal Pow(decimal num, int times)
        {
            decimal result = 1;
            if (times > 0)
            {
                for (int i = 0; i < times; i++)
                {
                    result *= num;
                }
                return result;
            }
            else if (times == 0)
            {
                return 1;
            }
            else
            {
                for (int i = 0; i > times; i--)
                {
                    result /= num;
                }
                return result;
            }
        }
        public static T RangeTo<T>(T min, T max, T target) where T : IComparable<T>
        {
            if (target.CompareTo(min) < 0)
                return min;
            if (target.CompareTo(max) > 0)
                return max;
            return target;
        }
    }
    public struct PointL
    {
        public long X;
        public long Y;
        public PointL(long x, long y)
        {
            X = x; Y = y;
        }
        public static bool operator ==(PointL left, PointL right)
        {
            return left.X == right.X && left.Y == right.Y;
        }
        public static bool operator !=(PointL left, PointL right)
        {
            return !(left == right);
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return $"{{{X},{Y}}}";
        }
    }
    internal static class ExtensionMethods
    {
        public static SKRect ToSKRect(this Avalonia.Rect rect)
        {
            return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }
        public static void WriteLine(params object[] lines)
        {
            Console.WriteLine(string.Join(' ',lines));
        }
        public static string IfNullRetEmpty(this string? s) => s is null ? string.Empty : s;
    }
}
