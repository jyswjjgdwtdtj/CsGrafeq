using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace CsGrafeqApplication.ViewModels;

public class MainWindowViewModel : MainViewModel
{
    private readonly Window Window;
    private bool isMaximized;

    public MainWindowViewModel(Window window)
    {
        Window = window;
        window.PropertyChanged += WindowOnPropertyChanged;
    }

    public WindowState WindowState
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsMaximized
    {
        get => isMaximized;
        set
        {
            if (isMaximized != value)
            {
                if (value)
                {
                    isMaximized = true;
                    Window.WindowState = WindowState.Maximized;
                    this.RaiseAndSetIfChanged(ref field, value);
                }
                else
                {
                    isMaximized = false;
                    Window.WindowState = WindowState.Normal;
                    this.RaiseAndSetIfChanged(ref field, value);
                }
            }
        }
    }

    public double OffSet
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double LengthContract
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private void WindowOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
        {
            WindowState = (WindowState)e.NewValue;
            this.RaiseAndSetIfChanged(ref isMaximized, Window.WindowState == WindowState.Maximized,
                nameof(IsMaximized));
            if (isMaximized)
            {
                OffSet = 7;
                LengthContract = -7;
            }
            else
            {
                OffSet = 0;
                LengthContract = 0;
            }
        }
    }
}