using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;

namespace CsGrafeqApplication.Controls;
public partial class VariableBox : UserControl
{
    public static readonly DirectProperty<VariableBox, string> VariableNameProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, string>(
            nameof(VariableName), o => o.VariableName, (o, v) => o.VariableName = v);

    public static readonly DirectProperty<VariableBox, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<VariableBox, double>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v,defaultBindingMode:BindingMode.TwoWay,unsetValue:0);

    public static readonly StyledProperty<double> MinProperty = AvaloniaProperty.Register<VariableBox, double>(
        nameof(Min),defaultBindingMode:BindingMode.TwoWay,defaultValue:0);

    public double Min
    {
        get => GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public static readonly StyledProperty<double> MaxProperty = AvaloniaProperty.Register<VariableBox, double>(
        nameof(Max),defaultBindingMode:BindingMode.TwoWay,defaultValue:10);

    public double Max
    {
        get => GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public double Value
    {
        get => field;
        set
        {
            SetAndRaise(ValueProperty,ref field, value);
        }
    }

    public string VariableName
    {
        get => field;
        set => SetAndRaise(VariableNameProperty, ref field, value);
    }

    public VariableBox()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TemplateAppliedHandler(object? s, TemplateAppliedEventArgs e)
    {
        var tb = s as TextBox;
        var borderelement = e.NameScope.Find<Border>("PART_BorderElement");
        borderelement.CornerRadius = new CornerRadius(0);
        borderelement.BorderThickness = new Thickness(0, 0, 0, 2);
        borderelement.Background = Brushes.Transparent;
        borderelement.IsVisible = true;
        tb.LostFocus += (s, e) =>
        {
            borderelement.IsVisible = true;
            borderelement.Background = Brushes.Transparent;
        };
        tb.GotFocus += (s, e) =>
        {
            borderelement.IsVisible = true;
            borderelement.Background = Brushes.Transparent;
        };
    }
}