using System;
using SkiaSharp;

namespace CsGrafeqApplication.Addons;

public class Renderable : IDisposable
{
    /// <summary>
    ///     缓冲区的同步锁
    /// </summary>
    private readonly object BitmapLock = new();

    /// <summary>
    ///     缓冲区
    /// </summary>
    private SKBitmap Bitmap = new(1, 1);

    /// <summary>
    ///     缓冲区大小
    /// </summary>
    private SKSizeI Size = new(1, 1);

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
            if (flag) RenderTargetSize = Size;
        }
    } = true;

    /// <summary>
    ///     指示是否需要被重新绘制
    /// </summary>
    public bool Changed { get; set; }

    public SKSizeI RenderTargetSize
    {
        get => Size;
        set
        {
            Size = value;
            lock (BitmapLock)
            {
                Bitmap.Dispose();
                Bitmap = new SKBitmap(int.Max(value.Width, 1), int.Max(value.Height, 1));
            }
        }
    }

    /// <summary>
    ///     删除
    /// </summary>
    public void Dispose()
    {
        Bitmap?.Dispose();
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
        lock (BitmapLock)
        {
            return new SKCanvas(Bitmap);
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
        lock (BitmapLock)
        {
            Bitmap.TryRealCopyTo(bitmap);
        }
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
        lock (BitmapLock)
        {
            canvas.DrawBitmap(Bitmap, x, y);
        }
    }

    /// <summary>
    ///     绘制事件
    /// </summary>
    public event Action<SKCanvas?, SKRect>? OnRender;

    /// <summary>
    ///     在指定Canvas上绘制
    /// </summary>
    /// <param name="dc"></param>
    /// <param name="rect"></param>
    public void Render(SKCanvas? dc, SKRect rect)
    {
        if (!IsActive)
            return;
        OnRender?.Invoke(dc, rect);
    }

    ~Renderable()
    {
        Dispose();
    }
}