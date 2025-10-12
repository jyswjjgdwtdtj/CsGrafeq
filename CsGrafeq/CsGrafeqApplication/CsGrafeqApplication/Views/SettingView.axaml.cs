using Avalonia.Controls;
using Avalonia.Interactivity;
using CsGrafeqApplication.Controls.Displayers;

namespace CsGrafeqApplication.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
    }

    private void MovingOp_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        (Tag as DisplayControl)!.MovingOptimization = (bool)(sender as CheckBox)!.IsChecked!;
    }

    private void ZoomingOp_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        (Tag as DisplayControl)!.ZoomingOptimization = (bool)(sender as CheckBox)!.IsChecked!;
    }
}