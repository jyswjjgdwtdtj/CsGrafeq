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
    public override string Description => $"Center:({LocX},{LocY}),Radius:{Radius}";

    public override void RefreshValues()
    {
        InnerCircle = CircleGetter.GetCircle();
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        if (Filled)
            if ((vec - InnerCircle.Center).GetLength() - InnerCircle.Radius < 0)
                return Vec.Empty;
            else
                return (vec - InnerCircle.Center).Unit() * InnerCircle.Radius - vec;
        return (vec - InnerCircle.Center).Unit() * InnerCircle.Radius - vec;
    }
}

public struct CircleStruct
{
    public Vec Center;
    public double Radius;
}