using System.Text;
using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Shapes.GeometryMath;
using static CsGrafeq.Utilities.CsGrafeqMath;

namespace CsGrafeq.Shapes;

public abstract class Line : GeometryShape
{
    public LineStruct Current;
    public LineGetter LineGetter;

    public Line(LineGetter lineGetter)
    {
        LineGetter = lineGetter;
        LineGetter.Attach(this);
        RefreshValues();
    }

    public override LineGetter Getter => LineGetter;
    
    public override void RefreshValues()
    {
        Current = LineGetter.GetLine();
        Description = Current.ExpStr.Replace("x", "𝑥").Replace("y", "𝑦");
        InvokeShapeChanged();
    }

    public abstract bool CheckIsValid(Vec vec);

    public override Vec DistanceTo(Vec vec)
    {
        var res = DistanceToLine(Current.Point1, Current.Point2, vec, out var point);
        return CheckIsValid(point) ? point : Vec.Infinity;
    }

    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        return RangeIn(rect.Location.X, rect.Location.X + rect.Size.X,
            ((Current.Point1 + Current.Point2) / 2).X) && RangeIn(rect.Location.Y,
            rect.Location.Y + rect.Size.Y, ((Current.Point1 + Current.Point2) / 2).Y);
    }
}

public class Segment : Line
{
    public Segment(LineGetter_Segment segment) : base(segment)
    {
        TypeName = MultiLanguageResources.SegmentText;
    }

    public override void RefreshValues()
    {
        Current = LineGetter.GetLine();
        Description = Current.ExpStr + " " + Current.Distance;
        InvokeShapeChanged();
    }

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnSegment(Current.Point1, Current.Point2, vec);
    }
}

public class Half : Line
{
    public Half(LineGetter_Half segment) : base(segment)
    {
        TypeName = MultiLanguageResources.HalfLineText;
    }

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnHalf(Current.Point1, Current.Point2, vec);
    }
}

public class Straight : Line
{
    public Straight(LineGetter lineGetter) : base(lineGetter)
    {
        TypeName = MultiLanguageResources.StraightText;
    }

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnStraight(Current.Point1, Current.Point2, vec);
    }
}

public readonly struct LineStruct
{
    public readonly Vec Point1, Point2;

    public LineStruct(Vec point1, Vec point2)
    {
        Point1 = point1;
        Point2 = point2;
    }

    public (double a, double b, double c) GetNormal()
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
            var sb = new StringBuilder();
            if (a == 0)
            {
                //do nothing
            }
            else if (a == 1)
            {
                sb.Append("x");
            }
            else if (a == -1)
            {
                sb.Append("-x");
            }
            else
            {
                sb.Append(a + "x");
            }

            if (b == 0)
            {
                //do nothing
            }
            else if (b == 1)
            {
                sb.Append("+y");
            }
            else if (b == -1)
            {
                sb.Append("-y");
            }
            else if (b > 0)
            {
                sb.Append("+" + b + "y");
            }
            else
            {
                sb.Append(b + "y");
            }

            if (c == 0)
            {
                //do nothing
            }
            else if (c == 1)
            {
                sb.Append("+1");
            }
            else if (c == -1)
            {
                sb.Append("-1");
            }
            else if (c > 0)
            {
                sb.Append("+" + c);
            }
            else
            {
                sb.Append(c);
            }

            sb.Append("=0");
            return sb.ToString();
        }
    }

    public double Distance => (Point1 - Point2).GetLength();
}