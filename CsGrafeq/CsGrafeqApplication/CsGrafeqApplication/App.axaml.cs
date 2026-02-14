using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CsGrafeq.I18N;
using CsGrafeq.Windows.IME;
using CsGrafeqApplication.ViewModels;
using CsGrafeqApplication.Views;
using Material.Styles.Themes;
using System.Globalization;
using CsGrafeq;
using CsGrafeq.Setting;
using Avalonia.Media;


namespace CsGrafeqApplication;

public class App : Application
{
    public override void Initialize()
    {
        var lang = CultureInfo.CurrentCulture.IetfLanguageTag.ToLower();
        if (lang.Contains("zh") || lang.Contains("cn") || lang.Contains("hans"))
            Languages.SetLanguage(LanguagesEnum.Chinese);
        else
            Languages.SetLanguage(LanguagesEnum.English);
        AvaloniaXamlLoader.Load(this);
        var theme = Theme.Create(Themes.Theme.CurrentTheme);
        theme.SetPrimaryColor(Color.FromUInt32(Setting.Instance.PrimaryColor));
        Themes.Theme.CurrentTheme = theme;
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