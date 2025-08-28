using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApp.Addons.GeometryPad;

public class ShapeListBoxDataTemplate : IDataTemplate
{
    public required IDataTemplate? IsFunction { get; set; }
    public required IDataTemplate? IsPoint { get; set; }
    public required IDataTemplate? Common { get; set; }
    public string Name { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeoShape item)
        {
            if(item.IsDeleted)
                return null;
            switch (item)
            {
                case GeoPoint _:
                    return IsPoint.Build(item);
                case GeoLine _:
                case GeoCircle _:
                case GeoPolygon _:
                case Angle _:
                    return Common?.Build(param);
//              case ImplicitFunction _:
//                  return IsFunction?.Build(param);
            }
        }
        return null;
    }

    public bool Match(object? data)
    {
        return data is Shape;
    }
}