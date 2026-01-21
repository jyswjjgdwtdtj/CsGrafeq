using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Metadata;
using CsGrafeq.Shapes;
using CsGrafeqApplication.Core.Interfaces;

namespace CsGrafeqApplication.Controls;

public partial class ImplicitFunctionBox : TemplatedControl,IExpressionBoxWrapper
{
    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty.Register<ImplicitFunctionBox, IBrush>(
        nameof(Foreground));

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> BackgroundProperty = AvaloniaProperty.Register<ImplicitFunctionBox, IBrush>(
        nameof(Background));

    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DirectProperty<ImplicitFunctionBox, ImplicitFunction> FunctionProperty = AvaloniaProperty.RegisterDirect<ImplicitFunctionBox, ImplicitFunction>(
        nameof(Function), o => o.Function, (o, v) => o.Function = v);
    [Content]
    public ImplicitFunction Function
    {
        get => field;
        set => SetAndRaise(FunctionProperty, ref field, value);
    }
    public static readonly StyledProperty<bool> CanInputProperty = AvaloniaProperty.Register<ImplicitFunctionBox, bool>(
        nameof(CanInput));

    public bool CanInput
    {
        get => GetValue(CanInputProperty);
        set => SetValue(CanInputProperty, value);
    }
    
    public ImplicitFunctionBox()
    {
        InitializeComponent();
    }

    public void Clear()
    {
        Function.SetExpression("");
    }
}