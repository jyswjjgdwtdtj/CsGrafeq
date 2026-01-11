//#define CANDRAWFUNCTION

global using CsGrafeq;
global using CsGrafeq.I18N;
global using CsGrafeqApplication.Core.Utils;
using ReactiveUI;

namespace CsGrafeqApplication;

public class Setting:ReactiveObject
{
    public static Setting Instance { get; } = new();
    /// <summary>
    ///     标识鼠标指针在触摸屏上可被视为“触摸”操作的范围（以像素为单位）。
    /// </summary>
    public static readonly double PointerTouchRange = OS.GetOSType() == OSType.Android ? 15 : 5;

    /// <summary>
    /// 是否允许函数绘图
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
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = false;


}