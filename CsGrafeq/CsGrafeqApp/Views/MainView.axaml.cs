using System.Net.Mime;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace CsGrafeqApp.Views;

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
}