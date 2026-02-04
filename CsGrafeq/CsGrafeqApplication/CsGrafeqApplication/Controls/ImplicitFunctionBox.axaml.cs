using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;
using CsGrafeqApplication.Core.Interfaces;
using CsGrafeqApplication.Function;

namespace CsGrafeqApplication.Controls;

public partial class ImplicitFunctionBox : TemplatedControl, IExpressionBoxWrapper
{
    public static readonly StyledProperty<IBrush> ForegroundProperty =
        AvaloniaProperty.Register<ImplicitFunctionBox, IBrush>(
            nameof(Foreground));

    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<ImplicitFunctionBox, IBrush>(
            nameof(Background));

    public static readonly DirectProperty<ImplicitFunctionBox, ImplicitFunction> FunctionProperty =
        AvaloniaProperty.RegisterDirect<ImplicitFunctionBox, ImplicitFunction>(
            nameof(Function), o => o.Function, (o, v) => o.Function = v);

    public static readonly StyledProperty<bool> CanInputProperty = AvaloniaProperty.Register<ImplicitFunctionBox, bool>(
        nameof(CanInput));

    public ImplicitFunctionBox()
    {
        InitializeComponent();
    }

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    [Content]
    public ImplicitFunction Function
    {
        get => field;
        set => SetAndRaise(FunctionProperty, ref field, value);
    }

    public bool CanInput
    {
        get => GetValue(CanInputProperty);
        set => SetValue(CanInputProperty, value);
    }

    public void Clear()
    {
        Function.SetExpression("");
    }
}