using Avalonia.Controls;
using Avalonia.Threading;

namespace CsGrafeqApplication.Dialogs.Windows;

public partial class MsgBoxWindow : Avalonia.Controls.Window
{
    public MsgBoxWindow()
    {
        InitializeComponent();
        ShowInTaskbar = false;
        CanResize = false;
    }

    public async void CloseSafe()
    {
        await Dispatcher.UIThread.InvokeAsync(Close);
    }
}