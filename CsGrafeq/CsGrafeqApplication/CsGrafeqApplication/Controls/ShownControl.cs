using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace CsGrafeqApplication.Controls;
public class ShownControl : TemplatedControl
{
    public static readonly StyledProperty<bool> IsShownProperty =
        AvaloniaProperty.Register<ShownControl, bool>(nameof(IsShown), defaultValue: true);

    public static readonly StyledProperty<double> DurationProperty = AvaloniaProperty.Register<ShownControl, double>(
        nameof(Duration),defaultValue:0.1);

    public double Duration
    {
        get => GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly StyledProperty<double> TargetHeightProperty = AvaloniaProperty.Register<ShownControl, double>(
        nameof(TargetHeight));
    public double TargetHeight
    {
        get => GetValue(TargetHeightProperty);
        set => SetValue(TargetHeightProperty, value);
    }

    public bool IsShown
    {
        get => GetValue(IsShownProperty);
        set
        {
            SetValue(IsShownProperty, value);
        }
    }

    private Animation AnimTo=new Animation(), AnimBack=new Animation();
    private Setter TargetSetter=new(HeightProperty,0d);
    public ShownControl()
    {
        this.PropertyChanged += (s, e) =>
        {
            if(e.Property==TargetHeightProperty)
            {
                TargetSetter.Value = TargetHeight;
            }
            else if (e.Property == DurationProperty)
            {
                AnimBack.Duration = TimeSpan.FromSeconds(Duration);
                AnimTo.Duration = TimeSpan.FromSeconds(Duration);
            }
            else if (e.Property == IsShownProperty)
            {
                Console.WriteLine(123);
                if (IsShown)
                {
                    AnimTo.RunAsync(this,new CancellationToken());
                }
                else
                {
                    AnimBack.RunAsync(this,new  CancellationToken());
                }
            }
        };
        AnimTo.Duration = TimeSpan.FromSeconds(0.05);
        AnimTo.FillMode=FillMode.Forward;
        var keyframe1 = new KeyFrame();
        keyframe1.Cue = new Cue(0);
        keyframe1.Setters.Add(new Setter(ContentPresenter.HeightProperty, 0d));
        var keyframe2 = new KeyFrame();
        keyframe2.Cue = new Cue(1);
        keyframe2.Setters.Add(TargetSetter);
        AnimTo.Children.Add(keyframe1);
        AnimTo.Children.Add(keyframe2);
        
        AnimBack.Duration = TimeSpan.FromSeconds(0.05);
        AnimBack.FillMode=FillMode.Forward;
        var keyframe3 = new KeyFrame();
        keyframe3.Cue = new Cue(0);
        keyframe3.Setters.Add(TargetSetter);
        var keyframe4 = new KeyFrame();
        keyframe4.Cue = new Cue(1);
        keyframe4.Setters.Add(new Setter(ContentPresenter.HeightProperty, 0d));
        AnimBack.Children.Add(keyframe3);
        AnimBack.Children.Add(keyframe4);
    }

    static ShownControl()
    {
    }
}