using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CsGrafeq.Setting;
using CsGrafeq.Windows.IME;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance;
    public MainWindow()
    {
        InitializeComponent();
        this.AttachDevTools();
        Instance = this;
        this.Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Setting.Save();
    }

    public void SetClientSize(Size size)
    {
        this.HandleResized(size,WindowResizeReason.User);
    }

    private void MinimizeClicked(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseWindowClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void WindowLoaded(object? sender, RoutedEventArgs e)
    {
        IME.DisableIme(this);
    }
}