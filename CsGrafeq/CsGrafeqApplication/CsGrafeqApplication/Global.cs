using System;
using Avalonia.Input;

namespace CsGrafeqApplication;

internal static class Global
{
    public static event EventHandler<KeyEventArgs>? KeyDown;
    public static event EventHandler<KeyEventArgs>? KeyUp;

    public static void CallKeyDown(object? sender, KeyEventArgs e)
    {
        KeyDown?.Invoke(sender, e);
    }

    public static void CallKeyUp(object? sender, KeyEventArgs e)
    {
        KeyUp?.Invoke(sender, e);
    }
}