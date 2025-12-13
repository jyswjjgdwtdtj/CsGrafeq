using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CsGrafeq.CSharpMath.Editor;

namespace CsGrafeqApplication.Controls;

[PseudoClasses(":isfirst")]
public partial class OneSymbolKeyButton : UserControl
{

    public static readonly DirectProperty<OneSymbolKeyButton, string> ButtonProperty = AvaloniaProperty.RegisterDirect<OneSymbolKeyButton, string>(
        nameof(Button), o => o.Button, (o, v) => o.Button = v);

    public static readonly DirectProperty<OneSymbolKeyButton, CgMathKeyboardInput> KeyboardInputProperty = AvaloniaProperty.RegisterDirect<OneSymbolKeyButton, CgMathKeyboardInput>(
        nameof(KeyboardInput), o => o.KeyboardInput, (o, v) => o.KeyboardInput = v);

    public CgMathKeyboardInput KeyboardInput
    {
        get => field;
        set => SetAndRaise(KeyboardInputProperty, ref field, value);
    }

    public string Button
    {
        get => field;
        set => SetAndRaise(ButtonProperty, ref field, value);
    }
    public OneSymbolKeyButton()
    {
        
        AvaloniaXamlLoader.Load(this);
        var fm = TopLevel.GetTopLevel(this)?.FocusManager;
        if (fm != null)
        {
            PART_Button.Click += (s, e) =>
            {
                if (fm.GetFocusedElement() is MathBox mb)
                {
                    mb.PressKey(KeyboardInput);
                }
            };
        }
    }

}