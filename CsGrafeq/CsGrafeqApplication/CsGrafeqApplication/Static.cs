using Avalonia;
using Avalonia.Themes.Fluent;
using Material.Styles.Themes;
using ReactiveUI;

namespace CsGrafeqApplication;

public class Static : ReactiveObject
{
    public static MaterialThemeBase Theme { get;}
    static Static()
    {
        Instance = new Static();
        Theme=Application.Current.LocateMaterialTheme<MaterialThemeBase>();
    }

    private Static()
    {
    }

    public static Static Instance { get; }

    public byte DefaultOpacity
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 128;

    public static FluentTheme FluentTheme { get; } = new();
}