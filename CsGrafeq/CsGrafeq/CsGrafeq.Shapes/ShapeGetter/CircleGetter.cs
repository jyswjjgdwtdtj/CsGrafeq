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
    }

    public override string ActionName => "Circle";
    public override GeometryShape[] Parameters => [Point1, Point2, Point3];

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
    }

    public Point Center { get; init; }
    public ExpNumber Radius { get; init; }

    public override string ActionName => "Circle";
    public override GeometryShape[] Parameters => [Center];

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
    }

    public override string ActionName => "Circle";
    public override GeometryShape[] Parameters => [Center, Point];

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
/*public class CircleGetter_FromCenterAndDistance : CircleGetter
{
    Point Center;
    NumberGetter Distance;
    public CircleGetter_FromCenterAndDistance(Point center, NumberGetter NumberGetter)
    {
        Center = center;
        Distance= NumberGetter;
    }
    public override CircleStruct GetCircle()
    {
        return new CircleStruct { Center = Center.Location, Radius = Distance.GetNumber() };
    }
    public override void Attach(GeometryShape subShape)
    {
        Center.AddSubShape(subShape);
        //Distance.Attach(handler);
    }*/