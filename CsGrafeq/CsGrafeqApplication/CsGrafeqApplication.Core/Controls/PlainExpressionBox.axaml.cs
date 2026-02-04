using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using CsGrafeqApplication.Core.Interfaces;

namespace CsGrafeqApplication.Core.Controls;

public class PlainExpressionBox : TemplatedControl, IExpressionBox
{
    public static readonly DirectProperty<PlainExpressionBox, bool> IsCorrectProperty =
        AvaloniaProperty.RegisterDirect<PlainExpressionBox, bool>(
            nameof(IsCorrect), o => o.IsCorrect);

    public static readonly DirectProperty<PlainExpressionBox, bool> HasTextProperty =
        AvaloniaProperty.RegisterDirect<PlainExpressionBox, bool>(
            nameof(HasText), o => o.HasText);

    public static readonly DirectProperty<PlainExpressionBox, string?> ExpressionProperty =
        AvaloniaProperty.RegisterDirect<PlainExpressionBox, string?>(
            nameof(Expression), o => o.Expression, (o, v) => o.Expression = v);

    public static readonly StyledProperty<bool> CanInputProperty = AvaloniaProperty.Register<PlainExpressionBox, bool>(
        nameof(CanInput), true);

    public bool IsCorrect
    {
        get;
        private set => SetAndRaise(IsCorrectProperty, ref field, value);
    }

    public bool HasText
    {
        get;
        private set => SetAndRaise(HasTextProperty, ref field, value);
    }

    public string? Expression
    {
        get => field;
        set => SetAndRaise(ExpressionProperty, ref field, value);
    }

    public bool CanInput
    {
        get => GetValue(CanInputProperty);
        set => SetValue(CanInputProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? Inputted;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var textBox = e.NameScope.Get<TextBox>("PART_TextBox");
        textBox.TextChanged += (_, _) =>
        {
            HasText = !string.IsNullOrEmpty(textBox.Text);
            IsCorrect = true;
            Inputted?.Invoke(null, new RoutedEventArgs());
        };
    }
}