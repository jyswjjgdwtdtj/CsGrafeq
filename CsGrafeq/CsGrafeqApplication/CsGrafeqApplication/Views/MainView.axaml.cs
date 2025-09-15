using System.Net.Mime;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CsGrafeqApplication.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public void ExitClicked(object sender, RoutedEventArgs e)
    {
        (TopLevel.GetTopLevel(this) as Window)?.Close();
    }
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        MsgboxContainer.IsVisible = false;
    }
    private void Setting_OnClick(object? sender, RoutedEventArgs e)
    {
        Msg(new SettingView() { Tag=MainDisplayControl});
    }
    

    private void Msg(Control content)
    {
        content.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        content.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        ContentContainer.Child = content;
        MsgboxContainer.IsVisible = true;
    }

    private void GeometryPad_OnClick(object? sender, RoutedEventArgs e)
    {
        Msg(new TextBlock() { Text="123"});
    }
}