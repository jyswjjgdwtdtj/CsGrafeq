using System;
using System.Threading;
using Avalonia;
using CsGrafeqApplication.Controls.Displayers;
using SkiaSharp;

namespace CsGrafeqApplication;

internal class RenderExtension
{
    public static void RenderMovedPlace(Action<SKRect,CancellationToken> rm, Size size, Point next, Point previous,CancellationToken ct)
    {
        if(ct.IsCancellationRequested)
            return;
        var width = (float)size.Width;
        var height = (float)size.Height;
        var horizonDelta = (float)Abs(previous.X - next.X);
        var verticalDelta = (float)Abs(previous.Y - next.Y);
        if (next.X < previous.X) //向左移动
        {
            rm.Invoke(new SKRect(width - horizonDelta, 0, width, height),ct);
            if (next.Y < previous.Y) //向上移动
                rm.Invoke(new SKRect(0, height - verticalDelta, width - horizonDelta, height),ct);
            else if (next.Y > previous.Y) //向下移动
                rm.Invoke(new SKRect(0, 0, width - horizonDelta, verticalDelta),ct);
        }
        else if (next.X > previous.X) //向右移动
        {
            rm.Invoke(new SKRect(0, 0, horizonDelta, height),ct);
            if (next.Y < previous.Y) //向上
                rm.Invoke(new SKRect(horizonDelta, height - verticalDelta, width, height),ct);
            else if (next.Y > previous.Y) //向下
                rm.Invoke(new SKRect(horizonDelta, 0, width, verticalDelta),ct);
        }
        else
        {
            if (next.Y < previous.Y) //向上
                rm.Invoke(new SKRect(0, height - verticalDelta, width, height),ct);
            else if (next.Y > previous.Y)
                rm.Invoke(new SKRect(0, 0, width, verticalDelta),ct);
        }
    }
}