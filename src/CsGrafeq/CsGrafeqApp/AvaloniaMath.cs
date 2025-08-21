using Avalonia;
using SkiaSharp;

namespace CsGrafeqApp;

public static class AvaloniaMath
{
    public static Rect RegulateRectangle(Rect rectangle)
    {
        var x = rectangle.Position.X;
        var y = rectangle.Position.Y;
        var width = rectangle.Width;
        var height = rectangle.Height;
        if (width < 0)
            x += width;
        if (height < 0)
            y += height;
        width = Abs(width);
        height = Abs(height);
        return new Rect(x, y, width, height);
    }

    public static Point Sub(this Point p1, Point p2)
    {
        return new Point(p1.X - p2.X, p1.Y - p2.Y);
    }

    public static double GetLength(this Point p1)
    {
        return Sqrt(p1.X * p1.X + p1.Y * p1.Y);
    }

    public static double Arg(this Point p)
    {
        return Atan2(p.Y, p.X);
    }

    public static SKRect ToSKRect(this Rect rect)
    {
        return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
    }

    public static Point ToAvaPoint(this Vec v)
    {
        return new Point(v.X, v.Y);
    }
}