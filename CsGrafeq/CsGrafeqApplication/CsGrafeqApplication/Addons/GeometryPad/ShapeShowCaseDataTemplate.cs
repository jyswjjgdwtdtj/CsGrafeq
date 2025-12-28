using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class ShapeShowCaseDataTemplate : IDataTemplate
{
    public required IDataTemplate? ShapeShowcaseTemplate { get; set; }
    public required IDataTemplate? ExpNumberTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeometryGetter getter)
        {
            var item = getter.Parameters;
            var stack = new StackPanel();
            var ctls = new List<Control>();
            stack.Orientation = Orientation.Horizontal;
            for (var i = 0; i < item.Length; i++)
            {
                ctls.Add(new TextBlock
                {
                    Text = item[i].TypeName + ":", Classes = { "Prompt" }, VerticalAlignment = VerticalAlignment.Center
                });
                var c = ShapeShowcaseTemplate!.Build(item[i])!;
                c.DataContext = item[i];
                ctls.Add(c);
                ctls.Add(new TextBlock { Text = ",", VerticalAlignment = VerticalAlignment.Center });
            }

            if (getter is IHasExpNumberShapeGetter hasExpNumberShapeGetter)
            {
                var expns = hasExpNumberShapeGetter.ExpNumbers;
                foreach (var expNumberData in expns)
                {
                    ctls.Add(new TextBlock
                    {
                        Text = expNumberData.Description + ":", Classes = { "Prompt" },
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    var c = ExpNumberTemplate!.Build(expNumberData)!;
                    c.DataContext = expNumberData.Number;
                    ctls.Add(c);
                    ctls.Add(new TextBlock { Text = ",", VerticalAlignment = VerticalAlignment.Center });
                }
            }

            if (ctls.Count > 0)
                ctls.RemoveAt(ctls.Count - 1);
            stack.Children.AddRange(ctls);
            return stack;
        }

        return new Control();
    }

    public bool Match(object? data)
    {
        return data is GeometryGetter;
    }
}