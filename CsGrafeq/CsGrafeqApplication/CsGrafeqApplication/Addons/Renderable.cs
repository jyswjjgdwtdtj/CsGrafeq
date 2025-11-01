using System;
using SkiaSharp;

namespace CsGrafeqApplication.Addons;

public class Renderable : IDisposable
{
    private readonly object BitmapLock = new();
    private SKBitmap Bitmap = new(1, 1);
    public bool Changed { get; set; } = false;

    public void Dispose()
    {
        Bitmap?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void SetBitmapSize(SKSizeI size)
    {
        lock (BitmapLock)
        {
            Bitmap.Dispose();
            Bitmap = new SKBitmap(int.Max(size.Width, 1), int.Max(size.Height, 1));
        }
    }

    public SKCanvas GetBitmapCanvas()
    {
        lock (BitmapLock)
        {
            return new SKCanvas(Bitmap);
        }
    }

    public SKSizeI GetSize()
    {
        lock (BitmapLock)
        {
            return new SKSizeI(Bitmap.Width, Bitmap.Height);
        }
    }

    public SKBitmap GetCopy()
    {
        lock (BitmapLock)
        {
            return Bitmap.Copy();
        }
    }

    public void DrawBitmap(SKCanvas canvas, int x, int y)
    {
        lock (BitmapLock)
        {
            canvas.DrawBitmap(Bitmap, x, y);
        }
    }

    internal event Action<SKCanvas, SKRect>? OnRender;

    internal void Render(SKCanvas dc, SKRect rect)
    {
        OnRender?.Invoke(dc, rect);
    }

    ~Renderable()
    {
        Dispose();
    }
}