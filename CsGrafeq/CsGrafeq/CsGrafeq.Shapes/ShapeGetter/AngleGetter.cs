using CsGrafeq.I18N;
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
        ShapeParameters = [AnglePoint, Point1, Point2];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.AngleText;

    public override void Attach(GeometryShape subShape)
    {
        AnglePoint.AddSubShape(subShape);
        ;
        Point1.AddSubShape(subShape);
        ;
        Point2.AddSubShape(subShape);
        ;
    }

    public override void UnAttach(GeometryShape subShape)
    {
        AnglePoint.RemoveSubShape(subShape);
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
    }

    public override AngleData GetAngle()
    {
        var aa = (((Vec)Point2.Location - AnglePoint.Location).Arg2() -
                  ((Vec)Point1.Location - AnglePoint.Location).Arg2()) /
            PI * 180;
        aa = aa % 360;
        if (aa > 180)
            aa = 360 - aa;
        return new AngleData(aa, AnglePoint.Location, Point1.Location, Point2.Location);
    }
}