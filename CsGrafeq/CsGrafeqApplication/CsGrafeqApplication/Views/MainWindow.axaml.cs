using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CsGrafeqApplication.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.AttachDevTools();
    }

    private void MinimizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseWindowClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}