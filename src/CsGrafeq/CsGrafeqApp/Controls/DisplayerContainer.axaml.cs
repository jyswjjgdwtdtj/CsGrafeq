using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using CsGrafeqApp.Controls.Displayers;

namespace CsGrafeqApp.Controls;

public class DisplayerContainer : TemplatedControl
{
    public static readonly DirectProperty<DisplayerContainer, Displayer?> DisplayerProperty = AvaloniaProperty.RegisterDirect<DisplayerContainer, Displayer?>(nameof(Displayer), o => o.Displayer, (o, v) => o.Displayer = v);
    [Content]
    public Displayer? Displayer
    {
        get => field;
        
        set => SetAndRaise(DisplayerProperty, ref field, value);
    }
}