using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CsGrafeq.CSharpMath.Editor;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

public partial class TwoSymbolKeyButtonInPlace : ToggleButton
{
    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> FirstButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(FirstButton), o => o.FirstButton, (o, v) => o.FirstButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> SecondButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(SecondButton), o => o.SecondButton, (o, v) => o.SecondButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput> FirstKeyboardInputProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput>(
            nameof(FirstKeyboardInput), o => o.FirstKeyboardInput, (o, v) => o.FirstKeyboardInput = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput> SecondKeyboardInputProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput>(
            nameof(SecondKeyboardInput), o => o.SecondKeyboardInput, (o, v) => o.SecondKeyboardInput = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> CurrentButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(CurrentButton), o => o.CurrentButton, (o, v) => o.CurrentButton = v);

    public TwoSymbolKeyButtonInPlace()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;
        IsChecked = true;
        CurrentButton = FirstButton;
        PART_Button.Click += (s, e) =>
        {
            TopLevel.GetTopLevel(this)?.Input(IsChecked ?? false ? FirstKeyboardInput : SecondKeyboardInput);
        };
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

    public CgMathKeyboardInput FirstKeyboardInput
    {
        get => field;
        set => SetAndRaise(FirstKeyboardInputProperty, ref field, value);
    }

    public CgMathKeyboardInput SecondKeyboardInput
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