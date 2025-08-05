using static CsGrafeq.Shapes.GeometryMath;
using static CsGrafeq.Math;
using static System.Math;


namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class PointGetter : GeometryGetter
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

    public static implicit operator PointGetter((double, double) f)
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

    public override string ActionName => "Intersect";
    public override GeometryShape[] Parameters => [Line1, Line2];

    public override Vec GetPoint()
    {
        var v = GetIntersectionPoint(Line1.Current.Point1, Line1.Current.Point2, Line2.Current.Point1,
            Line2.Current.Point2);
        if (Line1.CheckIsValid(v) && Line2.CheckIsValid(v))
            return v;
        return Vec.Invalid;
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Line1.ShapeChanged += handler;
        Line2.ShapeChanged += handler;
        Line1.SubShapes.Add(subShape);
        Line2.SubShapes.Add(subShape);
    }
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

    public override string ActionName => "PointOf";
    public override GeometryShape[] Parameters => [Point];

    public override Vec GetPoint()
    {
        return Point.Location;
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point.ShapeChanged += handler;
        Point.SubShapes.Add(subShape);
    }

    public static implicit operator PointGetter_FromPoint(Point f)
    {
        return new PointGetter_FromPoint(f);
    }
}

/// <summary>
///     轴对称点
/// </summary>
public class PointGetter_AxialSymmetryPoint : PointGetter
{
    private readonly Line Line;
    private readonly Point Point;

    public PointGetter_AxialSymmetryPoint(Point point, Line line)
    {
        Point = point;
        Line = line;
    }

    public override string ActionName => "Reflect";
    public override GeometryShape[] Parameters => [Point, Line];

    public override Vec GetPoint()
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var ControlPoint = Point.Location;
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        return new Vec(v1.X + t * dx, v1.Y + t * dy) * 2 - ControlPoint;
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point.ShapeChanged += handler;
        Line.ShapeChanged += handler;
        Point.SubShapes.Add(subShape);
        Line.SubShapes.Add(subShape);
    }
}

public abstract class PointGetter_Movable : PointGetter
{
    protected Vec ControlPoint;
    public bool Movable = true;
    public virtual GeometryShape? On => null;

    public double PointX
    {
        get => ControlPoint.X;
        set => SetControlPoint(new Vec(value, ControlPoint.Y));
    }

    public double PointY
    {
        get => ControlPoint.Y;
        set => SetControlPoint(new Vec(ControlPoint.X, value));
    }

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
}

public class PointGetter_OnLine : PointGetter_Movable
{
    private readonly Line Line;
    private double ratio;

    public PointGetter_OnLine(Line line, Vec InitialPoint)
    {
        Line = line;
        SetControlPoint(InitialPoint);
    }

    public override GeometryShape On => Line;
    public override string ActionName => "OnLine";
    public override GeometryShape[] Parameters => [Line];

    public override Vec GetPoint()
    {
        return new Vec(Line.Current.Point1.X + ratio * (Line.Current.Point2.X - Line.Current.Point1.X),
            Line.Current.Point1.Y + ratio * (Line.Current.Point2.Y - Line.Current.Point1.Y));
    }

    protected void SetRatio(Vec p)
    {
        if (Line.Current.Point1.X != Line.Current.Point2.X)
            ratio = (p.X - Line.Current.Point1.X) / (Line.Current.Point2.X - Line.Current.Point1.X);
        else if (Line.Current.Point1.Y != Line.Current.Point2.Y)
            ratio = (p.Y - Line.Current.Point1.Y) / (Line.Current.Point2.Y - Line.Current.Point1.Y);
        else
            ratio = 0.5;
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
        p.X = -b / a * y - c / a;
        SetRatio(p);
    }

    public override void SetControlPoint(Vec controlPoint)
    {
        base.SetControlPoint(controlPoint);
        var p = InternalGetPoint();
        SetRatio(p);
    }

    private Vec InternalGetPoint()
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        return new Vec(v1.X + t * dx, v1.Y + t * dy);
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Line.ShapeChanged += handler;
        Line.SubShapes.Add(subShape);
    }
}

