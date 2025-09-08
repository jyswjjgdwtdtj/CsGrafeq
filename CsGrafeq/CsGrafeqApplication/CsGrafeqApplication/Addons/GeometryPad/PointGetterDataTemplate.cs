using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class PointGetterDataTemplate : IDataTemplate
{
    public required IDataTemplate? IsLocation{get;set;}
    public required IDataTemplate? IsCommon { get; set; }
    public required IDataTemplate? IsOnShape { get; set; }
    public Control? Build(object? param)
    {
        if (param is Point item)
        {
            if(item.IsDeleted)
                return null;
            switch (item.PointGetter)
            {
                case PointGetter_FromLocation point:
                    return IsLocation?.Build(param);
                case PointGetter_Movable point:
                    return IsOnShape?.Build(param);
                default:
                    return IsCommon?.Build(param);
            }
        }

        return null;    
    }

    public bool Match(object? data)
    {
        return data is Point;
    }
}