using Avalonia.Themes.Fluent;
using ReactiveUI;

namespace CsGrafeqApplication;

public class Static : ReactiveObject
{
    static Static()
    {
        Instance = new Static();
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