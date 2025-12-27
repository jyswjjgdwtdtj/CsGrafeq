using Avalonia;
using Avalonia.Controls;
using CsGrafeqApplication.Core.Dialogs;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;

namespace CsGrafeqApplication.Core.Controls;

public class MsgBoxButton : Button
{
    public static readonly DirectProperty<MsgBoxButton, object?> MsgBoxContentProperty =
        AvaloniaProperty.RegisterDirect<MsgBoxButton, object?>(
            nameof(MsgBoxContent), o => o.MsgBoxContent, (o, v) => o.MsgBoxContent = v);

    public static readonly StyledProperty<BtnDefinitions> ButtonDefinitionsProperty =
        AvaloniaProperty.Register<MsgBoxButton, BtnDefinitions>(
            nameof(ButtonDefinitions));

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<MsgBoxButton, string>(
        nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public object? MsgBoxContent
    {
        get => field;
        set => SetAndRaise(MsgBoxContentProperty, ref field, value);
    }

    public BtnDefinitions ButtonDefinitions
    {
        get => GetValue(ButtonDefinitionsProperty);
        set => SetValue(ButtonDefinitionsProperty, value);
    }

    protected override void OnClick()
    {
        base.OnClick();
    }
}