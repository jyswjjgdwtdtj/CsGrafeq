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

    public override void Attach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point1.ShapeChanged += handler;
        Point2.ShapeChanged += handler;
        Point3.ShapeChanged += handler;
        Point1.SubShapes.Add(subShape);
        Point2.SubShapes.Add(subShape);
        Point3.SubShapes.Add(subShape);
    }
    public override void UnAttach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point1.ShapeChanged -= handler;
        Point2.ShapeChanged -= handler;
        Point3.ShapeChanged -= handler;
        Point1.SubShapes.Remove(subShape);
        Point2.SubShapes.Remove(subShape);
        Point3.SubShapes.Remove(subShape);
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
        return new CircleStruct { Center = Center.Location, Radius = (Center.Location - Point.Location).GetLength() };
    }

    public override void Attach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Center.ShapeChanged += handler;
        Point.ShapeChanged += handler;
        Point.SubShapes.Add(subShape);
        Center.SubShapes.Add(subShape);
    }
    public override void UnAttach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Center.ShapeChanged -= handler;
        Point.ShapeChanged -= handler;
        Point.SubShapes.Remove(subShape);
        Center.SubShapes.Remove(subShape);
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
    public override void Attach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Center.ShapeChanged += handler;
        //Distance.Attach(handler);
    }*/