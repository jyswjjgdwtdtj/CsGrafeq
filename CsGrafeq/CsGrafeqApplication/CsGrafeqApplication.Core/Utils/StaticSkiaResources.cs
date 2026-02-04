using System.Reactive.Linq;
using Avalonia.Platform;
using CsGrafeq;
using ReactiveUI;
using SkiaSharp;
using static CsGrafeqApplication.Core.Utils.PointRectHelper;

namespace CsGrafeqApplication.Core.Utils;

public static class StaticSkiaResources
{
    static StaticSkiaResources()
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
                AssetLoader.Open(new Uri("avares://CsGrafeqApplication.Core/Fonts/MapleMono-CN-Regular.ttf"))));
    }

    public static SKPaint FilledBlack { get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint FilledRed { get; } = new() { IsAntialias = true, Color = SKColors.Red };
    public static SKPaint StrokeBlack { get; } = new() { IsAntialias = true, Color = SKColors.Black, IsStroke = true };
    public static SKPaint StrokeRed { get; } = new() { IsAntialias = true, Color = SKColors.Red, IsStroke = true };
    public static SKPaint FilledBlue { get; } = new() { IsAntialias = true, Color = SKColors.Blue };
    public static SKPaint FilledWhite { get; } = new() { IsAntialias = true, Color = SKColors.White };

    public static SKPaint FilledTranparentGrey { get; } =
        new() { IsAntialias = true, Color = new SKColor(0x80, 0x80, 0x80, 70) };

    public static SKFont MapleMono { get; }
    public static SKPaint ShadowFilledBlack { get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint ShadowStrokeBlack { get; } = new() { IsAntialias = true, Color = SKColors.Black };
    public static SKPaint AntiAlias { get; } = new() { IsAntialias = true };

    public static void DrawBubble(this SKCanvas dc, string s, SKPoint point, SKPaint back, SKPaint fore)
    {
        var size = new SKSize();
        size.Width = MapleMono.MeasureText(s, FilledBlack);
        size.Height = MapleMono.Size;
        dc.DrawRoundRect(new SKRoundRect(CreateSKRectWH(point.X, point.Y, size.Width + 4, size.Height + 4), 4), back);
        dc.DrawText(s, new SKPoint(point.X + 2, point.Y + 2 + size.Height / 2 + 4), MapleMono, fore);
    }
}