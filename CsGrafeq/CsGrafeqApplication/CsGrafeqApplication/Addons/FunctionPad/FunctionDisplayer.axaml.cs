using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using CsGrafeq.Interval;
using CsGrafeq.Setting;
using CsGrafeqApplication.Dialogs.InfoDialog;
using CsGrafeqApplication.Dialogs.Params;
using CsGrafeqApplication.Function;
using CsGrafeqApplication.Utilities;

namespace CsGrafeqApplication.Addons.FunctionPad;

public partial class FunctionDisplayer : TemplatedControl
{
    public static readonly DirectProperty<FunctionDisplayer, FunctionPad> TargetProperty =
        AvaloniaProperty.RegisterDirect<FunctionDisplayer, FunctionPad>(
            nameof(Target), o => o.Target, (o, v) => o.Target = v);

    public FunctionDisplayer()
    {
        InitializeComponent();
    }

    public FunctionPad Target
    {
        get => field;
        set => SetAndRaise(TargetProperty, ref field, value);
    }

    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Control ctl && ctl.Tag is ImplicitFunction function)
            Target.DeleteFunction(function);
    }

    private void ImpFuncOnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control ctl) FlyoutBase.ShowAttachedFlyout(ctl);
    }

    private void EventHandledTrue(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void ImpFuncPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty && tb.Tag is ImplicitFunction func)
            {
                var newText = e.NewValue as string ?? "";
                var oldText = e.OldValue as string ?? "";
                if (func.IsCorrect)
                {
                    DataValidationErrors.ClearErrors(tb);
                }
                else
                {
                    DataValidationErrors.SetError(tb, new Exception());
                    this.Info(new TextBlock { Text = func.LastError }, InfoType.Error);
                }

                if (newText != oldText)
                    CommandHelper.DoTextBoxTextChange(tb, newText, oldText);
            }
    }

    private void ImpFuncLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.AddHandler(KeyDownEvent, TunnelTextBoxKeyDown, RoutingStrategies.Tunnel);
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.HorizontalContentAlignment = HorizontalAlignment.Left;
        }
    }

    public void TunnelTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        return;
        if (e.KeyModifiers == KeyModifiers.Control)
        {
            if (e.Key == Key.A)
            {
            }
            else
            {
                e.Handled = true;
            }
        }
    }

    private void NewFuncKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox newFuncTextBox)
            return;
        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None &&
            !string.IsNullOrWhiteSpace(newFuncTextBox.Text) && IntervalCompiler
                .TryCompile(newFuncTextBox.Text, Setting.Instance.EnableExpressionSimplification).Success(out _, out _))
        {
            Target.CreateAndAddFunction(newFuncTextBox.Text);
            newFuncTextBox.Text = "";
        }
    }

    private void NewFuncPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty)
            {
                var str = e.NewValue as string ?? "";
                var res = IntervalCompiler
                    .TryCompile(str, Setting.Instance.EnableExpressionSimplification);
                if (res.IsSuccessful || string.IsNullOrWhiteSpace(str))
                {
                    DataValidationErrors.ClearErrors(tb);
                }
                else
                {
                    DataValidationErrors.SetError(tb, new Exception());
                    this.Info(new TextBlock { Text = res.Error()!.Message }, InfoType.Error);
                }
            }
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

    private void FunctionExampleClicked(object? sender, RoutedEventArgs e)
    {
    }

    private void ExampleFuncItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border ctl && ctl.Tag is MsgBoxParams pa && pa.Param is TextBox tb && ctl.Child is TextBlock tbl)
        {
            tb.Text = tbl.Text;
            pa.CloseAction?.Invoke();
            tb.Focus();
        }
    }
}