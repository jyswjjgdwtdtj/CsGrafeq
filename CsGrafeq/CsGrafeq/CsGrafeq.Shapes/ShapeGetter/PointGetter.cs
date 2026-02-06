using CsGrafeq.I18N;
using CsGrafeq.Numeric;
using ReactiveUI;
using static CsGrafeq.Shapes.GeometryMath;
using static System.Math;


namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class PointGetter : GeometryGetter
{
    public abstract Vec GetPoint();

    public static implicit operator PointGetter(Vec f)
    {
        return new PointGetter_FromLocation(f);
    }

    public static implicit operator PointGetter((double, double) f)
    {
        return new PointGetter_FromLocation(f);
    }
}

#region Movable

public abstract class PointGetter_Movable : PointGetter
{
    public bool Fixed = false;

    public PointGetter_Movable()
    {
        PointX = new ExpNumber(0, this);
        PointY = new ExpNumber(0, this);
        PointX.UserSetValueStr += XChanged;
        PointY.UserSetValueStr += YChanged;
        ShapeParameters = [];
    }

    public abstract GeometricShape? On { get; }

    public ExpNumber PointX { get; init; }
    public ExpNumber PointY { get; init; }

    public abstract void XChanged();
    public abstract void YChanged();
    public abstract void SetPoint(Vec controlPoint);
    public abstract void SetStringPoint(Vector2<string> point);

    public override void Attach(GeometricShape subShape)
    {
        PointX.NumberChanged += subShape.RefreshValues;
        PointY.NumberChanged += subShape.RefreshValues;
    }

    public override void UnAttach(GeometricShape subShape)
    {
        PointX.NumberChanged -= subShape.RefreshValues;
        PointY.NumberChanged -= subShape.RefreshValues;
    }
}

public class PointGetter_FromLocation : PointGetter_Movable
{
    public PointGetter_FromLocation((double, double) initial) : this(initial.Item1, initial.Item2)
    {
    }

    public PointGetter_FromLocation(Vec initial) : this(initial.X, initial.Y)
    {
    }

    public PointGetter_FromLocation(double x, double y)
    {
        PointX.SetNumber(x);
        PointY.SetNumber(y);
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.PointText;
    public override GeometricShape? On => null;

    public override Vec GetPoint()
    {
        return new Vec(PointX.Value, PointY.Value);
    }

    public override void XChanged()
    {
    }

    public override void YChanged()
    {
    }

    public override void SetPoint(Vec controlPoint)
    {
        PointX.SetNumber(controlPoint.X);
        PointY.SetNumber(controlPoint.Y);
        this.RaisePropertyChanged(nameof(PointX));
        this.RaisePropertyChanged(nameof(PointY));
    }

    public override void SetStringPoint(Vector2<string> point)
    {
        PointX.ValueStr = point.X;
        PointY.ValueStr = point.Y;
        this.RaisePropertyChanged(nameof(PointX));
        this.RaisePropertyChanged(nameof(PointY));
    }
}

public abstract class PointGetter_OnShape<T> : PointGetter_Movable where T : GeometricShape
{
    protected bool IsPointXPrior = true;
    protected bool UseExpression;

    public PointGetter_OnShape(T shape, Vec InitialPoint)
    {
        OnShape = shape;
        SetPoint(InitialPoint);
    }

    protected T OnShape { get; init; }
    public override GeometricShape On => OnShape;
    public override MultiLanguageData ActionName => MultiLanguageResources.PointText;

    public sealed override Vec GetPoint()
    {
        Refresh();
        return new Vec(PointX.Value, PointY.Value);
    }

    public void Refresh()
    {
        CheckExp();
        if (UseExpression)
        {
            if (IsPointXPrior)
                PointY.SetNumber(YFromX(PointX.Value));
            else
                PointX.SetNumber(XFromY(PointY.Value));
        }
        else
        {
            var vec = GetPointWithoutExpression();
            PointX.SetNumber(vec.X);
            PointY.SetNumber(vec.Y);
        }
    }

    protected abstract double XFromY(double y);
    protected abstract double YFromX(double x);
    protected abstract Vec GetPointWithoutExpression();

    public override void Attach(GeometricShape subShape)
    {
        base.Attach(subShape);
        OnShape.AddSubShape(subShape);
    }

    public override void UnAttach(GeometricShape subShape)
    {
        base.UnAttach(subShape);
        OnShape.RemoveSubShape(subShape);
    }

    protected void CheckExp()
    {
        if (PointX.IsExpression)
        {
            UseExpression = true;
            IsPointXPrior = true;
        }
        else if (PointY.IsExpression)
        {
            UseExpression = true;
            IsPointXPrior = false;
        }
        else
        {
            UseExpression = false;
        }
    }

    public override void XChanged()
    {
        CheckExp();
        PointY.SetNumber(YFromX(PointX.Value));
    }

