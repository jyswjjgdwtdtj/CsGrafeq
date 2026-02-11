using System.Globalization;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CsGrafeq.I18N;
using CsGrafeq.Windows.IME;
using CsGrafeqApplication.ViewModels;
using CsGrafeqApplication.Views;

namespace CsGrafeqApplication;

public class App : Application
{
    public override void Initialize()
    {
        var lang = CultureInfo.CurrentCulture.IetfLanguageTag.ToLower();
        if (lang.Contains("zh") || lang.Contains("cn") || lang.Contains("hans"))
            Languages.SetLanguage("zh-hans");
        else
            Languages.SetLanguage("en-us");
        AvaloniaXamlLoader.Load(this);
        var _ = SkiaHelper.FilledMid;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}