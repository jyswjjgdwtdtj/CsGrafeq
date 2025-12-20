using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using Material.Styles.Themes;
using SkiaSharp;

namespace CsGrafeqApplication.Controls;

public static class SkiaEx
{
    public static SKColor Light;
    public static SKColor Mid;
    public static SKPaint FilledBlack { get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint FilledRed { get; }= new() { IsAntialias = true, Color = SKColors.Red };
    public static SKPaint StrokeBlack{ get; } = new() { IsAntialias = true, Color = SKColors.Black, IsStroke = true };
    public static SKPaint StrokeRed{ get; } = new() { IsAntialias = true, Color = SKColors.Red, IsStroke = true };
    public static SKPaint FilledBlue{ get; } = new() { IsAntialias = true, Color = SKColors.Blue };
    public static SKPaint FilledWhite{ get; } = new() { IsAntialias = true, Color = SKColors.White };

    public static SKPaint FilledTranparentGrey{ get; } =
        new() { IsAntialias = true, Color = new SKColor(0x80, 0x80, 0x80, 70) };

    public static SKFont MapleMono{ get; }
    public static SKPaint ShadowFilledBlack{ get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint ShadowStrokeBlack{ get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint FilledMid{ get; } = new() { IsAntialias = true };
    public static SKPaint ShadowFilledMid{ get; } = new() { IsAntialias = true };
    public static SKPaint StrokeMid{ get; } = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint ShadowStrokeMid{ get; } = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint FilledTpMid{ get; } = new() { IsAntialias = true };
    public static SKPaint AntiAlias{ get; } = new() { IsAntialias = true };

    static SkiaEx()
    {
        Static.Theme.CurrentThemeChanged.Subscribe((t) =>
        {
            Refresh();
        });
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
        Light = Static.Theme.CurrentTheme.PrimaryLight.ForegroundColor.ToSKColor();
        Mid = Static.Theme.CurrentTheme.PrimaryMid.ForegroundColor.ToSKColor();
        FilledMid.Color = Mid;
        FilledMid.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        StrokeMid.Color = Mid;
        StrokeMid.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        ShadowFilledMid.Color = Mid;
        ShadowFilledMid.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        ShadowStrokeMid.Color = Mid;
        ShadowStrokeMid.ImageFilter = SKImageFilter.CreateDropShadow(
            0,
            0,
            2,
            2,
            Light
        );
        FilledTpMid.Color = Mid.WithAlpha(90);
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