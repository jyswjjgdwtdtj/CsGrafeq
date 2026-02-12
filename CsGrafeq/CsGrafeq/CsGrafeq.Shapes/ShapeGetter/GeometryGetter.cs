using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public abstract class GeometryGetter : Getter
{
    /// <summary>
    ///     操作名称
    /// </summary>
    public abstract MultiLanguageData ActionName { get; }

    /// <summary>
    ///     用到的图形 即显示在UI上“函数”的参数
    /// </summary>
    public IReadOnlyList<ShapeParameter>? ShapeParameters { get; init; }

    public IReadOnlyList<NumberParameter>? NumberParameters { get; init; }

    /// <summary>
    ///     由绑定到的图形调用 代表完成Getter的桥接作用 将这个Getter对应的图形绑定到其基于的图形
    /// </summary>
    /// <param name="subShape">代表要绑定的图形</param>
    public abstract void Attach(GeometricShape subShape);

    /// <summary>
    ///     取消桥接
    /// </summary>
    /// <param name="subShape"></param>
    public abstract void UnAttach(GeometricShape subShape);

    public virtual bool Adjust()
    {
        return false;
    }
}