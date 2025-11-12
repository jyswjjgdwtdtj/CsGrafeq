using CsGrafeq.Collections;
using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;
using static CsGrafeq.Utilities.CsGrafeqMath;
using static System.Math;

namespace CsGrafeq.Shapes;

public class Point : GeometryShape
{
    public readonly DistinctList<TextGetter> TextGetters = new();

    public Point(PointGetter pointgetter)
    {
        PointGetter = pointgetter;
        PointGetter.Attach(this);
        RefreshValues();
    }

    public PointGetter PointGetter
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Vector2Double Location { get; } = new(0, 0);

    public override PointGetter Getter => PointGetter;

    public override string TypeName => "Point";

    public override void RefreshValues()
    {
        var loc = PointGetter.GetPoint();
        Location.SetValue(loc.X, loc.Y);
        InvokeShapeChanged();
    }

    public override Vec NearestOf(Vec vec)
    {
        return Location;
    }
    public override bool IsIntersectedWithRect(CgRectangle rect)
    {
        var v = Location - rect.Location;
        return RangeIn(0, rect.Size.X, v.X) && RangeIn(0, rect.Size.Y, v.Y);
    }
}