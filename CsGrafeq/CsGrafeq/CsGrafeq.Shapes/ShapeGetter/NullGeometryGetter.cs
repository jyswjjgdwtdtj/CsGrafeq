namespace CsGrafeq.Shapes.ShapeGetter;

public class NullGeometryGetter : GeometryGetter
{
    public override string ActionName => "null getter";
    public override GeometryShape[] Parameters => [];

    public override void Attach(Action handler, GeometryShape subShape)
    {
    }

    public override void UnAttach(Action handler, GeometryShape subShape)
    {
    }
}