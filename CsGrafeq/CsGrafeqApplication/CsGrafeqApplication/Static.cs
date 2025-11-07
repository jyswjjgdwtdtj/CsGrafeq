using System;
using Avalonia.Controls;

namespace CsGrafeqApplication;

public static class Static
{
    public static Action<Control,InfoType> Info;

    public enum InfoType
    {
        Information,
        Warning,
        Error
    }
}