using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Metadata;
using CsGrafeqApplication.Addons;
using SkiaSharp;
using AvaPoint = Avalonia.Point;
using AddonPointerEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgs;
using AddonPointerWheelEventArgs = CsGrafeqApplication.Addons.Addon.AddonPointerWheelEventArgs;
using AddonPointerEventArgsBase = CsGrafeqApplication.Addons.Addon.AddonPointerEventArgsBase;

namespace CsGrafeqApplication.Controls.Displayers;

public abstract class Displayer : SkiaControl
{
    public const bool DoNext = true;
    public const bool Intercept = false;
    private bool CanPerform = true;
    protected AvaPoint LastPoint;
    protected PointerPointProperties LastPointerProperties = new();
    protected SKBitmap TotalBuffer = new(1, 1);
    public bool ZoomingOptimization = false;
    public bool MovingOptimization = false;

    public Displayer()
    {
        Addons.CollectionChanged += ChildrenChanged;
        GestureRecognizers.Add(new ScrollGestureRecognizer
            { CanHorizontallyScroll = false, CanVerticallyScroll = false });
        GestureRecognizers.Add(new PinchGestureRecognizer());
        AddHandler(Gestures.ScrollGestureEvent, (s, e) => { Zoom(Pow(1.04, e.Delta.Y), Bounds.Center); });
        AddHandler(Gestures.PinchEvent, (s, e) => { Zoom(e.Scale - 1, e.ScaleOrigin); });
        Languages.LanguageChanged += Invalidate;
    }

    [Content] public AddonList Addons { get; } = new();

    public DisplayerContainer Owner { get; set; }

    protected sealed override void OnSkiaRender(SKRenderEventArgs e)
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
            if (addon.AddonPointerPressed(args) == Intercept)
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
            if (addon.AddonPointerMoved(args) == Intercept)
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
            if (addon.AddonPointerReleased(args) == Intercept)
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
            if (addon.AddonPointerWheeled(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    public abstract void Zoom(double del, AvaPoint center);

    protected bool CallAddonPointerTapped(TappedEventArgs e)
    {
        var loc = e.GetPosition(this);
        var args = new AddonPointerEventArgsBase(loc.X, loc.Y, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.AddonPointerTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonPointerDoubleTapped(TappedEventArgs e)
    {
        var loc = e.GetPosition(this);
        var args = new AddonPointerEventArgsBase(loc.X, loc.Y, e.KeyModifiers);
        foreach (var addon in Addons)
            if (addon.AddonPointerDoubleTapped(args) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonKeyDown(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.AddonKeyDown(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonKeyPress(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.AddonKeyPress(e) == Intercept)
                return Intercept;
        return DoNext;
    }

    protected bool CallAddonKeyUp(KeyEventArgs e)
    {
        foreach (var addon in Addons)
            if (addon.AddonKeyUp(e) == Intercept)
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
                    foreach (var i in Addons)
                    {
                        i.Bitmap.Dispose();
                        i.Bitmap = new SKBitmap((int)e.NewSize.Width, (int)e.NewSize.Height);
                    }

                    Invalidate();
                }
    }

    public void Invalidate(Addon addon)
    {
        if (!CanPerform)
            return;
        if (Addons.Contains(addon))
            using (var dc = new SKCanvas(addon.Bitmap))
            {
                dc.Clear();
                addon.AddonRender(dc, Bounds.ToSKRect());
            }

        CompoundBuffer();
        InvalidateVisual();
    }

    public void Invalidate()
    {
        if (!CanPerform)
            return;
        foreach (var i in Addons)
            using (var dc = new SKCanvas(i.Bitmap))
            {
                dc.Clear(SKColors.Transparent);
                i.AddonRender(dc, Bounds.ToSKRect());
            }

        CompoundBuffer();
        InvalidateVisual();
    }

    public void InvalidateNotUpdate()
    {
        if (!CanPerform)
            return;
        foreach (var i in Addons)
            using (var dc = new SKCanvas(i.Bitmap))
            {
                dc.Clear(SKColors.Transparent);
                i.AddonRender(dc, Bounds.ToSKRect());
            }

        CompoundBuffer();
    }

    public void InvalidateBuffer()
    {
        if (!CanPerform)
            return;
        CompoundBuffer();
        InvalidateVisual();
    }

    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var adn in Addons)
            if (adn.Owner == null)
            {
                adn.Bitmap = new SKBitmap((int)Max(Bounds.Width, 1), (int)Max(Bounds.Height, 1));
                adn.Owner = this;
                Invalidate(adn);
            }
    }

    public abstract void CompoundBuffer();

    public void Suspend()
    {
        CanPerform = false;
    }

    public void Resume(bool perform = true)
    {
        CanPerform = true;
        if (perform)
            Invalidate();
    }
}