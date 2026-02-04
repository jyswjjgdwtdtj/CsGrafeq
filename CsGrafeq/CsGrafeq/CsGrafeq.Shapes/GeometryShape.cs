using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeq.Shapes;

/// <summary>
///     代表会被直接绘制的几何图形
/// </summary>
public abstract class GeometryShape : RefreshableShape
{
    public List<GeometryShape> SubShapes = new();

    /// <summary>
    ///     代表其Getter
    /// </summary>
    public abstract GeometryGetter Getter { get; }

    /// <summary>
    ///     鼠标是否在图形之上
    /// </summary>
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

    /// <summary>
    ///     是否被选中
    /// </summary>
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
            InvokeChanged();
        }
    } = false;

    /// <summary>
    ///     添加子几何图形
    /// </summary>
    /// <param name="subShape"></param>
    public void AddSubShape(GeometryShape subShape)
    {
        SubShapes.Add(subShape);
        ShapeChanged += subShape.RefreshValues;
    }

    /// <summary>
    ///     移除子几何图形
    /// </summary>
    /// <param name="subShape"></param>
    public void RemoveSubShape(GeometryShape subShape)
    {
        SubShapes.Remove(subShape);
        ShapeChanged -= subShape.RefreshValues;
    }

    /// <summary>
    ///     到一点最近的距离
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public abstract Vec DistanceTo(Vec vec);

    /// <summary>
    ///     Selected被改变
    /// </summary>
    public event ShapeChangedHandler<bool>? SelectedChanged;

    public override void Dispose()
    {
    }

    /// <summary>
    ///     是否与一个矩形相交
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public abstract bool IsIntersectedWithRect(CgRectangle rect);
}