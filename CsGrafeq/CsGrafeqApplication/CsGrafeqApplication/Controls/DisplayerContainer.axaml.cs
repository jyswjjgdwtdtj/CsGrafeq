using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using CsGrafeqApplication.Addons.GeometryPad;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.ViewModels;

namespace CsGrafeqApplication.Controls;

public partial class DisplayerContainer : UserControl
{
    public static readonly DirectProperty<DisplayerContainer, bool> IsOperationVisibleProperty =
        AvaloniaProperty.RegisterDirect<DisplayerContainer, bool>(nameof(VM.IsOperationVisible),
            o => o.VM.IsOperationVisible,
            (o, v) => o.VM.IsOperationVisible = v);

    private readonly Animation anim = new();
    private readonly DisplayerContainerViewModel VM = new();
    private CancellationTokenSource InfoCancellation = new();

    public DisplayerContainer()
    {
        DataContext = VM;
        VM.Displayer = new DisplayControl { Addons = { new GeometryPad() } };
        InitializeComponent();
        anim.Delay = TimeSpan.FromSeconds(3);
        anim.Duration = TimeSpan.FromSeconds(0.2);
        anim.FillMode = FillMode.Forward;
        var keyframe1 = new KeyFrame();
        keyframe1.Cue = new Cue(0);
        keyframe1.Setters.Add(new Setter(OpacityProperty, 1.0));
        var keyframe2 = new KeyFrame();
        keyframe2.Cue = new Cue(1);
        keyframe2.Setters.Add(new Setter(OpacityProperty, 0.0));
        anim.Children.Add(keyframe1);
        anim.Children.Add(keyframe2);

        var previousWidth = 300d;
        Splitter.DragCompleted += (s, e) =>
        {
            if (Splitter.Bounds.Left == 0) Toggle.IsChecked = true;
        };
        Toggle.IsCheckedChanged += (s, e) =>
        {
            if (Toggle.IsChecked is null)
                return;
            var ischecked = (bool)Toggle.IsChecked;
            if (ischecked)
            {
                previousWidth = PART_Grid.ColumnDefinitions[0].ActualWidth;
                PART_Grid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                Splitter.IsVisible = false;
                VM.Displayer.Invalidate();
            }
            else
            {
                if (previousWidth == 0)
                    previousWidth = 300;
                PART_Grid.ColumnDefinitions[0].Width = new GridLength(previousWidth, GridUnitType.Pixel);
                Splitter.IsVisible = true;
            }
        };
        Static.MsgBox = MsgBox;
        Static.Info = Info;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        VM.Displayer.InvalidateBuffer();
    }

    private void MsgBox(Control content)
    {
        content.VerticalAlignment = VerticalAlignment.Stretch;
        content.HorizontalAlignment = HorizontalAlignment.Stretch;
        ContentContainer.Child = content;
        MsgBoxContainer.IsVisible = true;
        PopupBack.Background = Brushes.Transparent;
    }

    private async void Info(Control content)
    {
        InfoCancellation.Cancel();
        InfoCancellation = new CancellationTokenSource();
        InfoPresenter.Content = content;
        ((Control)InfoPresenter.Parent).Opacity = 1;
        await anim.RunAsync(InfoPresenter.Parent, InfoCancellation.Token);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        MsgBoxContainer.IsVisible = false;
        PopupBack.Background = null;
    }

    private void Setting_Clicked(object? sender, RoutedEventArgs e)
    {
        MsgBox(new Control());
    }

    private void StepBack_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].CmdManager.UnDo();
    }

    private void StepOver_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].CmdManager.ReDo();
    }

    private void ZoomOut_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Zoom(1 / 1.05, Bounds.Center);
    }

    private void ZoomIn_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Zoom(1.05, Bounds.Center);
    }
}