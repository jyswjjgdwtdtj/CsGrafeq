#define CANDRAWFUNCTION

global using CsGrafeq;
global using CsGrafeq.I18N;
using CsGrafeq.MVVM;
using ReactiveUI;
using SkiaSharp;

namespace CsGrafeq.Setting;

public partial class Setting : ObservableObject
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
}