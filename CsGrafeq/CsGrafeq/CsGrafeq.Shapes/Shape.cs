global using ShapeChangedHandler = System.Action;
using ReactiveUI;
using static CsGrafeq.Utilities.ColorHelper;

namespace CsGrafeq.Shapes;

public delegate void ShapeChangedHandler<T>(Shape shape, T args);

public delegate void ShapeChangedHandler<T1, T2>(T1 shape, T2 args) where T1 : Shape;

/// <summary>
///     几何图形基类
/// </summary>
public abstract class Shape : ReactiveObject, IDisposable
{
    /// <summary>
    ///     是否可以交互
    /// </summary>
    protected bool CanInteract
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    /// <summary>
    ///     颜色 argb格式
    /// </summary>
    public uint Color
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeShapeChanged();
        }
    } = GetRandomColor();

    /// <summary>
    ///     是否可见
    /// </summary>
    public bool Visible
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeShapeChanged();
        }
    } = true;

    /// <summary>
    ///     名字
    /// </summary>
    public string Name
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    /// <summary>
    ///     当前图形名称
    /// </summary>
    public abstract string TypeName { get; }

    /// <summary>
    ///     显示在UI上
    /// </summary>
    public string Type => TypeName + ":";

    /// <summary>
    ///     简介
    /// </summary>
    public string Description
    {
        get => field;
        protected set => this.RaiseAndSetIfChanged(ref field, value);
    } = "";

    /// <summary>
    ///     是否被删除（但仍保留在撤销链上）
    /// </summary>
    public bool IsDeleted
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeShapeChanged();
        }
    } = false;

    /// <summary>
    ///     用户是否可以从UI改变
    /// </summary>
    public bool IsUserEnabled
    {
        get => field;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
            InvokeShapeChanged();
        }
    } = true;

    public abstract void Dispose();

    /// <summary>
    ///     触发ShapeChanged 代表Shape被改变
    /// </summary>
    public void InvokeShapeChanged()
    {
        ShapeChanged?.Invoke();
    }

    public event ShapeChangedHandler? ShapeChanged;
}