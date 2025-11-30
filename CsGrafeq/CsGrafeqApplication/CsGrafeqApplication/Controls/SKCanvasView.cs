#define  USE_FIRST
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace CsGrafeqApplication.Controls;
#if  USE_FIRST
/// <summary>
///     A Xaml canvas control that can be drawn on using SkiaSharp drawing commands
///     which facilitates porting from existing Xamarin Forms applications.
/// </summary>
/// <remarks>
///     See: https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.Views/SkiaSharp.Views.UWP/SKXamlCanvas.cs.
///     <see cref="Decorator" /> was used instead of <see cref="Canvas" />,
///     because <see cref="Decorator" /> facilitates the relative positioning of any additional controls.
/// </remarks>
public class SKCanvasView : Control
{
    /// <summary>
    ///     用于DrawingContext的Custom方法
    /// </summary>
    private readonly CustomDrawOperation customDrawOper;

    public SKCanvasView()
    {
        customDrawOper = new CustomDrawOperation(ValidRect, this);
        customDrawOper.SKDraw += (s, e) => { OnSKRender(e); };
        OnValidRectRefresh();
        SizeChanged += (s, e) => { OnValidRectRefresh(); };
    }

    public Rect ValidRect
    {
        get => field;
        private set
        {
            field = value;
            customDrawOper.Bounds = value;
        }
    }

    /// <summary>
    ///     绘制事件
    /// </summary>
    public event EventHandler<SKRenderEventArgs>? SkiaRender;

    /// <summary>
    ///     绘制函数
    /// </summary>
    protected virtual void OnSKRender(SKRenderEventArgs e)
    {
        SkiaRender?.Invoke(this, e);
    }

    public sealed override void Render(DrawingContext context)
    {
        context.Custom(customDrawOper);
    }

    protected virtual void OnValidRectRefresh()
    {
        ValidRect = Bounds;
    }

    private class CustomDrawOperation : ICustomDrawOperation
    {
        private readonly object BufferLock = new();
        private WriteableBitmap Buffer;
        private SKCanvasView Canvas;

        public CustomDrawOperation(Rect bounds, SKCanvasView canvas, uint clearColor = 0x00FFFFFF)
        {
            Bounds = bounds;
            SKDraw += (s, e) => { e.Canvas.Clear(clearColor); };
            Buffer = new WriteableBitmap(new PixelSize(Max((int)bounds.Width, 100), Max((int)bounds.Height, 100)),
                Dpi);
            Canvas = canvas;
        }

        public Vector Dpi { get; } = new(300, 300);

        public void Dispose()
        {
        }

        public Rect Bounds
        {
            get => field;
            set
            {
                if (field == value) return;
                lock (BufferLock)
                {
                    if (field.Width < value.Width || field.Height < value.Height)
                    {
                        Buffer.Dispose();
                        Buffer = new WriteableBitmap(new PixelSize((int)value.Width, (int)value.Height),
                            Dpi);
                    }
                }

                field = value;
            }
        }

        public bool HitTest(Point p)
        {
            return false;
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }

        public void Render(ImmediateDrawingContext context)
        {
            context.DrawRectangle(new ImmutableSolidColorBrush(Colors.Green),null, new Rect(Bounds.Size));
            lock (BufferLock)
            {
                using (var lb = Buffer.Lock())
                {
                    var info = new SKImageInfo(lb.Size.Width, lb.Size.Height, lb.Format.ToSkColorType(),
                        SKAlphaType.Premul);
                    using (var surface = SKSurface.Create(info, lb.Address, lb.RowBytes))
                    {
                        using var canvas = surface.Canvas;
                        SKDraw?.Invoke(null, new SKRenderEventArgs(canvas));
                    }
                }

                context.DrawBitmap(Buffer, Bounds, new Rect(Bounds.Size));
            }
        }

        public event EventHandler<SKRenderEventArgs> SKDraw;
    }

    public class SKRenderEventArgs : EventArgs
    {
        public SKRenderEventArgs(SKCanvas canvas)
        {
            Canvas = canvas;
        }

        public SKCanvas Canvas { get; init; }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
    }
}
#else
public class SKCanvasView : Control
{
    class RenderingLogic : ICustomDrawOperation
    {
        public Action<SKCanvas>? RenderCall;
        public Rect Bounds { get; set; }
        public void Dispose() {}
        public bool Equals(ICustomDrawOperation? other) => other == this;
        public bool HitTest(Point p) { return false; }

        public void Render(ImmediateDrawingContext context)
        {
            context.DrawRectangle(new ImmutableSolidColorBrush(Colors.Transparent),null, Bounds);
            var skia = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (skia != null)
            {
                using (var lease = skia.Lease()) {
                    SKCanvas canvas = lease.SkCanvas;
                    canvas.ClipRect(new Rect(Bounds.Size).ToSKRect());
                    canvas.Clear(SKColors.Transparent);
                    RenderCall?.Invoke(canvas);
                }
            }
        }
    }

    private RenderingLogic renderingLogic;
    protected virtual void OnSKRender(SKRenderEventArgs e)
    {
        SKRender?.Invoke(this, e);
    }

    public event EventHandler<SKRenderEventArgs>? SKRender;

    public SKCanvasView()
    {
        renderingLogic = new RenderingLogic();
        renderingLogic.RenderCall += (canvas) => OnSKRender(new SKRenderEventArgs(canvas));
        OnValidRectRefresh();
        SizeChanged += (s, e) => { OnValidRectRefresh(); };
    }

    public sealed override void Render(DrawingContext context)
    {
        context.Custom(renderingLogic);
    }
    protected virtual void OnValidRectRefresh()
    {
        ValidRect = Bounds;
    }
    public Rect ValidRect
    {
        get => field;
        private set
        {
            field = value;
            renderingLogic.Bounds = value;
        }
    }
}
public class SKRenderEventArgs : EventArgs
{
    public SKRenderEventArgs(SKCanvas canvas)
    {
        Canvas = canvas;
    }

    public SKCanvas Canvas { get; init; }
}
#endif