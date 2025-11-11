using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Shapes.ShapeGetter.AngleGetter;

namespace CsGrafeq.Shapes;

public class Angle : GeometryShape
{
    public AngleData AngleData;
    public AngleGetter AngleGetter;

    public Angle(AngleGetter getter)
    {
        AngleGetter = getter;
        AngleGetter.Attach(this);
        RefreshValues();
    }

    public override GeometryGetter Getter => AngleGetter;
    public override string TypeName => "Angle";

    public override void RefreshValues()
    {
        AngleData = AngleGetter.GetAngle();
        Description = "Degree:" + AngleData.Angle;
    }

    public override Vec NearestOf(Vec vec)
    {
        return Vec.Infinity;
    }
}