using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CsGrafeq.Utilities;

namespace CsGrafeqApplication.Core.Controls;

public partial class MathTextBox : UserControl
{
    public static readonly DirectProperty<MathTextBox, MathBox?> InnerMathBoxProperty =
        AvaloniaProperty.RegisterDirect<MathTextBox, MathBox?>(
            nameof(InnerMathBox), o => o.InnerMathBox);

    public MathTextBox()
    {
        InitializeComponent();
        InnerMathBox = PART_MathBox;
    }

    public MathBox? InnerMathBox
    {
        get => field;
        private set => SetAndRaise(InnerMathBoxProperty, ref field, value);
    }

    public event EventHandler<RoutedEventArgs>? MathInputted;

    private void PART_MathBox_OnMathInputted(object? sender, EventArgs e)
    {
        var caret = PART_MathBox.CaretPosition;
        var offset = PART_ScrollViewer.Offset.ToVec();
        var width = PART_ScrollViewer.Viewport.Width;
        var mw = PART_MathBox.MeasuredWidth;
        if (mw < width)
        {
        }
        else if (caret - offset.X > width - 5)
        {
            offset.X = caret - width + 5;
        }
        else if (caret < offset.X)
        {
            offset.X = caret;
        }

        PART_ScrollViewer.Offset = offset.ToVector();
        MathInputted?.Invoke(PART_MathBox, new RoutedEventArgs());
    }
}