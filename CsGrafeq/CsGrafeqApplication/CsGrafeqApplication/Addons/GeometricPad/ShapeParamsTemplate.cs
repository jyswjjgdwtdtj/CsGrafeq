using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using CsGrafeq.Shapes.ShapeGetter;
using ReactiveUI;

namespace CsGrafeqApplication.Addons.GeometricPad;

public class ShapeParamsTemplate : IDataTemplate
{
    public required IDataTemplate? ShapeTemplate { get; set; }
    public required IDataTemplate? NumberTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeometryGetter getter)
        {
            var item = getter.ShapeParameters;
            var stack = new StackPanel();
            var ctls = new List<Control>();
            stack.Orientation = Orientation.Horizontal;
            foreach (var shapeParams in item ?? [])
            {
                var tb = new TextBlock { Classes = { "Prompt" }, VerticalAlignment = VerticalAlignment.Center };
                tb.Bind(TextBlock.TextProperty,
                    (shapeParams.Description ?? shapeParams.Shape.TypeName).WhenAnyValue(x => x.Data)
                    .Select(d => d + ":"));
                ctls.Add(tb);
                var c = ShapeTemplate!.Build(shapeParams)!;
                c.DataContext = shapeParams.Shape;
                ctls.Add(c);
                ctls.Add(new TextBlock { Text = ",", VerticalAlignment = VerticalAlignment.Center });
            }

            var expns = getter.NumberParameters;
            foreach (var expNumberData in expns ?? [])
            {
                ctls.Add(new TextBlock
                {
                    Text = expNumberData.Description.Data + ":",
                    Classes = { "Prompt" },
                    VerticalAlignment = VerticalAlignment.Center
                });
                var c = NumberTemplate!.Build(expNumberData)!;
                c.DataContext = expNumberData.Number;
                ctls.Add(c);
                ctls.Add(new TextBlock { Text = ",", VerticalAlignment = VerticalAlignment.Center });
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