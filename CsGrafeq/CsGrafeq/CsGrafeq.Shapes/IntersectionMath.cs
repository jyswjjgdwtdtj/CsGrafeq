using static System.Math;
using static CsGrafeq.Utilities.ThrowHelper;

namespace CsGrafeq.Shapes;

public static class IntersectionMath
{
    public static uint FromTwoShape<TShape1, TShape2>(TShape1 shape1, TShape2 shape2, out (Vec, Vec) vs)
        where TShape1 : GeometryShape
        where TShape2 : GeometryShape
    {
        vs = (Vec.Invalid, Vec.Invalid);
        GeometryShape[] ss = [shape1, shape2];
        ss.Sort((s1, s2) =>
        {
            var i1 = GetIndex(s1);
            var i2 = GetIndex(s2);
            if (i1 < i2)
                return -1;
            if (i1 == i2)
                return 0;
            return 1;
        });
        if (ss[0] is Point)
            return 0;
        if (ss[0] is Line l1)
            if (ss[1] is Line l2)
            {
                vs.Item1 = FromTwoLine(l1.Current, l2.Current);
                return 1;
            }
            else if (ss[1] is Circle c1)
            {
                vs = FromLineAndCircle(l1.Current, c1.InnerCircle);
                return 1;
            }
            else
            {
                return 0;
            }

        if (ss[0] is Circle circle1 && ss[1] is Circle circle2)
        {
            vs = FromTwoCircle(circle1.InnerCircle, circle2.InnerCircle);
            return 2;
        }

        return 0;
    }

    public static (Vec v1, Vec v2) FromTwoCircle(CircleStruct shape1, CircleStruct shape2)
    {
        var c1 = shape1.Center;
        var c2 = shape2.Center;
        var r1 = shape1.Radius;
        var r2 = shape2.Radius;

        var dx = c2.X - c1.X;
        var dy = c2.Y - c1.Y;
        var dis2 = dx * dx + dy * dy;
        if (dis2 > Pow(r1 + r2, 2) || dis2 < Pow(r1 - r2, 2))
            return (Vec.Invalid, Vec.Invalid);
        var t = Atan2(dy, dx);
        var a = Acos((r1 * r1 - r2 * r2 + dis2) / (2 * r1 * Sqrt(dis2)));
        var v3 = new Vec(c1.X + r1 * Cos(t + a), c1.Y + r1 * Sin(t + a));
        var v4 = new Vec(c1.X + r1 * Cos(t - a), c1.Y + r1 * Sin(t - a));
        return (v3, v4);
    }

    public static (Vec v1, Vec v2) FromLineAndCircle(LineStruct line, CircleStruct circle)
    {
        var v1 = line.Point1;
        var v2 = line.Point2;
        var cp = circle.Center;
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((cp.X - v1.X) * dx + (cp.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        var nv = new Vec(v1.X + t * dx, v1.Y + t * dy);
        var m = new Vec(dx, dy).Unit() *
                Sqrt(circle.Radius * circle.Radius - Pow((cp - nv).GetLength(), 2));
        v1 = nv - m;
        v2 = nv + m;
        return (v1, v2);
    }

    public static Vec FromTwoLine(LineStruct line1, LineStruct line2)
    {
        return FromTwoLine(line1.Point1, line1.Point2, line2.Point1, line2.Point2);
    }

    public static Vec FromTwoLine(Vec s1, Vec e1, Vec s2, Vec e2)
    {
        double k1, k2;
        k1 = (s1.Y - e1.Y) / (s1.X - e1.X);
        k2 = (s2.Y - e2.Y) / (s2.X - e2.X);
        if (k1 == k2)
            return Vec.Invalid;
        if (s1.X == e1.X) return new Vec(s1.X, k2 * s1.X - k2 * s2.X + s2.Y);
        if (s2.X == e2.X) return new Vec(s2.X, k1 * s2.X - k1 * s1.X + s1.Y);
        var x = (k1 * s1.X - s1.Y + s2.Y - k2 * s2.X) / (k1 - k2);
        return new Vec(x, k1 * x - k1 * s1.X + s1.Y);
    }

    private static int GetIndex(GeometryShape s)
    {
        switch (s)
        {
            case Point _:
                return 0;
            case Line _:
                return 5;
            case Circle _:
                return 10;
            case Polygon _:
                return 15;
            default:
                Throw("");
                return 20;
        }
    }
}