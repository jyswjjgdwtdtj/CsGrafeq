using static System.Math;

namespace CsGrafeq;

public struct Vec
{
    public double X;
    public double Y;

    public Vec(double x, double y)
    {
        X = x;
        Y = y;
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
        return left.X * right.Y - left.Y * right.X;
    }

    public double GetLength()
    {
        return Sqrt(X * X + Y * Y);
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

    public static readonly Vec Invalid = new(double.NaN, double.NaN);
    public static readonly Vec Infinity = new(double.PositiveInfinity, double.PositiveInfinity);
    public static readonly Vec NegInfinity = new(double.NegativeInfinity, double.NegativeInfinity);
    public static readonly Vec Empty = new(0, 0);

    public double Arg2()
    {
        return Atan2(Y, X) % (2 * PI);
    }

    public Vec Unit()
    {
        var len=GetLength();
        return new Vec(X/len, Y/len);
    }
}