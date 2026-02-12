using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace CsGrafeq.Bitmap;
/// <summary>
/// ARGB8888格式的位图，提供直接访问像素数据的功能
/// </summary>
public unsafe class PixelBitmap(SKBitmap bitmap)
    : PixelBitmapBase(bitmap.Width, bitmap.Height, (uint*)bitmap.GetPixels().ToPointer())
{
    public SKBitmap SKBitmap { get;init;} = bitmap;

    public PixelBitmap(int width, int height) : this(new SKBitmap(width, height))
    {
    }

    ~PixelBitmap()
    {
        Dispose();
    }
    public override void Dispose()
    {
        SKBitmap.Dispose();
        GC.SuppressFinalize(this);
    }
    public override void SetPixel(int x, int y, uint color)
    {
        Pixels[y * Width + x] = color;
    }
    public PixelBitmap Resize(int newWidth, int newHeight)
    {
        Dispose();
        return new (newWidth,newHeight);
    }

    public static void Resize(ref PixelBitmap bitmap, int width, int height)
    {
        bitmap=bitmap.Resize(width, height);
    }
    
    
    public uint Color;
    private readonly ConcurrentBag<int> _dirtyPixels = new();
    public void SetPixel_Buffered(int x, int y)
    {
        _dirtyPixels.Add(y * Width + x);
    }
    public void Flush()
    {
        foreach (var i in _dirtyPixels)
        {
            *(Pixels+i) = Color;
        }
        _dirtyPixels.Clear();
    }
}