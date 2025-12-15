using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using CsGrafeq.CSharpMath.Editor;
using CsGrafeqApplication.Core.Utils;

namespace CsGrafeqApplication.Core.Controls;

public partial class Keyboard : UserControl
{

    private void BackspaceButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this)?.Input(CgMathKeyboardInput.Backspace);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var BackspaceButton=e.NameScope.Find<Button>("BackspaceButton");
        if (BackspaceButton != null)
        {
            BackspaceButton.Click += BackspaceButton_OnClick;
        }
    }
}