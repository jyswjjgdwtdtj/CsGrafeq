using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using SkiaSharp;

namespace CsGrafeqApplication.Controls;

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

    public static SKFont MapleMono;
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
        var fontMngr = SKFontManager.Default;
        MapleMono = new SKFont(
            fontMngr.CreateTypeface(
                AssetLoader.Open(new Uri("avares://CsGrafeqApplication/Fonts/MapleMono-CN-Regular.ttf"))));
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
        var TextFont = MapleMono;
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