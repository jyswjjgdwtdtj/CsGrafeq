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
        AngleGetter.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }

    public override GeometryGetter Getter => AngleGetter;
    protected override string TypeName => "Angle";
    public override string Description => "Degreee:" + AngleData.Angle;

    public override void RefreshValues()
    {
        AngleData = AngleGetter.GetAngle();
    }

    public override Vec HitTest(Vec vec)
    {
        return Vec.Infinity;
    }
}