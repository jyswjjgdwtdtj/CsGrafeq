using System.Runtime.CompilerServices;
using SkiaSharp;

namespace CsGrafeq.Bitmap;

public unsafe struct CgBitmap
{
    public readonly int Width;
    public readonly int Height;
    public readonly uint* Pixels;
    public CgBitmap(int width, int height, Span<uint> pixels)
    {
        Width = width;
        Height = height;
        if(width*height != pixels.Length)
            throw new ArgumentException("Pixel length does not match width and height.");
        Pixels =(uint*)Unsafe.AsPointer(in pixels.GetPinnableReference());
        
    }
    public uint GetPixel(int x, int y)
    {
        return Pixels[y * Width + x];
    }

    public void SetPixel(int x, int y, uint color)
    {
        Pixels[y * Width + x] = color;
    }
    
    public void SetRectangle(int x, int y, int rectWidth, int rectHeight, uint color)
    {
        for (int j = y; j < y+rectHeight; j++)
        {
            (new Span<uint>(Pixels+j * Width + x, rectWidth)).Fill(color);
        }
    }
}