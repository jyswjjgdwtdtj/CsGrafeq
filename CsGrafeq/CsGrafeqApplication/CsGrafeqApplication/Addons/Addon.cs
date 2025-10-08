using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CsGrafeqApplication.Controls.Displayers;
using SkiaSharp;

namespace CsGrafeqApplication.Addons;

public abstract class Addon : UserControl
{
    public const bool DoNext = true;
    public const bool Intercept = false;
    private readonly OnceLock IsLoaded = new();

    private Displayer? _Owner;

    //Addon内部勿动
    public SKBitmap Bitmap = new();
    public bool IsEnabled = true;

    public virtual Displayer? Owner
    {
        get => _Owner;
        set
        {
            _Owner = value;
            IsLoaded.SetValueTrue();
        }
    }

    public abstract string Name { get; }
    protected abstract void Render(SKCanvas dc, SKRect rect);

    internal void AddonRender(SKCanvas dc, SKRect rect)
    {
        if (IsLoaded.Value)
            Render(dc, rect);
    }

    internal bool AddonKeyPress(KeyEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return KeyPress(e);
    }

    internal bool AddonKeyDown(KeyEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return KeyDown(e);
    }

    internal bool AddonKeyUp(KeyEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return KeyUp(e);
    }

    internal bool AddonPointerMoved(AddonPointerEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerMoved(e);
    }

    internal bool AddonPointerPressed(AddonPointerEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerPressed(e);
    }

    internal bool AddonPointerReleased(AddonPointerEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerReleased(e);
    }

    internal bool AddonPointerWheeled(AddonPointerWheelEventArgs e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerWheeled(e);
    }

    internal bool AddonPointerTapped(AddonPointerEventArgsBase e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerTapped(e);
    }

    internal bool AddonPointerDoubleTapped(AddonPointerEventArgsBase e)
    {
        if (!IsLoaded.Value || !IsEnabled)
            return DoNext;
        return PointerDoubleTapped(e);
    }

    //true代表可继续传递
    //false代表拦截
    protected virtual bool KeyDown(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool KeyUp(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool KeyPress(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool PointerReleased(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool PointerPressed(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool PointerMoved(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool PointerTapped(AddonPointerEventArgsBase e)
    {
        return DoNext;
    }

    protected virtual bool PointerDoubleTapped(AddonPointerEventArgsBase e)
    {
        return DoNext;
    }

    protected virtual bool PointerWheeled(AddonPointerWheelEventArgs e)
    {
        return DoNext;
    }

    private class OnceLock
    {
        public bool Value { get; private set; }

        public void SetValueTrue()
        {
            Value = true;
        }
    }

    public class AddonPointerEventArgsBase : EventArgs
    {
        public readonly KeyModifiers KeyModifiers;
        public readonly double X, Y;

        public AddonPointerEventArgsBase(double x, double y, KeyModifiers modifiers)
        {
            X = x;
            Y = y;
            KeyModifiers = modifiers;
        }

        public Point Location => new(X, Y);
    }

    public class AddonPointerEventArgs : AddonPointerEventArgsBase
    {
        public readonly PointerPointProperties Properties;
        public readonly double X, Y;

        public AddonPointerEventArgs(double x, double y, PointerPointProperties properties, KeyModifiers modifiers) :
            base(x, y, modifiers)
        {
            Properties = properties;
        }
    }

    public class AddonPointerWheelEventArgs : AddonPointerEventArgs
    {
        public readonly Vec Delta;
        public AddonPointerWheelEventArgs(double x, double y, PointerPointProperties properties, KeyModifiers modifiers,
            Vec delta) : base(x, y, properties, modifiers)
        {
            Delta = delta;
        }
    }
    public readonly CommandManager CmdManager = new();
    public void Undo()
    {
        CmdManager.UnDo();
        Owner?.Invalidate();
    }

    public void Redo()
    {
        CmdManager.ReDo();
        Owner?.Invalidate();
    }

    public Control Setting { get; init; }
}