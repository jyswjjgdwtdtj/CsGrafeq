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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Threading;

namespace CsGrafeqApplication;

internal static class Extension
{
    internal static void GetFocus(this Control control)
    {
        Dispatcher.UIThread.Post(() => { control.Focus(); }, DispatcherPriority.Loaded);
    }

    internal static bool ArrayEqual<T>(T[] array1, T[] array2) where T : class
    {
        if (array1.Length != array2.Length)
            return false;
        for (var i = 0; i < array1.Length; i++)
            if (ReferenceEquals(array1[i], array2[i]))
                return false;
        return true;
    }

    public static IEnumerable<T> JoinToOne<T>(this IEnumerable<IEnumerable<T>> col)
    {
        foreach (var i in col)
        foreach (var j in i)
            yield return j;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Void()
    {
    }
    public static TOutput VoidFunc<TInput, TOutput>(TInput d) where TOutput : struct
    {
        return default;
    }
}