public class PointGetter_OnCircle : PointGetter_Movable
{
    private readonly Circle Circle;
    private double theta;

    public PointGetter_OnCircle(Circle circle, Vec InitialPoint)
    {
        Circle = circle;
        SetControlPoint(InitialPoint);
    }

    public override GeometryShape On => Circle;
    public override string ActionName => "OnCircle";
    public override GeometryShape[] Parameters => [Circle];

    public override Vec GetPoint()
    {
        return new Vec(Circle.InnerCircle.Center.X + Cos(theta) * Circle.InnerCircle.Radius,
            Circle.InnerCircle.Center.Y + Sin(theta) * Circle.InnerCircle.Radius);
    }

    public override void SetControlPoint(Vec controlPoint)
    {
        if ((controlPoint - Circle.InnerCircle.Center).GetLength() == 0)
            theta = 0;
        else
            theta = (controlPoint - Circle.InnerCircle.Center).Arg2();
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Circle.ShapeChanged += handler;
        Circle.SubShapes.Add(subShape);
    }
}

public class PointGetter_FromLocation : PointGetter_Movable
{
    public PointGetter_FromLocation(Vec location)
    {
        ControlPoint = location;
    }

    public override string ActionName => "";
    public override GeometryShape? On => null;
    public override GeometryShape[] Parameters => [];

    public override Vec GetPoint()
    {
        return ControlPoint;
    }

    public static implicit operator PointGetter_FromLocation(Vec f)
    {
        return new PointGetter_FromLocation(f);
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
    }
}

public class PointGetter_NearestPointOnLine : PointGetter
{
    private readonly Line Line;
    private readonly Point Point;

    public PointGetter_NearestPointOnLine(Line line, Point point)
    {
        Line = line;
        Point = point;
    }

    public override string ActionName => "Nearest";
    public override GeometryShape[] Parameters => [Point, Line];

    public override Vec GetPoint()
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var ControlPoint = Point.PointGetter.GetPoint();
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        if (Line is Half)
            t = RangeTo(0, double.PositiveInfinity, t);
        else if (Line is Segment)
            t = RangeTo(0, 1, t);
        return new Vec(v1.X + t * dx, v1.Y + t * dy);
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Line.ShapeChanged += handler;
        Point.ShapeChanged += handler;
        Line.SubShapes.Add(subShape);
        Point.SubShapes.Add(subShape);
    }
}

public abstract class PointGetter_FromTwoPoint : PointGetter
{
    protected readonly Point Point1, Point2;

    public PointGetter_FromTwoPoint(Point point1, Point point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public override GeometryShape[] Parameters => [Point1, Point2];

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point1.ShapeChanged += handler;
        Point2.ShapeChanged += handler;

        Point1.SubShapes.Add(subShape);
        Point2.SubShapes.Add(subShape);
    }
}

public class PointGetter_EndOfLine : PointGetter
{
    private readonly bool First;
    private readonly Line line;

    public PointGetter_EndOfLine(Line line, bool first)
    {
        this.line = line;
        First = first;
    }

    public override string ActionName => (First ? "First" : "Second") + "EndOf";
    public override GeometryShape[] Parameters => [line];

    public override Vec GetPoint()
    {
        if (First)
            return line.Current.Point1;
        return line.Current.Point2;
    }

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        line.ShapeChanged += handler;
        line.SubShapes.Add(subShape);
    }
}

public class PointGetter_MiddlePoint : PointGetter_FromTwoPoint
{
    public PointGetter_MiddlePoint(Point point1, Point point2) : base(point1, point2)
    {
    }

    public override string ActionName => "MidPoint";

    public override Vec GetPoint()
    {
        return (Point1.Location + Point2.Location) / 2;
    }
}

