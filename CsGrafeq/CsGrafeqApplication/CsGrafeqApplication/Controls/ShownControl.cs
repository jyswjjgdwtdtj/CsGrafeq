using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Styling;

namespace CsGrafeqApplication.Controls;

public class ShownControl : TemplatedControl
{
    public static readonly StyledProperty<bool> IsShownProperty =
        AvaloniaProperty.Register<ShownControl, bool>(nameof(IsShown), true);

    public static readonly StyledProperty<double> DurationProperty = AvaloniaProperty.Register<ShownControl, double>(
        nameof(Duration), 0.1);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ShownControl, Orientation>(
            nameof(Orientation), Orientation.Vertical);

    public static readonly DirectProperty<ShownControl, Control> ChildProperty = AvaloniaProperty.RegisterDirect<ShownControl, Control>(
        nameof(Child), o => o.Child, (o, v) => o.Child = v);
    [Content]
    public Control Child
    {
        get => field;
        set => SetAndRaise(ChildProperty, ref field, value);
    }
    private static readonly Animation AnimHeightToZero = new()
    {
        Duration = TimeSpan.FromSeconds(0),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame()
            {
                Cue = new Cue(0),
                Setters =
                {
                    new Setter(HeightProperty, 1d)
                }
            },
            new KeyFrame()
            {
                Cue = new Cue(1),
                Setters =
                {
                    new Setter(HeightProperty, 0d)
                }
            }
        }
    };
    private static readonly Animation AnimWidthToZero = new()
    {
        Duration = TimeSpan.FromSeconds(0),
        FillMode = FillMode.Forward,
        Children =
        {
            new KeyFrame()
            {
                Cue = new Cue(0),
                Setters =
                {
                    new Setter(WidthProperty, 1d)
                }
            },
            new KeyFrame()
            {
                Cue = new Cue(1),
                Setters =
                {
                    new Setter(WidthProperty, 0d)
                }
            }
        }
    };
    private readonly Animation AnimBackVer = new();
    private readonly Animation AnimToVer = new();
    private readonly Animation AnimBackHor = new();
    private readonly Animation AnimToHor = new();
    private readonly Setter TargetSetterVer = new(HeightProperty, 300d);
    private readonly Setter TargetSetterHor = new(WidthProperty, 300d);
    private CancellationTokenSource LastCancellationToken=new();

    static ShownControl()
    {
        
    }

    public ShownControl()
    {
        PropertyChanged += (s, e) =>
        {
            if (e.Property == DurationProperty)
            {
                AnimBackVer.Duration = TimeSpan.FromSeconds(Duration);
                AnimToVer.Duration = TimeSpan.FromSeconds(Duration);
                AnimBackHor.Duration = TimeSpan.FromSeconds(Duration);
                AnimToHor.Duration = TimeSpan.FromSeconds(Duration);
            }
            else if (e.Property == IsShownProperty)
            {
                Child?.Measure(new Size(10000,10000));
                if (Orientation == Orientation.Vertical)
                    TargetSetterVer.Value = Child?.DesiredSize.Height ?? 0d;
                else
                    TargetSetterHor.Value = Child?.DesiredSize.Width ?? 0d;
                Child?.InvalidateMeasure();
                if (!IsLoaded)
                {
                }
                else
                {
                    LastCancellationToken.Cancel();
                    LastCancellationToken = new CancellationTokenSource();
                    if(Orientation == Orientation.Vertical)
                        if (IsShown)
                            AnimToVer.RunAsync(this, LastCancellationToken.Token);
                        else
                            AnimBackVer.RunAsync(this,LastCancellationToken.Token);
                    else
                    if (IsShown)
                        AnimToHor.RunAsync(this,LastCancellationToken.Token);
                    else
                        AnimBackHor.RunAsync(this,LastCancellationToken.Token);
                }
            }

            if (e.Property == ChildProperty)
            {
                this.LogicalChildren.Clear();
                if(Child!= null)
                    this.LogicalChildren.Add(Child);
            }
        };
        AddHandler(LoadedEvent, (s, e) =>
        {
            if (Child != null)
            {
                if (Orientation == Orientation.Vertical)
                {
                    this.Height = 0;
                    this.Width = double.NaN;
                }
                else
                {
                    this.Height = double.NaN;
                    this.Width = 0;
                }
            }
        });
        AnimToVer.Duration = TimeSpan.FromSeconds(0.05);
        AnimToVer.FillMode = FillMode.Forward;
        AnimToVer.Children.Add(new KeyFrame(){Cue = new Cue(0),Setters = { new Setter(HeightProperty, 0d)}});
        AnimToVer.Children.Add(new KeyFrame(){Cue = new Cue(1),Setters = {TargetSetterVer}});

        AnimBackVer.Duration = TimeSpan.FromSeconds(0.05);
        AnimBackVer.FillMode = FillMode.Forward;
        AnimBackVer.Children.Add(new KeyFrame(){Cue = new Cue(0),Setters = { TargetSetterVer}});
        AnimBackVer.Children.Add(new KeyFrame(){Cue = new Cue(1),Setters = {new Setter(HeightProperty, 0d)}});
        
        AnimToHor.Duration = TimeSpan.FromSeconds(0.05);
        AnimToHor.FillMode = FillMode.Forward;
        AnimToHor.Children.Add(new KeyFrame(){Cue = new Cue(0),Setters = { new Setter(WidthProperty, 0d)}});
        AnimToHor.Children.Add(new KeyFrame(){Cue = new Cue(1),Setters = {TargetSetterHor}});

        AnimBackHor.Duration = TimeSpan.FromSeconds(0.05);
        AnimBackHor.FillMode = FillMode.Forward;
        AnimBackHor.Children.Add(new KeyFrame(){Cue = new Cue(0),Setters = { TargetSetterHor}});
        AnimBackHor.Children.Add(new KeyFrame(){Cue = new Cue(1),Setters = {new Setter(WidthProperty, 0d)}});
    }

    public double Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }
    public bool IsShown
    {
        get => GetValue(IsShownProperty);
        set => SetValue(IsShownProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}