using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Math;
using static CsGrafeq.Shapes.GeometryMath;

namespace CsGrafeq.Shapes;

public class Polygon : FilledShape
{
    public Vec[] Locations = Array.Empty<Vec>();
    public PolygonGetter PolygonGetter;

    public Polygon(PolygonGetter getter)
    {
        PolygonGetter = getter;
        PolygonGetter.Attach(RefreshValues, this);
        RefreshValues();
    }

    public override string TypeName => "Polygon";
    public override PolygonGetter Getter => PolygonGetter;
    public override void RefreshValues()
    {
        Locations = PolygonGetter.GetPolygon();
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        var len = Locations.Length;
        return FindMin(GetEnumDistance(Locations, vec))-vec;
    }
    private static IEnumerable<(double,Vec)> GetEnumDistance(Vec[] vecs, Vec test)
    {
        for (int i = 0,j=vecs.Length-1; i < vecs.Length - 1; i++,j=i+1)
        {
            var res = DistanceToLine(vecs[i], vecs[j], test, out var point);
            yield return FuzzyOnSegment(vecs[i], vecs[j], point)
                ? (res,point)
                : (double.PositiveInfinity,Vec.Infinity);
        }
    }

    private static Vec FindMin(IEnumerable<(double distance, Vec tar)> vecs)
    {
        var distance = double.PositiveInfinity;
        var tar = Vec.Infinity;
        foreach (var vec in vecs)
            if (distance > vec.distance)
            {
                distance = vec.distance;
                tar = vec.tar;
            }
        return tar;
    }
}