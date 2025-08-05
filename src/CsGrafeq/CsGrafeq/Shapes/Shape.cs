using ReactiveUI;

namespace CsGrafeq.Shapes;

public delegate void ShapeChangedHandler();

public delegate void ShapeChangedHandler<T>(Shape shape, T args);

public delegate void ShapeChangedHandler<T1, T2>(T1 shape, T2 args) where T1 : Shape;

public abstract class Shape : ReactiveObject
{
    protected bool CanInteract = true;



    public bool Visible
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeEvent();
        }
    } = true;

    public string Name
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    protected abstract string TypeName { get; }
    public string Type => TypeName + ":";
    public abstract string Description { get; }

    public virtual void InvokeEvent()
    {
        ShapeChanged?.Invoke();
    }

    public event ShapeChangedHandler? ShapeChanged;
}