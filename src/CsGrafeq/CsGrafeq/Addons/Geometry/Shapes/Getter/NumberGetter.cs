using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGrafeq.Geometry.Shapes.Getter;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class NumberGetter : Getter
    {
        public abstract double GetNumber();
    }
    /*internal class NumberGetter_FromTwoPoint : NumberGetter
    {
        private Point Point1, Point2;
        public NumberGetter_FromTwoPoint(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler)
        {
            Point1.Changed += handler;
            Point2.Changed += handler;
        }
        public override double GetNumber()
        {
            return (Point1.Location - Point2.Location).GetLength();
        }
    }
    internal class NumberGetter_FromNumber : NumberGetter
    {
        private double Distance;
        public NumberGetter_FromNumber(double distance)
        {
            Distance = distance;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler)
        {
        }
        public override double GetNumber()
        {
            return Distance;
        }
        public void SetDistance(double d)
        {
            Distance = d;
        }
    }*/
}
