using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;
using Avalonia;

namespace CsGrafeq.Shapes;

public abstract class GeometryShape : RefreshableShape
{
    public List<GeometryShape> SubShapes = new();

    public void AddSubShape(GeometryShape subShape)
    {
        SubShapes.Add(subShape);
        ShapeChanged += subShape.RefreshValues;
    }
    public void RemoveSubShape(GeometryShape subShape)
    {
        SubShapes.Remove(subShape);
        ShapeChanged -= subShape.RefreshValues;
    }
    public abstract GeometryGetter Getter { get; }

    public bool PointerOver
    {
        get => field;
        set
        {
            if (!CanInteract)
                return;
            this.RaiseAndSetIfChanged(ref field, value);
        }
    }

    public bool Selected
    {
        get => field;
        set
        {
            if (!CanInteract)
                return;
            var v = field;
            this.RaiseAndSetIfChanged(ref field, value);
            if (v != value) SelectedChanged?.Invoke(this, value);
            InvokeShapeChanged();
        }
    } = false;

    public abstract Vec NearestOf(Vec vec);
    public event ShapeChangedHandler<bool>? SelectedChanged;

    public override void Dispose()
    {
    }
    public abstract bool IsIntersectedWithRect(CgRectangle rect);
}