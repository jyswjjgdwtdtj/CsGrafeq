using Avalonia.Interactivity;

namespace CsGrafeqApplication.Core.Interfaces;

public interface IExpressionBox
{
    public bool HasText { get; }
    public string? Expression { get; }
    public bool IsCorrect { get;}
    public bool CanInput { get; set; }
    
    public event EventHandler<RoutedEventArgs>? Inputted;
}