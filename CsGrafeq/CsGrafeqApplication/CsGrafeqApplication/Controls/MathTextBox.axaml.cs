using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace CsGrafeqApplication.Controls;

public class MathTextBox : TemplatedControl
{

    public static readonly DirectProperty<MathTextBox, string> TextProperty = AvaloniaProperty.RegisterDirect<MathTextBox, string>(
        nameof(Text), o => o.Text, (o, v) => o.Text = v);

    public string Text
    {
        get => field;
        set => SetAndRaise(TextProperty, ref field, value);
    } = "";


    public static readonly DirectProperty<MathTextBox, string> ExpressionProperty = AvaloniaProperty.RegisterDirect<MathTextBox, string>(
        nameof(Expression), o => o.Expression, (o, v) => o.Expression = v);

    public string Expression
    {
        get => field;
        set => SetAndRaise(ExpressionProperty, ref field, value);
    } = "";
}