    public override void YChanged()
    {
        CheckExp();
        PointX.SetNumber(XFromY(PointY.Value));
    }

    public override void SetStringPoint(Vector2<string> point)
    {
        var x = point.X;
        var y = point.Y;
        if (double.TryParse(x, out var px))
        {
            if (double.TryParse(y, out var py))
            {
                //x,y均为数字
                UseExpression = false;
                SetPoint(new Vec(px, py));
            }
            else
            {
                //x为数字，y为表达式
                PointY.ValueStr = y;
            }
        }
        else
        {
            PointX.ValueStr = x;
        }
    }
}

public sealed class PointGetter_OnLine(Line line, Vec InitialPoint) : PointGetter_OnShape<Line>(line, InitialPoint)
{
    private readonly Line Line = line;
    private double ratio;

    private double GetRatio(Vec p)
    {
        double ratio;
        if (Line.Current.Point1.X != Line.Current.Point2.X)
            ratio = (p.X - Line.Current.Point1.X) / (Line.Current.Point2.X - Line.Current.Point1.X);
        else if (Line.Current.Point1.Y != Line.Current.Point2.Y)
            ratio = (p.Y - Line.Current.Point1.Y) / (Line.Current.Point2.Y - Line.Current.Point1.Y);
        else
            ratio = 0.5;
        if (Line is Half)
            ratio = CsGrafeqMath.RangeTo(0, double.PositiveInfinity, ratio);
        else if (Line is Segment)
            ratio = CsGrafeqMath.RangeTo(0, 1, ratio);
        return ratio;
    }

    protected override Vec GetPointWithoutExpression()
    {
        return new Vec(
            Line.Current.Point1.X + ratio * (Line.Current.Point2.X - Line.Current.Point1.X),
            Line.Current.Point1.Y + ratio * (Line.Current.Point2.Y - Line.Current.Point1.Y));
    }

    protected override double YFromX(double pointX)
    {
        if (double.IsNaN(pointX))
            return pointX;
        var (a, b, c) = Line.Current.GetNormal();
        if (a == 0) return c / b;

        if (b == 0) return double.NaN;

        return (-c - a * pointX) / b;
    }

    protected override double XFromY(double pointY)
    {
        if (double.IsNaN(pointY))
            return pointY;
        var (a, b, c) = Line.Current.GetNormal();
        if (b == 0) return c / a;

        if (a == 0) return double.NaN;

        return (-c - b * pointY) / a;
    }

    public override void SetPoint(Vec controlPoint)
    {
        var p = InternalGetPoint(controlPoint);
        ratio = GetRatio(p);
        PointX.SetNumber(p.X);
        PointY.SetNumber(p.Y);
    }

    private Vec InternalGetPoint(Vec controlPoint)
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((controlPoint.X - v1.X) * dx + (controlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        return new Vec(v1.X + t * dx, v1.Y + t * dy);
    }

    public override void XChanged()
    {
        CheckExp();
        PointY.SuspendNumberChanged();
        PointY.SetNumber(YFromX(PointX.Value));
        PointY.ResumeNumberChanged();
        ratio = GetRatio(new Vec(PointX.Value, PointY.Value));
    }

    public override void YChanged()
    {
        CheckExp();
        PointX.SuspendNumberChanged();
        PointX.SetNumber(XFromY(PointY.Value));
        PointX.ResumeNumberChanged();
        ratio = GetRatio(new Vec(PointX.Value, PointY.Value));
    }
}

public class PointGetter_OnCircle(Circle circle, Vec InitialPoint) : PointGetter_OnShape<Circle>(circle, InitialPoint)
{
    private readonly Circle Circle = circle;
    private double theta;

    protected override Vec GetPointWithoutExpression()
    {
        return new Vec(Circle.Current.Center.X + Cos(theta) * Circle.Current.Radius,
            Circle.Current.Center.Y + Sin(theta) * Circle.Current.Radius);
    }

    public override void SetPoint(Vec controlPoint)
    {
        UseExpression = false;
        if ((controlPoint - Circle.Current.Center).GetLength() == 0)
            theta = 0;
        else
            theta = (controlPoint - Circle.Current.Center).Arg2();
        var vec = GetPointWithoutExpression();
        PointX.SetNumber(vec.X);
        PointY.SetNumber(vec.Y);
    }

    private double GetTheta(Vec controlPoint)
    {
        double theta;
        if ((controlPoint - Circle.Current.Center).GetLength() == 0)
            theta = 0;
        else
            theta = (controlPoint - Circle.Current.Center).Arg2();
        return theta;
    }

