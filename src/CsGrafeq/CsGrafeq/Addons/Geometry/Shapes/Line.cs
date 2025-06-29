using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq.Base;
using CsGrafeq.Geometry.Shapes.Getter;
using static CsGrafeq.ExMethods;
using static CsGrafeq.Geometry.GeometryMath;

namespace CsGrafeq.Geometry.Shapes
{
    public abstract class Line : Shape
    {
        internal PointGetter PointGetter1, PointGetter2;
        internal Vec Point1,Point2;
        internal Line()
        {

        }
        internal Line(PointGetter pointGetter1, PointGetter pointGetter2)
        {
            if (pointGetter1 != null)
            {
                PointGetter1 = pointGetter1;
                pointGetter1.AddToChangeEvent(RefreshValues,this);
            }
            if (pointGetter2 != null)
            {
                PointGetter2 = pointGetter2;
                pointGetter2.AddToChangeEvent(RefreshValues,this);
            }
            RefreshValues();
        }

        internal override void RefreshValues()
        {
            Point1 = PointGetter1.GetPoint();
            Point2 = PointGetter2.GetPoint();
            InvokeEvent();
        }
        internal abstract bool CheckIsValid(Vec vec);
    }
    public class StraightLine : Line
    {
        internal StraightLine() { }
        internal StraightLine(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
        {
        }
        internal override bool CheckIsValid(Vec vec)
        {
            return true;
        }
    }
    public class LineSegment : Line
    {
        internal LineSegment(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
        {
        }
        internal override bool CheckIsValid(Vec vec)
        {
            if (Point1.X == Point2.X)
                return InRange(Point1.Y, Point2.Y, vec.Y);
            else
                return InRange(Point1.X,Point2.X,vec.X);
        }
    }
    public class HalfLine : Line
    {
        internal HalfLine(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
        {
        }
        internal override bool CheckIsValid(Vec vec)
        {
            if (Point1.X == Point2.X)
                return Sgn(Point2.Y-Point1.Y)==Sgn(vec.Y-Point1.Y);
            else
                return Sgn(Point2.X - Point1.X) == Sgn(vec.X - Point1.X);
        }
    }
    /// <summary>
    /// 中垂线
    /// 名字好复杂！
    /// </summary>
    public class PerpendicularBisector : StraightLine 
    {
        private Vec RealPoint1, RealPoint2;
        private Vec MiddlePoint;
        internal PerpendicularBisector(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
        {
        }
        internal PerpendicularBisector(LineSegment linesegment) : base(linesegment.PointGetter1,linesegment.PointGetter2)
        {
        }

        internal override void RefreshValues()
        {
            RealPoint1 = PointGetter1.GetPoint();
            RealPoint2 = PointGetter2.GetPoint();
            MiddlePoint = (RealPoint1+RealPoint2)/2;
            Point1 = MiddlePoint;
            double k = (RealPoint1.Y - RealPoint2.Y) / (RealPoint1.X - RealPoint2.X);
            double theta = (Math.Atan2(-1 / k, 1));
            if (RealPoint1.Y - RealPoint2.Y > 0)
                Point2 = new Vec(Point1.X + (Math.Cos(theta)), Point1.Y - (Math.Cos(theta)) / k);
            else
                Point2 = new Vec(Point1.X - (Math.Cos(theta)), Point1.Y + (Math.Cos(theta)) / k);
            InvokeEvent();
        }
    }
    public class AngleBisector:StraightLine
    {
        private PointGetter AnglePoint;
        internal AngleBisector(PointGetter anglePoint,PointGetter point1,PointGetter point2){
            AnglePoint = anglePoint;
            AnglePoint.AddToChangeEvent(RefreshValues,this);
            PointGetter1 = point1;
            PointGetter1.AddToChangeEvent(RefreshValues,this);
            PointGetter2 = point2;
            PointGetter2.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }

        internal override void RefreshValues()
        {
            Vec p1 = PointGetter1.GetPoint();
            Vec p2 = PointGetter2.GetPoint();
            Vec ap= AnglePoint.GetPoint();
            Point1 = ap;
            p1 -= ap;
            p2 -= ap;
            double theta = (p1.Unit()+p2.Unit()).Arg2();
            if (theta == Math.PI / 2)//90
                Point2 = new Vec(ap.X, ap.Y + 1);
            else if (theta == Math.PI / 2 * 3)//270
                Point2 = new Vec(ap.X, ap.Y - 1);
            else
                Point2 = new Vec(ap.X - Math.Cos(theta), ap.Y - Math.Sin(theta));
            InvokeEvent();
        }
    }
    public abstract class SpecialLine : StraightLine
    {
        protected Line Line;
        internal SpecialLine(PointGetter pointGetter1, Line line)
        {
            PointGetter1 = pointGetter1;
            Line = line;
            PointGetter1.AddToChangeEvent(RefreshValues,this);
            Line.Changed += RefreshValues;
            RefreshValues();
        }
        internal abstract override void RefreshValues();
    }
    public class VerticalLine : SpecialLine
    {
        internal VerticalLine(PointGetter pointGetter1, Line line) : base(pointGetter1, line)
        {
        }

