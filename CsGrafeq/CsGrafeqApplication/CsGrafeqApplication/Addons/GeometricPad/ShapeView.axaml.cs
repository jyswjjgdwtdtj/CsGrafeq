using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CsGrafeq.Shapes;
using CsGrafeqApplication.Dialogs.InfoDialog;
using CsGrafeqApplication.Utilities;

namespace CsGrafeqApplication.Addons.GeometricPad;

public partial class ShapeView : TemplatedControl
{
    public static readonly DirectProperty<ShapeView, GeoShape> ShapeProperty =
        AvaloniaProperty.RegisterDirect<ShapeView, GeoShape>(
            nameof(Shape), o => o.Shape, (o, v) => o.Shape = v);

    public ShapeView()
    {
        InitializeComponent();
    }

    public GeoShape Shape
    {
        get => field;
        set => SetAndRaise(ShapeProperty, ref field, value);
    }

    /// <summary>
    ///     拦截操作的通用方法
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EventHandledTrue(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    ///     删除按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
            if (btn.Tag is GeometryShape shape)
                CommandHelper.DoGeoShapesDelete([shape]);
            else if (btn.Tag is GeoShape s) CommandHelper.DoShapeDelete(s);

        e.Handled = true;
    }

    /// <summary>
    ///     绑定拦截操作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NumberTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) tb.AddHandler(KeyDownEvent, TunnelTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    ///     使在TextBox中可以通过左右键来移动到同级TextBox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox box)
        {
            var parent = box.Parent;
            if (parent != null)
            {
                if (e.Key == Key.Left)
                {
                    var ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    var index = ls.IndexOf(box);
                    if (index > 0)
                        ls[index - 1].Focus();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Right)
                {
                    var ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    var index = ls.IndexOf(box);
                    if (index < ls.Count - 1)
                        ls[index + 1].Focus();
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeySymbol?.Length == 1 && (e.KeySymbol?[0] ?? 0) >= 33 && (e.KeySymbol?[0] ?? 0) < 127)
            {
                var keyChar = e.KeySymbol[0];
                switch (keyChar)
                {
                    case >= 'a' and <= 'z':
                    case >= 'A' and <= 'Z':
                    case >= '0' and <= '9':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '(':
                    case ')':
                    case '%':
                    case ',':
                    case '^':
                        return;
                }
            }

            if (e.KeyModifiers == KeyModifiers.None)
                switch (e.PhysicalKey)
                {
                    case PhysicalKey.Backspace:
                    case PhysicalKey.Delete:
                    case PhysicalKey.ArrowLeft:
                    case PhysicalKey.ArrowRight:
                        return;
                }
        }

        e.Prevent();
    }

    /// <summary>
    ///     拦截除了部分操作以外的键盘快捷键输入
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TunnelTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.None)
            return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Key == Key.A)
            {
            }
            else if (e.Key == Key.C)
            {
            }
            else if (e.Key == Key.V)
            {
            }
            else
            {
                e.Prevent();
            }

            return;
        }

        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            e.Prevent();
    }

    /// <summary>
    ///     监听数字输入框的变化 设置是否出错 并将操作添加入CmdManager
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Number_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty)
            {
                var n = tb.Tag as ExpNumber;
                if (n is not null && tb.IsFocused)
                {
                    if (n.IsError)
                    {
                        DataValidationErrors.SetError(tb, n.Error);
                        this.Info(new TextBlock { Text = n.Error.Message }, InfoType.Error);
                    }
                    else
                    {
                        (n.Owner as GeometryShape)?.RefreshValues();
                        DataValidationErrors.ClearErrors(tb);
                    }

                    CommandHelper.CommandManager.Do(
                        new TextChangedCommand((string?)e.OldValue ?? "", (string?)e.NewValue ?? "", n, tb));
                }
            }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        foreach (var control in this.GetTemplateChildren().OfType<TextBox>()) control.Styles.Add(Themes.FluentTheme);
    }
}