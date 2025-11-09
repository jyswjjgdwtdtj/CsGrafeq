using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using CsGrafeq.Shapes;

namespace CsGrafeqApplication.Addons.GeometryPad;

public class ImpFuncShowDataTemplate:IDataTemplate
{
    public IDataTemplate ImpFuncDataTemplate { get; set; } = new DataTemplate();
    public bool Match(object? data)
    {
        return true;
    }

    public Control Build(object? data)
    {
        if (data is ImplicitFunction impf&&impf.IsCorrect&&!impf.IsDeleted&&impf.ShowFormula)
        {
            return ImpFuncDataTemplate?.Build(impf) ?? new Control();
        }
        return new Control();
    }
}