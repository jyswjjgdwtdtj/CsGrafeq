using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Skia;
using CsGrafeq.Utilities;

namespace CsGrafeqApplication.Controls;

public class MathTextBox : TemplatedControl
{

    public static readonly DirectProperty<MathTextBox, MathBox?> InnerMathBoxProperty = AvaloniaProperty.RegisterDirect<MathTextBox, MathBox?>(
        nameof(InnerMathBox), o => o.InnerMathBox);

    public MathBox? InnerMathBox
    {
        get => field;
        private set => SetAndRaise(InnerMathBoxProperty, ref field, value);
    }

    public event EventHandler<RoutedEventArgs>? MathInputted;

    private MathBox? MathBox;
    private ScrollViewer?  ScrollViewer;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MathBox = e.NameScope.Find<MathBox>("PART_MathBox");
        ScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        if (MathBox != null&&ScrollViewer != null)
        {
            InnerMathBox=MathBox;
            MathBox.MathInputted += (_, _) =>
            {
                var caret = MathBox.CaretPosition;
                var offset = ScrollViewer.Offset.ToVec();
                var width=ScrollViewer.Viewport.Width;
                var mw = MathBox.MeasuredWidth;
                if (mw < width)
                {
                    
                }
                else if (caret-offset.X > width-5)
                {
                    offset.X = caret - width+5;
                }else if (caret < offset.X )
                {
                    offset.X = caret;
                }
                ScrollViewer.Offset = offset.ToVector();
                MathInputted?.Invoke(MathBox,new RoutedEventArgs());
            };
        }
        else
        {
            InnerMathBox = null;
        }
    }
}