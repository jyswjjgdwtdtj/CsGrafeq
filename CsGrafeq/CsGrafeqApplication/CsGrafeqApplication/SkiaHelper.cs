using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Skia;
using Material.Styles.Themes;
using ReactiveUI;
using SkiaSharp;

namespace CsGrafeqApplication;

public static class SkiaHelper
{
    public static SKColor Light = SKColors.Cyan;
    public static SKColor Mid = SKColors.Cyan;

    static SkiaHelper()
    {
        Themes.Theme.ThemeChangedEndObservable.Subscribe(t => { Refresh(t); });
        Refresh(Themes.Theme);
        Setting.Instance.WhenAnyValue((setting => setting.CompoundBlendMode)).Subscribe((s) =>
        {
            CompoundBufferPaint.BlendMode = s;
        });
    }
    
    public static SKPaint CompoundBufferPaint { get; } = new() {};
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

    /// <summary>
    ///     基于内存拷贝的位图复制 用以替换超低效率超高内存分配的SKBitmap.CopyTo
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    public static bool TryRealCopyTo(this SKBitmap source, SKBitmap dest)
    {
        if (source.Width != dest.Width || source.Height != dest.Height)
            return false;
        source.GetPixelSpan().CopyTo(dest.GetPixelSpan());
        return true;
    }
    public unsafe static uint ToUint(this SKColor color)
    {
        return (*(uint*)Unsafe.AsPointer(ref color));
    }
    private struct TwoUInt
    {
        public uint Low;
        public uint High;
    }
}