using CsGrafeqApp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoShape = CsGrafeqApp.Shapes.Shape;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public class PolygonGetter:Getter
    {
        protected Point[] Points;
        public PolygonGetter(Point[] points)
        {
            Points = new Point[points.Length];
            Array.Copy(Points,points,points.Length);
        }
        public PolygonGetter(Point p1,params Point[] points)
        {
            Points = new Point[points.Length+1];
            Array.Copy(points, Points, points.Length);
            Points[points.Length]=p1;
        }
        public override string ActionName => "Polygon";
        public override GeoShape[] Parameters => Points;
        public override void AddToChangeEvent(ShapeChangedHandler handler, GeoShape subShape)
        {
            foreach(var i in Points)
            {
                i.ShapeChanged+= handler;
                i.SubShapes.Add(subShape);
            }
        }
        public virtual Vec[] GetPolygon()
        {
            Vec[] vs=new Vec[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                vs[i]= Points[i].Location;
            }
            return vs;
        }

    }
}
