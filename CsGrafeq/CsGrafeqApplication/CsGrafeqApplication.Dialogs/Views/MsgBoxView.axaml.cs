using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CsGrafeqApplication.Dialogs.Interfaces;

namespace CsGrafeqApplication.Dialogs.Views;

public partial class MsgBoxView : UserControl, IClosable,IDialogResult<string>
{
    public MsgBoxView()
    {
        InitializeComponent();
    }

    public event EventHandler? Closing;

    public void Close()
    {
        Closing?.Invoke(null,EventArgs.Empty);
    }

    public void CloseWindow(object sender, EventArgs eventArgs)
    {
        ((IClose)this).Close();
    }

    public string DialogResult { get; set; } = "";

    private void ButtonClicked(object? sender, RoutedEventArgs e)
    {
        DialogResult = ((Button)sender).Tag.ToString();
        Close();
    }

    private void CloseClicked(object? sender, RoutedEventArgs e)
    {
        DialogResult = "";
        Close();
    }
}