using CsGrafeq.I18N;
using static System.Math;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class LineGetter : GeometryGetter
{
    public abstract LineStruct GetLine();
}

public abstract class LineGetter_TwoPoint : LineGetter
{
    protected Point Point1, Point2;

    public LineGetter_TwoPoint(Point p1, Point p2)
    {
        Point1 = p1;
        Point2 = p2;
        ShapeParameters =
        [
            new ShapeParameter(Point1, MultiLanguageResources.StartPointText),
            new ShapeParameter(Point2, MultiLanguageResources.EndPointText)
        ];
    }

    public override void Attach(GeometryShape subShape)
    {
        Point1.AddSubShape(subShape);
        Point2.AddSubShape(subShape);
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
    }
}

public class LineGetter_Connected : LineGetter_TwoPoint
{
    public LineGetter_Connected(Point point1, Point point2) : base(point1, point2)
    {
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.StraightText;

    public override LineStruct GetLine()
    {
        return new LineStruct(Point1.Location, Point2.Location);
    }
}

public sealed class LineGetter_Segment : LineGetter_Connected
{
    public LineGetter_Segment(Point point1, Point point2) : base(point1, point2)
    {
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.SegmentText;
}

public sealed class LineGetter_Half : LineGetter_Connected
{
    public LineGetter_Half(Point point1, Point point2) : base(point1, point2)
    {
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.HalfLineText;
}

/// <summary>
///     中垂线
/// </summary>
public class LineGetter_PerpendicularBisector : LineGetter_TwoPoint
{
    public LineGetter_PerpendicularBisector(Point p1, Point p2) : base(p1, p2)
    {
        ShapeParameters = [new ShapeParameter(Point1), new ShapeParameter(Point2)];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.PerpendicularBisectorText;

    public override LineStruct GetLine()
    {
        var RealPoint1 = (Vec)Point1.Location;
        var RealPoint2 = (Vec)Point2.Location;
        var MiddlePoint = (RealPoint1 + RealPoint2) / 2;
        var p1 = MiddlePoint;
        Vec p2;
        var k = (RealPoint1.Y - RealPoint2.Y) / (RealPoint1.X - RealPoint2.X);
        var theta = Atan2(-1 / k, 1);
        if (RealPoint1.Y - RealPoint2.Y > 0)
            p2 = new Vec(p1.X + Cos(theta), p1.Y - Cos(theta) / k);
        else
            p2 = new Vec(p1.X - Cos(theta), p1.Y + Cos(theta) / k);
        return new LineStruct(p1, p2);
    }
}

public class LineGetter_AngleBisector : LineGetter
{
    public Point Point1, Point2, AnglePoint;

    public LineGetter_AngleBisector(Point p1, Point p2, Point anglePoint)
    {
        Point1 = p1;
        Point2 = p2;
        AnglePoint = anglePoint;
        ShapeParameters = [Point1, Point2, new ShapeParameter(AnglePoint, MultiLanguageResources.AngleText)];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.AngleBisectorText;

    public override void Attach(GeometryShape subShape)
    {
        Point1.AddSubShape(subShape);
        Point2.AddSubShape(subShape);
        AnglePoint.AddSubShape(subShape);
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
        AnglePoint.RemoveSubShape(subShape);
    }

    public override LineStruct GetLine()
    {
        var p1 = (Vec)Point1.Location;
        var p2 = (Vec)Point2.Location;
        var ap = (Vec)AnglePoint.Location;
        Vec v1, v2;
        v1 = ap;
        p1 -= ap;
        p2 -= ap;
        var theta = (p1.Unit() + p2.Unit()).Arg2();
        if (theta == PI / 2) //90
            v2 = new Vec(ap.X, ap.Y + 1);
        else if (theta == PI / 2 * 3) //270
            v2 = new Vec(ap.X, ap.Y - 1);
        else
            v2 = new Vec(ap.X - Cos(theta), ap.Y - Sin(theta));
        return new LineStruct(v1, v2);
    }
}

public abstract class LineGetter_PointAndLine : LineGetter
{
    public LineGetter_PointAndLine(Point point, Line line)
    {
        Line = line;
        Point = point;
        ShapeParameters = [point, line];
    }

