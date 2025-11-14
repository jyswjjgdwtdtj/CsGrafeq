using System;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using CsGrafeqApplication.ViewModels;
using ReactiveUI;

namespace CsGrafeqApplication.Controls;
public partial class GlobalMathKeyBoard : ContentControl
{
    public GlobalMathKeyBoard()
    {
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PseudoClasses")]
    private static extern IPseudoClasses GetPseudoClasses(StyledElement se);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var visibilityCheckBox=e.NameScope.Find<CheckBox>("VisibilityCheckBox");
        var keyboardContainer=e.NameScope.Find<Border>("PART_KeyBoardContainer");
        if (keyboardContainer != null && visibilityCheckBox != null)
        {
            var kbcPs = GetPseudoClasses(keyboardContainer);
            GetPseudoClasses(keyboardContainer).Add(":keyboardopened");
            visibilityCheckBox.IsChecked = true;
            visibilityCheckBox.IsCheckedChanged += (_, _) =>
            {
                kbcPs.Set(":keyboardopened", visibilityCheckBox.IsChecked??false);
            };
            keyboardContainer.SizeChanged += (_, _) =>
            {
                keyboardContainer.InvalidateArrange();
            };
        }
    }
}