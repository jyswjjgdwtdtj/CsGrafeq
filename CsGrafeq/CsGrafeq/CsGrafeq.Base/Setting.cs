#define CANDRAWFUNCTION

global using CsGrafeq;
global using CsGrafeq.I18N;
using System.ComponentModel.DataAnnotations;
using CsGrafeq.MVVM;
using ReactiveUI;
using SkiaSharp;

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
        set => this.RaiseAndSetIfChanged(ref field, value);
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

    public bool MoveOptimizationUserEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ZoomOptimizationUserEnabled
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ShowKeyboard
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ShowAxes
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ShowAxesNumber
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ShowAxesMajorGrid
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool ShowAxesMinorGrid
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;
    public int ClientSizeWidth
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 800;
    public int ClientSizeHeight
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 600;

    public SKBlendMode CompoundBlendMode
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = SKBlendMode.SrcOver;
}