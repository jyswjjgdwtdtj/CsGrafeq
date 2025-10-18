using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.ViewModels;

namespace CsGrafeqApplication.Views;

public partial class SettingView : UserControl
{
    private DisplayControl displayControl;
    public SettingView(DisplayControl displayControl)
    {
        InitializeComponent();
        DataContext = new ViewModelBase();
        this.displayControl = displayControl;
        MovingOp.IsChecked=displayControl.MovingOptimization;
        ZoomingOp.IsChecked=displayControl.ZoomingOptimization;
    }

    private void MovingOp_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        displayControl.MovingOptimization = MovingOp.IsChecked??false;
    }

    private void ZoomingOp_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        displayControl.ZoomingOptimization =ZoomingOp.IsChecked??false;
    }
}