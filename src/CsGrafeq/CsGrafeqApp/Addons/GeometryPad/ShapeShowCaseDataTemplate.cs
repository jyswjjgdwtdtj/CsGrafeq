using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;

namespace CsGrafeqApp.Addons.GeometryPad;

public class ShapeShowCaseDataTemplate : IDataTemplate
{
    public required IDataTemplate? ShapeShowcaseTemplate { get; set; }

    public Control? Build(object? param)
    {
        if (param is GeoShape[] item)
        {
            var stack = new StackPanel();
            var ctls = new Control[item.Length * 3 - 1];
            var index = 0;
            stack.Orientation = Orientation.Horizontal;
            for (var i = 0; i < item.Length - 1; i++)
            {
                ctls[index++] = new TextBlock { Text = item[i].Type, Classes = { "Prompt" } };
                var c = ShapeShowcaseTemplate!.Build(item[i])!;
                c.DataContext = item[i];
                ctls[index++] = c;
                ctls[index++] = new TextBlock { Text = "," };
            }

            ctls[index++] = new TextBlock { Text = item[item.Length - 1].Type, Classes = { "Prompt" } };
            var cc = ShapeShowcaseTemplate!.Build(item[item.Length - 1])!;
            cc.DataContext = item[item.Length - 1];
            ctls[index++] = cc;
            stack.Children.AddRange(ctls);
            return stack;
        }

        return new Control();
    }

    public bool Match(object? data)
    {
        return data is GeoShape[];
    }
}