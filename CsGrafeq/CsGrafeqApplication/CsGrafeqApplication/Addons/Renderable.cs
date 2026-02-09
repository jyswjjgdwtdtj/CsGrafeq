using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using CsGrafeq.Bitmap;
using SkiaSharp;

namespace CsGrafeqApplication.Addons;

public class Renderable : IDisposable
{
    public Renderable()
    {
        _bitmap = new(0, 0);
    }
    /// <summary>
    ///     缓冲区的同步锁
    /// </summary>
    private readonly Lock _bitmapLock = new();

    /// <summary>
    ///     缓冲区
    /// </summary>
    private PixelBitmap _bitmap;

    /// <summary>
    ///     缓冲区大小
    /// </summary>
    private SKSizeI _size = new(0, 0);

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
                PixelBitmap.Resize(ref _bitmap, _size.Width, _size.Height);
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
            return new SKCanvas(_bitmap.SKBitmap);
        }
    }

    public void ClearAndFlush()
    {
        if (!IsActive)
            return;
        lock (_bitmapLock)
        {
            _bitmap.Clear();
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
            _bitmap.TryCopyTo(bitmap.GetPixelSpan());
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
            canvas.DrawBitmap(_bitmap.SKBitmap, x, y, SkiaHelper.CompoundBufferPaint);
        }
    }

    /// <summary>
    ///     绘制事件
    /// </summary>
    public event Action<SKCanvas?, SKRect,CancellationToken>? OnRenderCanvas;
    /// <summary>
    ///     绘制事件
    /// </summary>
    public event Action<PixelBitmap, SKRect, CancellationToken>? OnRender;

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
            dc.DrawImage(_bitmap.ToWriteableBitmap(),sourceRect, destRect);
        }
    }

    ~Renderable()
    {
        Dispose();
    }
}