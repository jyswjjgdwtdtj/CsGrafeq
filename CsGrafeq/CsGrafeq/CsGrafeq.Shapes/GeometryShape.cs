using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public abstract class GeometryShape : Shape
{
    public List<GeometryShape> SubShapes = new();
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
            bool v = field;
            this.RaiseAndSetIfChanged(ref field, value);
            if (v != value)
            {
                SelectedChanged?.Invoke(this, value);
            }
            InvokeEvent();
        }
    } = false;

    public abstract Vec HitTest(Vec vec);
    public abstract void RefreshValues();
    public event ShapeChangedHandler<bool>? SelectedChanged;
}