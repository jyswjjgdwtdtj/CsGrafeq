using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CsGrafeq.I18N;
using CsGrafeqApplication.Addons.GeometricPad;
using CsGrafeqApplication.Addons.FunctionPad;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.Core.Utils;
using CsGrafeqApplication.Dialogs.InfoDialog;
using CsGrafeqApplication.Dialogs.Interfaces;
using CsGrafeqApplication.Utilities;
using CsGrafeqApplication.ViewModels;
using Material.Styles.Themes;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;

namespace CsGrafeqApplication.Controls;

public partial class DisplayerContainer : UserControl, IInfoDialog
{
    // 可按需调整
    private const double MinOperationWidth = 50;
    private const double ReserveRightMin = 300; // 右侧至少保留空间，避免盖住Displayer等

    public static readonly DirectProperty<DisplayerContainer, bool> IsOperationVisibleProperty =
        AvaloniaProperty.RegisterDirect<DisplayerContainer, bool>(nameof(VM.IsOperationVisible),
            o => o.VM.IsOperationVisible,
            (o, v) => o.VM.IsOperationVisible = v);

    private readonly Animation anim = new();
    private readonly DisplayerContainerViewModel VM = new();
    private double _dragStartWidth;
    private double _dragStartX;

    private bool _isDragging;
    private CancellationTokenSource InfoCancellation = new();

    public DisplayerContainer()
    {
        KeyDown += GlobalKeyDown;
        DataContext = VM;
        VM.Displayer = new DisplayControl { Addons = { new GeometricPad(),new FunctionPad()} };
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
    }

    public async void Info(object content, InfoType infotype)
    {
        var color = infotype switch
        {
            InfoType.Warning => Colors.Yellow,
            InfoType.Error => Colors.DarkRed,
            _ => Themes.Theme.CurrentTheme.PrimaryMid.Color
        };
        ErrorIcon.IsVisible = infotype == InfoType.Error;
        WarningIcon.IsVisible = infotype == InfoType.Warning;
        InfoOuterContainer.Background = new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B));
        InfoCancellation.Cancel();
        InfoCancellation = new CancellationTokenSource();
        InfoPresenter.Content = content;
        InfoOuterContainer.Opacity = 1;
        await anim.RunAsync(InfoOuterContainer, InfoCancellation.Token);
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
            case Key.S:
            {
                if (e.KeyModifiers == KeyModifiers.Control)
                {
                    SaveClicked(sender, e);
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

    private void StepBack_Clicked(object? sender, RoutedEventArgs e)
    {
        CommandHelper.CommandManager.UnDo();
    }

    private void StepOver_Clicked(object? sender, RoutedEventArgs e)
    {
        CommandHelper.CommandManager.ReDo();
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
        VM.Displayer.Addons[VM.AddonIndex].Delete();
    }

    private void SelectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[VM.AddonIndex].SelectAll();
    }

    private void DeSelectAll_Clicked(object? sender, RoutedEventArgs e)
    {
        VM.Displayer.Addons[VM.AddonIndex].DeselectAll();
    }

    private void LanguageSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Languages.SetLanguage(Languages.AllowedLanguages[(sender as ComboBox)?.SelectedIndex ?? 0]);
    }
    private const string githubRepUrl = "https://github.com/jyswjjgdwtdtj/CsGrafeq";
    private void Github_Clicked(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = new Uri(githubRepUrl).AbsoluteUri,
            UseShellExecute = true
        });
    }

    private void ThemeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        (TopLevel.GetTopLevel(this) as Window)?.SystemDecorations = SystemDecorations.Full;
        if (sender is ComboBox comboBox)
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

    private async void SaveClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Open Text File",
                FileTypeChoices = [FilePickerFileTypes.ImageAll],
                DefaultExtension = ".jpg",
                ShowOverwritePrompt = true
            });
            if (file != null)
            {
                var size = new PixelSize((int)VM.Displayer.Bounds.Width, (int)VM.Displayer.Bounds.Height);
                var rr = new RenderTargetBitmap(size);
                using (var dc = rr.CreateDrawingContext())
                {
                    var rt = new RenderTargetBitmap(size);
                    rt.Render(VM.Displayer);
                    dc.DrawImage(rt, VM.Displayer.Bounds);
                    rt = new RenderTargetBitmap(new PixelSize((int)InfoContainer.Bounds.Width,
                        (int)InfoContainer.Bounds.Height));
                    rt.Render(InfoContainer);
                    dc.DrawImage(rt, InfoContainer.Bounds);
                }

                rr.Save(file.Path.AbsolutePath);
            }
        }
    }

    private void ColorSettingColorChanged(object? sender, ColorChangedEventArgs e)
    {
        var newtheme = Material.Styles.Themes.Theme.Create(Themes.Theme.CurrentTheme);
        newtheme.SetPrimaryColor(e.NewColor);
        Themes.Theme.CurrentTheme = newtheme;
    }

    private void OuterBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control c)
            return;
        var pseudoClasses = OuterBorder.UnsafeGetPseudoClasses();
        pseudoClasses.Set(":dragging", true);
        _isDragging = true;

        // 捕获指针，保证拖到控件外也能继续收到Moved/Released
        e.Pointer.Capture(c);

        var pos = e.GetPosition(PART_Grid); // 相对包含OperationContainer的布局取坐标
        _dragStartX = pos.X;
        _dragStartWidth = OperationContainer.Bounds.Width;

        e.Handled = true;
    }

    private void OuterBorderMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        var pos = e.GetPosition(PART_Grid);
        var delta = pos.X - _dragStartX;

        var target = _dragStartWidth + delta;

        // 最大宽度：不超过容器宽度 - 预留右侧空间
        var containerWidth = PART_Grid.Bounds.Width;
        var max = Max(MinOperationWidth, containerWidth - ReserveRightMin);

        target = Clamp(target, MinOperationWidth, max);

        OperationContainer.Width = target;
        OperationContainer.InvalidateArrange();
        OperationContainer.InvalidateMeasure();
        e.Handled = true;
    }

    private void OuterBorderReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is not Control c)
            return;

        _isDragging = false;
        var pseudoClasses = OuterBorder.UnsafeGetPseudoClasses();
        pseudoClasses.Set(":dragging", false);


        if (e.Pointer.Captured == c)
            e.Pointer.Capture(null);
        OperationContainer.InvalidateArrange();
        OperationContainer.InvalidateMeasure();

        e.Handled = true;
    }

    private void OuterBorderLoaded(object? sender, RoutedEventArgs e)
    {
        var pseudoClasses = OuterBorder.UnsafeGetPseudoClasses();
        pseudoClasses.Add(":dragging");
        pseudoClasses.Set(":dragging", false);
    }
}