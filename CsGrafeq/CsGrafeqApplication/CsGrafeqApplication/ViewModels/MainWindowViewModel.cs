using Avalonia;
using Avalonia.Controls;
using CsGrafeqApplication.Views;
using ReactiveUI;

namespace CsGrafeqApplication.ViewModels;

public class MainWindowViewModel:MainViewModel
{
    private Window Window;
    public MainWindowViewModel(Window window)
    {
        this.Window = window;
        window.PropertyChanged+=WindowOnPropertyChanged;
        
    }

    private void WindowOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Window.WindowStateProperty)
        {
            WindowState = (WindowState)e.NewValue;
            this.RaiseAndSetIfChanged(ref isMaximized,  Window.WindowState == WindowState.Maximized,nameof(MainWindowViewModel.IsMaximized));
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

    public WindowState WindowState
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }
    private bool isMaximized;
    
    public bool IsMaximized
    {
        get=>this.isMaximized;
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
}