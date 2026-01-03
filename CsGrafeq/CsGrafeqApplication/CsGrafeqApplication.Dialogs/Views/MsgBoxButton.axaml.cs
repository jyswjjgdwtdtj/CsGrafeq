using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeqApplication.Dialogs.Models;
using CsGrafeqApplication.Dialogs.Params;

namespace CsGrafeqApplication.Dialogs.Views;

public class MsgBoxButton : Button
{
    public static readonly DirectProperty<MsgBoxButton, IDataTemplate?> MsgBoxContentTemplateProperty =
        AvaloniaProperty.RegisterDirect<MsgBoxButton, IDataTemplate?>(
            nameof(MsgBoxContentTemplate), o => o.MsgBoxContentTemplate, (o, v) => o.MsgBoxContentTemplate = v);


    public static readonly DirectProperty<MsgBoxButton, object?> MsgBoxContentTemplateParamProperty = AvaloniaProperty.RegisterDirect<MsgBoxButton, object?>(
        nameof(MsgBoxContentTemplateParam), o => o.MsgBoxContentTemplateParam, (o, v) => o.MsgBoxContentTemplateParam = v);

    public object? MsgBoxContentTemplateParam
    {
        get => field;
        set => SetAndRaise(MsgBoxContentTemplateParamProperty, ref field, value);
    }

    public static readonly StyledProperty<ButtonDefinitions> ButtonDefinitionsProperty =
        AvaloniaProperty.Register<MsgBoxButton, ButtonDefinitions>(
            nameof(ButtonDefinitions));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MsgBoxButton, string>(
        nameof(MsgBoxTitle));

    public static readonly StyledProperty<double> MsgBoxMinWidthProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgBoxMinWidth),300);

    public double MsgBoxMinWidth
    {
        get => GetValue(MsgBoxMinWidthProperty);
        set => SetValue(MsgBoxMinWidthProperty, value);
    }

    public static readonly StyledProperty<double> MsgBoxMaxWidthProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgBoxMaxWidth),800);

    public static readonly StyledProperty<bool> MsgBoxTopMostProperty = AvaloniaProperty.Register<MsgBoxButton, bool>(
        nameof(MsgBoxTopMost),false);

    public bool MsgBoxTopMost
    {
        get => GetValue(MsgBoxTopMostProperty);
        set => SetValue(MsgBoxTopMostProperty, value);
    }

    public static readonly StyledProperty<double> MsgBoxWidthProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgBoxWidth), double.NaN);

    public double MsgBoxWidth
    {
        get => GetValue(MsgBoxWidthProperty);
        set => SetValue(MsgBoxWidthProperty, value);
    }

    public static readonly StyledProperty<double> MsgboxHeightProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgboxHeight),Double.NaN);

    public double MsgboxHeight
    {
        get => GetValue(MsgboxHeightProperty);
        set => SetValue(MsgboxHeightProperty, value);
    }

    public static readonly StyledProperty<double> MsgBoxMaxHeightProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgBoxMaxHeight),600);

    public double MsgBoxMaxHeight
    {
        get => GetValue(MsgBoxMaxHeightProperty);
        set => SetValue(MsgBoxMaxHeightProperty, value);
    }

    public static readonly StyledProperty<double> MsgBoxMinHeightProperty = AvaloniaProperty.Register<MsgBoxButton, double>(
        nameof(MsgBoxMinHeight),150);

    public double MsgBoxMinHeight
    {
        get => GetValue(MsgBoxMinHeightProperty);
        set => SetValue(MsgBoxMinHeightProperty, value);
    }

    public static readonly StyledProperty<WindowStartupLocation> WindowStartupLocationProperty = AvaloniaProperty.Register<MsgBoxButton, WindowStartupLocation>(
        nameof(WindowStartupLocation), WindowStartupLocation.CenterOwner);

    public WindowStartupLocation WindowStartupLocation
    {
        get => GetValue(WindowStartupLocationProperty);
        set => SetValue(WindowStartupLocationProperty, value);
    }

    public double MsgBoxMaxWidth
    {
        get => GetValue(MsgBoxMaxWidthProperty);
        set => SetValue(MsgBoxMaxWidthProperty, value);
    }

    public string MsgBoxTitle
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public IDataTemplate? MsgBoxContentTemplate
    {
        get => field;
        set => SetAndRaise(MsgBoxContentTemplateProperty, ref field, value);
    }

    public ButtonDefinitions ButtonDefinitions
    {
        get => GetValue(ButtonDefinitionsProperty);
        set => SetValue(ButtonDefinitionsProperty, value);
    }

    public static readonly DirectProperty<MsgBoxButton, string> DialogResultProperty = AvaloniaProperty.RegisterDirect<MsgBoxButton, string>(
        nameof(DialogResult), o => o.DialogResult, (o, v) => o.DialogResult = v);

    public string DialogResult
    {
        get => field;
        set => SetAndRaise(DialogResultProperty, ref field, value);
    }

    protected async override void OnClick()
    {
        base.OnClick();
        var res = await MessageBoxManager.GetMessageBoxStandard(new MsgBoxParams()
        {
            ButtonDefinitions = ButtonDefinitions,
            Content = (MsgBoxContentTemplate?.Match(MsgBoxContentTemplateParam)??false)?MsgBoxContentTemplate.Build(MsgBoxContentTemplateParam):null,
            Title = MsgBoxTitle,
            MinWidth = MsgBoxMinWidth,
            MaxHeight = MsgBoxMaxHeight,
            WindowStartupLocation = WindowStartupLocation,
            MaxWidth = MsgBoxMaxWidth,
            MinHeight = MsgBoxMinHeight,
            Width = MsgBoxWidth,
            Height = MsgboxHeight,
        }).ShowWindowDialogAsync((Window)TopLevel.GetTopLevel(this));
        //DialogResult = res;
        //OnDialogClosed();
    }

    public event EventHandler? DialogClosed;

    protected virtual void OnDialogClosed()
    {
        DialogClosed?.Invoke(this, EventArgs.Empty);
    }
}