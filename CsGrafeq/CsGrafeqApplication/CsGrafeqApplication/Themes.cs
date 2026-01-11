using Avalonia;
using Avalonia.Themes.Fluent;
using CsGrafeq.MVVM;
using Material.Styles.Themes;
using ReactiveUI;

namespace CsGrafeqApplication;

public static class Themes
{
    static Themes()
    {
        Theme = Application.Current.LocateMaterialTheme<MaterialThemeBase>();
    }
    public static MaterialThemeBase Theme { get; }
    public static FluentTheme FluentTheme { get; } = new();
}