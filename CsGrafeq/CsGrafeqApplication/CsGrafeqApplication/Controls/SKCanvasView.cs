using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace CsGrafeqApplication.Controls;
/// <summary>
/// A Xaml canvas control that can be drawn on using SkiaSharp drawing commands
/// which facilitates porting from existing Xamarin Forms applications.
/// </summary>
/// <remarks>
/// See: https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.Views/SkiaSharp.Views.UWP/SKXamlCanvas.cs.
/// <see cref="Decorator"/> was used instead of <see cref="Canvas"/>,
/// because <see cref="Decorator"/> facilitates the relative positioning of any additional controls.
/// </remarks>
public class SKCanvasView: UserControl
{
    /// <summary>
    ///     用于DrawingContext的Custom方法
    /// </summary>
    private readonly CustomDrawOperation customDrawOper;

    public SKCanvasView()
    {
        Background = new SolidColorBrush(Colors.Transparent);
        customDrawOper = new CustomDrawOperation(ValidRect);
        customDrawOper.SKDraw += (s, e) => { OnSkiaRender(e); };
        OnValidRectRefresh();
        SizeChanged += (s, e) => { OnValidRectRefresh(); };
#if Windows
#endif
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
    protected virtual void OnSkiaRender(SKRenderEventArgs e)
    {
        SkiaRender?.Invoke(this, e);
    }

    public sealed override void Render(DrawingContext context)
    {
        context.Custom(customDrawOper);
    }

    public virtual void OnValidRectRefresh()
    {
        ValidRect = Bounds;
    }

    private class CustomDrawOperation : ICustomDrawOperation
    {
        private readonly object BufferLock = new();
        private WriteableBitmap Buffer;
        public Vector Dpi { get; set; }=new Vector(300, 300);

        public CustomDrawOperation(Rect bounds, uint clearColor = 0x00FFFFFF)
        {
            Bounds = bounds;
            SKDraw += (s, e) => { e.Canvas.Clear(clearColor); };
            Buffer = new WriteableBitmap(new PixelSize(Max((int)bounds.Width, 100), Max((int)bounds.Height, 100)),
                Dpi);
        }

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
            return true;
        }

        public bool Equals(ICustomDrawOperation? other)
        {
            return false;
        }

        public void Render(ImmediateDrawingContext context)
        {
            lock (BufferLock)
            {
                using (var lb = Buffer.Lock())
                {
                    var info = new SKImageInfo(lb.Size.Width, lb.Size.Height, lb.Format.ToSkColorType(),
                        SKAlphaType.Premul);
                    using (var surface = SKSurface.Create(info, lb.Address, lb.RowBytes))
                    {
                        using var canvas = surface.Canvas;
                        canvas.Scale(2f);
                        SKDraw?.Invoke(null, new SKRenderEventArgs(canvas));
                    }
                }
                context.DrawBitmap(Buffer, Bounds, Bounds);
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

    protected class PropertyResetEventArgs<T>(T FormerValue)
    {
        public readonly T Value = FormerValue;
    }
}