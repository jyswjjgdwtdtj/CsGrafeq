using CsGrafeq.Interfaces;
using CsGrafeq.Numeric.Exact;
using static CsGrafeq.Numeric.CsGrafeqMath;

namespace CsGrafeq.Shapes;

public static class GeometryMath
{
    public static Result<(T, T)> TryGetValidVec<T,TValue>(T v1, T v2, T v3, T v4) where T:IPoint<TValue>
    {
        var vs = new [] { v1, v2, v3, v4 };
        var vs2 = new T[4];
        var i = 0;
        foreach (var v in vs)
        {
            if (v.IsInvalid())
                continue;
            vs2[i] = v;
            i++;
        }

        if (i == 2)
            return Result<(T, T)>.Success((vs2[0], vs2[1]));
        return Result<(T, T)>.Failure("");
    }

    public static VectorExact SolveFunction(ExactNumber a, ExactNumber b, ExactNumber c, ExactNumber d, ExactNumber e, ExactNumber f)
    {
        var det = a * e - b * d;
        if (det == 0)
            return VectorExact.NaN;
        var mat = new ExactNumber[2, 2];
        mat[0, 0] = e / det;
        mat[0, 1] = -b / det;
        mat[1, 0] = -d / det;
        mat[1, 1] = a / det;
        var x = mat[0, 0] * c + mat[0, 1] * f;
        var y = mat[1, 0] * c + mat[1, 1] * f;
        return new VectorExact(x, y);
    }


    /// <summary>
    ///     ss,se为线段 s,e为直线
    /// </summary>
    public static VectorExact GetIntersectionOfSegmentAndLine(VectorExact segmentStart, VectorExact segmentEnd, VectorExact lineStart, VectorExact lineEnd)
    {
        var j = IntersectionMath.FromTwoLine(segmentStart, segmentEnd, lineStart, lineEnd);
        if (RangeIn(segmentStart.X, segmentEnd.X, j.X) && RangeIn(segmentStart.Y, segmentEnd.Y, j.Y)) return j;
        return VectorExact.NaN;
    }

    public static double DistanceToLine(Vec v1, Vec v2, Vec test, out Vec onPoint)
    {
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((test.X - v1.X) * dx + (test.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        onPoint = new Vec(v1.X + t * dx, v1.Y + t * dy);
        return (onPoint - test).GetLength();
    }

    public static bool FuzzyOnSegment(Vec v1, Vec v2, Vec test)
    {
        if (v1.X == v2.X)
            return RangeIn(v1.Y, v2.Y, test.Y);
        return RangeIn(v1.X, v2.X, test.X);
    }

    public static bool FuzzyOnHalf(Vec v1, Vec v2, Vec test)
    {
        if (v1.X == v2.X)
            return Sgn(v2.Y - v1.Y) == Sgn(test.Y - v1.Y);
        return Sgn(v2.X - v1.X) == Sgn(test.X - v1.X);
    }

    public static bool FuzzyOnStraight(Vec v1, Vec v2, Vec test)
    {
        return true;
    }
}