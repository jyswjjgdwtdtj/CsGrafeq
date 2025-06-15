using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq.Geometry.Shapes
{
    internal class Polygon:Shape
    {
        private PointGetter[] Points;
        internal Vec[] Locations;
        internal bool Fill=true;
        internal Polygon()
        {
            
        }
        internal Polygon(PointGetter[] points)
        {
            if(points.Length<3)
                throw new ArgumentLengthLessThanThreeException();
            Points = points;
            Locations = new Vec[Points.Length];
            foreach (PointGetter p in Points)
                p.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }
        internal Polygon(PointGetter p1,PointGetter p2,PointGetter p3,params PointGetter[] points)
        {
            List<PointGetter> ps = new List<PointGetter>();
            ps.Add(p1);
            ps.Add(p2);
            ps.Add(p3);
            ps.AddRange(points);
            Points=ps.ToArray();
            Locations = new Vec[Points.Length];
            foreach (PointGetter p in Points)
                p.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            for(int i=0; i<Points.Length; i++)
            {
                Locations[i] = Points[i].GetPoint();
            }
            InvokeEvent();
        }
    }
    internal class Triangle : Polygon
    {
        private Point Point1, Point2, Point3;
        internal Triangle(Point point1, Point point2,Point point3)
        {
            Point1= point1;
            Point1.Changed += RefreshValues;
            Point2= point2;
            Point2.Changed += RefreshValues;
            Point3 = point3;
            Point3.Changed += RefreshValues;
            Locations = new Vec[3];
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            Locations[0] = Point1.Location;
            Locations[1] = Point2.Location;
            Locations[2] = Point3.Location;
            InvokeEvent();
        }

    }
    internal class ArgumentLengthLessThanThreeException : Exception
    {
        public ArgumentLengthLessThanThreeException() :base("参数不可少于3个")
        {
        }
    }
}
