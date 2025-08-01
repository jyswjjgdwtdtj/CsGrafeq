﻿

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeqApp.Shapes.GeometryMath;
using static CsGrafeqApp.InternalMath;
using CsGrafeqApp.Classes;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public abstract class PointGetter:Getter
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
    public class PointGetter_FromTwoLine : PointGetter
    {
        private readonly Line Line1, Line2;
        public PointGetter_FromTwoLine(Line line1, Line line2)
        {
            Line1 = line1;
            Line2 = line2;
        }
        public override Vec GetPoint()
        {
            Vec v= GetIntersectionPoint(Line1.Current.Point1, Line1.Current.Point2, Line2.Current.Point1, Line2.Current.Point2);
            if (Line1.CheckIsValid(v) && Line2.CheckIsValid(v))
                return v;
            return Vec.Invalid;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler,Shape subShape)
        {
            Line1.ShapeChanged += handler;
            Line2.ShapeChanged += handler;
            Line1.SubShapes.Add(subShape);
            Line2.SubShapes.Add(subShape);
        }
        public override string ActionName => "Intersect";
        public override Shape[] Parameters => [Line1,Line2];
    }
    
    #endregion
    #region FromPoint
    public class PointGetter_FromPoint : PointGetter
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
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point.ShapeChanged += handler;
            Point.SubShapes.Add(subShape);
        }

        public static implicit operator PointGetter_FromPoint(Point f)
        {
            return new PointGetter_FromPoint(f);
        }
        public override string ActionName =>"PointOf";
        public override Shape[] Parameters =>[Point];
    }
    /// <summary>
    /// 轴对称点
    /// </summary>
    public class PointGetter_AxialSymmetryPoint:PointGetter
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
            Vec v1 = Line.Current.Point1;
            Vec v2 = Line.Current.Point2;
            Vec ControlPoint = Point.Location;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            return new Vec(v1.X + t * dx, v1.Y + t * dy)*2-ControlPoint;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point.ShapeChanged += handler;
            Line.ShapeChanged += handler;
            Point.SubShapes.Add(subShape);
            Line.SubShapes.Add(subShape);
        }
        public override string ActionName => "Reflect";
        public override Shape[] Parameters => [Point,Line];

    }
    public abstract class PointGetter_Movable:PointGetter
    {
        protected Vec ControlPoint;
        public bool Movable = true;
        public virtual Shape? On => null;
        public virtual void SetControlPoint(Vec controlPoint)
        {
            ControlPoint = controlPoint;
        }
        public virtual void SetControlX(double x)
        {
            ControlPoint.X = x;
        }
        public virtual void SetControlY(double y)
        {
            ControlPoint.Y = y;
        }
        public double PointX
        {
            get { return ControlPoint.X; }
            set { SetControlPoint(new Vec(value, ControlPoint.Y)); }
        }
        public double PointY
        {
            get { return ControlPoint.Y; }
            set { SetControlPoint(new Vec(ControlPoint.X, value)); }
        }
    }
    
    public class PointGetter_OnLine : PointGetter_Movable
    {
        private Line Line;
        private double ratio = 0;
        public override Shape On => Line;
        public PointGetter_OnLine(Line line, Vec InitialPoint)
        {
            Line = line;
            SetControlPoint(InitialPoint);
        }
        public override Vec GetPoint()
        {
            return new Vec(Line.Current.Point1.X + ratio * (Line.Current.Point2.X - Line.Current.Point1.X), Line.Current.Point1.Y + ratio * (Line.Current.Point2.Y - Line.Current.Point1.Y));
        }
        protected void SetRatio(Vec p)
        {
            if (Line.Current.Point1.X != Line.Current.Point2.X)
            {
                ratio = (p.X - Line.Current.Point1.X) / (Line.Current.Point2.X - Line.Current.Point1.X);
            }
            else if (Line.Current.Point1.Y != Line.Current.Point2.Y)
            {
                ratio = (p.Y - Line.Current.Point1.Y) / (Line.Current.Point2.Y - Line.Current.Point1.Y);
            }
            else
            {
                ratio = 0.5;
            }
            if (Line is Half)
                ratio = RangeTo(0, double.PositiveInfinity, ratio);
            else if (Line is Segment)
                ratio = RangeTo(0, 1, ratio);
        }
        public override void SetControlX(double x)
        {
            //aX+bY+c=0
            var p = ControlPoint;
            var (a, b, c) = Line.Current.GetNormal();
            if (b == 0)
            {
                p.X = c / a;
                return;
            }
            if (a == 0)
            {
                p.Y = c / b;
                p.X = x;
                return;
            }
            p.X = x;
            p.Y = -a / b * x - c / b;
            SetRatio(p);
        }
        public override void SetControlY(double y)
        {
            //aX+bY+c=0
            var p = ControlPoint;
            var (a, b, c) = Line.Current.GetNormal();
            if (a == 0)
            {
                p.Y = c / b;
                return;
            }
            if (b == 0)
            {
                p.X = c / a;
                p.Y = y;
                return;
            }
            p.Y = y;
            p.X = -b/a * y - c / a;
            SetRatio(p);
        }
        public override void SetControlPoint(Vec controlPoint)
        {
            base.SetControlPoint(controlPoint);
            Vec p = InternalGetPoint();
            SetRatio(p);
        }
        private Vec InternalGetPoint()
        {
            Vec v1 = Line.Current.Point1;
            Vec v2 = Line.Current.Point2;
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            return new Vec(v1.X + t * dx, v1.Y + t * dy);
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Line.ShapeChanged += handler;
            Line.SubShapes.Add(subShape);
        }
        public override string ActionName => "OnLine";
        public override Shape[] Parameters => [Line];
    }
    public class PointGetter_OnCircle : PointGetter_Movable
    {
        private Circle Circle;
        private double theta = 0;
        public override Shape On => Circle;
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
        public override void AddToChangeEvent(ShapeChangedHandler handler,Shape subShape)
        {
            Circle.ShapeChanged += handler;
            Circle.SubShapes.Add(subShape);
        }
        public override string ActionName => "OnCircle";
        public override Shape[] Parameters => [Circle];
    }
    public class PointGetter_FromLocation : PointGetter_Movable
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
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
        }
        public override string ActionName => "";
        public override Shape? On => null;
        public override Shape[] Parameters => [];
        
    }
    public class PointGetter_NearestPointOnLine : PointGetter
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
            Vec v1 = Line.Current.Point1;
            Vec v2 = Line.Current.Point2;
            Vec ControlPoint = Point.PointGetter.GetPoint();
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            if (Line is Half)
                t = RangeTo(0, double.PositiveInfinity, t);
            else if (Line is Segment)
                t = RangeTo(0, 1, t);
            return new Vec(v1.X + t * dx, v1.Y + t * dy);
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Line.ShapeChanged += handler;
            Point.ShapeChanged += handler;
            Line.SubShapes.Add(subShape);
            Point.SubShapes.Add(subShape);
        }
        public override string ActionName => "Nearest";
        public override Shape[] Parameters => [Point,Line];
    }
    public abstract class PointGetter_FromTwoPoint:PointGetter
    {
        protected readonly Point Point1, Point2;
        public PointGetter_FromTwoPoint(Point point1, Point point2)
        {
            Point1 = point1;
            Point2 = point2;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point1.ShapeChanged += handler;
            Point2.ShapeChanged += handler;

            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
        }
        public override Shape[] Parameters => [Point1, Point2];
    }
    internal class PointGetter_EndOfLine : PointGetter
    {
        private Line line;
        private bool First;
        public PointGetter_EndOfLine(Line line, bool first)
        {
            this.line = line;
            First = first;
        }
        public override Vec GetPoint()
        {
            if (First)
                return line.Current.Point1;
            else
                return line.Current.Point2;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            line.ShapeChanged += handler;
            line.SubShapes.Add(subShape);
        }
        public override string ActionName => (First?"First":"Second")+"EndOf";
        public override Shape[] Parameters => [line];
    }
    public class PointGetter_MiddlePoint : PointGetter_FromTwoPoint
    {
        public PointGetter_MiddlePoint(Point point1, Point point2) : base(point1, point2)
        {
        }
        public override Vec GetPoint()
        {
            return (Point1.Location + Point2.Location) / 2;
        }
        public override string ActionName => "MidPoint";
    }
   /* public class PointGetter_FromTwoPointAndAngle : PointGetter_FromTwoPoint
    {
        private AngleGetter AngleGetter;
        public PointGetter_FromTwoPointAndAngle(AngleGetter ag,Point AnglePoint, Point point2) : base(AnglePoint, point2)
        {
            AngleGetter = ag;
        }
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
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
    }*/
    public abstract class PointGetter_FromThreePoint : PointGetter
    {
        protected Point Point1, Point2, Point3;
        public PointGetter_FromThreePoint(Point point1,Point point2,Point point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
        public override abstract Vec GetPoint();
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point1.ShapeChanged += handler;
            Point2.ShapeChanged += handler;
            Point3.ShapeChanged += handler;

            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
            Point3.SubShapes.Add(subShape);
        }
        public override Shape[] Parameters => [Point1,Point2,Point3];
    }
	public class PointGetter_MedianCenter : PointGetter_FromThreePoint
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
        public override string ActionName => "MedianCenter";
	}
	public class PointGetter_OrthoCenter : PointGetter_FromThreePoint
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
        public override string ActionName => "OrthoCenter";
    }
	public class PointGetter_InCenter : PointGetter_FromThreePoint
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
        public override string ActionName => "InCenter";
    }
	public class PointGetter_OutCenter : PointGetter_FromThreePoint
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
        public override string ActionName => "OutCenter";
    }
    #endregion
    #region FromPolygon
    #endregion
    #region FromCircle
    public class PointGetter_FromLineAndCircle:PointGetter
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
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Line.ShapeChanged += handler;
            Circle.ShapeChanged += handler;
            Line.SubShapes.Add(subShape);
            Circle.SubShapes.Add(subShape);
        }
        public override Vec GetPoint()
        {
            Vec v1 = Line.Current.Point1;
            Vec v2 = Line.Current.Point2;
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
            return Vec.Invalid;
        }
        public override string ActionName => "LineAndCircle";
        public override Shape[] Parameters => [Line,Circle];
    }
    #endregion
}
