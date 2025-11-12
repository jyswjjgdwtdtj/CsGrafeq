using CsGrafeq.Shapes.ShapeGetter;
using static System.Math;

namespace CsGrafeq.Shapes;

public class Circle : FilledShape
{
    private readonly CircleGetter CircleGetter;
    public CircleStruct InnerCircle;

    public Circle(CircleGetter cg)
    {
        CircleGetter = cg;
        cg.Attach(this);
        RefreshValues();
    }

    public override string TypeName => "Circle";
    public double Radius => InnerCircle.Radius;
    public double LocY => InnerCircle.Center.Y;
    public double LocX => InnerCircle.Center.X;
    public override CircleGetter Getter => CircleGetter;

    public override void RefreshValues()
    {
        InnerCircle = CircleGetter.GetCircle();
        Description = $"Center:({LocX},{LocY}),Radius:{Radius}";
        InvokeShapeChanged();
    }

    public override Vec NearestOf(Vec vec)
    {
        return InnerCircle.Center + (vec - InnerCircle.Center).Unit() * InnerCircle.Radius;
    }
    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        var o = rect.Location + rect.Size / 2;
        var cc = InnerCircle.Center - o;
        o = rect.Size / 2;
        cc.X = Abs(cc.X);
        cc.Y = Abs(cc.Y);
        var bc = cc - o;
        bc.X = Max(bc.X, 0);
        bc.Y = Max(bc.Y, 0);
        return bc.GetLength() <= InnerCircle.Radius;
    }
}

public struct CircleStruct
{
    public Vec Center;
    public double Radius;
}