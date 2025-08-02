using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using CsGrafeqApp.Controls;

namespace CsGrafeqApp.Controls;

[PseudoClasses(":checked")]
public class CheckedControl : TemplatedControl
{
    static CheckedControl()
    {
        AffectsArrange<CheckedControl>(IsCheckedProperty,ContentProperty);
        AffectsMeasure<CheckedControl>(IsCheckedProperty,ContentProperty);
        AffectsRender<CheckedControl>(IsCheckedProperty,ContentProperty);
    }
    public static readonly DirectProperty<CheckedControl, bool> IsCheckedProperty = AvaloniaProperty.RegisterDirect<CheckedControl, bool>(nameof(IsChecked), o => o.IsChecked, (o, v) => o.IsChecked = v);
    public static readonly DirectProperty<CheckedControl, Control?> ContentProperty = AvaloniaProperty.RegisterDirect<CheckedControl, Control?>(nameof(Content), o => o.Content, (o, v) => o.Content = v);
    public bool IsChecked
    {
        get => field;
        set => SetAndRaise(IsCheckedProperty, ref field, value);
    }
    [Content]
    public Control? Content
    {
        get => field;
        set => SetAndRaise(ContentProperty, ref field, value);
    }
    public CheckedControl()
    {
        PointerPressed += (s, e) =>
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                IsChecked = !IsChecked;
            }
        };
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsCheckedProperty)
            {
                PseudoClasses.Set(":checked", IsChecked);
            }
        };
    }
}