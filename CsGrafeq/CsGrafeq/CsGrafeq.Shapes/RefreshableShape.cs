namespace CsGrafeq.Shapes;

/// <summary>
///     需要刷新内容的图形
/// </summary>
public abstract class RefreshableShape : Shape
{
    /// <summary>
    ///     刷新内容
    /// </summary>
    public abstract void RefreshValues();
}