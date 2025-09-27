using CsGrafeq.Collections;
using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public class Point : GeometryShape
{
    public readonly DistinctList<TextGetter> TextGetters = new();
    private Vec _Location;
    public PointGetter PointGetter { 
        get {
            return field;
        }
        set {
            this.RaiseAndSetIfChanged(ref field, value,nameof(PointGetter));
        } 
    }

    public Point(PointGetter pointgetter)
    {
        PointGetter = pointgetter;
        PointGetter.Attach(RefreshValues, this);
        RefreshValues();
    }

    public Vec Location => _Location;

    public override PointGetter Getter => PointGetter;

    public override string TypeName => "Point";
    
    public override void RefreshValues()
    {
        _Location = PointGetter.GetPoint();
        Console.WriteLine("GetPoint:"+_Location);
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        return vec - Location;
    }
}