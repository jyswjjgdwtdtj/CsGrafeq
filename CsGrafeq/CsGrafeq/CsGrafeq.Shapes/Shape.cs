global using ShapeChangedHandler = System.Action;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public delegate void ShapeChangedHandler<T>(Shape shape, T args);

public delegate void ShapeChangedHandler<T1, T2>(T1 shape, T2 args) where T1 : Shape;

public abstract class Shape : ReactiveObject
{
    protected bool CanInteract = true;
    /// <summary>
    ///     As ARGB order
    /// </summary>
    public uint Color
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeEvent();
        }
    } = ColorExtension.GetRandomColor();

    /// <summary>
    ///     Refers to Visibility
    /// </summary>
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

    public abstract string TypeName { get; }
    public string Type => TypeName + ":";

    public string Description
    {
        get => field;
        protected set => this.RaiseAndSetIfChanged(ref field, value, nameof(Description));
    } = "";

    public virtual void InvokeEvent()
    {
        ShapeChanged?.Invoke();
    }

    public event ShapeChangedHandler? ShapeChanged;

    public bool IsDeleted
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeEvent();
        }
    } = false;
    public bool IsUserEnabled
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeEvent();
        }
    } = true;
    
}