using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;

namespace CsGrafeqApplication.Controls;

/// <summary>
/// 逻辑上来说这应该是一个UserControl 但是UserControl属性绑定不上去 不知道为什么
/// </summary>
public class VariableBox : UserControl
{
    public static readonly DirectProperty<VariableBox, string> ValueNameProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, string>(
            nameof(ValueName), o => o.ValueName, (o, v) => o.ValueName = v);

    public static readonly DirectProperty<VariableBox, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, double>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v);

    public static readonly DirectProperty<VariableBox, SliderData> MySliderDataProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, SliderData>(
            nameof(MySliderData), o => o.MySliderData);

    public VariableBox()
    {
        MySliderData.Min = -10;
        MySliderData.Max = 10;
        PropertyChanged += (s, e) =>
        {
        };
        MySliderData.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(MySliderData.Value):
                    Value = MySliderData.Value;
                    break;
            }
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var min=e.NameScope.Find<TextBox>("RangeMinimum");
        var max=e.NameScope.Find<TextBox>("RangeMaximum");
        var value=e.NameScope.Find<TextBox>("ValueTextBlock");
        EventHandler<TemplateAppliedEventArgs> tbt=(object? s, TemplateAppliedEventArgs te) =>
        {
            var tb=s as TextBox;
            var borderelement=te.NameScope.Find<Border>("PART_BorderElement");
            borderelement.CornerRadius = new CornerRadius(0);
            borderelement.BorderThickness = new Thickness(0,0,0,2);
            borderelement.IsVisible=false;
            borderelement.Background=Brushes.Transparent;
            tb.LostFocus+=(s,e)=>
            {
                borderelement.IsVisible=false;
                borderelement.Background=Brushes.Transparent;
            };
            tb.GotFocus += (s, e) =>
            {
                borderelement.IsVisible = true;
                borderelement.Background=Brushes.Transparent;
            };
        };
        min.TemplateApplied += tbt;
        max.TemplateApplied += tbt;
        value.TemplateApplied += tbt;
        
    }

    public SliderData MySliderData { get; } = new();
    public double Value
    {
        get => MySliderData.Value;
        set
        {
            var old = MySliderData.Value;
            MySliderData.Value = value;
            RaisePropertyChanged(ValueProperty, old, value);
        }
    }

    public string ValueName
    {
        get => field;
        set => SetAndRaise(ValueNameProperty, ref field, value);
    }
}

public class SliderData : ReactiveObject
{
    public double Min
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double Max
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double Value
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
}