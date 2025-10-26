using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Metadata;
using Avalonia.Threading;
using CsGrafeqApplication.Addons;
using DynamicData;
using FastExpressionCompiler;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using AddonPointerEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgs;
using AddonPointerEventArgsBase = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgsBase;
using AddonPointerWheelEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerWheelEventArgs;
using AvaPoint = Avalonia.Point;

namespace CsGrafeqApplication.Controls.Displayers;

public abstract class Displayer : SKCanvasView
{
    public const bool DoNext = true;
    public const bool Intercept = false;
    protected AvaPoint LastPoint;
    protected PointerPointProperties LastPointerProperties = new();
    protected SKBitmap TotalBuffer = new(1, 1);
    private Clock RenderClock = new Clock(1);
    public bool ZoomingOptimization { get; set; } = false;
    public bool MovingOptimization { get; set; }= false;
    public bool ZOPEnable { get; set; } = true;
    public bool MOPEnable  { get; set; } = true;

    public Displayer()
    {
        Addons.CollectionChanged += ChildrenChanged;
        GestureRecognizers.Add(new ScrollGestureRecognizer
            { CanHorizontallyScroll = false, CanVerticallyScroll = false });
        GestureRecognizers.Add(new PinchGestureRecognizer());
        AddHandler(Gestures.ScrollGestureEvent, (s, e) => { Zoom(Pow(1.04, e.Delta.Y), Bounds.Center); });
        AddHandler(Gestures.PinchEvent, (s, e) => { Zoom(e.Scale, e.ScaleOrigin); });
        Languages.LanguageChanged += ForceToRender;
        RenderClock.OnElapsed += Render;
    }

    [Content] public AddonList Addons { get; } = new();

    public DisplayerContainer Owner { get; set; }

    protected override sealed void OnSkiaRender(SKRenderEventArgs e)
    {
        var dc = e.Canvas;
        lock (TotalBuffer)
        {
            dc.DrawBitmap(TotalBuffer, SKPoint.Empty);
        }
    }

    public abstract double MathToPixelX(double x);
    public abstract double MathToPixelY(double x);
    public abstract double PixelToMathX(double x);
    public abstract double PixelToMathY(double x);

    public AvaPoint MathToPixel(Vec vec)
    {
        return new AvaPoint(MathToPixelX(vec.X), MathToPixelY(vec.Y));
    }

    public SKPoint MathToPixelSK(Vec vec)
    {
        return new SKPoint((float)MathToPixelX(vec.X), (float)MathToPixelY(vec.Y));
    }

    public Vec PixelToMath(AvaPoint point)
    {
        return new Vec(PixelToMathX(point.X), PixelToMathY(point.Y));
    }

    public Vec PixelToMath(SKPoint point)
    {
        return new Vec(PixelToMathX(point.X), PixelToMathY(point.Y));
    }

    protected bool CallAddonPointerPressed(PointerPressedEventArgs e)
    {
        LastPoint = e.GetPosition(this);
        LastPointerProperties = e.Properties;
        var loc = LastPoint;
        var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.CallAddonPointerPressed(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonPointerMoved(PointerEventArgs e)
    {
        LastPoint = e.GetPosition(this);
        LastPointerProperties = e.Properties;
        var loc = LastPoint;
        var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.CallAddonPointerMoved(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonPointerReleased(PointerReleasedEventArgs e)
    {
        LastPoint = e.GetPosition(this);
        LastPointerProperties = e.Properties;
        var loc = LastPoint;
        var args = new AddonPointerEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.CallAddonPointerReleased(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonPointerWheeled(PointerWheelEventArgs e)
    {
        LastPoint = e.GetPosition(this);
        LastPointerProperties = e.Properties;
        var loc = LastPoint;
        var args = new AddonPointerWheelEventArgs(loc.X, loc.Y, e.Properties, e.KeyModifiers,
            new Vec(e.Delta.X, e.Delta.Y));
        foreach (var addon in Addons)
            if (addon.CallAddonPointerWheeled(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    public abstract void Zoom(double del, AvaPoint center);

    protected bool CallAddonPointerTapped(TappedEventArgs e)
    {
        var loc = e.GetPosition(this);
        var args = new AddonPointerEventArgsBase(loc.X, loc.Y, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.CallAddonPointerTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonPointerDoubleTapped(TappedEventArgs e)
    {
        var loc = e.GetPosition(this);
        var args = new AddonPointerEventArgsBase(loc.X, loc.Y, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.CallAddonPointerDoubleTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonKeyDown(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.CallAddonKeyDown(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonKeyUp(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.CallAddonKeyUp(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (e.NewSize.Width > e.PreviousSize.Width || e.NewSize.Height > e.PreviousSize.Height)
            if ((int)e.NewSize.Width != (int)e.PreviousSize.Width || (int)e.NewSize.Height != e.PreviousSize.Height)
                lock (TotalBuffer)
                {
                    TotalBuffer.Dispose();
                    TotalBuffer = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                    foreach (var adn in Addons)
                    {
                        foreach (var layer in adn.Layers)
                        {
                            layer.Bitmap.Dispose();
                            layer.Bitmap = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                        }
                    }
                    ForceToRender();
                }
    }
    protected void Invalidate()
    {
        foreach(var i in Addons)
        {
            Invalidate(i);
        }
    }
    protected void Invalidate(Renderable layer)
    {
        using (var dc = new SKCanvas(layer.Bitmap))
        {
            dc.Clear();
            layer.Render(dc, Bounds.ToSKRect());
        }
        layer.Changed = false;
    }

    protected void Invalidate(Addon adn)
    {
        foreach (var layer in adn.Layers)
            Invalidate(layer);
        adn.Changed = false;
    }
    protected void ForceToRender()
    {
        RenderClock.Cancel();
        Invalidate();
        CompoundBuffers();
        InvalidateVisual();
    }
    private void Render()
    {
        bool paintflag = false;
        foreach (var adn in Addons)
        {
            if (adn.Changed)
            {
                paintflag = true;
                adn.Changed = false;
                Invalidate(adn);
            }
            else
            {
                foreach (var layer in adn.Layers)
                {
                    if (layer.Changed)
                    {
                        paintflag = true;
                        layer.Changed = false;
                        Invalidate(layer);
                    }
                }
            }
        }
        if (paintflag)
        {
            CompoundBuffers();
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
        }
    }
    internal void AskForRender()
    {
        RenderClock.Touch();
    }
    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var adn in Addons)
            if (adn.Owner != this)
            {
                foreach(var i in adn.Layers)
                {
                    i.Bitmap?.Dispose();
                    i.Bitmap = new SKBitmap((int)Max(Bounds.Width, 1), (int)Max(Bounds.Height, 1));
                    adn.Owner = this;
                }
            }
    }
    /// <summary>
    /// 将每个Addon中的层合成入总缓冲区
    /// </summary>
    public abstract void CompoundBuffers();
    public List<Renderable> NeedRenderingLayers=new();
    public List<Addon> NeedRenderingAddons=new();
    public bool NeedRenderingAll = false;
}