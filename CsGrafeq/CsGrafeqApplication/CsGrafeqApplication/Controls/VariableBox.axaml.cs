using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;

namespace CsGrafeqApplication.Controls;
[PseudoClasses(":modifying")]
public class VariableBox : TemplatedControl
{
    public static readonly DirectProperty<VariableBox, string> ValueNameProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, string>(
            nameof(ValueName), o => o.ValueName, (o, v) => o.ValueName = v);

    public static readonly DirectProperty<VariableBox, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, double>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v);

    public static readonly DirectProperty<VariableBox, bool> IsModifyStatusProperty = AvaloniaProperty.RegisterDirect<VariableBox, bool>(
        nameof(IsModifyStatus), o => o.IsModifyStatus, (o, v) => o.IsModifyStatus = v);

    public static readonly DirectProperty<VariableBox, SliderData> MySliderDataProperty = AvaloniaProperty.RegisterDirect<VariableBox, SliderData>(
        nameof(MySliderData), o => o.MySliderData);
    public SliderData MySliderData { get; } = new SliderData();
    
    public bool IsModifyStatus
    {
        get => field;
        set => SetAndRaise(IsModifyStatusProperty, ref field, value);
    }

    public VariableBox()
    {
        MySliderData.Min = -10;
        MySliderData.Max = 10;
        PropertyChanged += (s, e) =>
        {
            if (e.Property == IsModifyStatusProperty)
            {
                PseudoClasses.Set(":modifying", IsModifyStatus);
            }
        };
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        //偷懒了.....^_^
        if (e.NameScope.Find<TextBlock>("ValueTextBlock") != null)
        {
            e.NameScope.Find<TextBlock>("ValueTextBlock")!.Tapped += GoModifying;
            e.NameScope.Find<TextBlock>("RangeMinimum")!.Tapped += GoModifying;
            e.NameScope.Find<TextBlock>("RangeMaximum")!.Tapped += GoModifying;
        }
    }

    private void GoModifying(object? sender, RoutedEventArgs e)
    {
        IsModifyStatus = true;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        IsModifyStatus = false;
    }

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
public class SliderData:ReactiveObject
{
    public double Min
    {
        get=>field;
        set=>this.RaiseAndSetIfChanged(ref field,value);
    }

    public double Max
    {
        get;
        set=>this.RaiseAndSetIfChanged(ref field,value);
    }
    public double Value
    {
        get=>field;
        set=>this.RaiseAndSetIfChanged(ref field,value);
    }
}