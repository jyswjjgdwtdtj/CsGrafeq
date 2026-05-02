using CsGrafeq.Collections;
using CsGrafeq.Numeric.Exact;
using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;
using static CsGrafeq.Numeric.CsGrafeqMath;

namespace CsGrafeq.Shapes;

public class Point : GeometricShape
{
    public readonly DistinctList<TextGetter> TextGetters = new();

    public Point(PointGetter pointgetter)
    {
        TypeName = MultiLanguageResources.PointText;
        PointGetter = pointgetter;
        PointGetter.Attach(this);
        RefreshValues();
    }

    public PointGetter PointGetter
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Vec Location { get; private set; } = Vec.Empty;

    public override PointGetter Getter => PointGetter;

    public override void RefreshValues()
    {
        Location=PointGetter.GetPoint();
        InvokeChanged();
    }

    public override Vec NearestFrom(Vec vec)
    {
        return Location;
    }

    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        var v = Location - rect.Location;
        return RangeIn(0, rect.Size.X, v.X) && RangeIn(0, rect.Size.Y, v.Y);
    }
}