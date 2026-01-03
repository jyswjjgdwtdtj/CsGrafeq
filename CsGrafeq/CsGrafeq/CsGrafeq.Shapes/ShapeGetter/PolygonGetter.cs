using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public class PolygonGetter : GeometryGetter
{
    protected Point[] Points;

    public PolygonGetter(Point[] points)
    {
        Points = [.. points];
        ShapeParameters = [.. Points];
    }

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.PolygonText;

    public override void Attach(GeometryShape subShape)
    {
        foreach (var i in Points)
        {
            i.AddSubShape(subShape);
            ;
        }
    }

    public override void UnAttach(GeometryShape subShape)
    {
        foreach (var i in Points) i.RemoveSubShape(subShape);
    }

    public virtual Vec[] GetPolygon()
    {
        var vs = new Vec[Points.Length];
        for (var i = 0; i < Points.Length; i++) vs[i] = Points[i].Location;
        return vs;
    }
}