using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using CsGrafeqApp.Controls.Displayers;

namespace CsGrafeqApp.Controls;

public class DisplayerContainer : TemplatedControl
{
    public static readonly DirectProperty<DisplayerContainer, Displayer?> DisplayerProperty = AvaloniaProperty.RegisterDirect<DisplayerContainer, Displayer?>(nameof(Displayer), o => o.Displayer, (o, v) => o.Displayer = v);
    [Content]
    public Displayer? Displayer
    {
        get => field;
        set => SetAndRaise(DisplayerProperty, ref field, value);
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var border =e.NameScope.Find<Border>("PART_CoverDisplayer");
        if(border!=null)
            border.PropertyChanged += (s, e) =>
            {
                if (e.Property == Border.BoundsProperty&&Displayer!=null)
                {
                    Displayer.ValidRect = ((Rect)e.NewValue);
                    Displayer.InvalidateBuffer();
                }
            };
    }
}