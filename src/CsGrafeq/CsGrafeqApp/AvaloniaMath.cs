using Avalonia;
using System;
using SkiaSharp;
using CsGrafeq;
using static System.Math;
namespace CsGrafeqApp;

public static class AvaloniaMath
{
    public static Rect RegulateRectangle(Rect rectangle)
    {
        double x= rectangle.Position.X;
        double y= rectangle.Position.Y;
        double width= rectangle.Width;
        double height= rectangle.Height;
        if (width < 0)
            x += width;
        if (height < 0)
            y += height;
        width = Abs(width);
        height = Abs(height);
        return new Rect(x, y, width, height);
    }
    public static Avalonia.Point Sub(this Avalonia.Point p1,Avalonia.Point p2)
    {
        return new Avalonia.Point(p1.X - p2.X, p1.Y - p2.Y);
    }
    public static double GetLength(this Avalonia.Point p1)
    {
        return Sqrt(p1.X * p1.X + p1.Y * p1.Y);
    }
    public static double Arg(this Avalonia.Point p)
    {
        return Atan2(p.Y, p.X);
    }
    public static SKRect ToSKRect(this Avalonia.Rect rect)
    {
        return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
    }
    public static Avalonia.Point ToAvaPoint(this Vec v)
    {
        return new Avalonia.Point(v.X, v.Y);
    }
}