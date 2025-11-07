using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.Threading;
using CsGrafeqApplication.Addons;
using SkiaSharp;
using AddonPointerEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgs;
using AddonPointerEventArgsBase = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgsBase;
using AddonPointerWheelEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerWheelEventArgs;
using AvaPoint = Avalonia.Point;

namespace CsGrafeqApplication.Controls.Displayers;

public abstract class Displayer : SKCanvasView,ICustomHitTest
{
    public const bool DoNext = true;
    public const bool Intercept = false;
    private readonly Clock RenderClock = new(1);
    protected AvaPoint LastPoint;
    protected PointerPointProperties LastPointerProperties = new();
    public List<Addon> NeedRenderingAddons = new();
    public bool NeedRenderingAll = false;
    public List<Renderable> NeedRenderingLayers = new();
    protected SKBitmap TotalBuffer = new(1, 1);
    protected object TotalBufferLock = new();

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
        IsHitTestVisible = true;
        
    }
    /// <summary>
    /// 
    /// </summary>
    public bool ZoomingOptimization { get; set; } = false;
    public bool MovingOptimization { get; set; } = false;
    public bool ZOPEnable { get; set; } = true;
    public bool MOPEnable { get; set; } = true;

    [Content] public AddonList Addons { get; } = new();

    public DisplayerContainer Owner { get; set; }

    protected sealed override void OnSkiaRender(SKRenderEventArgs e)
    {
        var dc = e.Canvas;
        lock (TotalBufferLock)
        {
            dc.DrawBitmap(TotalBuffer, SKPoint.Empty);
        }
    }
    public bool HitTest(AvaPoint point)
    {
        if (point.Y < 30)
        {
            Console.WriteLine(point.ToString());
            return false;
        }
        return true;
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
        {
            if (e.NewSize.Width > TotalBuffer.Width || e.NewSize.Height > TotalBuffer.Height)
            {
                lock (TotalBufferLock)
                {
                    TotalBuffer.Dispose();
                    TotalBuffer = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                    foreach (var adn in Addons)
                    foreach (var layer in adn.Layers)
                        layer.SetBitmapSize(new SKSizeI((int)e.NewSize.Width, (int)e.NewSize.Height));
                }
                ForceToRender();
                
            }
        }
    }

    protected void Invalidate()
    {
        foreach (var i in Addons) Invalidate(i);
    }

    protected void Invalidate(Renderable layer)
    {
        using (var dc = layer.GetBitmapCanvas())
        {
            dc?.Clear();
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
        var paintflag = false;
        foreach (var adn in Addons)
            if (adn.Changed)
            {
                paintflag = true;
                adn.Changed = false;
                Invalidate(adn);
            }
            else
            {
                foreach (var layer in adn.Layers)
                    if (layer.Changed)
                    {
                        paintflag = true;
                        layer.Changed = false;
                        Invalidate(layer);
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
                foreach (var i in adn.Layers)
                {
                    i.SetBitmapSize(new SKSizeI((int)Max(Bounds.Width, 1), (int)Max(Bounds.Height, 1)));
                    adn.Owner = this;
                }
    }

    /// <summary>
    ///     将每个Addon中的层合成入总缓冲区
    /// </summary>
    public abstract void CompoundBuffers();
}