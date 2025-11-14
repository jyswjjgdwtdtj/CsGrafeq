using static System.Math;

namespace CsGrafeq;

/// <summary>
///     总是代表数学空间的向量或点 区别于Avalonia.Point，其总为像素坐标
/// </summary>
public struct Vec : IEquatable<Vec>
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

    /// <summary>
    ///     点乘
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static double operator *(Vec left, Vec right)
    {
        return left.X * right.X + left.Y * right.Y;
    }

    /// <summary>
    ///     叉乘
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     无效向量（XY均为NaN）
    /// </summary>
    public static readonly Vec Invalid = new(double.NaN, double.NaN);

    /// <summary>
    ///     正无穷向量（XY均为正无穷）
    /// </summary>
    public static readonly Vec Infinity = new(double.PositiveInfinity, double.PositiveInfinity);

    /// <summary>
    ///     负无穷向量（XY均为负无穷）
    /// </summary>
    public static readonly Vec NegInfinity = new(double.NegativeInfinity, double.NegativeInfinity);

    /// <summary>
    ///     空向量（XY均为0）
    /// </summary>
    public static readonly Vec Empty = new(0, 0);

    /// <summary>
    ///     极坐标角度，范围[0,2π)
    /// </summary>
    /// <returns></returns>
    public double Arg2()
    {
        return Atan2(Y, X) % (2 * PI);
    }

    /// <summary>
    ///     单位向量
    /// </summary>
    /// <returns></returns>
    public Vec Unit()
    {
        var len = GetLength();
        return new Vec(X / len, Y / len);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vec vec && Equals(vec);
    }

    public bool Equals(Vec other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
}