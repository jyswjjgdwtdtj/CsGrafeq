using Avalonia.Controls;

namespace CsGrafeqApplication.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        KeyDown += Global.CallKeyDown;
        KeyUp += Global.CallKeyUp;
    }
}