namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class GeometryGetter : Getter
{
    public abstract string ActionName { get; }
    public abstract GeometryShape[] Parameters { get; }
    public abstract void Attach(ShapeChangedHandler handler, GeometryShape subShape);
    public abstract void UnAttach(ShapeChangedHandler handler, GeometryShape subShape);

    public virtual bool Adjust()
    {
        return false;
    }
}