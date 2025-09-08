global using PointL = CsGrafeq.PointBase<long>;
global using PointI = CsGrafeq.PointBase<int>;
global using PointF = CsGrafeq.PointBase<float>;
global using PointD = CsGrafeq.PointBase<decimal>;
using System.Numerics;

namespace CsGrafeq;

public struct PointBase<T> where T : INumber<T>
{
    public T X;
    public T Y;

    public PointBase(T x, T y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(PointBase<T> left, PointBase<T> right)
    {
        return left.X.Equals(right.X) && left.Y.Equals(right.Y);
    }

    public static bool operator !=(PointBase<T> left, PointBase<T> right)
    {
        return !(left == right);
    }

    public static PointBase<T> operator +(PointBase<T> left, PointBase<T> right)
    {
        return new PointBase<T>(left.X + right.X, left.Y + right.Y);
    }

    public static PointBase<T> operator -(PointBase<T> left, PointBase<T> right)
    {
        return new PointBase<T>(left.X - right.X, left.Y - right.Y);
    }

    public static PointBase<T> operator *(PointBase<T> left, T ratio)
    {
        return new PointBase<T>(left.X * ratio, left.Y * ratio);
    }

    public static PointBase<T> operator /(PointBase<T> left, T ratio)
    {
        return new PointBase<T>(left.X / ratio, left.Y / ratio);
    }

    public static T operator *(PointBase<T> left, PointBase<T> right)
    {
        return left.X * right.X + left.Y * right.Y;
    }

    public static T operator ^(PointBase<T> left, PointBase<T> right)
    {
        return left.X * right.Y - left.Y * right.X;
    }

    public override string ToString()
    {
        return $"{{{X},{Y}}}";
    }

    /// <summary>
    ///     曼哈顿距离
    /// </summary>
    public T Length => T.Abs(X) + T.Abs(Y);

    public static implicit operator PointBase<T>((T, T) TTuple)
    {
        return new PointBase<T>(TTuple.Item1, TTuple.Item2);
    }

    public static readonly PointBase<T> Empty = new(T.Zero, T.Zero);
}