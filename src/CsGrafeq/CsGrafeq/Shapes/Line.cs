using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Shapes.GeometryMath;
using static CsGrafeq.Shapes.ShapeGetter.LineGetter;

namespace CsGrafeq.Shapes;

public abstract class Line : GeometryShape
{
    public TwoPoint Current;
    public LineGetter LineGetter;

    public Line(LineGetter lineGetter)
    {
        LineGetter = lineGetter;
        LineGetter.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }

    protected override string TypeName => "Line";

    public override LineGetter Getter => LineGetter;
    public TwoPoint LineData => Current;
    public override string Description => Current.ExpStr;

    public override void RefreshValues()
    {
        Current = LineGetter.GetLine();
        InvokeEvent();
    }

    public abstract bool CheckIsValid(Vec vec);

    public override Vec HitTest(Vec vec)
    {
        var res = DistanceToLine(Current.Point1, Current.Point2, vec, out var point);
        return CheckIsValid(point) ? vec - point : Vec.Infinity;
    }
}

public class Segment : Line
{
    public Segment(LineGetter_Segment segment) : base(segment)
    {
    }

    protected override string TypeName => "Segment";
    public override string Description => Current.ExpStr + " " + Current.Distance;

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnSegment(Current.Point1, Current.Point2, vec);
    }
}

public class Half : Line
{
    public Half(LineGetter_Half segment) : base(segment)
    {
    }

    protected override string TypeName => "Half";

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnHalf(Current.Point1, Current.Point2, vec);
    }
}

public class Straight : Line
{
    public Straight(LineGetter lineGetter) : base(lineGetter)
    {
    }

    protected override string TypeName => "Straight";

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnStraight(Current.Point1, Current.Point2, vec);
    }
}
/*public abstract class Line : Shape
{
    public PointGetter PointGetter1, PointGetter2;
    public Vec Point1,Point2;
    protected Line() { }
    public Line(PointGetter pointGetter1, PointGetter pointGetter2)
    {
        PointGetter1 = pointGetter1;
        pointGetter1.AddToChangeEvent(RefreshValues, this);
        PointGetter2 = pointGetter2;
        pointGetter2.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }
    public override void RefreshValues()
    {
        Point1 = PointGetter1.GetPoint();
        Point2 = PointGetter2.GetPoint();
        InvokeEvent();
    }
    public abstract bool CheckIsValid(Vec vec);
    public string ExpStr
    {
        get
        {
            return GetExpStr(Point1,Point2);
        }
    }
    public static string GetExpStr(double a,double b,double c)
    {
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
    public static string GetExpStr(Vec v1,Vec v2)
    {
        if (v1.X == v2.X)
            return GetExpStr(1, 0, -v2.X);
        if (v1.Y == v2.Y)
            return GetExpStr(0,1,-v2.Y);
        return GetExpStr(v2.Y-v1.Y,v1.X-v2.X,v2.X*v1.Y-v1.X*v2.Y);
    }
    public override string TypeName
    {
        get => "Line:";
    }
}
public class StraightLine : Line
{
    protected StraightLine() { }
    public StraightLine(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
    {
    }
    public override bool CheckIsValid(Vec vec)
    {
        return true;
    }
    public override string TypeName
    {
        get => "StraightLine:";
    }
}
public class LineSegment : Line
{
    public LineSegment(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
    {
    }
    public override bool CheckIsValid(Vec vec)
    {
        if (Point1.X == Point2.X)
            return RangeIn(Point1.Y, Point2.Y, vec.Y);
        else
            return RangeIn(Point1.X,Point2.X,vec.X);
    }
    public override string TypeName
    {
        get => "LineSegment:";
    }
}
public class HalfLine : Line
{
    public HalfLine(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
    {
    }
    public override bool CheckIsValid(Vec vec)
    {
        if (Point1.X == Point2.X)
            return Sgn(Point2.Y-Point1.Y)==Sgn(vec.Y-Point1.Y);
        else
            return Sgn(Point2.X - Point1.X) == Sgn(vec.X - Point1.X);
    }
    public override string TypeName
    {
        get => "HalfLine:";
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
    public PerpendicularBisector(PointGetter pointGetter1, PointGetter pointGetter2) : base(pointGetter1, pointGetter2)
    {
    }
    public PerpendicularBisector(LineSegment linesegment) : base(linesegment.PointGetter1,linesegment.PointGetter2)
    {
    }

    public override void RefreshValues()
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
    public AngleBisector(PointGetter anglePoint,PointGetter point1,PointGetter point2){
        AnglePoint = anglePoint;
        AnglePoint.AddToChangeEvent(RefreshValues,this);
        PointGetter1 = point1;
        PointGetter1.AddToChangeEvent(RefreshValues,this);
        PointGetter2 = point2;
        PointGetter2.AddToChangeEvent(RefreshValues,this);
        RefreshValues();
    }

    public override void RefreshValues()
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
    public SpecialLine(PointGetter pointGetter1, Line line)
    {
        PointGetter1 = pointGetter1;
        Line = line;
        PointGetter1.AddToChangeEvent(RefreshValues,this);
        Line.ShapeChanged += RefreshValues;
        RefreshValues();
    }
    public abstract override void RefreshValues();
}
public class VerticalLine : SpecialLine
{
    public VerticalLine(PointGetter pointGetter1, Line line) : base(pointGetter1, line)
    {
    }

    public override void RefreshValues()
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
    public ParallelLine(PointGetter pointGetter1, Line line):base(pointGetter1, line)
    {
    }
    public override void RefreshValues()
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
public class FittedLine : StraightLine
{
    private PointGetter[] PointGetters;
    private Vec[] Vecs;
    public FittedLine(PointGetter[] pointGetters)
    {
        PointGetters = pointGetters;
        Vecs= new Vec[pointGetters.Length];
        foreach(var i in pointGetters)
        {
            i.AddToChangeEvent(RefreshValues,this);
        }
        RefreshValues();
    }
    public override void RefreshValues()
    {
        double meanX=0,meanY=0;
        for(int i = 0; i < PointGetters.Length; i++)
        {
            Vecs[i] = PointGetters[i].GetPoint();
            meanX += Vecs[i].X;
            meanY += Vecs[i].Y;
        }
        meanX/= Vecs.Length;
        meanY/= Vecs.Length;
        double a = 0, c = 0;
        for (int i = 0; i < PointGetters.Length; i++)
        {
            double x = Vecs[i].X;
            double y = Vecs[i].Y;
            a += (x -meanX  ) * (y - meanY);
            c += (x - meanX) * (x-meanX);
        }
        double m = a / c;
        double b = meanY - m *meanX;
        if (c == 0)//x的常值函数
        {
            Point1 = new Vec(meanX,1);
            Point2= new Vec(meanX,2);
        }
        else
        {
            Point1 = new Vec(1,m+b);
            Point2= new Vec(2,2*m+b);
        }
        InvokeEvent();
    }

}*/