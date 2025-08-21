using GeoShape = CsGrafeq.Shapes.Shape;

namespace CsGrafeq.Shapes.ShapeGetter;

public class PolygonGetter : GeometryGetter
{
    protected Point[] Points;

    public PolygonGetter(Point[] points)
    {
        Points = new Point[points.Length];
        Points = [.. points];
    }

    public override string ActionName => "Polygon";
    public override GeometryShape[] Parameters => Points;

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        foreach (var i in Points)
        {
            i.ShapeChanged += handler;
            i.SubShapes.Add(subShape);
        }
    }

    public virtual Vec[] GetPolygon()
    {
        var vs = new Vec[Points.Length];
        for (var i = 0; i < Points.Length; i++) vs[i] = Points[i].Location;
        return vs;
    }
}