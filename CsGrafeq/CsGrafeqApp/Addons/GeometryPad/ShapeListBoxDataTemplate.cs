using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApp.Addons.GeometryPad;

public class ShapeListBoxDataTemplate : IDataTemplate
{
    public required IDataTemplate? IsFunction { get; set; }
    public required IDataTemplate? IsCommonPoint { get; set; }
    public required IDataTemplate? IsOnShapePoint { get; set; }
    public required IDataTemplate? IsLocationPoint { get; set; }
    public required IDataTemplate? IsPolygon { get; set; }
    public required IDataTemplate? IsCircle { get; set; }
    public required IDataTemplate? IsLine { get; set; }
    public required IDataTemplate? Common { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeoShape item)
            switch (item)
            {
                case GeoPoint point:
                {
                    if (point.PointGetter is PointGetter_FromLocation)
                        return IsLocationPoint?.Build(param);
                    if (point.PointGetter is PointGetter_Movable)
                        return IsOnShapePoint?.Build(param);
                    return IsCommonPoint?.Build(param);
                }
                case GeoLine _:
                case GeoCircle _:
                case GeoPolygon _:
                case Angle _:
                    return Common?.Build(param);
//              case ImplicitFunction _:
//                  return IsFunction?.Build(param);
            }

        return new Control();
    }

    public bool Match(object? data)
    {
        return data is Shape;
    }
}