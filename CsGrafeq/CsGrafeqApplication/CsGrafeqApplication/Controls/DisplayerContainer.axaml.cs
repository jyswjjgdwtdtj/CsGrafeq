using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using CsGrafeqApplication.Addons.GeometryPad;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.ViewModels;
using CsGrafeqApplication.Views;
using DialogHostAvalonia;
using Microsoft.Win32;

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
        Static.Info = Info;
    }
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
    }


    
    private async void Info(Control content,Static.InfoType infotype)
    {
        var color = infotype switch
        {
            Static.InfoType.Warning=>Colors.Yellow,
            Static.InfoType.Error=>Colors.Red,
            _ => Color.FromRgb(0x77,0xcc,0xbb)
        };
        InfoOuterContainer.Background=new SolidColorBrush(Color.FromArgb(128,color.R,color.G,color.B));
        InfoCancellation.Cancel();
        InfoCancellation = new CancellationTokenSource();
        InfoPresenter.Content = content;
        ((Control)InfoPresenter.Parent).Opacity = 1;
        await anim.RunAsync(InfoPresenter.Parent, InfoCancellation.Token);
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
            case Key.Z: // Ctrl+Z ����
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
            case Key.Y: // Ctrl+Y ����
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
        DialogHost.Show(new SettingView(VM.Displayer as DisplayControl));
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

    private void Github_Clicked(object? sender, RoutedEventArgs e)
    {
        RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"http\shell\open\command\");
        string s = key.GetValue("").ToString();
        Console.WriteLine(s);
        System.Diagnostics.Process.Start(s, "https://github.com/jyswjjgdwtdtj/CsGrafeq");
    
    }

    private void ThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (TopLevel.GetTopLevel(this) as Window)?.SystemDecorations = SystemDecorations.Full;
        if (sender is ComboBox comboBox)
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case 1:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case 2:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Default;
                    break;
            }
        }
    }
/*
    private static bool IsWindow = (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)!;
    private Window CurrentWindow;
    private double PressedXPosRatio = 0;
    private int PressedYPos;
    private PixelPoint PressedWidnowPos;
    private void TitlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (IsWindow&&e.Properties.IsLeftButtonPressed)
        {
            CurrentWindow=TopLevel.GetTopLevel(this) as Window;
            var PressedPoint = CurrentWindow.PointToScreen(e.GetCurrentPoint(CurrentWindow).Position);
            PressedYPos=PressedPoint.Y;
            PressedXPosRatio=PressedPoint.X/CurrentWindow.Width;
            PressedWidnowPos = CurrentWindow.Position;
        }
    }

    private void TitlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        Console.WriteLine(CurrentWindow.PointToScreen(e.GetCurrentPoint(CurrentWindow).Position));
        preventMove=false;
    }

    private void TitlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (IsWindow&&e.Properties.IsLeftButtonPressed&&(!preventMove))
        {
            if (CurrentWindow.WindowState == WindowState.Maximized)
                CurrentWindow.WindowState = WindowState.Normal;
            var currentPos =CurrentWindow.PointToScreen(e.GetCurrentPoint(CurrentWindow).Position);
            CurrentWindow.Position = new(PressedWidnowPos.X+currentPos.X-(int)(CurrentWindow.Width*PressedXPosRatio), PressedWidnowPos.Y+currentPos.Y-PressedYPos);
        }
    }
    private bool preventMove = false;
    private void TitleDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (IsWindow)
        {
            if (CurrentWindow.WindowState == WindowState.Maximized)
                CurrentWindow.WindowState = WindowState.Normal;
            else if (CurrentWindow.WindowState == WindowState.Normal)
                CurrentWindow.WindowState = WindowState.Maximized;
            e.Handled = true;
            preventMove=true;
        }
    }*/
}