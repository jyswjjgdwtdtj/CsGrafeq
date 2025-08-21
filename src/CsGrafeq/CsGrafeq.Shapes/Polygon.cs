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
        PolygonGetter.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }

    protected override string TypeName => "Polygon";
    public override PolygonGetter Getter => PolygonGetter;
    public override string Description => "";

    public override void RefreshValues()
    {
        Locations = PolygonGetter.GetPolygon();
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        var len = Locations.Length;
        if (!Filled)
        {
            var wn = 0;
            Vec p1, p2;
            Vec v1, v2;
            for (int i = 0, j = len - 1; i < len - 1; j = i++)
            {
                p1 = Locations[i];
                p2 = Locations[j];
                if (OnSegment(p1, p2, vec)) return Vec.Empty;
                v1 = p2 - p1;
                v2 = vec - p1;
                var k = Sgn(v1 ^ v2);
                var d1 = Sgn(p1.Y - vec.Y);
                var d2 = Sgn(p2.Y - p2.Y);
                if (k > 0 && d1 <= 0 && d2 > 0) wn--;
                if (k < 0 && d1 > 0 && d2 <= 0) wn++;
            }

            if (wn != 0)
                return Vec.Empty;
        }

        return FindMin(GetEnumDistance(Locations, vec));
    }

    private static bool OnSegment(Vec v1, Vec v2, Vec test)
    {
        return Sgn((v1 - test) ^ (v2 - test)) == 0 && Sgn((v1 - test) * (v2 - test)) == 1;
    }

    private static IEnumerable<(double, Vec)> GetEnumDistance(Vec[] vecs, Vec test)
    {
        for (var i = 0; i < vecs.Length - 1; i++)
            yield return (DistanceToSegment(vecs[i], vecs[i + 1], test, out var vec), vec - test);
        yield return (DistanceToSegment(vecs[vecs.Length - 1], vecs[0], test, out var vv), vv - test);
    }

    private static Vec FindMin(IEnumerable<(double, Vec)> vecs)
    {
        var distance = double.PositiveInfinity;
        var tar = Vec.Infinity;
        foreach (var vec in vecs)
            if (distance > vec.Item1)
            {
                distance = vec.Item1;
                tar = vec.Item2;
            }

        return tar;
    }
}