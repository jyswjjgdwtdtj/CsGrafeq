using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class GeoShapeAndElseTemplate : IDataTemplate
{
    public required IDataTemplate? IsGeometryShape { get; set; }
    public required IDataTemplate? IsElse { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeoShape s) return s is GeometryShape ? IsGeometryShape?.Build(s) : IsElse?.Build(s);
        return new Control { Tag = "Invalid" };
    }

    public bool Match(object? data)
    {
        return data is Shape s;
    }
}