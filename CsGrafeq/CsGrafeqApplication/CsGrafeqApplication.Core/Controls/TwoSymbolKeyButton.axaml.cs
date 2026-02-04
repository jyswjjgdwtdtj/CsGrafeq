using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CsGrafeq.Keyboard;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

public partial class TwoSymbolKeyButton : ToggleButton
{
    public static readonly StyledProperty<float> SecondFontSizeProperty =
        AvaloniaProperty.Register<TwoSymbolKeyButton, float>(
            nameof(SecondFontSize));

    public static readonly DirectProperty<TwoSymbolKeyButton, string> FirstButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(FirstButton), o => o.FirstButton, (o, v) => o.FirstButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, string> SecondButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(SecondButton), o => o.SecondButton, (o, v) => o.SecondButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, KeyboardInput> FirstKeyboardInputProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, KeyboardInput>(
            nameof(FirstKeyboardInput), o => o.FirstKeyboardInput, (o, v) => o.FirstKeyboardInput = v,
            KeyboardInput.None);

    public static readonly DirectProperty<TwoSymbolKeyButton, KeyboardInput> SecondKeyboardInputProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, KeyboardInput>(
            nameof(SecondKeyboardInput), o => o.SecondKeyboardInput, (o, v) => o.SecondKeyboardInput = v,
            KeyboardInput.None);

    public static readonly DirectProperty<TwoSymbolKeyButton, string> CurrentButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(CurrentButton), o => o.CurrentButton, (o, v) => o.CurrentButton = v);

    public TwoSymbolKeyButton()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        IsChecked = true;
        CurrentButton = FirstButton;
        PART_Button.Click += (s, e) =>
        {
            TopLevel.GetTopLevel(this)?.Input(IsChecked ?? false ? FirstButton : SecondButton);
        };
    }

    public float SecondFontSize
    {
        get => GetValue(SecondFontSizeProperty);
        set => SetValue(SecondFontSizeProperty, value);
    }

    public string CurrentButton
    {
        get => field;
        set => SetAndRaise(CurrentButtonProperty, ref field, value);
    }

    public string SecondButton
    {
        get => field;
        set => SetAndRaise(SecondButtonProperty, ref field, value);
    } = "";

    public string FirstButton
    {
        get => field;
        set => SetAndRaise(FirstButtonProperty, ref field, value);
    } = "";

    public KeyboardInput FirstKeyboardInput
    {
        get => field;
        set => SetAndRaise(FirstKeyboardInputProperty, ref field, value);
    }

    public KeyboardInput SecondKeyboardInput
    {
        get => field;
        set => SetAndRaise(SecondKeyboardInputProperty, ref field, value);
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsCheckedProperty) CurrentButton = IsChecked ?? false ? FirstButton : SecondButton;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
    }
}