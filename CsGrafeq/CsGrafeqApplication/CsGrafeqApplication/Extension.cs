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
global using static CsGrafeq.Utilities.CsGrafeqMath;
global using static System.Math;
using System.Runtime.CompilerServices;

namespace CsGrafeqApplication;

internal static class Extension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Void()
    {
    }

    public static TOutput VoidFunc<TInput, TOutput>(TInput d) where TOutput : struct
    {
        return default;
    }
}