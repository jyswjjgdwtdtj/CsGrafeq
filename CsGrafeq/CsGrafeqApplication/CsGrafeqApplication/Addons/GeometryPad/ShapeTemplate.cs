using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class ShapeTemplate : IDataTemplate
{
    public required IDataTemplate? LocationPoint { get; set; }
    public required IDataTemplate? ControlledPoint { get; set; }
    public required IDataTemplate? CommonPoint { get; set; }
    public required IDataTemplate? RadiusCircle { get; set; }
    public required IDataTemplate? Common { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeometryShape item)
            switch (item)
            {
                case GeoPoint point:
                {
                    switch (point.PointGetter)
                    {
                        case PointGetter_FromLocation pgloc:
                            return LocationPoint?.Build(pgloc);
                        case PointGetter_Movable pgmov:
                            return ControlledPoint?.Build(pgmov);
                        default:
                            return CommonPoint?.Build(param);
                    }
                }
                case GeoCircle circle:
                {
                    if (circle.Getter is CircleGetter_FromCenterAndRadius circleGetter)
                        return RadiusCircle?.Build(circle);
                    return Common?.Build(param);
                }
                case GeoLine _:
                case GeoPolygon _:
                case Angle _:
                    return Common?.Build(param);
                //              case ImplicitFunction _:
                //                  return IsFunction?.Build(param);
            }

        return new Control { Tag = "Invalid" };
    }

    public bool Match(object? data)
    {
        return data is Shape;
    }
}