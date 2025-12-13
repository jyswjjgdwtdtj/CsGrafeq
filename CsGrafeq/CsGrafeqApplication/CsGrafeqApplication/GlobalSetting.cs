//#define ALLOW_EXPRESSION
global using CsGrafeq;
global using CsGrafeq.I18N;
namespace CsGrafeqApplication;

public static class GlobalSetting
{
    /// <summary>
    ///     标识鼠标指针在触摸屏上可被视为“触摸”操作的范围（以像素为单位）。
    /// </summary>
    public static readonly double PointerTouchRange = OS.GetOSType() == OSType.Android ? 15 : 5;
    
    #if ALLOW_EXPRESSION
    public static bool AllowExpression { get; }=true;
    #else    
    public static bool AllowExpression { get; }=false;
    #endif
}