using Avalonia;
using CsGrafeq;
using SkiaSharp;
using static System.Math;

namespace CsGrafeqApplication.Core.Utils;

public static class PointRectHelper
{
    public static Rect RegulateRectangle(this Rect rectangle)
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


    public static SKPoint OffSetBy(this SKPoint point, float dx, float dy)
    {
        return new SKPoint(point.X + dx, point.Y + dy);
    }

    public static SKRect CreateSKRectWH(float x, float y, float width, float height)
    {
        return new SKRect(x, y, x + width, y + height);
    }

    public static SKRect CreateSKRectWH(double x, double y, double width, double height)
    {
        return new SKRect((float)x, (float)y, (float)(x + width), (float)(y + height));
    }


    public static bool ContainsPoint(this SKRect rect, SKPoint point)
    {
        return rect.Top < point.Y && point.Y < rect.Bottom && rect.Left < point.X && point.X < rect.Right;
    }
}