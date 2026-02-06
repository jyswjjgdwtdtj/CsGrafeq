using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using SkiaSharp;

namespace CsGrafeqApplication.Addons;

public class Renderable : IDisposable
{
    /// <summary>
    ///     缓冲区的同步锁
    /// </summary>
    private readonly Lock _bitmapLock = new();

    /// <summary>
    ///     缓冲区
    /// </summary>
    private SKBitmap _bitmap = new(1, 1);

    /// <summary>
    ///     缓冲区大小
    /// </summary>
    private SKSizeI _size = new(1, 1);

    /// <summary>
    ///     指示是否处于活动
    /// </summary>
    public bool IsActive
    {
        get => field;
        set
        {
            var flag = value && !field;
            field = value;
            Changed = true;
            if (flag) RenderTargetSize = _size;
        }
    } = true;

    /// <summary>
    ///     指示是否需要被重新绘制
    /// </summary>
    public bool Changed { get; set; }

    public SKSizeI RenderTargetSize
    {
        get => _size;
        set
        {
            _size = value;
            lock (_bitmapLock)
            {
                _bitmap.Dispose();
                _bitmap = new SKBitmap(int.Max(value.Width, 1), int.Max(value.Height, 1));
            }
        }
    }

    /// <summary>
    ///     删除
    /// </summary>
    public void Dispose()
    {
        _bitmap.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     获取缓冲区Canvas
    /// </summary>
    /// <returns>缓冲区Canvas</returns>
    public SKCanvas? GetBitmapCanvas()
    {
        if (!IsActive)
            return null;
        lock (_bitmapLock)
        {
            return new SKCanvas(_bitmap);
        }
    }

    public void ClearAndFlush()
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
        {
            using var canvas = new SKCanvas(_bitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.Flush();
        }
    }

    /// <summary>
    ///     获取缓冲区的拷贝
    /// </summary>
    /// <returns></returns>
    public void CopyRenderTargetTo(SKBitmap bitmap)
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
            _bitmap.TryRealCopyTo(bitmap);
    }

    /// <summary>
    ///     在canvas上绘制缓冲区
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void DrawRenderTargetTo(SKCanvas canvas, int x, int y)
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
        {
            canvas.DrawBitmap(_bitmap, x, y);
        }
    }

    /// <summary>
    ///     绘制事件
    /// </summary>
    public event Action<SKCanvas?, SKRect,CancellationToken>? OnRenderCanvas;
    /// <summary>
    ///     绘制事件
    /// </summary>
    public event Action<SKBitmap, SKRect, CancellationToken>? OnRender;

    /// <summary>
    ///     当前缓冲区上绘制
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="ct"></param>
    public void Render(SKRect rect,CancellationToken ct)
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
        {
            OnRender?.Invoke(_bitmap, rect,ct);
            OnRenderCanvas?.Invoke(GetBitmapCanvas(), rect, ct);
        }
    }
    public void Render(SKRect rect){
        Render(rect, CancellationToken.None);
    }
    public void DrawBitmapTo(DrawingContext dc, Rect destRect, Rect sourceRect)
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
        {
            dc.DrawImage(ToAvaloniaImage(_bitmap),sourceRect, destRect);
        }
    }
    public static Bitmap ToAvaloniaImage(SKBitmap bitmap)
    {
        using (var memoryStream = new MemoryStream())
        {
            bitmap.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memoryStream);
        }
    }

    ~Renderable()
    {
        Dispose();
    }
}