using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using CsGrafeq.CSharpMath.Editor;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

public partial class Keyboard : TemplatedControl
{
    public static readonly DirectProperty<Keyboard, Control> ContentProperty =
        AvaloniaProperty.RegisterDirect<Keyboard, Control>(
            nameof(Content), o => o.Content, (o, v) => o.Content = v);

    public Keyboard()
    {
        InitializeComponent();
    }

    [Content]
    public Control Content
    {
        get => field;
        set => SetAndRaise(ContentProperty, ref field, value);
    }

    private void BackspaceButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.Input(CgMathKeyboardInput.Backspace);
    }
}