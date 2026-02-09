using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace CsGrafeqApplication.Events;

public readonly struct MouseEventArgs(RoutedEvent? sourceEvent,RoutedEventArgs sourceEventArgs,Point position,KeyModifiers modifiers=default,PointerPointProperties properties=default,Vec delta = default)
{
    public RoutedEvent? SourceEvent { get; } = sourceEvent;
    public RoutedEventArgs SourceEventArgs { get; } = sourceEventArgs;
    public Point Position { get; } = position;
    public PointerPointProperties Properties { get; } = properties;
    public Vec Delta { get; } = delta;
    public KeyModifiers KeyModifiers { get; } = modifiers;
}