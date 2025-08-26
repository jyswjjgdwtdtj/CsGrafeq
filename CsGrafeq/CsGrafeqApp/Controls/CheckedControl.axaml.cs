using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace CsGrafeqApp.Controls;

[PseudoClasses(":checked", ":over")]
public class CheckedControl : TemplatedControl
{
    public static readonly DirectProperty<CheckedControl, bool> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<CheckedControl, bool>(nameof(IsChecked), o => o.IsChecked,
            (o, v) => o.IsChecked = v);

    public static readonly DirectProperty<CheckedControl, bool> IsOverProperty =
        AvaloniaProperty.RegisterDirect<CheckedControl, bool>(nameof(IsOver), o => o.IsOver, (o, v) => o.IsOver = v);

    public static readonly DirectProperty<CheckedControl, Control?> ContentProperty =
        AvaloniaProperty.RegisterDirect<CheckedControl, Control?>(nameof(Content), o => o.Content,
            (o, v) => o.Content = v);

    public static readonly StyledProperty<uint> ColorProperty =
        AvaloniaProperty.Register<CheckedControl, uint>(nameof(Color));

    static CheckedControl()
    {
        AffectsRender<CheckedControl>(IsCheckedProperty, ContentProperty, IsOverProperty);
    }

    public CheckedControl()
    {
        PointerPressed += (s, e) =>
        {
            this.GetFocus();
            if (e.Properties.IsLeftButtonPressed) IsChecked = !IsChecked;
            e.Handled = true;
        };
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsCheckedProperty) PseudoClasses.Set(":checked", IsChecked);
            if (e.Property == IsPointerOverProperty) IsOver = IsPointerOver;
            if (e.Property == IsOverProperty) PseudoClasses.Set(":over", IsOver);
        };
        Resources.TryGetResource("MedianColor", null, out var color);
        Color = 0x050505;
    }

    public uint Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public bool IsChecked
    {
        get => field;
        set => SetAndRaise(IsCheckedProperty, ref field, value);
    }

    public bool IsOver
    {
        get => field;
        set => SetAndRaise(IsOverProperty, ref field, value);
    }

    [Content]
    public Control? Content
    {
        get => field;
        set => SetAndRaise(ContentProperty, ref field, value);
    }
}