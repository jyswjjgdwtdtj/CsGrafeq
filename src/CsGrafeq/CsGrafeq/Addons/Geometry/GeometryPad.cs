using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using CsGrafeq.Base;
using CsGrafeq.Geometry.Shapes.Getter;
using CsGrafeq.Geometry.Shapes;
using static CsGrafeq.Base.Values;
using static CsGrafeq.Geometry.GeometryMath;
using System.Runtime.InteropServices.ComTypes;
using CsGrafeq.Implicit;


namespace CsGrafeq.Geometry
{
    public class GeometryPad:Addon
    {
        private ShapeList Shapes=new ShapeList();
        public List<Point> SelectedPoints = new List<Point>();
        public List<Circle> SelectedCircles= new List<Circle>();
        public List<Line> SelectedLines= new List<Line>();
        public GeometryPad()
        {
            Enabled = true;
            _RenderMode=RenderMode.All;
        }
        Point MovingPoint = null;
        Vec DownPoint;
        protected override bool OnMouseDown(MouseEventArgs e)
        {
            DownPoint=new Vec(e.Location);
            foreach (Shape shape in Shapes)
            {
                if(shape is Point point)
                    if (Math.Pow(MathToPixelX(point.Location.X) - e.X, 2) + Math.Pow(MathToPixelY(point.Location.Y) - e.Y, 2) < 25 && shape.Visible)
                    {
                        MovingPoint = point;
                        return false;
                    }
            }
            MovingPoint = null;
            return true;
        }
        protected override bool OnMouseMove(AddonMouseMoveEventArgs e)
        {
            if (MovingPoint == null)
                return true;
            if ((new Vec(e.MouseEventArgs.Location) - DownPoint).GetLength() > 0.5)
            {
                SelectedCircles.Clear();
                SelectedLines.Clear();
                SelectedPoints.Clear();
            }
            (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(new Vec(PixelToMathX(e.X),PixelToMathY(e.Y)));
            MovingPoint.RefreshValues();
            AskForRender();
            return false;
        }
        protected override bool OnMouseUp(MouseEventArgs e)
        {
            if (MovingPoint == null)
                return true;
            if((new Vec(e.Location) - DownPoint).GetLength() > 0.5)
            {
                SelectedCircles.Clear();
                SelectedLines.Clear();
                SelectedPoints.Clear();
            }
            (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(MovingPoint.Location);
            MovingPoint = null;
            AskForRender();
            return false;
        }
        protected override bool OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    foreach (var i in SelectedPoints)
                        Shapes.Remove(i);
                    SelectedPoints.Clear();
                    foreach (var i in SelectedCircles)
                        Shapes.Remove(i);
                    SelectedCircles.Clear();
                    foreach (var i in SelectedLines)
                        Shapes.Remove(i);
                    SelectedLines.Clear();
                    AskForRender();
                    return true;
            }
            return base.OnKeyDown(e);
        }
        private System.Drawing.Font font = new System.Drawing.Font("Microsoft Yahei", 8);
        private System.Drawing.SolidBrush GTBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(70, System.Drawing.Color.Gray));
        protected override void Render(System.Drawing.Graphics g, System.Drawing.Rectangle r)
        {
            UnitLengthX = OwnerArguments.GetUX();
            UnitLengthY = OwnerArguments.GetUY();
            Vec LT= new Vec(PixelToMathX(r.Left),PixelToMathY(r.Bottom));
            Vec RT= new Vec(PixelToMathX(r.Right), PixelToMathY(r.Bottom));
            Vec RB= new Vec(PixelToMathX(r.Right), PixelToMathY(r.Top));
            Vec LB= new Vec(PixelToMathX(r.Left), PixelToMathY(r.Top));
            for(int i = 0; i < Shapes.Count; i++)
            {
                Shape shape = Shapes[i];
                if (!shape.Visible)
                    continue;
                if (shape is Point)
                {
                    Point p = shape as Point;
                    if (SelectedPoints.Contains(p))
                    {
                        g.FillEllipse(Brush_Red, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 3, (float)MathToPixelY(p.Location.Y) - 3, 6, 6));
                    }
                    else if (p == MovingPoint)
                    {
                        g.FillEllipse(Brush_Blue, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 3, (float)MathToPixelY(p.Location.Y) - 3, 6, 6));
                        g.DrawBubblePopup($"({p.Location.X},{p.Location.Y}) {((p.PointGetter is PointGetter_Movable)?"":"不可移动")}", font, MathToPixelForPoint(p.Location).OffSetBy(2, 2 + 20));
                    }
                    else
                        g.FillEllipse(Brush_Black, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 2, (float)MathToPixelY(p.Location.Y) - 2, 4, 4));
                    if (shape.Name is null ||shape.Name=="")
                        g.DrawBubblePopup("Point:" + (char)(Shapes.GetIndex(shape as Point)+65), font, MathToPixelForPoint(p.Location).OffSetBy(2, 2));
                    else
                        g.DrawBubblePopup("Point:" + (char)(Shapes.GetIndex(shape as Point) + 65)+" Name:"+shape.Name, font, MathToPixelForPoint(p.Location).OffSetBy(2, 2));
                }
                else if (shape is StraightLine)
                {
                    StraightLine s = shape as StraightLine;
                    Vec v1 = s.Point1;
                    Vec v2 = s.Point2;
                    (Vec, Vec) vs = GetValidVec(
                        GetIntersectionLSAndSL(LT, RT, v1, v2),
                        GetIntersectionLSAndSL(RT, RB, v1, v2),
                        GetIntersectionLSAndSL(RB, LB, v1, v2),
                        GetIntersectionLSAndSL(LB, LT, v1, v2)
                    );
                    vs.Item1.X = MathToPixelX(vs.Item1.X);
                    vs.Item2.X = MathToPixelX(vs.Item2.X);
                    vs.Item1.Y = MathToPixelY(vs.Item1.Y);
                    vs.Item2.Y = MathToPixelY(vs.Item2.Y);
                    if (SelectedLines.Contains(s))
                        g.DrawLine(Pen_Red, vs.Item1.ToPointF(), vs.Item2.ToPointF());
                    else
                        g.DrawLine(Pen_Black, vs.Item1.ToPointF(), vs.Item2.ToPointF());
                }
                else if (shape is LineSegment)
                {
                    LineSegment s = shape as LineSegment;
                    Vec v1 = s.Point1;
                    Vec v2 = s.Point2;

                    v1.X = MathToPixelX(v1.X);
                    v2.X = MathToPixelX(v2.X);
                    v1.Y = MathToPixelY(v1.Y);
                    v2.Y = MathToPixelY(v2.Y);
                    if (SelectedLines.Contains(s))
                        g.DrawLine(Pen_Red, v1.ToPointF(), v2.ToPointF());
                    else
                        g.DrawLine(Pen_Black, v1.ToPointF(), v2.ToPointF());
                }
                else if (shape is Polygon)
                {
                    Polygon polygon = shape as Polygon;
                    System.Drawing.PointF[] ps = new System.Drawing.PointF[polygon.Locations.Length];
                    for (int j = 0; j < ps.Length; j++)
                    {
                        ps[j] = MathToPixelForPointF(polygon.Locations[j]);
                    }
                    if (polygon.Fill)
                    {
                        g.FillPolygon(GTBrush, ps);
                    }
                    else
                    {
                        g.DrawPolygon(Pen_Black, ps);
                    }
                }
                else if (shape is PlotTextLabel)
                {
                    PlotTextLabel label = shape as PlotTextLabel;
                    g.DrawBubblePopup(label.Text, font, MathToPixelForPoint(label.Location));
                }
                else if (shape is PixelTextLabel)
                {
                    PixelTextLabel label = shape as PixelTextLabel;
                    g.DrawBubblePopup(label.Text, font, label.Location.ToPoint());
                }
                else if(shape is Circle)
                {
                    Circle circle = shape as Circle;
                    CircleStruct cs=circle.InnerCircle;
                    System.Drawing.PointF pf = MathToPixelForPointF(new Vec(cs.Center.X-cs.Radius,cs.Center.Y+cs.Radius));
                    System.Drawing.SizeF s = new System.Drawing.SizeF((float)(2*cs.Radius*UnitLengthX),(float)(2*cs.Radius*UnitLengthY));
                    if(SelectedCircles.Contains(circle))
                        g.DrawEllipse(Pen_Red, new System.Drawing.RectangleF(pf, s));
                    else
                        g.DrawEllipse(Pen_Black, new System.Drawing.RectangleF(pf, s));

                }
                else if(shape is Angle)
                {
                    Angle angle = shape as Angle;
                    System.Drawing.PointF pf = MathToPixelForPointF(angle.AnglePoint);
                    double arg1 = ((MathToPixelForPoint(angle.Point1).Sub(MathToPixelForPoint(angle.AnglePoint))).Arg() / Math.PI * 180).Mod(360);
                    double arg2 = ((MathToPixelForPoint(angle.Point2).Sub(MathToPixelForPoint(angle.AnglePoint))).Arg() / Math.PI * 180).Mod(360);
                    double aa = (((angle.Point2 - angle.AnglePoint).Arg2() -(angle.Point1 - angle.AnglePoint).Arg2() )) / Math.PI * 180;
                    aa = aa.Mod(360);
                    if (aa > 180)
                        aa = 360 - aa;
                    double a = arg2 - arg1;
                    a = a.Mod(360);
                    if (a > 180)
                        a -= 360;
                    g.DrawPie(Pen_Black, new System.Drawing.RectangleF(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)( a));
                    g.DrawBubblePopup($"{Math.Abs(aa).ToString("0.00")}°", font, MathToPixelForPoint(angle.AnglePoint).OffSetBy(2, 2 - 20));
                }
            }
        }
        private System.Drawing.Point MathToPixelForPoint(Vec vec)
        {
            return new System.Drawing.Point((int)MathToPixelX(vec.X), (int)MathToPixelY(vec.Y));
        }
        private Vec PixelToMathForPoint(System.Drawing.Point p)
        {
            return new Vec(PixelToMathX(p.X), PixelToMathY(p.Y));
        }
        private System.Drawing.PointF MathToPixelForPointF(Vec vec)
        {
            return new System.Drawing.PointF((float)MathToPixelX(vec.X), (float)MathToPixelY(vec.Y));
        }
        private class ShapeList : List<Shape>
        {
            private Point[] PointCounter = new Point[100];
            public ShapeList()
            {
            }
            public new void Add(Shape shape)
            {
                if (Contains(shape))
                    return;
                base.Add(shape);
                if (shape is Point)
                {
                    for(int i = 0; i < 100; i++)
                    {
                        if (PointCounter[i] == null)
                        {
                            PointCounter[i]=shape as Point;
                            return;
                        }
                    }
                    throw new IndexOutOfRangeException();
                }
            }
            public new void Remove(Shape shape)
            {
                if (!Contains(shape))
                    return;
                base.Remove(shape);
                foreach(var i in shape.SubShapes)
                    Remove(i);
                if(shape is Point p)
                {
                    PointCounter[GetIndex(p)] = null;
                }
            }
            public new void AddRange(IEnumerable<Shape> shapes)
            {
                foreach (Shape shape in shapes)
                {
                    Add(shape);
                }
            }
            
            public int GetIndex(Point p)
            {
                for(int i = 0; i < 100; i++)
                {
                    if (PointCounter[i]==p)
                        return i;
                }
                return -1;
            }
            public Point FromIndex(int index)
            {
                return PointCounter[index];
            }
        }
        public Point PutPoint(System.Drawing.Point Location)
        {
            RefreshOwnerArguments();
            Vec v = new Vec(Location.X,Location.Y);
            Vec rv = PixelToMathForPoint(Location);
            Vec vv =(PixelToMathForPoint(Location));
            List<(double,Shape)> shapes = new List<(double,Shape)> ();
            foreach(var s in Shapes)
            {
                if (s is Line)
                {
                    Line Line= (Line)s;
                    Vec v1 = Line.Point1;
                    Vec v2 = Line.Point2;
                    double dx = v2.X - v1.X;
                    double dy = v2.Y - v1.Y;
                    double t = ((vv.X - v1.X) * dx + (vv.Y - v1.Y) * dy) / (dx * dx + dy * dy);
                    Vec nv1 = new Vec(v1.X + t * dx, v1.Y + t * dy);
                    Vec nv = new Vec(MathToPixelForPoint(nv1));
                    if(s is LineSegment)
                    {
                        if (v1.X == v2.X)
                        {
                            if (!InRange(v1.Y, v2.Y, nv1.Y))
                                continue;
                        }
                        else
                        {
                            if (!InRange(v1.X, v2.X, nv1.X))
                                continue;
                        }
                    }
                    double distance = (nv - v).GetLength();
                    if (distance<5)
                    {
                        shapes.Add((distance,s));
                    }
                }else if(s is Circle)
                {
                    Circle c= (Circle)s;
                    Vec vvv = (rv - c.InnerCircle.Center)- (rv - c.InnerCircle.Center).Unit()* c.InnerCircle.Radius;
                    double distance = new Vec(vvv.X * UnitLengthX , vvv.Y * UnitLengthY).GetLength();
                    if (distance <5)
                    {
                        shapes.Add((distance, s));
                    }
                }
            }
            (double,Shape)[] ss=shapes.OrderBy(key => key.Item1).ToArray();
            Point newp;
            if (ss.Length == 0)
            {
                newp = new Point(new PointGetter_FromLocation(PixelToMathForPoint(Location)));
            }
            else if (ss.Length == 1)
            {
                Shape shape = ss[0].Item2;
                if (shape is Circle)
                {
                    newp = new Point(new PointGetter_OnCircle((Circle)shape, PixelToMathForPoint(Location)));
                }
                else if (shape is Line)
                {
                    newp = new Point(new PointGetter_OnLine((Line)shape, PixelToMathForPoint(Location)));
                }
                else
                    throw new Exception();
            }
            else
            {
                Shape s1 = ss[0].Item2;
                Shape s2 = ss[1].Item2;
                if (s1 is Line && s2 is Line)
                    newp = new Point(new PointGetter_FromTwoLine(s1 as Line, s2 as Line));
                else if (s1 is Line l1 && s2 is Circle c1)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l1.Point1, l1.Point2, c1.InnerCircle.Center, c1.InnerCircle.Radius);
                    v1 = new Vec(MathToPixelForPoint(v1));
                    v2 = new Vec(MathToPixelForPoint(v2));
                    newp = new Point(new PointGetter_FromLineAndCircle(l1, c1, (v1 - v).GetLength() < (v2 - v).GetLength()));
                }
                else if (s1 is Circle c2 && s1 is Line l2)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l2.Point1, l2.Point2, c2.InnerCircle.Center, c2.InnerCircle.Radius);
                    v1 = new Vec(MathToPixelForPoint(v1));
                    v2 = new Vec(MathToPixelForPoint(v2));
                    newp = new Point(new PointGetter_FromLineAndCircle(l2, c2, (v1 - v).GetLength() < (v2 - v).GetLength()));
                }
                else
                {
                    //暂不支持
                    return null;
                }
            }
            Shapes.Add(newp);
            return newp;
        }
        public Point GetPoint(System.Drawing.Point Location)
        {
            Vec v = PixelToMathForPoint(Location);
            double distance = 1000;
            Point p = null;
            foreach (var s in Shapes)
            {
                if (s is Point)
                {
                    Point pp = (Point)s;
                    double dis = (MathToPixelForPoint(pp.Location).Sub(Location).GetLength());
                    if (dis < 5 && dis < distance)
                    {
                        distance = dis;
                        p = pp;
                    }
                }
            }
            if (p != null)
            {
                return p;
            }
            return null;
        }
        public Line GetLine(System.Drawing.Point Location)
        {
            Vec v = PixelToMathForPoint(Location);
            double distance = 1000;
            Line p = null;
            foreach (var s in Shapes)
            {
                if (!(s is Line)) { continue; }
                Line Line = (Line)s;
                Vec v1 = Line.Point1;
                Vec v2 = Line.Point2;
                double dx = v2.X - v1.X;
                double dy = v2.Y - v1.Y;
                double t = ((v.X - v1.X) * dx + (v.Y - v1.Y) * dy) / (dx * dx + dy * dy);
                Vec nv1 = new Vec(v1.X + t * dx, v1.Y + t * dy);
                Vec nv = new Vec(MathToPixelForPoint(nv1));
                if (s is LineSegment)
                {
                    if (v1.X == v2.X)
                    {
                        if (!InRange(v1.Y, v2.Y, nv1.Y))
                            continue;
                    }
                    else
                    {
                        if (!InRange(v1.X, v2.X, nv1.X))
                            continue;
                    }
                }
                double dis = (nv - new Vec(Location)).GetLength();
                if (dis < 5&&dis<distance)
                {
                    p = Line;
                    distance= dis;
                }
            }
            if (p != null)
            {
                return p;
            }
            return null;
        }
        public void AddShape(Shape s)
        {
            Shapes.Add(s);
        }
    }
    
    internal static partial class GeometryMath
    {
        public static System.Drawing.Point Sub(this System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return new System.Drawing.Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static double GetLength(this System.Drawing.Point p1)
        {
            return Math.Sqrt(p1.X*p1.X+p1.Y*p1.Y);
        }
        public static double Arg(this System.Drawing.Point p)
        {
            return Math.Atan2(p.Y,p.X);
        }
        public static (Vec,Vec) GetPointFromCircleAndLine(Vec v1,Vec v2,Vec cp,double radius)
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
                if (v.IsInValidVec())
                    continue;
                vs2[i] = v;
                i++;
            }
            if (i == 2)
                return (vs2[0], vs2[1]);
            if (i == 0)
                return (Vec.InvalidVec, Vec.InvalidVec);
            return (vs2[0], vs2[2]);

        }
        public static Vec SolveFunction(double a, double b, double c, double d, double e, double f)
        {
            double det = a * e - b * d;
            if (det == 0)
                return Vec.InvalidVec;
            double[,] mat = new double[2,2];
            mat[0, 0] = e / det;
            mat[0, 1] = -b / det;
            mat[1, 0] = -d / det;
            mat[1, 1] = a / det;
            double x = mat[0, 0] * c + mat[0, 1] * f;
            double y = mat[1, 0] * c + mat[1, 1] * f;
            return new Vec(x, y);
        }
    }
}
