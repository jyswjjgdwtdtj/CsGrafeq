using Avalonia.Controls;

namespace CsGrafeqApplication.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        KeyDown += GlobalEvents.CallKeyDown;
        KeyUp += GlobalEvents.CallKeyUp;
    }
}