using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;
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

    public override string TypeName => "Line";

    public override LineGetter Getter => LineGetter;
    public TwoPoint LineData => Current;


    public override void RefreshValues()
    {
        Current = LineGetter.GetLine();
        Description = Current.ExpStr.Replace("x", "𝑥").Replace("y", "𝑦");
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

    public override string TypeName => "Segment";
    public override void RefreshValues()
    {
        Current = LineGetter.GetLine();
        Description = Current.ExpStr + " " + Current.Distance;
        InvokeEvent();
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
    }

    public override string TypeName => "Half";

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

    public override string TypeName => "Straight";

    public override bool CheckIsValid(Vec vec)
    {
        return FuzzyOnStraight(Current.Point1, Current.Point2, vec);
    }
}