using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":checked", ":over",":displayed")]
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
    
    public static readonly StyledProperty<Flyout?> FlyoutProperty =AvaloniaProperty.Register<CheckedControl, Flyout?>(nameof(Flyout));

    public static readonly StyledProperty<bool?> IsDisplayedProperty =
        AvaloniaProperty.Register<CheckedControl, bool?>(nameof(IsDisplayed));
    static CheckedControl()
    {
        AffectsRender<CheckedControl>(IsCheckedProperty, ContentProperty, IsOverProperty);
    }
    
    public Flyout? Flyout { get => GetValue(FlyoutProperty); set => SetValue(FlyoutProperty, value); }

    public bool? IsDisplayed
    {
        get => GetValue(IsDisplayedProperty);
        set => SetValue(IsDisplayedProperty, value);
    }

    public CheckedControl()
    {
        Tapped += (s, e) =>
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        };
        DoubleTapped += (s, e) =>
        {
            Flyout?.ShowAt(this);
        };
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsCheckedProperty) PseudoClasses.Set(":checked", IsChecked);
            if (e.Property == IsPointerOverProperty) IsOver = IsPointerOver;
            if (e.Property == IsOverProperty) PseudoClasses.Set(":over", IsOver);
            if (e.Property == IsDisplayedProperty)
            {
                PseudoClasses.Set(":displayed", IsDisplayed??false);
            }
        };
        Resources.TryGetResource("MedianColor", null, out var color);
        Color = 0x050505;
        IsDisplayed = true;
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