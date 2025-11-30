using System;
using System.Collections.Generic;
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
    public readonly CommandManager CmdManager = new();
    private readonly OnceLock IsAddonLoaded = new();
    internal readonly List<Renderable> Layers = new();

    public bool Changed { get; set; } = false;

    //Addon内部勿动
    public bool IsAddonEnabled { get; set; } = true;

    public virtual Displayer? Owner
    {
        get => field;
        set
        {
            field = value;
            IsAddonLoaded.SetValueTrue();
        }
    }

    public abstract string AddonName { get; }

    public Control? Setting { get; init; }

    internal void CallAddonRender(SKCanvas dc, SKRect rect)
    {
        if (IsAddonLoaded.Value)
            foreach (var layer in Layers)
                layer.Render(dc, rect);
    }

    internal bool CallAddonKeyDown(KeyEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonKeyDown(e);
    }

    internal bool CallAddonKeyUp(KeyEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonKeyUp(e);
    }

    internal bool CallAddonPointerMoved(AddonPointerEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerMoved(e);
    }

    internal bool CallAddonPointerPressed(AddonPointerEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerPressed(e);
    }

    internal bool CallAddonPointerReleased(AddonPointerEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerReleased(e);
    }

    internal bool CallAddonPointerWheeled(AddonPointerWheelEventArgs e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerWheeled(e);
    }

    internal bool CallAddonPointerTapped(AddonPointerEventArgsBase e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerTapped(e);
    }

    internal bool CallAddonPointerDoubleTapped(AddonPointerEventArgsBase e)
    {
        if (!IsAddonLoaded.Value || !IsAddonEnabled || Owner == null)
            return DoNext;
        return AddonPointerDoubleTapped(e);
    }

    //true代表可继续传递
    //false代表拦截
    protected virtual bool AddonKeyDown(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool AddonKeyUp(KeyEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerReleased(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerPressed(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerMoved(AddonPointerEventArgs e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerTapped(AddonPointerEventArgsBase e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerDoubleTapped(AddonPointerEventArgsBase e)
    {
        return DoNext;
    }

    protected virtual bool AddonPointerWheeled(AddonPointerWheelEventArgs e)
    {
        return DoNext;
    }

    public void Undo()
    {
        CmdManager.UnDo();
    }

    public void Redo()
    {
        CmdManager.ReDo();
    }

    public abstract void Delete();
    public abstract void SelectAll();
    public abstract void DeSelectAll();

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
}