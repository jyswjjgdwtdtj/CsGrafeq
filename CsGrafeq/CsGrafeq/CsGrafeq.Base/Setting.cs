//#define CANDRAWFUNCTION

global using CsGrafeq;
global using CsGrafeq.I18N;
using CsGrafeq.MVVM;
using ReactiveUI;

namespace CsGrafeq;

public class Setting : ObservableObject
{
    /// <summary>
    ///     标识鼠标指针在触摸屏上可被视为“触摸”操作的范围（以像素为单位）。
    /// </summary>
    public static readonly double PointerTouchRange = OS.GetOSType() == OSType.Android ? 15 : 5;

    public static Setting Instance { get; } = new();

    /// <summary>
    ///     是否允许函数绘图
    /// </summary>
#if CANDRAWFUNCTION
    public static bool CanDrawFunction { get; } = true;
#else
    public static bool CanDrawFunction { get; } = false;
#endif
    public byte DefaultOpacity
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 128;

    public bool EnableExpressionSimplification
    {
        get;
        set
        {
            this.RaiseAndSetIfChanged(ref field, value);
        }
    } = false;

    public bool MoveOptimization
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public bool ZoomOptimization
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;

    public bool ShowKeyboard
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;
}