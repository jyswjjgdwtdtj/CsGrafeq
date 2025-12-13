using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using CsGrafeq.CSharpMath.Editor;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":isfirst")]
public partial class TwoSymbolKeyButtonInPlace : UserControl
{
    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> FirstButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(FirstButton), o => o.FirstButton, (o, v) => o.FirstButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> SecondButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(SecondButton), o => o.SecondButton, (o, v) => o.SecondButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput> FirstKeyboardInputProperty = AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput>(
        nameof(FirstKeyboardInput), o => o.FirstKeyboardInput, (o, v) => o.FirstKeyboardInput = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput> SecondKeyboardInputProperty = AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, CgMathKeyboardInput>(
        nameof(SecondKeyboardInput), o => o.SecondKeyboardInput, (o, v) => o.SecondKeyboardInput = v);
    

    

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, bool>(
            nameof(IsFirst), o => o.IsFirst, (o, v) => o.IsFirst = v);

    public static readonly DirectProperty<TwoSymbolKeyButtonInPlace, string> CurrentButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButtonInPlace, string>(
            nameof(CurrentButton), o => o.CurrentButton, (o, v) => o.CurrentButton = v);

    public TwoSymbolKeyButtonInPlace()
    {
        PropertyChanged += OnPropertyChanged;
        IsFirst = true;
        CurrentButton = FirstButton;
    }

    public string CurrentButton
    {
        get => field;
        set => SetAndRaise(CurrentButtonProperty, ref field, value);
    }

    public bool IsFirst
    {
        get => field;
        set => SetAndRaise(IsFirstProperty, ref field, value);
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
        if (e.Property == IsFirstProperty)
        {
            PseudoClasses.Set(":isfirst", IsFirst);
            CurrentButton = IsFirst ? FirstButton : SecondButton;
        }
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var part_Button = e.NameScope.Find<Button>("PART_Button");
        if (part_Button != null)
        {
            var fm = TopLevel.GetTopLevel(this)?.FocusManager;
            if (fm != null)
            {
                part_Button.Click += (s, e) =>
                {
                    var f = fm.GetFocusedElement();
                    if (f is MathBox mb)
                    {
                        mb.PressKey(IsFirst?FirstKeyboardInput:SecondKeyboardInput);
                    }
                };
            }
        }
    }
}