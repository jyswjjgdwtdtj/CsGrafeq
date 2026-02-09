using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.Skia;
using Avalonia.Threading;
using CsGrafeq.I18N;
using CsGrafeqApplication.Addons;
using CsGrafeqApplication.Core.Controls;
using CsGrafeqApplication.Events;
using CsGrafeqApplication.ViewModels;
using ReactiveUI;
using SkiaSharp;
using AvaPoint = Avalonia.Point;

namespace CsGrafeqApplication.Controls.Displayers;

public abstract class Displayer : SKCanvasView, ICustomHitTest
{
    public static readonly DirectProperty<Displayer, DisplayerContainerViewModel> ContainerViewModelProperty = AvaloniaProperty.RegisterDirect<Displayer, DisplayerContainerViewModel>(
        nameof(ContainerViewModel), o => o.ContainerViewModel, (o, v) => o.ContainerViewModel = v);

    public DisplayerContainerViewModel ContainerViewModel
    {
        get => field;
        set => SetAndRaise(ContainerViewModelProperty, ref field, value);
    }
    public static readonly DirectProperty<Displayer, bool> IsRenderingProperty = AvaloniaProperty.RegisterDirect<Displayer, bool>(
        nameof(IsRendering), o => o.IsRendering);

    public bool IsRendering
    {
        get => field;
        protected set => SetAndRaise(IsRenderingProperty, ref field, value);
    }
    
    public const bool DoNext = true;
    public const bool Intercept = false;
    private readonly Clock RenderClock = new(1);
    protected AvaPoint LastPoint;
    protected PointerPointProperties LastPointerProperties = new();
    public List<Addon> NeedRenderingAddons = new();
    public bool NeedRenderingAll = false;
    public List<Renderable> NeedRenderingLayers = new();
    protected object TotalBufferLock = new();
    
    protected CancellationTokenSource RenderCts = new();

    public Displayer()
    {
        Addons.CollectionChanged += ChildrenChanged;
        Languages.LanguageChanged += () =>
        {
            ForceToRender(CancellationToken.None);
        };
        RenderClock.OnElapsed += Render;
        IsHitTestVisible = true;
        PropertyChanged += (s, e) =>
        {
            if (e.Property == ContainerViewModelProperty)
            {
                this[!DataContextProperty] = this[!ContainerViewModelProperty];
                ContainerViewModel?.WhenAnyValue(i => i.AddonIndex).Subscribe(d =>
                {
                    CompoundBuffers();
                    InvalidateVisual();
                });
            }
        };
    }
    protected SKBitmap TotalBuffer { get; set; } = new(1, 1);
    protected SKBitmap TempBuffer { get; set; } = new(1, 1);

    [Content] public AddonList Addons { get; } = new();

    /// <summary>
    ///     并无卵用
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool HitTest(AvaPoint point)
    {
        if (point.Y < 30) return false;
        return true;
    }

    protected sealed override void OnSKRender(SKRenderEventArgs e)
    {
        var dc = e.Canvas;
        lock (TotalBufferLock)
        {
            dc.DrawBitmap(TotalBuffer, SKPoint.Empty,SkiaHelper.CompoundBufferPaint);
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

    protected bool CallPointerPressed(MouseEventArgs e)
    {
        LastPoint = e.Position;
        LastPointerProperties = e.Properties;
        foreach (var addon in Addons)
            if (addon.CallPointerPressed(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallPointerMoved(MouseEventArgs e)
    {
        LastPoint = e.Position;
        LastPointerProperties = e.Properties;
        foreach (var addon in Addons)
            if (addon.CallPointerMoved(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallPointerReleased(MouseEventArgs e)
    {
        LastPoint = e.Position;
        LastPointerProperties = e.Properties;
        foreach (var addon in Addons)
            if (addon.CallPointerReleased(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallPointerWheeled(MouseEventArgs e)
    {
        LastPoint = e.Position;
        LastPointerProperties = e.Properties;
        foreach (var addon in Addons)
            if (addon.CallPointerWheeled(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    public abstract void Zoom(double del, AvaPoint center);

    protected bool CallPointerTapped(MouseEventArgs e)
    {
        var args = new MouseEventArgs(e.SourceEvent,e.SourceEventArgs,e.Position, e.KeyModifiers, LastPointerProperties);
        foreach (var addon in Addons)
            if (addon.CallPointerTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallPointerDoubleTapped(MouseEventArgs e)
    {
        var args = new MouseEventArgs(e.SourceEvent,e.SourceEventArgs,e.Position, e.KeyModifiers, LastPointerProperties);
        foreach (var addon in Addons)
            if (addon.CallPointerDoubleTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallKeyDown(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.CallKeyDown(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallKeyUp(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.CallKeyUp(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (e.NewSize.Width > e.PreviousSize.Width || e.NewSize.Height > e.PreviousSize.Height)
            if (e.NewSize.Width > TotalBuffer.Width || e.NewSize.Height > TotalBuffer.Height)
            {
                lock (TotalBufferLock)
                {
                    TotalBuffer.Dispose();
                    TempBuffer.Dispose();
                    TotalBuffer = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                    TempBuffer = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                    foreach (var adn in Addons)
                    foreach (var layer in adn.Layers)
                        layer.RenderTargetSize = new SKSizeI((int)e.NewSize.Width, (int)e.NewSize.Height);
                }

                ForceToRender(CancellationToken.None);
            }
    }

    /// <summary>
    ///     使所有层无效
    /// </summary>
    protected void Invalidate(CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        foreach (var i in Addons) Invalidate(i,ct);
    }

    /// <summary>
    ///     使Renderable层无效
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="ct"></param>
    protected void Invalidate(Renderable layer,CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        layer.ClearAndFlush();
        layer.Render(Bounds.ToSKRect(),ct);
        layer.Changed = false;
    }

    /// <summary>
    ///     使Addon中的所有层无效
    /// </summary>
    /// <param name="adn"></param>
    /// <param name="ct"></param>
    protected void Invalidate(Addon adn,CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        adn.AddonChanged = false;
        Task.WaitAll(adn.Layers.Select(i => Task.Run(() => Invalidate(i,ct), ct)), ct);
    }

    /// <summary>
    ///     强制重绘
    /// </summary>
    protected void ForceToRender(CancellationToken ct)
    {
        RenderClock.Cancel();
        Invalidate(ct);
        CompoundBuffers();
        InvalidateVisual();
    }

    private void Render()
    {
        Render(CancellationToken.None);
    }
    private void Render(CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        var paintflag = false;
        foreach (var adn in Addons)
            if (adn.AddonChanged)
            {
                paintflag = true;
                adn.AddonChanged = false;
                Invalidate(adn,ct);
            }
            else
            {
                foreach (var layer in adn.Layers)
                    if (layer.Changed)
                    {
                        paintflag = true;
                        layer.Changed = false;
                        Invalidate(layer,ct);
                    }
            }

        if (paintflag)
        {
            CompoundBuffers();
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
        }
    }

    /// <summary>
    ///     要求重绘
    /// </summary>
    internal void AskForRender()
    {
        RenderClock.Touch();
    }

    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var adn in Addons)
            if (adn.Owner != this)
            {
                adn.Owner = this;
                foreach (var i in adn.Layers)
                    i.RenderTargetSize = new SKSizeI((int)Max(Bounds.Width, 1), (int)Max(Bounds.Height, 1));
            }
    }

    /// <summary>
    ///     将每个Addon中的层合成入总缓冲区
    /// </summary>
    public abstract void CompoundBuffers();
}