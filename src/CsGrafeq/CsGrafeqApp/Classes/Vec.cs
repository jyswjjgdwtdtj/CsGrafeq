using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SkiaSharp;

namespace CsGrafeqApp.Classes
{
    public struct Vec
    {
        public double X;
        public double Y;
        public Vec(double x, double y)
        {
            X = x; Y = y;
        }
        public static bool operator ==(Vec left, Vec right)
        {
            return left.X.Equals(right.X) && left.Y.Equals(right.Y);
        }
        public static bool operator !=(Vec left, Vec right)
        {
            return !(left == right);
        }
        public static Vec operator +(Vec left, Vec right)
        {
            return new Vec(left.X + right.X, left.Y + right.Y);
        }
        public static Vec operator -(Vec left, Vec right)
        {
            return new Vec(left.X - right.X, left.Y - right.Y);
        }
        public static Vec operator *(Vec left, double ratio)
        {
            return new Vec(left.X * ratio, left.Y * ratio);
        }
        public static Vec operator /(Vec left, double ratio)
        {
            return new Vec(left.X / ratio, left.Y / ratio);
        }
        public static double operator *(Vec left, Vec right)
        {
            return left.X * right.X + left.Y * right.Y;
        }
        public static double operator ^(Vec left, Vec right)
        {
            return left.X * right.Y- left.Y * right.X;
        }
        public double GetLength()
        {
            return Math.Sqrt(X * X + Y * Y);
        }
        public override string ToString()
        {
            return $"{{{X},{Y}}}";
        }
        public static implicit operator Vec((double, double) TTuple)
        {
            return new Vec(TTuple.Item1, TTuple.Item2);
        }
        public bool IsInvalid()
        {
            return double.IsNaN(X) || double.IsNaN(Y);
        }
        public static readonly Vec Invalid = new Vec(double.NaN, double.NaN);
        public static readonly Vec Infinity = new Vec(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly Vec NegInfinity = new Vec(double.NegativeInfinity, double.NegativeInfinity);
        public static readonly Vec Empty = new Vec(0,0);
        public double Arg2()
        {
            return Math.Atan2(Y, X) % (2 * Math.PI);
        }
        public Vec Unit()
        {
            double arg = Arg2();
            return new Vec(Math.Cos(arg), Math.Sin(arg));
        }
        public Avalonia.Point ToAvaPoint()
        {
            return new Avalonia.Point(X, Y);
        }
        public SKPoint ToSKPoint() => new SKPoint((float)X,(float)Y);
    }
}
