using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace CsGrafeqApp.Desktop;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        var builder = BuildAvaloniaApp();
        return builder.StartWithClassicDesktopLifetime(args);
    }


    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}