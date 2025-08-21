using Avalonia.Collections;
using CsGrafeq.Collections;
using CsGrafeq.Shapes;
using CsGrafeqApp.Addons.GeometryPad;
using System;
using System.Text.Json;
using static CsGrafeqApp.Extension;

namespace CsGrafeqApp.ViewModels;

internal class GeometryPadViewModel:ViewModelBase
{
    public GeometryPadViewModel()
    {
        var dom = JsonDocument.Parse(Properties.Resources.Actions);
        foreach(var i in dom.RootElement.EnumerateArray())//RootElement:Array
        {
            HasNameActionList actionDatas = new(i.GetProperty("Name").GetString());
            foreach (var j in i.GetProperty("List").EnumerateArray())//List:Array
            {
                var s = ActionData.UseShape.None;
                var useshape = j.GetProperty("UseShape");
                foreach (var k in useshape.EnumerateArray())
                {
                    switch (k.GetString())
                    {
                        case "Point":
                            s|= ActionData.UseShape.Point;
                            break;
                        case "Line":
                            s|= ActionData.UseShape.Line;
                            break;
                        case "Polygon":
                            s|= ActionData.UseShape.Polygon;
                            break;
                        case "Circle":
                            s|= ActionData.UseShape.Circle;
                            break;
                        default:
                            Throw<object>("Unknown shape: " + k.GetString());
                            break;
                    }
                }
                actionDatas.Add(new ActionData(
                    j.GetProperty("Name").GetString(),
                    j.GetProperty("Description").GetString(),
                    s)
                );
            }
            Operations.Add(actionDatas);
        }
    }
    internal AvaloniaList<HasNameActionList> Operations { get; } =
        new();

    internal ShapeList Shapes { get;} = new ShapeList();
}