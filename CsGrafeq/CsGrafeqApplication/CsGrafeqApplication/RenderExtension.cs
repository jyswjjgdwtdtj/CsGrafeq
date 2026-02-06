using System;
using Avalonia;
using CsGrafeqApplication.Controls.Displayers;
using SkiaSharp;

namespace CsGrafeqApplication;

internal class RenderExtension
{
    public static void RenderMovedPlace(SKCanvas dc, RenderHandler rm, Size size, Point next, Point previous)
    {
        var width = (float)size.Width;
        var height = (float)size.Height;
        var horizonDelta = (float)Abs(previous.X - next.X);
        var verticalDelta = (float)Abs(previous.Y - next.Y);
        if (next.X < previous.X) //向左移动
        {
            rm.Invoke(dc, new SKRect(width - horizonDelta, 0, width, height));
            if (next.Y < previous.Y) //向上移动
                rm.Invoke(dc,
                    new SKRect(0, height - verticalDelta, width - horizonDelta, height));
            else if (next.Y > previous.Y) //向下移动
                rm.Invoke(dc, new SKRect(0, 0, width - horizonDelta, verticalDelta));
        }
        else if (next.X > previous.X) //向右移动
        {
            rm.Invoke(dc, new SKRect(0, 0, horizonDelta, height));
            if (next.Y < previous.Y) //向上
                rm.Invoke(dc,
                    new SKRect(horizonDelta, height - verticalDelta, width, height));
            else if (next.Y > previous.Y) //向下
                rm.Invoke(dc, new SKRect(horizonDelta, 0, width, verticalDelta));
        }
        else
        {
            if (next.Y < previous.Y) //向上
                rm.Invoke(dc, new SKRect(0, height - verticalDelta, width, height));
            else if (next.Y > previous.Y)
                rm.Invoke(dc, new SKRect(0, 0, width, verticalDelta));
        }
    }
    public static void RenderMovedPlace(Action<SKRect> rm, Size size, Point next, Point previous)
    {
        var width = (float)size.Width;
        var height = (float)size.Height;
        var horizonDelta = (float)Abs(previous.X - next.X);
        var verticalDelta = (float)Abs(previous.Y - next.Y);
        if (next.X < previous.X) //向左移动
        {
            rm.Invoke(new SKRect(width - horizonDelta, 0, width, height));
            if (next.Y < previous.Y) //向上移动
                rm.Invoke(new SKRect(0, height - verticalDelta, width - horizonDelta, height));
            else if (next.Y > previous.Y) //向下移动
                rm.Invoke(new SKRect(0, 0, width - horizonDelta, verticalDelta));
        }
        else if (next.X > previous.X) //向右移动
        {
            rm.Invoke(new SKRect(0, 0, horizonDelta, height));
            if (next.Y < previous.Y) //向上
                rm.Invoke(new SKRect(horizonDelta, height - verticalDelta, width, height));
            else if (next.Y > previous.Y) //向下
                rm.Invoke(new SKRect(horizonDelta, 0, width, verticalDelta));
        }
        else
        {
            if (next.Y < previous.Y) //向上
                rm.Invoke(new SKRect(0, height - verticalDelta, width, height));
            else if (next.Y > previous.Y)
                rm.Invoke(new SKRect(0, 0, width, verticalDelta));
        }
    }
}