    public Line Line { get; init; }
    public Point Point { get; init; }

    public override void Attach(GeometryShape subShape)
    {
        Line.AddSubShape(subShape);
        Point.AddSubShape(subShape);
        ;
        ;
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Line.RemoveSubShape(subShape);
        Point.RemoveSubShape(subShape);
    }
}

public class LineGetter_Vertical : LineGetter_PointAndLine
{
    public LineGetter_Vertical(Point point, Line line) : base(point, line)
    {
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.VerticalLineText;

    public override LineStruct GetLine()
    {
        var v1 = Point.Location;
        Vec v2;
        var ps = Line.Current;
        var k = (ps.Point1.Y - ps.Point2.Y) / (ps.Point1.X - ps.Point2.X);
        var theta = Atan2(-1 / k, 1);
        if (ps.Point1.Y - ps.Point2.Y > 0)
            v2 = new Vec(v1.X + Cos(theta), v1.Y - Cos(theta) / k);
        else
            v2 = new Vec(v1.X - Cos(theta), v1.Y + Cos(theta) / k);
        return new LineStruct(v1, v2);
    }
}

public class LineGetter_Parallel : LineGetter_PointAndLine
{
    public LineGetter_Parallel(Point point, Line line) : base(point, line)
    {
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.Instance.ParallelLineText;

    public override LineStruct GetLine()
    {
        var v1 = Point.Location;
        Vec v2;
        var ps = Line.Current;
        var k = (ps.Point1.Y - ps.Point2.Y) / (ps.Point1.X - ps.Point2.X);
        double theta;
        if (ps.Point1.X == ps.Point2.X)
            theta = PI / 2;
        else theta = Atan2(-1 / k, 1);
        if (ps.Point1.Y - ps.Point2.Y > 0)
            v2 = new Vec(v1.X + Cos(theta), v1.Y + Cos(theta) * k);
        else
            v2 = new Vec(v1.X - Cos(theta), v1.Y - Cos(theta) * k);
        return new LineStruct(v1, v2);
    }
}

public class LineGetter_Fitted : LineGetter
{
    public LineGetter_Fitted(Point[] points)
    {
        Points = [..points];
        ShapeParameters = [..points];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.FittedText;
    public Point[] Points { get; init; }

    public override LineStruct GetLine()
    {
        double meanX = 0, meanY = 0;
        var Vecs = new Vec[Points.Length];
        for (var i = 0; i < Points.Length; i++)
        {
            Vecs[i] = Points[i].Location;
            meanX += Vecs[i].X;
            meanY += Vecs[i].Y;
        }

        meanX /= Vecs.Length;
        meanY /= Vecs.Length;
        double a = 0, c = 0;
        for (var i = 0; i < Points.Length; i++)
        {
            var x = Vecs[i].X;
            var y = Vecs[i].Y;
            a += (x - meanX) * (y - meanY);
            c += (x - meanX) * (x - meanX);
        }

        var m = a / c;
        var b = meanY - m * meanX;
        Vec Point1, Point2;
        if (c == 0) //x的常值函数
        {
            Point1 = new Vec(meanX, 1);
            Point2 = new Vec(meanX, 2);
        }
        else
        {
            Point1 = new Vec(1, m + b);
            Point2 = new Vec(2, 2 * m + b);
        }

        return new LineStruct(Point1, Point2);
    }

    public override void Attach(GeometryShape subShape)
    {
        foreach (var i in Points) i.AddSubShape(subShape);
    }

    public override void UnAttach(GeometryShape subShape)
    {
        foreach (var i in Points) i.RemoveSubShape(subShape);
    }
}