    protected override double YFromX(double x)
    {
        var r = Circle.Current.Radius;
        var cx = Circle.Current.Center.X;
        var cy = Circle.Current.Center.Y;
        var val = r * r - (x - cx) * (x - cx);
        if (val < 0) return double.NaN;
        val = Sqrt(val);
        var y1 = cy + val;
        var y2 = cy - val;
        return Abs(y1 - PointY.Value) < Abs(y2 - PointY.Value) ? y1 : y2;
    }

    protected override double XFromY(double y)
    {
        var r = Circle.Current.Radius;
        var cx = Circle.Current.Center.X;
        var cy = Circle.Current.Center.Y;
        var val = r * r - (y - cy) * (y - cy);
        if (val < 0) return double.NaN;
        val = Sqrt(val);
        var x1 = cx + val;
        var x2 = cx - val;
        return Abs(x1 - PointX.Value) < Abs(x2 - PointX.Value) ? x1 : x2;
    }

    public override void XChanged()
    {
        CheckExp();
        PointY.SuspendNumberChanged();
        PointY.SetNumber(YFromX(PointX.Value));
        PointY.ResumeNumberChanged();
        theta = GetTheta(new Vec(PointX.Value, PointY.Value));
    }

    public override void YChanged()
    {
        CheckExp();
        PointX.SuspendNumberChanged();
        PointX.SetNumber(XFromY(PointY.Value));
        PointX.ResumeNumberChanged();
        theta = GetTheta(new Vec(PointX.Value, PointY.Value));
    }
}

#endregion

#region FromPointAndLine

public class PointGetter_NearestPointOnLine : PointGetter
{
    private readonly Line Line;
    private readonly Point Point;

    public PointGetter_NearestPointOnLine(Point point, Line line)
    {
        Line = line;
        Point = point;
        ShapeParameters = [point, line];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.NearestPointText;

    public override Vec GetPoint()
    {
        var v1 = Line.Current.Point1;
        var v2 = Line.Current.Point2;
        var ControlPoint = Point.PointGetter.GetPoint();
        var dx = v2.X - v1.X;
        var dy = v2.Y - v1.Y;
        var t = ((ControlPoint.X - v1.X) * dx + (ControlPoint.Y - v1.Y) * dy) / (dx * dx + dy * dy);
        if (Line is Half)
            t = CsGrafeqMath.RangeTo(0, double.PositiveInfinity, t);
        else if (Line is Segment)
            t = CsGrafeqMath.RangeTo(0, 1, t);
        return new Vec(v1.X + t * dx, v1.Y + t * dy);
    }

    public override void Attach(GeometricShape subShape)
    {
        Line.AddSubShape(subShape);
        Point.AddSubShape(subShape);
    }

    public override void UnAttach(GeometricShape subShape)
    {
        Line.RemoveSubShape(subShape);
        Point.RemoveSubShape(subShape);
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
        ShapeParameters = [point, line];
    }

    public override MultiLanguageData ActionName => MultiLanguageResources.AxialSymmetryText;

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

    public override void Attach(GeometricShape subShape)
    {
        Point.AddSubShape(subShape);
        Line.AddSubShape(subShape);
    }

    public override void UnAttach(GeometricShape subShape)
    {
        Point.RemoveSubShape(subShape);
        Line.RemoveSubShape(subShape);
    }
}

#endregion

#region FromTwoPoints

public abstract class PointGetter_FromTwoPoint : PointGetter
{
    protected readonly Point Point1, Point2;

    public PointGetter_FromTwoPoint(Point point1, Point point2)
    {
        Point1 = point1;
        Point2 = point2;
        ShapeParameters = [point1, point2];
    }

    public override void Attach(GeometricShape subShape)
    {
        Point1.AddSubShape(subShape);
        Point2.AddSubShape(subShape);

        ;
        ;
    }

    public override void UnAttach(GeometricShape subShape)
    {
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
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
        ActionName = first
            ? MultiLanguageResources.StartPointText
            : MultiLanguageResources.EndPointText;
        ShapeParameters = [line];
    }

    public override MultiLanguageData ActionName { get; }

    public override Vec GetPoint()
    {
        if (First)
            return line.Current.Point1;
        return line.Current.Point2;
    }

    public override void Attach(GeometricShape subShape)
    {
        line.AddSubShape(subShape);
        ;
    }

    public override void UnAttach(GeometricShape subShape)
    {
        line.RemoveSubShape(subShape);
    }
}

public class PointGetter_MiddlePoint : PointGetter_FromTwoPoint
{
    public PointGetter_MiddlePoint(Point point1, Point point2) : base(point1, point2)
    {
    }

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.MiddlePointText;

