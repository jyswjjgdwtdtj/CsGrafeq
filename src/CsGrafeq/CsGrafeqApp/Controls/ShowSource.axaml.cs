using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using ReactiveUI;
namespace CsGrafeqApp.Controls;

public class ShowSource : TemplatedControl
{
    public ShowSource()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.Property == SourceProperty || e.Property == DataTemplateProperty)
                SetControl();

        };
    }
    public static readonly DirectProperty<ShowSource, Control?> ControlProperty =
        AvaloniaProperty.RegisterDirect<ShowSource, Control?>(nameof(Control),
            o => o.Control);
    public Control? Control
    {
        get => field;
        private set => SetAndRaise(ControlProperty, ref field, value);
    }


    public static readonly DirectProperty<ShowSource, object?> SourceProperty =
        AvaloniaProperty.RegisterDirect<ShowSource, object?>(nameof(Source),
            o => o.Source,
            (o, v) => o.Source = v);
    public object? Source
    {
        get => field;
        set => SetAndRaise(SourceProperty, ref field, value);
    }
    public static readonly DirectProperty<ShowSource, IDataTemplate?> DataTemplateProperty =
        AvaloniaProperty.RegisterDirect<ShowSource,IDataTemplate?>(nameof(DataTemplate),
            o => o.DataTemplate,
            (o, v) => o.DataTemplate = v);
    public IDataTemplate? DataTemplate
    {
        get => field;
        set => SetAndRaise(DataTemplateProperty, ref field, value);
    }
    private void SetControl()
    {
        Control= DataTemplate?.Build(Source);
        if(Control!=null)
            Control.DataContext = Source;
    }

}