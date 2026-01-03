using CsGrafeq.I18N;
using static CsGrafeq.Shapes.GeometryMath;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class CircleGetter : GeometryGetter
{
    public abstract CircleStruct GetCircle();
}

public class CircleGetter_FromThreePoint : CircleGetter
{
    private readonly Point Point1;
    private readonly Point Point2;
    private readonly Point Point3;

    public CircleGetter_FromThreePoint(Point point1, Point point2, Point point3)
    {
        Point1 = point1;
        Point2 = point2;
        Point3 = point3;
        ShapeParameters=[Point1, Point2, Point3];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.CircleText;

    public override CircleStruct GetCircle()
    {
        var x1 = Point1.Location.X;
        var y1 = Point1.Location.Y;
        var x2 = Point2.Location.X;
        var y2 = Point2.Location.Y;
        var x3 = Point3.Location.X;
        var y3 = Point3.Location.Y;
        var c = SolveFunction(
            2 * (x2 - x1),
            2 * (y2 - y1),
            x2 * x2 + y2 * y2 - x1 * x1 - y1 * y1,
            2 * (x3 - x2),
            2 * (y3 - y2),
            x3 * x3 + y3 * y3 - x2 * x2 - y2 * y2);
        return new CircleStruct { Center = c, Radius = (c - Point1.Location).GetLength() };
    }

    public override void Attach(GeometryShape subShape)
    {
        Point1.AddSubShape(subShape);
        Point2.AddSubShape(subShape);
        Point3.AddSubShape(subShape);
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
        Point3.RemoveSubShape(subShape);
    }
}

public class CircleGetter_FromCenterAndRadius : CircleGetter
{
    public CircleGetter_FromCenterAndRadius(Point center)
    {
        Radius = new ExpNumber(1, this);
        Radius.ValueStr = "1";
        Center = center;
        ExpNumbers = [new( Radius,MultiLanguageResources.RadiusText )];
        ShapeParameters = [Center];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.CircleText;

    public Point Center { get; init; }
    public ExpNumber Radius { get; init; }

    public IReadOnlyList<NumberParameter> ExpNumbers { get; init; }

    public override CircleStruct GetCircle()
    {
        return new CircleStruct { Center = Center.Location, Radius = Radius.Value };
    }

    public override void Attach(GeometryShape subShape)
    {
        Center.AddSubShape(subShape);
        ;
        Radius.NumberChanged += subShape.RefreshValues;
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Center.RemoveSubShape(subShape);
        Radius.NumberChanged -= subShape.RefreshValues;
    }
}

public class CircleGetter_FromCenterAndPoint : CircleGetter
{
    private readonly Point Center;
    private readonly Point Point;

    public CircleGetter_FromCenterAndPoint(Point center, Point point)
    {
        Center = center;
        Point = point;
        ShapeParameters = [Center, Point];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.CircleText;

    public override CircleStruct GetCircle()
    {
        return new CircleStruct
            { Center = Center.Location, Radius = ((Vec)Center.Location - Point.Location).GetLength() };
    }

    public override void Attach(GeometryShape subShape)
    {
        Center.AddSubShape(subShape);
        Point.AddSubShape(subShape);
        ;
        ;
    }

    public override void UnAttach(GeometryShape subShape)
    {
        Center.RemoveSubShape(subShape);
        Point.RemoveSubShape(subShape);
    }
}

public class CircleGetter_Apollonius : CircleGetter
{
    private readonly Point PointA;
    private readonly Point PointB;

    public CircleGetter_Apollonius(Point a, Point b)
    {
        PointA = a;
        PointB = b;
        Ratio = new ExpNumber(1, this);
        Ratio.ValueStr = "1";
        NumberParameters = [new(Ratio, MultiLanguageResources.RatioText)];
        ShapeParameters = [PointA, PointB];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.ApolloniusText;

    public ExpNumber Ratio { get; init; }

    public override CircleStruct GetCircle()
    {
        var x1 = PointA.Location.X;
        var y1 = PointA.Location.Y;
        var x2 = PointB.Location.X;
        var y2 = PointB.Location.Y;
        var k = Ratio.Value;

        if (double.IsNaN(k) || k <= 0)
            return new CircleStruct { Center = Vec.Invalid, Radius = double.PositiveInfinity };

        var k2 = k * k;
        var denom = 1 - k2;
        // 当 denom 接近 0 时 (k==1) 退化为直线（非圆）
        if (Math.Abs(denom) < 1e-12)
            return new CircleStruct { Center = Vec.Invalid, Radius = double.PositiveInfinity };

        var cx = (x1 - k2 * x2) / denom;
        var cy = (y1 - k2 * y2) / denom;

        var numerator = x1 * x1 + y1 * y1 - k2 * (x2 * x2 + y2 * y2);
        var rSq = cx * cx + cy * cy - numerator / denom;
        if (double.IsNaN(rSq) || rSq < 0)
            rSq = 0;

        var radius = Math.Sqrt(Math.Max(0, rSq));
        return new CircleStruct { Center = new Vec(cx, cy), Radius = radius };
    }

    public override void Attach(GeometryShape subShape)
    {
        PointA.AddSubShape(subShape);
        PointB.AddSubShape(subShape);
        Ratio.NumberChanged += subShape.RefreshValues;
    }

    public override void UnAttach(GeometryShape subShape)
    {
        PointA.RemoveSubShape(subShape);
        PointB.RemoveSubShape(subShape);
        Ratio.NumberChanged -= subShape.RefreshValues;
    }
}