using SkiaSharp;
using BigPoint= CsGrafeq.PointBase<CsGrafeq.Numeric.BigNumber<long,double>>;
namespace CsGrafeqApplication.Utilities;

public static class PointHelper
{
    public static BigPoint ToBigPoint(this Avalonia.Point p)
    {
        return new BigPoint(new(0,p.X), new(0,p.Y));
    }

    public static BigPoint ToBigPoint(this SKPoint p)
    {
        return new BigPoint(new(0,p.X), new(0,p.Y));
    }
}