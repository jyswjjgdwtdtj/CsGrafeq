using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace CsGrafeq.Bitmap;

public abstract unsafe class PixelBitmapBase:IDisposable
{
    public readonly int Width;
    public readonly int Height;
    public readonly uint* Pixels;
    public readonly int Length;
    protected PixelBitmapBase(int width, int height,uint* pixels)
    {
        Width = width;
        Height = height;
        Length=width*height;
        Pixels = pixels;
    }
    public uint GetPixel(int x, int y)
    {
        return Pixels[y * Width + x];
    }
    public virtual void SetPixel(int x, int y, uint color)
    {
        Pixels[y * Width + x] = color;
    }
     public virtual void SetRectangle(int x, int y, int rectWidth, int rectHeight, uint color)
    {
        for (int j = y; j < y+rectHeight; j++)
        {
            (new Span<uint>(Pixels+Math.Min( j* Width + x,Length-100), rectWidth)).Fill(color);
        }
    }
     public virtual void Clear(uint color = 0)
     {
         new Span<uint>(Pixels, (int)(Width * Height)).Fill(color);
     }
     public abstract void Dispose();

     public bool TryCopyTo<T>(Span<T> destination) where T : struct
     {
         Span<byte> des=MemoryMarshal.Cast<T,byte>(destination);
         Span<byte> src = new Span<byte>(Pixels, (Width * Height * sizeof(uint)));
         return src.TryCopyTo(des);
     }

     public WriteableBitmap ToWriteableBitmap()
     {
         var output=new WriteableBitmap(new PixelSize(Width, Height), new Vector(96, 96), PixelFormat.Bgra8888,
             AlphaFormat.Premul);
         using var locked = output.Lock();
         new Span<int>(Pixels,Width*Height).CopyTo(new Span<int>(locked.Address.ToPointer(),Width*Height));
         return output;
     }
     
     public Span<uint> PixelSpan=> new(Pixels, (Width * Height));
     public Span<byte> ByteSpan=> new(Pixels, (Width * Height*sizeof(uint)));
    
}