using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using CsGrafeqApplication.Addons.GeometryPad;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.ViewModels;
using CsGrafeqApplication.Views;

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
        KeyDown += GlobalKeyDown;
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
                VM.Displayer.AskForRender();
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
    private void GlobalKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete:
                {
                    if (e.KeyModifiers == KeyModifiers.None)
                    {
                        Delete_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.A:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        SelectAll_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.OemComma: // Ctrl + ,
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        Setting_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.Z: // Ctrl+Z ³·Ïú
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        StepBack_Clicked(sender, e);
                        e.Handled = true;
                    }
                    if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
                    {
                        StepOver_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.Y: // Ctrl+Y ÖØ×ö
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        StepOver_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.OemPlus: // Ctrl + +
            case Key.Add:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        ZoomIn_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.OemMinus: // Ctrl + -
            case Key.Subtract:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        ZoomOut_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.Escape:
                {
                    if (e.KeyModifiers == KeyModifiers.None)
                    {
                        DeSelectAll_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
            case Key.B:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        DeSelectAll_Clicked(sender, e);
                        e.Handled = true;
                    }
                }
                break;
        }
    }
    private void Setting_Clicked(object? sender, RoutedEventArgs e)
    {
        MsgBox(new SettingView(VM.Displayer as DisplayControl));
    }

    private void StepBack_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].Undo();
    }

    private void StepOver_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].Redo();
    }

    private void ZoomOut_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Zoom(1 / 1.05, Bounds.Center);
    }

    private void ZoomIn_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Zoom(1.05, Bounds.Center);
    }

    private void Delete_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].Delete();
    }

    private void SelectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].SelectAll();
    }
    private void DeSelectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[0].DeSelectAll();
    }

    private void LanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Languages.SetLanguage(Languages.AllowedLanguages[(sender as ComboBox)?.SelectedIndex ?? 0]);

    }
}