using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CsGrafeq.Keyboard;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

[PseudoClasses(":isfirst")]
public partial class OneSymbolKeyButton : UserControl
{
    public static readonly DirectProperty<OneSymbolKeyButton, string> ButtonProperty =
        AvaloniaProperty.RegisterDirect<OneSymbolKeyButton, string>(
            nameof(Button), o => o.Button, (o, v) => o.Button = v);

    public static readonly DirectProperty<OneSymbolKeyButton, KeyboardInput> KeyboardInputProperty =
        AvaloniaProperty.RegisterDirect<OneSymbolKeyButton, KeyboardInput>(
            nameof(KeyboardInput), o => o.KeyboardInput, (o, v) => o.KeyboardInput = v);

    public OneSymbolKeyButton()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public KeyboardInput KeyboardInput
    {
        get => field;
        set => SetAndRaise(KeyboardInputProperty, ref field, value);
    }

    public string Button
    {
        get => field;
        set => SetAndRaise(ButtonProperty, ref field, value);
    }

    private void PART_Button_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.Input(Button);
    }
}