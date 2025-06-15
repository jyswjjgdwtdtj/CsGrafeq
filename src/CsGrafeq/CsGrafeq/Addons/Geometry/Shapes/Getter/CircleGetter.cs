using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeq.Geometry.GeometryMath;
namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class CircleGetter : Getter
    {
        public abstract CircleStruct GetCircle();
    }
    internal class CircleGetter_FromThreePoint : CircleGetter
    {
        Point Point1, Point2, Point3;
        public CircleGetter_FromThreePoint(Point point1, Point point2, Point point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
        public override CircleStruct GetCircle()
        {
            double x1 = Point1.Location.X;
            double y1 = Point1.Location.Y;
            double x2 = Point2.Location.X;
            double y2 = Point2.Location.Y;
            double x3 = Point3.Location.X;
            double y3 = Point3.Location.Y;
            Vec c = SolveFunction(
                2 * (x2 - x1),
                2 * (y2 - y1),
                x2 * x2 + y2 * y2 - x1 * x1 - y1 * y1,
                2 * (x3 - x2),
                2 * (y3 - y2),
                x3 * x3 + y3 * y3 - x2 * x2 - y2 * y2);
            return new CircleStruct { Center = c, Radius = (c - Point1.Location).GetLength() };
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Point1.Changed += handler;
            Point2.Changed += handler;
            Point3.Changed += handler;
            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
            Point3.SubShapes.Add(subShape);
        }

    }

    internal class CircleGetter_FromCenterAndPoint : CircleGetter
    {
        Point Center, Point;
        public CircleGetter_FromCenterAndPoint(Point center, Point point)
        {
            Center = center;
            Point = point;
        }
        public override CircleStruct GetCircle()
        {
            return new CircleStruct { Center = Center.Location, Radius = (Center.Location - Point.Location).GetLength() };
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Center.Changed += handler;
            Point.Changed += handler;
            Point.SubShapes.Add(subShape);
            Center.SubShapes.Add(subShape);
        }

    }
}
    /*internal class CircleGetter_FromCenterAndDistance : CircleGetter
    {
        Point Center;
        NumberGetter Distance;
        public CircleGetter_FromCenterAndDistance(Point center, NumberGetter NumberGetter)
        {
            Center = center;
            Distance= NumberGetter;
        }
        public override CircleStruct GetCircle()
        {
            return new CircleStruct { Center = Center.Location, Radius = Distance.GetNumber() };
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Center.Changed += handler;
            //Distance.AddToChangeEvent(handler);
        }*/