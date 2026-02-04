using CsGrafeq.Shapes.ShapeGetter;
using static System.Math;

namespace CsGrafeq.Shapes;

public class Circle : FilledShape
{
    private readonly CircleGetter CircleGetter;
    public CircleStruct Current;

    public Circle(CircleGetter cg)
    {
        TypeName = MultiLanguageResources.CircleText;
        CircleGetter = cg;
        cg.Attach(this);
        RefreshValues();
    }

    public double Radius => Current.Radius;
    public double LocY => Current.Center.Y;
    public double LocX => Current.Center.X;
    public override CircleGetter Getter => CircleGetter;

    public override void RefreshValues()
    {
        Current = CircleGetter.GetCircle();
        Description =
            $"{MultiLanguageResources.CircleCenterText}:({LocX},{LocY}),{MultiLanguageResources.RadiusText}:{Radius}";
        InvokeChanged();
    }

    public override Vec DistanceTo(Vec vec)
    {
        return Current.Center + (vec - Current.Center).Unit() * Current.Radius;
    }

    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        var o = rect.Location + rect.Size / 2;
        var cc = Current.Center - o;
        o = rect.Size / 2;
        cc.X = Abs(cc.X);
        cc.Y = Abs(cc.Y);
        var bc = cc - o;
        bc.X = Max(bc.X, 0);
        bc.Y = Max(bc.Y, 0);
        return bc.GetLength() <= Current.Radius;
    }
}

public struct CircleStruct
{
    public Vec Center;
    public double Radius;
}