global using ShapeChangedHandler = System.Action;
using ReactiveUI;

namespace CsGrafeq.Shapes;

public delegate void ShapeChangedHandler<T>(Shape shape, T args);

public delegate void ShapeChangedHandler<T1, T2>(T1 shape, T2 args) where T1 : Shape;

/// <summary>
///     几何图形基类
/// </summary>
public abstract class Shape : InteractiveObject
{
    public ShapeList? Owner { get; internal set; }

    /// <summary>
    ///     名字
    /// </summary>
    public string Name
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";


    /// <summary>
    ///     触发ShapeChanged 代表Shape被改变
    /// </summary>
    public override void InvokeChanged()
    {
        ShapeChanged?.Invoke();
    }

    public event ShapeChangedHandler? ShapeChanged;
}