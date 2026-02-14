using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using CsGrafeq.I18N;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.Events;
using ReactiveUI;
using SkiaSharp;
using AvaPoint = Avalonia.Point;
using static CsGrafeqApplication.Extension;

namespace CsGrafeqApplication.Addons;

public abstract class Addon : ReactiveObject
{
    protected const bool DoNext = true;
    protected const bool Intercept = false;
    internal readonly List<Renderable> Layers = new();

    protected Addon()
    {
        AddonName = new MultiLanguageData { Chinese = "", English = "" };
        InfoTemplate = MainTemplate = new FuncDataTemplate<object?>(o => true, o => new Control());
    }

    public IDataTemplate? InfoTemplate { get; init; }
    public IDataTemplate? MainTemplate { get; init; }

    public bool AddonChanged { get; set; }

    /// <summary>
    ///     所有者
    /// </summary>
    public virtual Displayer? Owner
    {
        get => field;
        set
        {
            if (!(value is CartesianDisplayer))
                throw new Exception();
            field = value ?? throw new ArgumentNullException(nameof(value));
            PixelToMathX = value.PixelToMathX;
            PixelToMathY = value.PixelToMathY;
            PixelToMath = value.PixelToMath;
            PixelToMathSk = value.PixelToMath;
            MathToPixelX = value.MathToPixelX;
            MathToPixelY = value.MathToPixelY;
            MathToPixel = value.MathToPixel;
            MathToPixelSk = value.MathToPixelSk;
        }
    }

    public MultiLanguageData AddonName { get; protected init; }

    internal bool CallKeyDown(KeyEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnKeyDown(e);
    }

    internal bool CallKeyUp(KeyEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnKeyUp(e);
    }

    internal bool CallPointerPressed(MouseEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnPointerPressed(e);
    }

    internal void CallPointerMoved(MouseEventArgs e)
    {
        if ( Owner == null)
            return;
        OnPointerMoved(e);
    }

    internal void CallPointerReleased(MouseEventArgs e)
    {
        if ( Owner == null)
            return;
        OnPointerReleased(e);
    }

    internal bool CallPointerWheeled(MouseEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnPointerWheeled(e);
    }

    internal bool CallPointerTapped(MouseEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnPointerTapped(e);
    }

    internal bool CallPointerDoubleTapped(MouseEventArgs e)
    {
        if ( Owner == null)
            return DoNext;
        return OnPointerDoubleTapped(e);
    }

    //true代表可继续传递
    //false代表拦截
    protected virtual bool OnKeyDown(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool OnKeyUp(KeyEventArgs e)
    {
        return DoNext;
    }


    protected virtual bool OnPointerPressed(MouseEventArgs e)
    {
        return DoNext;
    }

    protected virtual void OnPointerMoved(MouseEventArgs e)
    {
    }
    
    protected virtual void OnPointerReleased(MouseEventArgs e)
    {
    }

    protected virtual bool OnPointerTapped(MouseEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool OnPointerDoubleTapped(MouseEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool OnPointerWheeled(MouseEventArgs e)
    {
        return DoNext;
    }

    public abstract void Delete();
    public abstract void SelectAll();
    public abstract void DeselectAll();

    #region CoordinateTransformFuncs

    /// <summary>
    ///     将数学坐标转换为像素坐标（Avalonia.Point） 来自Owner
    /// </summary>
    protected Func<Vec, AvaPoint> MathToPixel = VoidFunc<Vec, AvaPoint>;

    /// <summary>
    ///     将数学坐标转换为像素坐标（SKPoint） 来自Owner
    /// </summary>
    protected Func<Vec, SKPoint> MathToPixelSk = VoidFunc<Vec, SKPoint>;

    /// <summary>
    ///     将像素坐标转换为数学坐标（Avalonia.Point） 来自Owner
    /// </summary>
    protected Func<AvaPoint, Vec> PixelToMath = VoidFunc<AvaPoint, Vec>;

    /// <summary>
    ///     将像素坐标转换为数学坐标（SKPoint） 来自Owner
    /// </summary>
    protected Func<SKPoint, Vec> PixelToMathSk = VoidFunc<SKPoint, Vec>;

    protected Func<double, double> PixelToMathX = VoidFunc<double, double>,
        PixelToMathY = VoidFunc<double, double>,
        MathToPixelX = VoidFunc<double, double>,
        MathToPixelY = VoidFunc<double, double>;

    #endregion
}