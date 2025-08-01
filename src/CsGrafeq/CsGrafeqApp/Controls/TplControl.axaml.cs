using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

namespace CsGrafeqApp.Controls;

public partial class TplControl : UserControl
{
    public readonly static DirectProperty<TplControl, object?> SourceProperty = AvaloniaProperty.RegisterDirect<TplControl, object?>(
        nameof(Source), o => o.Source, (o, v) => o.Source = v);
    public readonly static DirectProperty<TplControl, IDataTemplate?> DataTemplateProperty = AvaloniaProperty.RegisterDirect<TplControl, IDataTemplate?>(
        nameof(DataTemplate),o=>o.DataTemplate,(o,v)=>o.DataTemplate=v);
    private IDataTemplate? _DataTemplate;
    public IDataTemplate? DataTemplate
    {
        get => _DataTemplate;
        set
        {
            this.SetAndRaise(DataTemplateProperty, ref _DataTemplate, value);
            Container.Child = DataTemplate?.Build(Source);
        }
    }
    private object? _Source;
    public object? Source
    {
        get => _Source;
        set
        {
            this.SetAndRaise(SourceProperty, ref _Source, value);
            Container.Child = DataTemplate?.Build(Source);
        }
    }
    public TplControl()
    {
        InitializeComponent();
    }
}