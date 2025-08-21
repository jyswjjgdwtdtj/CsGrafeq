global using PointL = CsGrafeq.PointBase<long>;
global using PointI = CsGrafeq.PointBase<int>;
global using PointF = CsGrafeq.PointBase<float>;
global using PointD = CsGrafeq.PointBase<decimal>;
global using GeoPoint = CsGrafeq.Shapes.Point;
global using GeoLine = CsGrafeq.Shapes.Line;
global using GeoPolygon = CsGrafeq.Shapes.Polygon;
global using GeoCircle = CsGrafeq.Shapes.Circle;
global using GeoShape = CsGrafeq.Shapes.Shape;
global using GeoSegment = CsGrafeq.Shapes.Segment;
global using CsGrafeq;
global using static CsGrafeq.Math;
global using static System.Math;
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Threading;

namespace CsGrafeqApp;

public static class Extension
{

    [DoesNotReturn]
    internal static TResult Throw<TException,TResult>(TException exception) where TException : Exception
    {
        throw exception;
    }
    [DoesNotReturn]
    internal static TResult Throw<TException,TResult>() where TException : Exception,new()
    {
        throw new TException();
    }
    [DoesNotReturn]
    internal static TResult Throw<TResult>(string message)
    {
        throw new Exception(message);
    }

    internal static void GetFocus(this Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Console.WriteLine(control.Focus());
        }, DispatcherPriority.Loaded);
    }
}