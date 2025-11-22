using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace CsGrafeqApplication.Controls;

public class GlobalMathKeyBoard : ContentControl
{

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var expander= e.NameScope.Find<Expander>("PART_Expander");
        expander.TemplateApplied += (_, te) =>
        {
            var togglebtn= te.NameScope.Find<ToggleButton>("PART_ToggleButton");
            togglebtn.TemplateApplied += (_, tte) =>
            {
                var path= tte.NameScope.Find<Path>("PART_ExpandIcon");
                Console.WriteLine(path?.GetType()?.ToString()??"null");
                var border = path?.Parent as Border;
            
                border.RenderTransform = new RotateTransform() {Angle = 180};
                path.Bind(Path.FillProperty, Resources.GetResourceObservable("CgForegroundBrush"));
            };
        };
    }
}