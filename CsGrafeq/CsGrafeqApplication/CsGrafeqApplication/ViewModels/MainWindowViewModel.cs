using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace CsGrafeqApplication.ViewModels;

public class MainWindowViewModel : MainViewModel
{
    private readonly Window _window;
    private bool _isMaximized;

    public MainWindowViewModel(Window window)
    {
        _window = window;
        window.PropertyChanged += WindowOnPropertyChanged;
    }

    public WindowState WindowState
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsMaximized
    {
        get => _isMaximized;
        set
        {
            if (_isMaximized != value)
            {
                if (value)
                {
                    _isMaximized = true;
                    _window.WindowState = WindowState.Maximized;
                    this.RaiseAndSetIfChanged(ref field, value);
                }
                else
                {
                    _isMaximized = false;
                    _window.WindowState = WindowState.Normal;
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
            this.RaiseAndSetIfChanged(ref _isMaximized, _window.WindowState == WindowState.Maximized,
                nameof(IsMaximized));
            if (_isMaximized)
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