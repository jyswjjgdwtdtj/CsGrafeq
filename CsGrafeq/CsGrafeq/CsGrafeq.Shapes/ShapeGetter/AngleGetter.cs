using static System.Math;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class AngleGetter : GeometryGetter
{
    public abstract AngleData GetAngle();

    public readonly struct AngleData
    {
        public readonly double Angle;
        public readonly Vec AnglePoint;
        public readonly Vec Point1;
        public readonly Vec Point2;

        public AngleData(double angle, Vec ap, Vec p1, Vec p2)
        {
            Angle = angle;
            AnglePoint = ap;
            Point1 = p1;
            Point2 = p2;
        }
    }
}

public class AngleGetter_FromThreePoint : AngleGetter
{
    private readonly Point AnglePoint;
    private readonly Point Point1;
    private readonly Point Point2;

    public AngleGetter_FromThreePoint(Point anglePoint, Point point1, Point point2)
    {
        AnglePoint = anglePoint;
        Point1 = point1;
        Point2 = point2;
    }

    public override string ActionName => "Angle";
    public override GeometryShape[] Parameters => [AnglePoint, Point1, Point2];

    public override void Attach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        AnglePoint.ShapeChanged += handler;
        AnglePoint.SubShapes.Add(subShape);
        Point1.ShapeChanged += handler;
        Point2.SubShapes.Add(subShape);
        Point2.ShapeChanged += handler;
        Point1.SubShapes.Add(subShape);
    }

    public override void UnAttach(ShapeChangedHandler handler, GeometryShape subShape)
    {
        AnglePoint.ShapeChanged -= handler;
        AnglePoint.SubShapes.Remove(subShape);
        Point1.ShapeChanged -= handler;
        Point2.SubShapes.Remove(subShape);
        Point2.ShapeChanged -= handler;
        Point1.SubShapes.Remove(subShape);
    }

    public override AngleData GetAngle()
    {
        var aa = (((Vec)Point2.Location - AnglePoint.Location).Arg2() - ((Vec)Point1.Location - AnglePoint.Location).Arg2()) /
            PI * 180;
        aa = aa % 360;
        if (aa > 180)
            aa = 360 - aa;
        return new AngleData(aa, AnglePoint.Location, Point1.Location, Point2.Location);
    }
}