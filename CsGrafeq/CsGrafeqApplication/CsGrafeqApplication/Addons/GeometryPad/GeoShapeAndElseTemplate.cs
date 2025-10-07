using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class GeoShapeAndElseTemplate : IDataTemplate
{
    public required IDataTemplate? IsGeometryShape { get; set; }
    public required IDataTemplate? IsElse { get; set; }
    public Control? Build(object? param)
    {
        if (param is Shape s)
        {
            if (s.IsDeleted)
                return new Control();
            return s is GeometryShape? IsGeometryShape?.Build(s) : IsElse?.Build(s);
        }

        return new Control();
    }

    public bool Match(object? data)
    {
        return data is Shape s && !s.IsDeleted;
    }
}