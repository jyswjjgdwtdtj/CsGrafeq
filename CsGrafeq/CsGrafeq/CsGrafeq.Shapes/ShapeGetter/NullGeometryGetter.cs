namespace CsGrafeq.Shapes.ShapeGetter;

public class NullGeometryGetter : GeometryGetter
{
    public override string ActionName => "null getter";
    public override GeometryShape[] Parameters => [];

    public override void Attach(GeometryShape subShape)
    {
    }

    public override void UnAttach(GeometryShape subShape)
    {
    }
}