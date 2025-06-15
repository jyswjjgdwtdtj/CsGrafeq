using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class AngleGetter:Getter
    {
        public abstract double GetAngle();
    }
    internal class AngleGetter_FromThreePoint:AngleGetter
    {
        private Point AnglePoint,Point1,Point2;
        public AngleGetter_FromThreePoint(Point anglePoint,Point point1,Point point2)
        {
            AnglePoint = anglePoint;
            Point1 = point1;
            Point2 = point2;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            AnglePoint.Changed += handler;
            Point1.Changed += handler;
            Point2.Changed += handler;
            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
            AnglePoint.SubShapes.Add(subShape);
        }
        public override double GetAngle()
        {
            Vec p1 = Point1.Location;
            Vec p2 = Point2.Location;
            Vec ap = AnglePoint.Location;
            p1-=ap;
            p2-=ap;
            return p1.Arg2() - p2.Arg2();
        }

    }
    internal class AngleGetter_FromNumber:AngleGetter
    {
        private double Angle;
        public AngleGetter_FromNumber(double angle)
        {
            SetAngle(angle);
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
        }
        public override double GetAngle()
        {
            return Angle;
        }
        public void SetAngle(double angle)
        {
            Angle = angle.Mod(2*Math.PI);
        }

    }
}
