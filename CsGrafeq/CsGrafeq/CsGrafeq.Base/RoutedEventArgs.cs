using Avalonia.Interactivity;

namespace CsGrafeq;

public class RoutedEventArgs<T> : RoutedEventArgs where T : EventArgs
{
    public readonly T InnerEventArgs;

    public RoutedEventArgs(T innerEventArgs)
    {
        InnerEventArgs = innerEventArgs;
    }
}