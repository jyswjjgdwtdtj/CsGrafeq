using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Metadata;
using CsGrafeqApp.Controls.Displayers;

namespace CsGrafeqApp.Controls;

public class DisplayerContainer : TemplatedControl
{
    public static readonly DirectProperty<DisplayerContainer, Displayer?> DisplayerProperty =
        AvaloniaProperty.RegisterDirect<DisplayerContainer, Displayer?>(nameof(Displayer), o => o.Displayer,
            (o, v) => o.Displayer = v);
    public DisplayerContainer()
    {
    }

    [Content]
    public Displayer? Displayer
    {
        get => field;
        set => SetAndRaise(DisplayerProperty, ref field, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var border = e.NameScope.Find<Border>("PART_CoverDisplayer");
        var grid = e.NameScope.Find<Grid>("PART_Grid");
        if (border != null)
        {
            border.PropertyChanged += (s, e) =>
            {
                if (e.Property == BoundsProperty && Displayer != null)
                {
                    Displayer.ValidRect = (Rect)e.NewValue;
                    Displayer.InvalidateBuffer();
                }
            };
            grid.KeyDown += (s, e) =>
            {
                Console.WriteLine(e.Key);
                Displayer.RaiseMyKeyDown(e);
            };
            grid.KeyUp += (s, e) =>
            {
                Displayer.RaiseMyKeyUp(e);
            };
        }
    }
}