    public override Vec GetPoint()
    {
        return ((Vec)Point1.Location + Point2.Location) / 2;
    }
}

#endregion

/* public class PointGetter_FromTwoPointAndAngle : PointGetter_FromTwoPoint
 {
     private AngleGetter AngleGetter;
     public PointGetter_FromTwoPointAndAngle(AngleGetter ag,Point AnglePoint, Point point2) : base(AnglePoint, point2)
     {
         AngleGetter = ag;
     }
     public override void Attach(GeometryShape subShape)
     {
         base.Attach(handler,subShape);
         AngleGetter.Attach(handler, subShape);
     }
     public override Vec GetPoint()
     {
         double theta = (Point2.Location - Point1.Location).Arg2();
         theta += AngleGetter.GetAngle();
         double distance=(Point2.Location - Point1.Location).GetLength();
         return new Vec(Point1.Location.X+Math.Cos(theta)*distance,Point1.Location.Y+Math.Sin(theta)*distance);
     }
 }*/

#region FromThreePoints

public abstract class PointGetter_FromThreePoint : PointGetter
{
    protected Point Point1, Point2, Point3;

    public PointGetter_FromThreePoint(Point point1, Point point2, Point point3)
    {
        Point1 = point1;
        Point2 = point2;
        Point3 = point3;
        ShapeParameters = [point1, point2, point3];
    }

    public abstract override Vec GetPoint();

    public override void Attach(GeometricShape subShape)
    {
        Point1.AddSubShape(subShape);
        Point2.AddSubShape(subShape);
        Point3.AddSubShape(subShape);
    }

    public override void UnAttach(GeometricShape subShape)
    {
        Point1.RemoveSubShape(subShape);
        Point2.RemoveSubShape(subShape);
        Point3.RemoveSubShape(subShape);
    }
}

public class PointGetter_Centroid : PointGetter_FromThreePoint
{
    public PointGetter_Centroid(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.CentroidText;

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

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.OrthocenterText;

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

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.IncenterText;

    public override Vec GetPoint()
    {
        var a = ((Vec)Point2.Location - Point3.Location).GetLength();
        var b = ((Vec)Point1.Location - Point3.Location).GetLength();
        var c = ((Vec)Point2.Location - Point1.Location).GetLength();
        return new Vec(
            (a * Point1.Location.X + b * Point2.Location.X + c * Point3.Location.X) / (a + b + c),
            (a * Point1.Location.Y + b * Point2.Location.Y + c * Point3.Location.Y) / (a + b + c)
        );
    }
}

public class PointGetter_Circumcenter : PointGetter_FromThreePoint
{
    public PointGetter_Circumcenter(Point point1, Point point2, Point point3) : base(point1, point2, point3)
    {
    }

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.CircumcenterText;

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

#region Intersection

public abstract class PointGetter_Intersection<TShape1, TShape2>
    : PointGetter
    where TShape1 : GeometricShape
    where TShape2 : GeometricShape
{
    protected readonly bool IsFirst;
    protected readonly TShape1 Shape1;
    protected readonly TShape2 Shape2;

    public PointGetter_Intersection(TShape1 shape1, TShape2 shape2, bool isFirst)
    {
        ShapeParameters = [shape1, shape2];
        IsFirst = isFirst;
        Shape1 = shape1;
        Shape2 = shape2;
    }

    public override MultiLanguageData ActionName { get; } = MultiLanguageResources.Instance.IntersectText;

    public override void Attach(GeometricShape subShape)
    {
        Shape1.AddSubShape(subShape);
        Shape1.AddSubShape(subShape);
        Shape2.AddSubShape(subShape);
        Shape2.AddSubShape(subShape);
    }

    public override void UnAttach(GeometricShape subShape)
    {
        Shape1.RemoveSubShape(subShape);
        Shape1.RemoveSubShape(subShape);
        Shape2.RemoveSubShape(subShape);
        Shape2.RemoveSubShape(subShape);
    }
}

public class PointGetter_FromLineAndCircle(Line line, Circle circle, bool isFirst)
    : PointGetter_Intersection<Line, Circle>(line, circle, isFirst)
{
    public override Vec GetPoint()
    {
        var vs = IntersectionMath.FromLineAndCircle(line.Current, circle.Current);
        var v = IsFirst ? vs.v1 : vs.v2;
        return line.CheckIsValid(v) ? v : Vec.Invalid;
    }
}

public class PointGetter_FromTwoCircle(Circle shape1, Circle shape2, bool isFirst)
    : PointGetter_Intersection<Circle, Circle>(shape1, shape2, isFirst)
{
    public sealed override Vec GetPoint()
    {
        var vs = IntersectionMath.FromTwoCircle(shape1.Current, shape2.Current);
        return IsFirst ? vs.v1 : vs.v2;
    }
}

public class PointGetter_FromTwoLine(Line line1, Line line2) : PointGetter_Intersection<Line, Line>(line1, line2, false)
{
    public override Vec GetPoint()
    {
        var v = IntersectionMath.FromTwoLine(line1.Current, line2.Current);
        if (line1.CheckIsValid(v) && line2.CheckIsValid(v))
            return v;
        return Vec.Invalid;
    }
}

#endregion