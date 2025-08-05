using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace CsGrafeqApp.Addons.GeometryPad
{
    public class ShapeShowCaseDataTemplate:IDataTemplate
    {
        public required IDataTemplate? ShapeShowcaseTemplate { get; set; }
        public Control? Build(object? param)
        {
            if (param is GeoShape[] item)
            {
                StackPanel stack = new StackPanel();
                Control[] ctls = new Control[item.Length*3-1];
                int index = 0;
                stack.Orientation = Avalonia.Layout.Orientation.Horizontal;
                SolidColorBrush grey = new SolidColorBrush(Colors.Gray);
                for(int i = 0; i < item.Length-1; i++)
                {
                    ctls[index++]=new TextBlock() { Text = item[i].Type,Foreground=grey};
                    Control c = ShapeShowcaseTemplate!.Build(item[i])!;
                    c.DataContext = item[i];
                    ctls[index++] = c;
                    ctls[index++] = new TextBlock() { Text=","};
                }
                ctls[index++] = new TextBlock() { Text = item[item.Length-1].Type, Foreground = grey };
                Control cc = ShapeShowcaseTemplate!.Build(item[item.Length - 1])!;
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
}
