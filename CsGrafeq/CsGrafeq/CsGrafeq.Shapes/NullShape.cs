using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeq.Shapes;

public class NullGeometryShape : GeometryShape
{
    public override string TypeName => "null";
    public override GeometryGetter Getter => new NullGeometryGetter();

    public override void RefreshValues()
    {
    }

    public override Vec NearestOf(Vec vec)
    {
        return Vec.Empty;
    }
}