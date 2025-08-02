using Avalonia.Controls.Shapes;
using CsGrafeqApp.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public abstract class LineGetter:Getter
    {
        public abstract TwoPoint GetLine();
        public readonly struct TwoPoint
        {
            public readonly Vec Point1, Point2;
            public TwoPoint(Vec point1, Vec point2)
            {
                Point1 = point1;
                Point2 = point2;
            }
            public (double a,double b,double c) GetNormal()
            {
                if (Point1.X == Point2.X)
                    return (1, 0, -Point2.X);
                if (Point1.Y == Point2.Y)
                    return (0, 1, -Point2.Y);
                return (Point2.Y - Point1.Y, Point1.X - Point2.X, Point2.X * Point1.Y - Point1.X * Point2.Y);
            }
            public string ExpStr
            {
                get
                {
                    var (a, b, c) = GetNormal();
                    StringBuilder sb = new StringBuilder();
                    if (a == 0)
                    {
                        //do nothing
                    }
                    else if (a == 1)
                        sb.Append("x");
                    else if (a == -1)
                        sb.Append("-x");
                    else
                        sb.Append(a + "x");
                    if (b == 0)
                    {
                        //do nothing
                    }
                    else if (b == 1)
                        sb.Append("+y");
                    else if (b == -1)
                        sb.Append("-y");
                    else if (b > 0)
                        sb.Append("+" + b + "y");
                    else
                        sb.Append(b + "y");
                    if (c == 0)
                    {
                        //do nothing
                    }
                    else if (c == 1)
                        sb.Append("+1");
                    else if (c == -1)
                        sb.Append("-1");
                    else if (c > 0)
                        sb.Append("+" + c);
                    else
                        sb.Append(c);
                    sb.Append("=0");
                    return sb.ToString();
                }
                
            }
            public double Distance =>(Point1-Point2).GetLength();
        }
        public override string ActionName => "Line";
        public override Shape[] Parameters => [];
    }
    public abstract class LineGetter_TwoPoint : LineGetter
    {
        protected Point Point1, Point2;
        public LineGetter_TwoPoint(Point p1,Point p2)
        {
            Point1 = p1; Point2 = p2;
        }
        public override Shape[] Parameters => [Point1, Point2];
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point1.ShapeChanged += handler;
            Point2.ShapeChanged += handler;
            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
        }
    }
    public class LineGetter_Connected:LineGetter_TwoPoint
    {
        public LineGetter_Connected(Point point1, Point point2):base(point1,point2)
        {
        }
        public override TwoPoint GetLine()
        {
            return new TwoPoint(Point1.Location, Point2.Location);
        }
        public override string ActionName => "Staight";
    }
    public sealed class LineGetter_Segment : LineGetter_Connected
    {
        public LineGetter_Segment(Point point1, Point point2) : base(point1, point2)
        {
        }
        public override string ActionName => "Segment";
    }
    public sealed class LineGetter_Half : LineGetter_Connected
    {
        public LineGetter_Half(Point point1, Point point2) : base(point1, point2)
        {
        }
        public override string ActionName => "Half";
    }
    /// <summary>
    /// 中垂线
    /// </summary>
    public class LineGetter_PerpendicularBisector : LineGetter_TwoPoint
    {
        public override string ActionName => "PerpendicularBisector";
        public LineGetter_PerpendicularBisector(Point p1,Point p2) : base(p1, p2) { }
        public override TwoPoint GetLine()
        {
            Vec RealPoint1 = Point1.Location;
            Vec RealPoint2 = Point2.Location;
            Vec MiddlePoint = (RealPoint1+RealPoint2)/2;
            Vec p1 = MiddlePoint;
            Vec p2;
            double k = (RealPoint1.Y - RealPoint2.Y) / (RealPoint1.X - RealPoint2.X);
            double theta = (Math.Atan2(-1 / k, 1));
            if (RealPoint1.Y - RealPoint2.Y > 0)
                p2 = new Vec(p1.X + (Math.Cos(theta)), p1.Y - (Math.Cos(theta)) / k);
            else
                p2 = new Vec(p1.X - (Math.Cos(theta)), p1.Y + (Math.Cos(theta)) / k);
            return new TwoPoint(p1, p2);
        }
    }
    public class LineGetter_AngleBisector : LineGetter
    {
        public Point Point1, Point2, AnglePoint;
        public LineGetter_AngleBisector(Point p1,Point p2,Point anglePoint)
        {
            Point1 = p1;
            Point2 = p2;
            AnglePoint= anglePoint;
        }
        public override Shape[] Parameters => [Point1, Point2,AnglePoint];
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Point1.ShapeChanged += handler;
            Point2.ShapeChanged += handler;
            AnglePoint.ShapeChanged += handler;
            Point1.SubShapes.Add(subShape);
            Point2.SubShapes.Add(subShape);
            AnglePoint.SubShapes.Add(subShape);
        }
        public override string ActionName => "AngleBisector";
        public override TwoPoint GetLine()
        {
            Vec p1 = Point1.Location;
            Vec p2 = Point2.Location;
            Vec ap = AnglePoint.Location;
            Vec v1,v2;
            v1 = ap;
            p1 -= ap;
            p2 -= ap;
            double theta = (p1.Unit() + p2.Unit()).Arg2();
            if (theta == Math.PI / 2)//90
                v2 = new Vec(ap.X, ap.Y + 1);
            else if (theta == Math.PI / 2 * 3)//270
                v2 = new Vec(ap.X, ap.Y - 1);
            else
                v2 = new Vec(ap.X - Math.Cos(theta), ap.Y - Math.Sin(theta));
            return new TwoPoint(v1,v2);
        }
    }
    public abstract class LineGetter_PointAndLine : LineGetter
    {
        public Line Line;
        public Point Point;
        public LineGetter_PointAndLine(Line line,Point point)
        {
            Line = line;
            Point = point;
        }
        public override Shape[] Parameters => [Line, Point];
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            Line.ShapeChanged += handler;
            Point.ShapeChanged += handler;
            Line.SubShapes.Add(subShape);
            Point.SubShapes.Add(subShape);
        }
    }
    public class LineGetter_Vertical : LineGetter_PointAndLine
    {
        public LineGetter_Vertical(Line line,Point point) : base(line, point) { }
        public override TwoPoint GetLine()
        {
            Vec v1 = Point.Location;
            Vec v2;
            TwoPoint ps = Line.Current;
            double k = (ps.Point1.Y - ps.Point2.Y) / (ps.Point1.X - ps.Point2.X);
            double theta = Math.Atan2(-1 / k, 1);
            if (ps.Point1.Y - ps.Point2.Y > 0)
                v2 = new Vec(v1.X + Math.Cos(theta), v1.Y - Math.Cos(theta) / k);
            else
                v2 = new Vec(v1.X - Math.Cos(theta), v1.Y + Math.Cos(theta) / k);
            return new TwoPoint(v1,v2);
        }
        public override string ActionName => "Vertical";
    }
    public class LineGetter_Parallel : LineGetter_PointAndLine
    {
        public LineGetter_Parallel(Line line, Point point) : base(line, point)
        {
        }
        public override TwoPoint GetLine()
        {
            Vec v1 = Point.Location;
            Vec v2;
            TwoPoint ps = Line.Current;
            double k = (ps.Point1.Y - ps.Point2.Y) / (ps.Point1.X - ps.Point2.X);
            double theta;
            if (ps.Point1.X == ps.Point2.X)
            {
                theta = Math.PI / 2;
            }
            else theta = Math.Atan2(-1 / k, 1);
            if (ps.Point1.Y - ps.Point2.Y > 0)
                v2 = new Vec(v1.X + Math.Cos(theta), v1.Y + Math.Cos(theta) * k);
            else
                v2 = new Vec(v1.X - Math.Cos(theta), v1.Y - Math.Cos(theta) * k);
            return new TwoPoint(v1, v2);
        }
    }
}
