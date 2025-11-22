using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Skia;
using CsGrafeq.Utilities;

namespace CsGrafeqApplication.Controls;

public class MathTextBox : TemplatedControl
{

    public static readonly DirectProperty<MathTextBox, string> TextProperty = AvaloniaProperty.RegisterDirect<MathTextBox, string>(
        nameof(Text), o => o.Text, (o, v) => o.Text = v);

    public string Text
    {
        get => field;
        private set => SetAndRaise(TextProperty, ref field, value);
    } = "";


    public static readonly DirectProperty<MathTextBox, string> ExpressionProperty = AvaloniaProperty.RegisterDirect<MathTextBox, string>(
        nameof(Expression), o => o.Expression, (o, v) => o.Expression = v);

    public string Expression
    {
        get => field;
        private set => SetAndRaise(ExpressionProperty, ref field, value);
    } = "";

    private MathBox? MathBox;
    private ScrollViewer?  ScrollViewer;
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        MathBox = e.NameScope.Find<MathBox>("PART_MathBox");
        ScrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        if (MathBox != null&&ScrollViewer != null)
        {
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
            };
        }
    }
}