        internal override void RefreshValues()
        {
            Point1 = PointGetter1.GetPoint();
            double k = (Line.Point1.Y - Line.Point2.Y) / (Line.Point1.X - Line.Point2.X);
            double theta = (Math.Atan2(-1/k,1));
            if(Line.Point1.Y - Line.Point2.Y>0)
                Point2 = new Vec(Point1.X + (Math.Cos(theta)), Point1.Y - (Math.Cos(theta)) / k);
            else
                Point2 = new Vec(Point1.X - (Math.Cos(theta)), Point1.Y + (Math.Cos(theta)) / k);
            InvokeEvent();
        }
    }
    public class ParallelLine :SpecialLine
    {
        internal ParallelLine(PointGetter pointGetter1, Line line):base(pointGetter1, line) 
        {
        }
        internal override void RefreshValues()
        {
            Point1 = PointGetter1.GetPoint();
            double k = (Line.Point1.Y - Line.Point2.Y) / (Line.Point1.X - Line.Point2.X);
            double theta;
            if(Line.Point1.X == Line.Point2.X)
            {
                theta = Math.PI / 2;
            }
            else theta = (Math.Atan2(-1 / k, 1));
            if (Line.Point1.Y - Line.Point2.Y > 0)
                Point2 = new Vec(Point1.X + (Math.Cos(theta)), Point1.Y + (Math.Cos(theta)) * k);
            else
                Point2 = new Vec(Point1.X - (Math.Cos(theta)), Point1.Y - (Math.Cos(theta)) * k);
            InvokeEvent();
        }
    }
    
}
namespace CsGrafeq.Geometry
{
    internal static partial class GeometryMath
    {
        internal static double Sgn(double num)
        {
            return double.IsNaN(num) ? num : Math.Sign(num);
        }
        internal static Vec GetIntersectionPoint(Vec s1, Vec e1, Vec s2, Vec e2)
        {
            double k1, k2;
            k1 = (s1.Y - e1.Y) / (s1.X - e1.X);
            k2 = (s2.Y - e2.Y) / (s2.X - e2.X);
            if (k1 == k2)
                return Vec.InvalidVec;
            if (s1.X == e1.X)
            {
                return new Vec(s1.X, k2 * s1.X - k2 * s2.X + s2.Y);
            }
            if (s2.X == e2.X)
            {
                return new Vec(s2.X, k1 * s2.X - k1 * s1.X + s1.Y);
            }
            double x = (k1 * s1.X - s1.Y + s2.Y - k2 * s2.X) / (k1 - k2);
            return new Vec(x, k1 * x - k1 * s1.X + s1.Y);
        }
        /// <summary>
        /// ss,se为线段 s,e为直线 
        /// </summary>
        internal static Vec GetIntersectionLSAndSL(Vec ss, Vec es, Vec s, Vec e)
        {
            Vec j = GetIntersectionPoint(ss, es, s, e);
            if (InRange(ss.X, es.X, j.X) && InRange(ss.Y, es.Y, j.Y))
            {
                return j;
            }
            return new Vec(double.NaN, double.NaN);
        }
        internal static Vec GetIntersectionSLAndSL(Vec ss, Vec es, Vec s, Vec e)
        {
            Vec j = GetIntersectionPoint(ss, es, s, e);
            if (InRange(ss.X, es.X, j.X) && InRange(ss.Y, es.Y, j.Y) && InRange(s.X, e.X, j.X) && InRange(s.Y, e.Y, j.Y))
            {
                return j;
            }
            return new Vec(double.NaN, double.NaN);
        }

        internal static bool InRange(double num1, double num2, double numtest)
        {
            SwapIfNotLess(ref num1, ref num2);
            return num1 <= numtest && numtest <= num2;
        }
        internal static double Mod(this double num,double m)
        {
            return num-m*Math.Floor(num/m);
        }

        internal static double ToRange(double num1, double num2, double numtest)
        {
            SwapIfNotLess(ref num1, ref num2);
            if (numtest < num1)
                return num1;
            if (numtest > num2)
                return num2;
            return numtest;
        }
        internal static void SwapIfNotLess(ref double num1, ref double num2)
        {
            if (num1 > num2)
            {
                double num3 = num1;
                num1 = num2;
                num2 = num3;
            }
        }
    }

}
