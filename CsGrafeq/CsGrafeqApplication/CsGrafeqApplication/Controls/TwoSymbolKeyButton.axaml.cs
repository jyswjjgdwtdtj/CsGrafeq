using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using CsGrafeq.CSharpMath.Editor;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":isfirst")]
public class TwoSymbolKeyButton : TemplatedControl
{
    public static readonly DirectProperty<TwoSymbolKeyButton, string> FirstButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(FirstButton), o => o.FirstButton, (o, v) => o.FirstButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, string> SecondButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(SecondButton), o => o.SecondButton, (o, v) => o.SecondButton = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, CgMathKeyboardInput> FirstKeyboardInputProperty = AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, CgMathKeyboardInput>(
        nameof(FirstKeyboardInput), o => o.FirstKeyboardInput, (o, v) => o.FirstKeyboardInput = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, CgMathKeyboardInput> SecondKeyboardInputProperty = AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, CgMathKeyboardInput>(
        nameof(SecondKeyboardInput), o => o.SecondKeyboardInput, (o, v) => o.SecondKeyboardInput = v);
    

    

    public static readonly DirectProperty<TwoSymbolKeyButton, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, bool>(
            nameof(IsFirst), o => o.IsFirst, (o, v) => o.IsFirst = v);

    public static readonly DirectProperty<TwoSymbolKeyButton, string> CurrentButtonProperty =
        AvaloniaProperty.RegisterDirect<TwoSymbolKeyButton, string>(
            nameof(CurrentButton), o => o.CurrentButton, (o, v) => o.CurrentButton = v);

    public TwoSymbolKeyButton()
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
                    if (fm.GetFocusedElement() is MathBox mb)
                    {
                        mb.PressKey(IsFirst?FirstKeyboardInput:SecondKeyboardInput);
                    }
                };
            }
        }
    }
}