using Avalonia;
using Avalonia.Controls;

namespace CsGrafeqApp;

public static class MessageBox
{
    private static Flyout Flyout = new Flyout();

    static MessageBox()
    {
        Flyout.Placement=PlacementMode.Top;
        Flyout.ShowMode=FlyoutShowMode.Standard;
    }
    public static void ShowAtTopLevel(Visual control,object? content)
    {
        Flyout.Content=content;
        Flyout.ShowAt(TopLevel.GetTopLevel(control));
    }
    public static void ShowAt(Control control,object? content)
    {
        Flyout.Content=content;
        Flyout.ShowAt(control);
    }
}