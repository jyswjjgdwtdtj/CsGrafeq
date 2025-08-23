using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeq.Shapes;

public class Circle : FilledShape
{
    private readonly CircleGetter CircleGetter;
    public CircleStruct InnerCircle;

    public Circle(CircleGetter cg)
    {
        CircleGetter = cg;
        cg.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }

    protected override string TypeName => "Circle";
    public double Radius => InnerCircle.Radius;
    public double LocY => InnerCircle.Center.Y;
    public double LocX => InnerCircle.Center.X;
    public override CircleGetter Getter => CircleGetter;

    public override void RefreshValues()
    {
        InnerCircle = CircleGetter.GetCircle();
        Description = $"Center:({LocX},{LocY}),Radius:{Radius}";
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        return (vec - InnerCircle.Center)-(vec - InnerCircle.Center).Unit() * InnerCircle.Radius;
    }
}

public struct CircleStruct
{
    public Vec Center;
    public double Radius;
}