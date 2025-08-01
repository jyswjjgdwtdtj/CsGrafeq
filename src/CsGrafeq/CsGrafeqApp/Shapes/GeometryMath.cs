using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using static CsGrafeqApp.InternalMath;
using CsGrafeqApp.Shapes.ShapeGetter;
using GeoPoint = CsGrafeqApp.Shapes.Point;
using CsGrafeqApp.Classes;
namespace CsGrafeqApp.Shapes
{
    internal static partial class GeometryMath
    {
        public static Rect RegulateRectangle(Rect rectangle)
        {
            double x= rectangle.Position.X;
            double y= rectangle.Position.Y;
            double width= rectangle.Width;
            double height= rectangle.Height;
            if (width < 0)
                x += width;
            if (height < 0)
                y += height;
            width = Math.Abs(width);
            height = Math.Abs(height);
            return new Rect(x, y, width, height);
        }
        public static Avalonia.Point Sub(this Avalonia.Point p1,Avalonia.Point p2)
        {
            return new Avalonia.Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static double GetLength(this Avalonia.Point p1)
        {
            return Math.Sqrt(p1.X * p1.X + p1.Y * p1.Y);
        }
        public static double Arg(this Avalonia.Point p)
        {
            return Math.Atan2(p.Y, p.X);
        }
        public static (Vec, Vec) GetPointFromCircleAndLine(Vec v1, Vec v2, Vec cp, double radius)
        {
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((cp.X - v1.X) * dx + (cp.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            Vec nv = new Vec(v1.X + t * dx, v1.Y + t * dy);
            Vec m = new Vec(dx, dy).Unit() * Math.Sqrt(radius * radius - Math.Pow((cp - nv).GetLength(), 2));
            v1 = nv - m;
            v2 = nv + m;
            return (v1, v2);
        }
        public static (Vec, Vec) GetValidVec(Vec v1, Vec v2, Vec v3, Vec v4)
        {
            Vec[] vs = new Vec[4] { v1, v2, v3, v4 };
            Vec[] vs2 = new Vec[4];
            int i = 0;
            foreach (var v in vs)
            {
                if (v.IsInvalid())
                    continue;
                vs2[i] = v;
                i++;
            }
            if (i == 2)
                return (vs2[0], vs2[1]);
            if (i == 0)
                return (Vec.Invalid, Vec.Invalid);
            return (vs2[0], vs2[2]);

        }
        public static Vec SolveFunction(double a, double b, double c, double d, double e, double f)
        {
            double det = a * e - b * d;
            if (det == 0)
                return Vec.Invalid;
            double[,] mat = new double[2, 2];
            mat[0, 0] = e / det;
            mat[0, 1] = -b / det;
            mat[1, 0] = -d / det;
            mat[1, 1] = a / det;
            double x = mat[0, 0] * c + mat[0, 1] * f;
            double y = mat[1, 0] * c + mat[1, 1] * f;
            return new Vec(x, y);
        }
        public static PointGetter[] ToPointGetters(this GeoPoint[] ps)
        {
            PointGetter[] points = new PointGetter[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                points[i] = new PointGetter_FromPoint(ps[i]);
            }
            return points;
        }
        
        internal static Vec GetIntersectionPoint(Vec s1, Vec e1, Vec s2, Vec e2)
        {
            double k1, k2;
            k1 = (s1.Y - e1.Y) / (s1.X - e1.X);
            k2 = (s2.Y - e2.Y) / (s2.X - e2.X);
            if (k1 == k2)
                return Vec.Invalid;
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
        public static double DistanceToLine(Vec v1, Vec v2, Vec test,out Vec OnPoint)
        {
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((test.X - v1.X) * dx + (test.Y - v1.Y) * dy) / (dx * dx + dy * dy);
           OnPoint = new Vec(v1.X + t * dx, v1.Y + t * dy);
            return (OnPoint - test).GetLength();
        }
        public static double DistanceToSegment(Vec v1,Vec v2,Vec test,out Vec OnPoint)
        {
            double res = DistanceToLine(v1, v2, test,out OnPoint);
            return FuzzyOnSegment(v1, v2, OnPoint) ? res : double.PositiveInfinity;
        }
        public static bool FuzzyOnSegment(Vec v1, Vec v2, Vec test)
        {
            if (v1.X == v2.X)
                return InRange(v1.Y, v2.Y, test.Y);
            else
                return InRange(v1.X, v2.X, test.X);
        }
        public static bool FuzzyOnHalf(Vec v1, Vec v2, Vec test)
        {
            if (v1.X == v2.X)
                return Sgn(v2.Y - v1.Y) == Sgn(test.Y - v1.Y);
            else
                return Sgn(v2.X - v1.X) == Sgn(test.X - v1.X);
        }
        public static bool FuzzyOnStraight(Vec v1, Vec v2, Vec test)
        {
            return true;
        }
    }
}
