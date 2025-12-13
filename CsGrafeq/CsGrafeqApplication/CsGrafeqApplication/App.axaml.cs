using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CsGrafeqApplication.ViewModels;
using CsGrafeqApplication.Views;
using CsGrafeq.Windows.IME;

namespace CsGrafeqApplication;

public class App : Application
{
    public override void Initialize()
    {
        Languages.SetLanguage("zh-hans");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext=new MainWindowViewModel(desktop.MainWindow);
            IME.DisableIme(desktop.MainWindow);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }
}