using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApp.Addons.GeometryPad;

public class PointGetterDataTemplate : IDataTemplate
{
    public required IDataTemplate? IsLocation{get;set;}
    public required IDataTemplate? IsCommon { get; set; }
    public required IDataTemplate? IsOnShape { get; set; }
    public Control? Build(object? param)
    {
        if (param is PointGetterAndName item)
            switch (item.PointGetter)
            {
                case PointGetter_FromLocation point:
                    return IsLocation?.Build(param);
                case PointGetter_Movable point:
                    return IsOnShape?.Build(param);
                default:
                    return IsCommon?.Build(param);
            }

        return new Control();
    }

    public bool Match(object? data)
    {
        return data is PointGetterAndName;
    }
}

public class PointGetterAndName
{
    public PointGetter PointGetter { get; set; }
    public string Name { get; set; }
}