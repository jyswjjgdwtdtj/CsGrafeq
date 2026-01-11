using System;
using Avalonia;
using Avalonia.Skia;
using Material.Styles.Themes;
using SkiaSharp;

namespace CsGrafeqApplication;

public static class SkiaHelper
{
    public static SKColor Light=SKColors.Cyan;
    public static SKColor Mid=SKColors.Cyan;

    static SkiaHelper()
    {
        Themes.Theme.ThemeChangedEndObservable.Subscribe(t =>
        {
            Refresh(t);
        });
        Refresh(Themes.Theme);
    }

    public static SKPaint FilledMid { get; } = new() { IsAntialias = true };
    public static SKPaint ShadowFilledMid { get; } = new() { IsAntialias = true };
    public static SKPaint StrokeMid { get; } = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint ShadowStrokeMid { get; } = new() { IsAntialias = true, IsStroke = true };
    public static SKPaint FilledTpMid { get; } = new() { IsAntialias = true };

    public static void Refresh(MaterialThemeBase theme)
    {
        Light = theme.CurrentTheme.PrimaryLight.Color.ToSKColor();
        Mid = theme.CurrentTheme.PrimaryMid.Color.ToSKColor();
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
}