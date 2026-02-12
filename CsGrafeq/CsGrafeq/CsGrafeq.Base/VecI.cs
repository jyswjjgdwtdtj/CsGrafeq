namespace CsGrafeq;

public struct VecI : IEquatable<VecI>
{
    public int X;
    public int Y;

    public VecI(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(VecI left, VecI right)
    {
        return left.X.Equals(right.X) && left.Y.Equals(right.Y);
    }

    public static bool operator !=(VecI left, VecI right)
    {
        return !(left == right);
    }

    public static VecI operator +(VecI left, VecI right)
    {
        return new VecI(left.X + right.X, left.Y + right.Y);
    }

    public static VecI operator -(VecI left, VecI right)
    {
        return new VecI(left.X - right.X, left.Y - right.Y);
    }

    /// <summary>
    ///     点乘
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static int operator *(VecI left, VecI right)
    {
        return left.X * right.X + left.Y * right.Y;
    }

    /// <summary>
    ///     叉乘
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static int operator ^(VecI left, VecI right)
    {
        return left.X * right.Y - left.Y * right.X;
    }

    public int GetLength()
    {
        return Math.Abs(X) + Math.Abs(Y);
    }

    public override string ToString()
    {
        return $"{{{X},{Y}}}";
    }

    public static implicit operator VecI((int, int) tTuple)
    {
        return new VecI(tTuple.Item1, tTuple.Item2);
    }

    public bool IsInvalid()
    {
        return double.IsNaN(X) || double.IsNaN(Y);
    }

    /// <summary>
    ///     空向量（XY均为0）
    /// </summary>
    public static readonly VecI Empty = new(0, 0);

    public override bool Equals(object? obj)
    {
        return obj is VecI vecI && Equals(vecI);
    }

    public bool Equals(VecI other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}