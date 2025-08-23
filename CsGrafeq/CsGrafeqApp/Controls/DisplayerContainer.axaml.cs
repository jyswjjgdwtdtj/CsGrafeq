using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Styling;
using CsGrafeqApp.Controls.Displayers;

namespace CsGrafeqApp.Controls;

public class DisplayerContainer : TemplatedControl
{
    public static readonly DirectProperty<DisplayerContainer, Displayer?> DisplayerProperty =
        AvaloniaProperty.RegisterDirect<DisplayerContainer, Displayer?>(nameof(Displayer), o => o.Displayer,
            (o, v) => o.Displayer = v);

    public static readonly DirectProperty<DisplayerContainer, bool> IsOperationVisibleProperty =
        AvaloniaProperty.RegisterDirect<DisplayerContainer, bool>(nameof(IsOperationVisible), o => o.IsOperationVisible,
            (o, v) => o.IsOperationVisible = v);

    public DisplayerContainer()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsOperationVisibleProperty)
            {
                //PseudoClasses.Set(":operationvisible", IsOperationVisible);
            }
        };
        anim.Delay=TimeSpan.FromSeconds(3);
        anim.Duration = TimeSpan.FromSeconds(0.2);
        anim.FillMode=FillMode.Forward;
        var keyframe1 = new KeyFrame();
        keyframe1.Cue = new Cue(0);
        keyframe1.Setters.Add(new Setter(ContentPresenter.OpacityProperty, 1.0));
        var keyframe2 = new KeyFrame();
        keyframe2.Cue = new Cue(1);
        keyframe2.Setters.Add(new Setter(ContentPresenter.OpacityProperty, 0.0));
        anim.Children.Add(keyframe1);
        anim.Children.Add(keyframe2);
    }

    [Content]
    public Displayer? Displayer
    {
        get => field;
        set
        {
            SetAndRaise(DisplayerProperty, ref field, value);
            value.Owner = this;
        }
    }

    public bool IsOperationVisible
    {
        get => field;
        set => SetAndRaise(IsOperationVisibleProperty, ref field, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MsgBoxPresenter = e.NameScope.Find<ContentPresenter>("MsgBoxPresenter");
        var border = e.NameScope.Find<Border>("PART_CoverDisplayer");
        var splitter = e.NameScope.Find<GridSplitter>("Splitter");
        var left = e.NameScope.Find<Border>("OperationContainer");
        var grid = e.NameScope.Find<Grid>("PART_Grid");
        if (border != null)
            border.PropertyChanged += (s, e) =>
            {
                if (e.Property == BoundsProperty && Displayer != null) Displayer.InvalidateBuffer();
            };
        var toggle = e.NameScope.Find<CheckBox>("Toggle");
        var previousWidth = 300d;
        toggle.Tapped += (s, e) =>
        {
            if (toggle.IsChecked is null)
                return;
            var ischecked = (bool)toggle.IsChecked;
            if (ischecked)
            {
                previousWidth = grid.ColumnDefinitions[0].ActualWidth;
                grid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                splitter.IsVisible = false;
                Displayer.Invalidate();
            }
            else
            {
                grid.ColumnDefinitions[0].Width = new GridLength(previousWidth, GridUnitType.Pixel);
                splitter.IsVisible = true;
            }
        };
    }

    private ContentPresenter MsgBoxPresenter;
    private CancellationTokenSource msgboxCancellation = new CancellationTokenSource();
    private Animation anim=new Animation();
    public async void MsgBox(Control control)
    {
        msgboxCancellation.Cancel();
        msgboxCancellation = new CancellationTokenSource();
        MsgBoxPresenter.Content=control;
        ((Control)(MsgBoxPresenter.Parent)).Opacity = 1;
        await anim.RunAsync(MsgBoxPresenter.Parent,msgboxCancellation.Token);
    }
}