/* public class PointGetter_FromTwoPointAndAngle : PointGetter_FromTwoPoint
 {
     private AngleGetter AngleGetter;
     public PointGetter_FromTwoPointAndAngle(AngleGetter ag,Point AnglePoint, Point point2) : base(AnglePoint, point2)
     {
         AngleGetter = ag;
     }
     public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
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

    public PointGetter_FromThreePoint(Point point1, Point point2, Point point3)
    {
        Point1 = point1;
        Point2 = point2;
        Point3 = point3;
    }

    public override GeometryShape[] Parameters => [Point1, Point2, Point3];
    public abstract override Vec GetPoint();

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Point1.ShapeChanged += handler;
        Point2.ShapeChanged += handler;
        Point3.ShapeChanged += handler;

        Point1.SubShapes.Add(subShape);
        Point2.SubShapes.Add(subShape);
        Point3.SubShapes.Add(subShape);
    }
}

public class PointGetter_MedianCenter : PointGetter_FromThreePoint
{
    public PointGetter_MedianCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override string ActionName => "MedianCenter";

    public override Vec GetPoint()
    {
        return new Vec(
            (Point1.Location.X + Point2.Location.X + Point3.Location.X) / 3,
            (Point1.Location.Y + Point2.Location.Y + Point3.Location.Y) / 3
        );
    }
}

public class PointGetter_OrthoCenter : PointGetter_FromThreePoint
{
    public PointGetter_OrthoCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override string ActionName => "OrthoCenter";

    public override Vec GetPoint()
    {
        var a = Point1.Location.X;
        var b = Point1.Location.Y;
        var c = Point2.Location.X;
        var d = Point2.Location.Y;
        var e = Point3.Location.X;
        var f = Point3.Location.Y;
        return SolveFunction(e - c, f - d, (f - d) * b + (e - c) * a, e - a, f - b, (f - b) * d + (e - a) * c);
    }
}

public class PointGetter_InCenter : PointGetter_FromThreePoint
{
    public PointGetter_InCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override string ActionName => "InCenter";

    public override Vec GetPoint()
    {
        var a = (Point2.Location - Point3.Location).GetLength();
        var b = (Point1.Location - Point3.Location).GetLength();
        var c = (Point2.Location - Point1.Location).GetLength();
        return new Vec(
            (a * Point1.Location.X + b * Point2.Location.X + c * Point3.Location.X) / (a + b + c),
            (a * Point1.Location.Y + b * Point2.Location.Y + c * Point3.Location.Y) / (a + b + c)
        );
    }
}

public class PointGetter_OutCenter : PointGetter_FromThreePoint
{
    public PointGetter_OutCenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override string ActionName => "OutCenter";

    public override Vec GetPoint()
    {
        var x1 = Point1.Location.X;
        var y1 = Point1.Location.Y;
        var x2 = Point2.Location.X;
        var y2 = Point2.Location.Y;
        var x3 = Point3.Location.X;
        var y3 = Point3.Location.Y;
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

#region FromCircle

public class PointGetter_FromLineAndCircle : PointGetter
{
    private readonly Circle Circle;
    private readonly bool IsFirst;
    private readonly Line Line;

    public PointGetter_FromLineAndCircle(Line line, Circle circle, bool isFirst)
    {
        Line = line;
        Circle = circle;
        IsFirst = isFirst;
    }

    public override string ActionName => "LineAndCircle";
    public override GeometryShape[] Parameters => [Line, Circle];

    public override void AddToChangeEvent(ShapeChangedHandler handler, GeometryShape subShape)
    {
        Line.ShapeChanged += handler;
        Circle.ShapeChanged += handler;
        Line.SubShapes.Add(subShape);
        Circle.SubShapes.Add(subShape);
    }

    public override Vec GetPoint()
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var cp = Circle.InnerCircle.Center;
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((cp.X - v1.X) * dx + (cp.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        var nv = new Vec(v1.X + t * dx, v1.Y + t * dy);
        var m = new Vec(dx, dy).Unit() *
                Sqrt(Circle.InnerCircle.Radius * Circle.InnerCircle.Radius - Pow((cp - nv).GetLength(), 2));
        v1 = nv - m;
        v2 = nv + m;
        v1 = IsFirst ? v1 : v2;
        if (Line.CheckIsValid(v1))
            return v1;
        return Vec.Invalid;
    }
}

#endregion