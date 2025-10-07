using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Intrinsics.Arm;

namespace CsGrafeqApplication.Controls;

/// <summary>
///     使用SkiaSharp直接绘制图形
/// </summary>
public class SkiaControl :UserControl
{
    /// <summary>
    ///     用于DrawingContext的Custom方法
    /// </summary>
    private readonly CustomDrawOperation customDrawOper;
    public SkiaControl()
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
        private WriteableBitmap Buffer;
        private object BufferLock=new object();
        public CustomDrawOperation(Rect bounds, uint clearColor = 0x00FFFFFF)
        {
            Bounds = bounds;
            SKDraw += (s, e) => { e.Canvas.Clear(clearColor); };
            Buffer = new WriteableBitmap(new PixelSize((int)(System.Math.Max((int)bounds.Width,100)), (int)((System.Math.Max((int)bounds.Height,100)))), new Vector(96,96));
        }

        public void Dispose()
        {
        }

        public Rect Bounds { get=>field; 
            set
            {
                if (field == value) return;
                lock (BufferLock)
                {
                    if (field.Width < value.Width || field.Height < value.Height)
                    {
                        Buffer.Dispose();
                        Buffer = new WriteableBitmap(new PixelSize((int)(value.Width), (int)(value.Height)), new Vector(96,96));
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
                    var info = new SKImageInfo(lb.Size.Width, lb.Size.Height, lb.Format.ToSkColorType(), SKAlphaType.Premul);
                    using (SKSurface surface = SKSurface.Create(info, lb.Address, lb.RowBytes))
                    {
                        using SKCanvas canvas = surface.Canvas;
                        SKDraw?.Invoke(null,new SKRenderEventArgs(canvas));
                    }
                }
                context.DrawBitmap(Buffer, Bounds);
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

public static class SkiaEx
{
    public static SKColor Light;
    public static SKColor Median;
    public static SKPaint FilledBlack = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint FilledRed = new() { IsAntialias = true, Color = SKColors.Red };
    public static SKPaint StrokeBlack = new() { IsAntialias = true, Color = SKColors.Black, IsStroke = true };
    public static SKPaint StrokeRed = new() { IsAntialias = true, Color = SKColors.Red, IsStroke = true };
    public static SKPaint FilledBlue = new() { IsAntialias = true, Color = SKColors.Blue };
    public static SKPaint FilledWhite = new() { IsAntialias = true, Color = SKColors.White };

    public static SKPaint FilledTranparentGrey =
        new() { IsAntialias = true, Color = new SKColor(0x80, 0x80, 0x80, 70) };

    public static SKFont TextFont;
    public static SKPaint FilledGray1 = new() { IsAntialias = true, Color = new SKColor(190, 190, 190) };
    public static SKPaint FilledGray2 = new() { IsAntialias = true, Color = new SKColor(128, 128, 128) };
    public static SKPaint ShadowFilledBlack = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint ShadowStrokeBlack = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint FilledLight = new() { IsAntialias = true };
    public static SKPaint StrokeLight = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint FilledMedian = new() { IsAntialias = true };
    public static SKPaint ShadowFilledMedian = new() { IsAntialias = true };
    public static SKPaint StrokeMedian = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint ShadowStrokeMedian = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint FilledTpMedian = new() { IsAntialias = true };
    public static SKPaint AntiAlias = new() { IsAntialias = true };

    static SkiaEx()
    {
        ShadowFilledBlack.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            SKColors.Gray
        );
        ShadowStrokeBlack.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            SKColors.Gray
        );
        TextFont = new SKFont(SKTypeface.FromStream(AssetLoader.Open(new Uri("avares://CsGrafeqApplication/Fonts/JetBrainsMono-Regular.ttf"))));
        Refresh();
    }

    public static void Refresh()
    {
        object? temp;
        App.Current.Resources.TryGetResource("Median", null, out temp);
        Median = new SKColor(((Color)temp).ToUInt32());
        App.Current.Resources.TryGetResource("Light", null, out temp);
        Light = new SKColor(((Color)temp).ToUInt32());

        FilledLight.Color = Light;
        FilledLight.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        StrokeLight.Color = Light;
        StrokeLight.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        FilledMedian.Color = Median;
        FilledMedian.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        StrokeMedian.Color = Median;
        StrokeMedian.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        ShadowFilledMedian.Color = Median;
        ShadowFilledMedian.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        ShadowStrokeMedian.Color = Median;
        ShadowStrokeMedian.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        FilledTpMedian.Color = Median.WithAlpha(90);
    }

    public static void DrawBubble(this SKCanvas dc, string s, SKPoint point, SKPaint back, SKPaint fore)
    {
        var size = new SKSize();
        size.Width = TextFont.MeasureText(s, FilledBlack);
        size.Height = TextFont.Size;
        dc.DrawRoundRect(new SKRoundRect(CreateSKRectWH(point.X, point.Y, size.Width + 4, size.Height + 4), 4), back);
        dc.DrawText(s, new SKPoint(point.X + 2, point.Y + 2 + size.Height / 2 + 4), TextFont, fore);
    }

    public static SKPoint OffSetBy(this SKPoint point, float dx, float dy)
    {
        return new SKPoint(point.X + dx, point.Y + dy);
    }

    public static SKRect CreateSKRectWH(float x, float y, float width, float height)
    {
        return new SKRect(x, y, x + width, y + height);
    }

    public static SKRect CreateSKRectWH(double x, double y, double width, double height)
    {
        return new SKRect((float)x, (float)y, (float)(x + width), (float)(y + height));
    }

    public static SKRect ToSKRect(this Rect rect)
    {
        return new SKRect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
    }

    public static bool ContainsPoint(this SKRect rect, SKPoint point)
    {
        return rect.Top < point.Y && point.Y < rect.Bottom && rect.Left < point.X && point.X < rect.Right;
    }
}