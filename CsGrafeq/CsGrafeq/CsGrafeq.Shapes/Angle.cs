using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Utilities.CsGrafeqMath;
using static CsGrafeq.Shapes.ShapeGetter.AngleGetter;

namespace CsGrafeq.Shapes;

public class Angle : GeometryShape
{
    public AngleData AngleData;
    public AngleGetter AngleGetter;

    public Angle(AngleGetter getter)
    {
        TypeName = MultiLanguageResources.AngleText;
        AngleGetter = getter;
        AngleGetter.Attach(this);
        RefreshValues();
    }

    public override GeometryGetter Getter => AngleGetter;
    public override void RefreshValues()
    {
        AngleData = AngleGetter.GetAngle();
        Description = "Degree:" + AngleData.Angle;
    }

    public override Vec DistanceTo(Vec vec)
    {
        return Vec.Infinity;
    }

    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        var v = AngleData.AnglePoint - rect.Location;
        return RangeIn(0, rect.Size.X, v.X) && RangeIn(0, rect.Size.Y, v.Y);
    }
}