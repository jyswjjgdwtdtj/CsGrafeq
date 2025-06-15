using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Geometry.GeometryMath;
using static System.Windows.Forms.LinkLabel;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class PointGetter:Getter
    {
        public abstract Vec GetPoint();

        public static implicit operator PointGetter(Point f)
        {
            return new PointGetter_FromPoint(f);
        }

        public static implicit operator PointGetter(Vec f)
        {
            return new PointGetter_FromLocation(f);
        }

        public static implicit operator PointGetter((double,double) f)
        {
            return new PointGetter_FromLocation(f);
        }
    }
    #region FromLine
    internal class PointGetter_FromTwoLine : PointGetter
    {
        private readonly Line Line1, Line2;
        public PointGetter_FromTwoLine(Line line1, Line line2)
        {
            Line1 = line1;
            Line2 = line2;
        }
        public override Vec GetPoint()
        {
            Vec v= GetIntersectionPoint(Line1.Point1, Line1.Point2, Line2.Point1, Line2.Point2);
            if (Line1.CheckIsValid(v) && Line2.CheckIsValid(v))
                return v;
            return Vec.InvalidVec;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler,Shape subShape)
        {
            Line1.Changed += handler;
            Line2.Changed += handler;
            Line1.SubShapes.Add(subShape);
            Line2.SubShapes.Add(subShape);
        }
    }
    
    #endregion
    #region FromPoint
    internal class PointGetter_FromPoint : PointGetter
    {
        private readonly Point Point;
        public PointGetter_FromPoint(Point point)
        {
            Point = point;
        }
        public override Vec GetPoint()
        {
            return Point.Location;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Point.Changed += handler;
            Point.SubShapes.Add(subShape);
        }

        public static implicit operator PointGetter_FromPoint(Point f)
        {
            return new PointGetter_FromPoint(f);
        }
    }
    /// <summary>
    /// 轴对称点
    /// </summary>
    internal class PointGetter_AxialSymmetryPoint:PointGetter
    {
        private readonly Point Point;
        private readonly Line Line;
        public PointGetter_AxialSymmetryPoint(Point point, Line line)
        {
            Point = point;
            Line = line;
        }
        public override Vec GetPoint()
        {
            Vec v1 = Line.Point1;
            Vec v2 = Line.Point2;
            Vec ControlPoint = Point.Location;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            return new Vec(v1.X + t * dx, v1.Y + t * dy)*2-ControlPoint;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Point.Changed += handler;
            Line.Changed += handler;
            Point.SubShapes.Add(subShape);
            Line.SubShapes.Add(subShape);
        }

    }
    internal abstract class PointGetter_Movable:PointGetter
    {
        protected Vec ControlPoint;
        public bool Movable = true;
        public virtual void SetControlPoint(Vec controlPoint)
        {
            ControlPoint = controlPoint;
            
        }
    }
    
    internal class PointGetter_OnLine : PointGetter_Movable
    {
        private Line Line;
        private double ratio = 0;
        public PointGetter_OnLine(Line line, Vec InitialPoint)
        {
            Line = line;
            SetControlPoint(InitialPoint);
        }
        public override Vec GetPoint()
        {
            return new Vec(Line.Point1.X + ratio * (Line.Point2.X - Line.Point1.X), Line.Point1.Y + ratio * (Line.Point2.Y - Line.Point1.Y));
        }
        public override void SetControlPoint(Vec controlPoint)
        {
            base.SetControlPoint(controlPoint);
            Vec p = InternalGetPoint();
            if (Line.Point1.X != Line.Point2.X)
            {
                ratio = (p.X - Line.Point1.X) / (Line.Point2.X - Line.Point1.X);
            }
            else if (Line.Point1.Y != Line.Point2.Y)
            {
                ratio = (p.Y - Line.Point1.Y) / (Line.Point2.Y - Line.Point1.Y);
            }
            else
            {
                ratio = 0.5;
            }
            if (Line is HalfLine)
                ratio = ToRange(0, double.PositiveInfinity, ratio);
            else if (Line is LineSegment)
                ratio = ToRange(0, 1, ratio);
            Line.RefreshValues();
        }
        private Vec InternalGetPoint()
        {
            Vec v1 = Line.Point1;
            Vec v2 = Line.Point2;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            return new Vec(v1.X + t * dx, v1.Y + t * dy);
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Line.Changed += handler;
            Line.SubShapes.Add(subShape);
        }
    }
    internal class PointGetter_OnCircle : PointGetter_Movable
    {
        private Circle Circle;
        private double theta = 0;
        public PointGetter_OnCircle(Circle circle, Vec InitialPoint)
        {
            Circle = circle;
            SetControlPoint(InitialPoint);
        }
        public override Vec GetPoint()
        {
            return new Vec(Circle.InnerCircle.Center.X + Math.Cos(theta)*Circle.InnerCircle.Radius, Circle.InnerCircle.Center.Y + Math.Sin(theta) * Circle.InnerCircle.Radius);
        }
        public override void SetControlPoint(Vec controlPoint)
        {
            if((controlPoint- Circle.InnerCircle.Center).GetLength()==0)
                theta = 0;
            else
                theta = (controlPoint - Circle.InnerCircle.Center).Arg2();
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler,Shape subShape)
        {
            Circle.Changed += handler;
            Circle.SubShapes.Add(subShape);
        }
    }
    internal class PointGetter_FromLocation : PointGetter_Movable
    {
        public PointGetter_FromLocation(Vec location)
        {
            ControlPoint = location;
        }
        public override Vec GetPoint()
        {
            return ControlPoint;
        }
        public static implicit operator PointGetter_FromLocation(Vec f)
        {
            return new PointGetter_FromLocation(f);
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
        }
    }
    internal class PointGetter_NearestPointOnLine : PointGetter
    {
        private Line Line;
        private Point Point;
        public PointGetter_NearestPointOnLine(Line line, Point point)
        {
            Line = line;
            Point = point;
        }
        public override Vec GetPoint()
        {
            Vec v1 = Line.PointGetter1.GetPoint();
            Vec v2 = Line.PointGetter2.GetPoint();
            Vec ControlPoint = Point.PointGetter.GetPoint();
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            if (Line is HalfLine)
                t = ToRange(0, double.PositiveInfinity, t);
            else if (Line is LineSegment)
                t = ToRange(0, 1, t);
            return new Vec(v1.X + t * dx, v1.Y + t * dy);
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Line.Changed += handler;
            Point.Changed += handler;
            Line.SubShapes.Add(subShape);
            Point.SubShapes.Add(subShape);
        }
    }
    
    internal abstract class PointGetter_FromTwoPoint:PointGetter
    {
        protected readonly Point Point1, Point2;
        public PointGetter_FromTwoPoint(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Point1.Changed += handler;
            Point2.Changed += handler;

            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
        }

    }
    internal class PointGetter_MiddlePoint : PointGetter_FromTwoPoint
    {
        public PointGetter_MiddlePoint(Point point1, Point point2) : base(point1, point2)
        {
        }
        public override Vec GetPoint()
        {
            return (Point1.Location + Point2.Location) / 2;
        }
    }
    internal class PointGetter_FromTwoPointAndAngle : PointGetter_FromTwoPoint
    {
        private AngleGetter AngleGetter;
        public PointGetter_FromTwoPointAndAngle(AngleGetter ag,Point AnglePoint, Point point2) : base(AnglePoint, point2)
        {
            AngleGetter = ag;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            base.AddToChangeEvent(handler,subShape);
            AngleGetter.AddToChangeEvent(handler, subShape);
        }
        public override Vec GetPoint()
        {
            double theta = (Point2.Location - Point1.Location).Arg2();
            theta += AngleGetter.GetAngle();
            double distance=(Point2.Location - Point1.Location).GetLength();
            return new Vec(Point1.Location.X+Math.Cos(theta)*distance,Point1.Location.Y+Math.Sin(theta)*distance);
        }
    }
    internal abstract class PointGetter_FromThreePoint : PointGetter
    {
        protected Point Point1, Point2, Point3;
        public PointGetter_FromThreePoint(Point point1,Point point2,Point point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
        public override abstract Vec GetPoint();
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
	internal class PointGetter_MedianCenter : PointGetter_FromThreePoint
	{
		public PointGetter_MedianCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
		{
		}
		public override Vec GetPoint()
		{
			return new Vec(
				(Point1.Location.X + Point2.Location.X + Point3.Location.X) / 3,
				(Point1.Location.Y + Point2.Location.Y + Point3.Location.Y) / 3
			);
		}
	}
	internal class PointGetter_OrthoCenter : PointGetter_FromThreePoint
	{
		public PointGetter_OrthoCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
		{
		}
		public override Vec GetPoint()
		{
			double a = Point1.Location.X;
			double b = Point1.Location.Y;
			double c = Point2.Location.X;
			double d = Point2.Location.Y;
			double e = Point3.Location.X;
			double f = Point3.Location.Y;
			return SolveFunction(e - c, f - d, (f - d) * b + (e - c) * a, e - a, f - b, (f - b) * d + (e - a) * c);
		}
	}
	internal class PointGetter_InCenter : PointGetter_FromThreePoint
	{
		public PointGetter_InCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
		{
		}
		public override Vec GetPoint()
		{
			double a = (Point2.Location - Point3.Location).GetLength();
			double b = (Point1.Location - Point3.Location).GetLength();
			double c = (Point2.Location - Point1.Location).GetLength();
			return new Vec(
				(a * Point1.Location.X + b * Point2.Location.X + c * Point3.Location.X) / (a + b + c),
				(a * Point1.Location.Y + b * Point2.Location.Y + c * Point3.Location.Y) / (a + b + c)
			);
		}
	}
	internal class PointGetter_OutCenter : PointGetter_FromThreePoint
	{
		public PointGetter_OutCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
		{
		}
		public override Vec GetPoint()
		{
            double x1 = Point1.Location.X;
            double y1 = Point1.Location.Y;
            double x2 = Point2.Location.X;
            double y2 = Point2.Location.Y;
            double x3= Point3.Location.X;
            double y3= Point3.Location.Y;
            return SolveFunction(
                2 * (x2 - x1), 
                2 * (y2 - y1), 
                x2 * x2 + y2 * y2 - x1 * x1 - y1 * y1,
				2 * (x3 - x2),
				2 * (y3 - y2),
				x3 * x3 + y3 * y3 - x2 * x2 - y2 * y2);
		}
	}
    #endregion
    #region FromPolygon
    #endregion
    #region FromCircle
    internal class PointGetter_FromLineAndCircle:PointGetter
    {
        private Line Line;
        private Circle Circle;
        private bool IsFirst;
        public PointGetter_FromLineAndCircle(Line line, Circle circle, bool isFirst)
        {
            Line = line;
            Circle = circle;
            IsFirst = isFirst;
        }
        public override void AddToChangeEvent(ShapeChangeHandler handler, Shape subShape)
        {
            Line.Changed += handler;
            Circle.Changed += handler;
            Line.SubShapes.Add(subShape);
            Circle.SubShapes.Add(subShape);
        }
        public override Vec GetPoint()
        {
            Vec v1 = Line.Point1;
            Vec v2 = Line.Point2;
            Vec cp=Circle.InnerCircle.Center;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((cp.X - v1.X) * dx + (cp.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            Vec nv = new Vec(v1.X + t * dx, v1.Y + t * dy);
            Vec m = new Vec(dx,dy).Unit()* Math.Sqrt(Circle.InnerCircle.Radius * Circle.InnerCircle.Radius - Math.Pow((cp-nv).GetLength(),2));
            v1 = nv - m;
            v2 = nv + m;
            v1 = IsFirst ? v1 : v2;
            if(Line.CheckIsValid(v1))
                return v1;
            return Vec.InvalidVec;
        }

    }
    #endregion
}
