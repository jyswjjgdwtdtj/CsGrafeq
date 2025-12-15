using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":checked", ":over")]
public class ToggleBox : TemplatedControl
{
    public static readonly DirectProperty<ToggleBox, bool> IsCheckedProperty =
        AvaloniaProperty.RegisterDirect<ToggleBox, bool>(nameof(IsChecked), o => o.IsChecked,
            (o, v) => o.IsChecked = v);

    public static readonly DirectProperty<ToggleBox, bool> IsOverProperty =
        AvaloniaProperty.RegisterDirect<ToggleBox, bool>(nameof(IsOver), o => o.IsOver, (o, v) => o.IsOver = v);

    public static readonly DirectProperty<ToggleBox, Control?> ContentProperty =
        AvaloniaProperty.RegisterDirect<ToggleBox, Control?>(nameof(Content), o => o.Content,
            (o, v) => o.Content = v);

    public static readonly StyledProperty<uint> ColorProperty =
        AvaloniaProperty.Register<ToggleBox, uint>(nameof(Color));

    public static readonly StyledProperty<Flyout?> FlyoutProperty =
        AvaloniaProperty.Register<ToggleBox, Flyout?>(nameof(Flyout));

    static ToggleBox()
    {
        AffectsRender<ToggleBox>(IsCheckedProperty, ContentProperty, IsOverProperty);
    }

    public ToggleBox()
    {
        Tapped += (s, e) =>
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        };
        DoubleTapped += (s, e) => { Flyout?.ShowAt(this); };
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsCheckedProperty) PseudoClasses.Set(":checked", IsChecked);
            if (e.Property == IsPointerOverProperty) IsOver = IsPointerOver;
            if (e.Property == IsOverProperty) PseudoClasses.Set(":over", IsOver);
        };
        Color = 0x050505;
    }

    public Flyout? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
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