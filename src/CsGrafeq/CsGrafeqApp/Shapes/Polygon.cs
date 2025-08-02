using Avalonia.Input;
using CsGrafeqApp.Classes;
using CsGrafeqApp.Shapes.ShapeGetter;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeqApp.InternalMath;
using static CsGrafeqApp.Shapes.GeometryMath;

namespace CsGrafeqApp.Shapes
{
    public class Polygon:FilledShape
    {
        public Vec[] Locations=Array.Empty<Vec>();
        public PolygonGetter PolygonGetter;
        public Polygon(PolygonGetter getter)
        {
            PolygonGetter=getter;
            PolygonGetter.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
        }
        public override void RefreshValues()
        {
            Locations = PolygonGetter.GetPolygon();
            InvokeEvent();
        }
        protected override string TypeName
        {
            get => "Polygon";
        }
        public override PolygonGetter Getter => PolygonGetter;
        public override string Description => "";
        public override Vec HitTest(Vec vec)
        {
            int len = Locations.Length;
            if (!Filled)
            {
                int wn = 0;
                Vec p1, p2;
                Vec v1, v2;
                for (int i = 0, j = len-1; i < len-1; j = i++)
                {
                    p1 = Locations[i];
                    p2 = Locations[j];
                    if (OnSegment(p1, p2, vec)) return Vec.Empty;
                    v1 = p2 - p1;
                    v2 = vec - p1;
                    int k = Math.Sign(v1^v2);
                    int d1 = Math.Sign(p1.Y-vec.Y);
                    int d2 = Math.Sign(p2.Y-p2.Y);
                    if (k > 0 && d1 <= 0 && d2 > 0) wn--;
                    if (k < 0 && d1 > 0 && d2 <= 0) wn++;
                }
                if (wn != 0)
                    return Vec.Empty;
            }

            return FindMin(GetEnumDistance(Locations, vec));
        }
        private static bool OnSegment(Vec v1,Vec v2,Vec test)
        {
            return Math.Sign(v1 - test ^ v2 - test) == 0 && Math.Sign((v1 - test) * (v2 - test)) == 1;
        }
        private static IEnumerable<(double,Vec)> GetEnumDistance(Vec[] vecs,Vec test)
        {
            for(int i=0;i<vecs.Length-1;i++)
                yield return (DistanceToSegment(vecs[i], vecs[i+1], test,out var vec),vec-test);
            yield return (DistanceToSegment(vecs[vecs.Length - 1], vecs[0],test,out var vv),vv-test);
        }

        private static Vec FindMin(IEnumerable<(double, Vec)> vecs)
        {
            double distance=Double.PositiveInfinity;
            Vec tar=Vec.Infinity;
            foreach (var vec in vecs)
            {
                if (distance > vec.Item1)
                {
                    distance = vec.Item1;
                    tar = vec.Item2;
                }
            }
            return tar; 
        }
    }
    
}
