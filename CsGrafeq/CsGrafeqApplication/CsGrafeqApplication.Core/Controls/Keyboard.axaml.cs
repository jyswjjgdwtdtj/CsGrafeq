using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

public partial class Keyboard : TemplatedControl
{
    public static readonly DirectProperty<Keyboard, Control> ContentProperty =
        AvaloniaProperty.RegisterDirect<Keyboard, Control>(
            nameof(Content), o => o.Content, (o, v) => o.Content = v);

    private Control? Container123, ContainerABC;

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        LogicalChildren.Clear();
        LogicalChildren.Add(Content);
        Container123 = e.NameScope.Find<Grid>("Container123");
        ContainerABC = e.NameScope.Find<StackPanel>("ContainerABC");
    }

    private void SwitchClicked(object? sender, RoutedEventArgs e)
    {
        if (Container123 is null || ContainerABC is null) return;
        if (Container123.IsVisible)
        {
            Container123.IsVisible = false;
            ContainerABC.IsVisible = true;
        }
        else
        {
            Container123.IsVisible = true;
            ContainerABC.IsVisible = false;
        }
    }

    private void CursorLeftClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this).TryFindTextBox(out var textBox)) textBox.CursorLeft();
    }

    private void CursorRightClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this).TryFindTextBox(out var textBox)) textBox.CursorRight();
    }

    private void BackspaceClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this).TryFindTextBox(out var textBox)) textBox.Backspace();
    }

    private void EnterClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this).TryFindTextBox(out var textBox))
        {
            textBox.RaiseEvent(new KeyEventArgs
            {
                Key = Key.Enter, RoutedEvent = KeyDownEvent, KeyModifiers = KeyModifiers.None,
                PhysicalKey = PhysicalKey.Enter, KeySymbol = "\r\n"
            });
            textBox.RaiseEvent(new KeyEventArgs
            {
                Key = Key.Enter, RoutedEvent = KeyUpEvent, KeyModifiers = KeyModifiers.None,
                PhysicalKey = PhysicalKey.Enter, KeySymbol = "\r\n"
            });
        }
    }

    private void DeleteClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this).TryFindTextBox(out var textBox)) textBox.Delete();
    }
}