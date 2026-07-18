using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using static CsGrafeqApplication.Core.Utils.UnsafeAccessorHelper;

namespace CsGrafeqApplication.Core.ViewModel;

public class KeyboardViewModel : ViewModelBase
{
    public KeyboardViewModel()
    {
        var topLevel = Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime view?TopLevel.GetTopLevel(view.MainView) : null;
        FocusManager = topLevel?.FocusManager as FocusManager;
        if (topLevel != null && FocusManager != null)
        {
        }
    }
    public FocusManager? FocusManager { get; set=>this.RaiseAndSetIfChanged(ref field,value); }
    public bool IsKeyboardEnabled
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}