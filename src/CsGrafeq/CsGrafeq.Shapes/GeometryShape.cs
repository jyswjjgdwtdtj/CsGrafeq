using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public abstract class GeometryShape : Shape
{
    public List<GeometryShape> SubShapes = new();
    public abstract GeometryGetter Getter { get; }
    public abstract Vec HitTest(Vec vec);
    public abstract void RefreshValues();
    
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
            if (field != value)
                SelectedChanged?.Invoke(this, value);
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeEvent();
        }
    } = false;
    public event ShapeChangedHandler<bool>? SelectedChanged;
}