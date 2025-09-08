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

    private void Msg(Control content)
    {
        ContentContainer.IsVisible = true;
        TextContainer.IsVisible=false;
        ContentContainer.Child = content;
        MsgboxContainer.IsVisible = true;
    }

    private void Msg(string content)
    {
        ContentContainer.IsVisible = false;
        TextContainer.IsVisible=true;
        TxtBlock.Text = content;
        MsgboxContainer.IsVisible = true;
    }

    private void GeometryPad_OnClick(object? sender, RoutedEventArgs e)
    {
        Msg("123");
    }
}