using static System.Math;
using static CsGrafeq.Math;

namespace CsGrafeq.Shapes;

public static class GeometryMath
{

    public static (Vec, Vec) GetValidVec(Vec v1, Vec v2, Vec v3, Vec v4)
    {
        var vs = new Vec[4] { v1, v2, v3, v4 };
        var vs2 = new Vec[4];
        var i = 0;
        foreach (var v in vs)
        {
            if (v.IsInvalid())
                continue;
            vs2[i] = v;
            i++;
        }

        if (i == 2)
            return (vs2[0], vs2[1]);
        if (i == 0)
            return (Vec.Invalid, Vec.Invalid);
        return (vs2[0], vs2[2]);
    }

    public static Vec SolveFunction(double a, double b, double c, double d, double e, double f)
    {
        var det = a * e - b * d;
        if (det == 0)
            return Vec.Invalid;
        var mat = new double[2, 2];
        mat[0, 0] = e / det;
        mat[0, 1] = -b / det;
        mat[1, 0] = -d / det;
        mat[1, 1] = a / det;
        var x = mat[0, 0] * c + mat[0, 1] * f;
        var y = mat[1, 0] * c + mat[1, 1] * f;
        return new Vec(x, y);
    }

    

    /// <summary>
    ///     ss,se为线段 s,e为直线
    /// </summary>
    public static Vec GetIntersectionOfSegmentAndLine(Vec segmentStart, Vec segmentEnd, Vec lineStart, Vec lineEnd)
    {
        var j = IntersectionMath.FromTwoLine(segmentStart, segmentEnd, lineStart, lineEnd);
        if (RangeIn(segmentStart.X, segmentEnd.X, j.X) && RangeIn(segmentStart.Y, segmentEnd.Y, j.Y)) return j;
        return new Vec(double.NaN, double.NaN);
    }

    public static Vec GetIntersectionOfTwoSegments(Vec s1, Vec e1, Vec s2, Vec e2)
    {
        var j = IntersectionMath.FromTwoLine(s1, e1, s2, e2);
        if (RangeIn(s1.X, e1.X, j.X) &&
            RangeIn(s1.Y, e1.Y, j.Y) &&
            RangeIn(s2.X, e2.X, j.X) &&
            RangeIn(s2.Y, e2.Y, j.Y))
            return j;
        return new Vec(double.NaN, double.NaN);
    }

    public static double DistanceToLine(Vec v1, Vec v2, Vec test, out Vec OnPoint)
    {
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((test.X - v1.X) * dx + (test.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        OnPoint = new Vec(v1.X + t * dx, v1.Y + t * dy);
        return (OnPoint - test).GetLength();
    }

    public static double DistanceToSegment(Vec v1, Vec v2, Vec test, out Vec OnPoint)
    {
        var res = DistanceToLine(v1, v2, test, out OnPoint);
        return FuzzyOnSegment(v1, v2, OnPoint) ? res : double.PositiveInfinity;
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