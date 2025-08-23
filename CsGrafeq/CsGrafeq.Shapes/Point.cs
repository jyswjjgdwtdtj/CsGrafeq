using CsGrafeq.Collections;
using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public class Point : GeometryShape
{
    public readonly DistinctList<TextGetter> TextGetters = new();
    private Vec _Location;
    public PointGetter PointGetter;

    public Point(PointGetter pointgetter)
    {
        //if(pointgetter is PointGetter_FromPoint)
        //throw new ArgumentNullException(nameof(pointgetter)+" 不能为指向点");
        PointGetter = pointgetter;
        PointGetter.AddToChangeEvent(RefreshValues, this);
        RefreshValues();
    }

    public Vec Location => _Location;

    public double LocationX
    {
        get => Location.X;
        set
        {
            if (PointGetter is PointGetter_Movable pm)
            {
                pm.SetControlPoint(new Vec(value, LocationY));
                RefreshValues();
                return;
            }

            throw new Exception();
        }
    }

    public double LocationY
    {
        get => Location.Y;
        set
        {
            if (PointGetter is PointGetter_Movable pm)
            {
                pm.SetControlPoint(new Vec(LocationX, value));
                RefreshValues();
                return;
            }

            throw new Exception();
        }
    }

    public override PointGetter Getter => PointGetter;

    protected override string TypeName => "Point";
    
    public override void RefreshValues()
    {
        var p = PointGetter.GetPoint();
        this.RaiseAndSetIfChanged(ref _Location.X, p.X, nameof(LocationX));
        this.RaiseAndSetIfChanged(ref _Location.Y, p.Y, nameof(LocationY));
        InvokeEvent();
    }

    public override Vec HitTest(Vec vec)
    {
        return vec - Location;
    }
}