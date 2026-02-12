using Avalonia;
using Avalonia.Input;

namespace CsGrafeqApplication.Events;

public static class EventArgsCaster
{
    public static MouseEventArgs Cast(this PointerEventArgs e,Visual relativeTo)
    {
        return new MouseEventArgs(e.RoutedEvent,e,e.GetPosition(relativeTo),e.KeyModifiers,e.Properties,Vec.Invalid);
    }
    public static MouseEventArgs Cast(this PointerWheelEventArgs e,Visual relativeTo)
    {
        return new MouseEventArgs(e.RoutedEvent,e,e.GetPosition(relativeTo),e.KeyModifiers,e.Properties,new Vec(e.Delta.X,e.Delta.Y));
    }
    public static MouseEventArgs Cast(this TappedEventArgs e,Visual relativeTo,PointerPointProperties properties=default)
    {
        return new MouseEventArgs(e.RoutedEvent,e,e.GetPosition(relativeTo),e.KeyModifiers,properties,Vec.Invalid);
    }
}