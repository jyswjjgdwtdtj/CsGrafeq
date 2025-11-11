using System;
using Avalonia.Controls;
using ReactiveUI;

namespace CsGrafeqApplication;

public class Static:ReactiveObject
{
    public static Static Instance { get; }

    static Static()
    {
        Instance = new Static();
    }
    private Static()
    {
        
    }
    public static Action<Control,InfoType> Info;

    public enum InfoType
    {
        Information,
        Warning,
        Error
    }

    public byte DefaultOpacity
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = 128;
}