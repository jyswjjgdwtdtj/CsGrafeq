//#define RECORD_INSTANCE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CsGrafeq.CSharpMath.Editor;
using CSharpMath.SkiaSharp;
using CSharpMath.Editor;
using CSharpMath.Rendering.BackEnd;
using Typography.OpenFont;
using CsGrafeq.Utilities;
using CSharpMath.Atom;
using CSharpMath.Avalonia;
using CSharpMath.Display.FrontEnd;
using HarfBuzzSharp;
using SkiaSharp;
using Color = System.Drawing.Color;
using MathPainter = CSharpMath.Avalonia.MathPainter;
using static CSharpMath.Evaluation;
using CaretShape = CSharpMath.Rendering.FrontEnd.CaretShape;
using Size = Avalonia.Size;
using Typeface = Avalonia.Media.Typeface;

namespace CsGrafeqApplication.Controls;

public class MathBox:Control
{
    public event EventHandler? MathInputted;
    private RectangleF CurrentMeasuredRect=new(Vector4.NaN);
    #if RECORD_INSTANCE
    private static readonly List<MathBox> Instances = new();

    static MathBox()
    {
    }

    private static MathBox? FocusedInstance
    {
        get => field;
        set
        {
            field = value;
            foreach (var it in Instances)
            {
                SetMathBoxIsFocused(it,false);
            }
            if(field != null)   
                SetMathBoxIsFocused(field, true);
        }
    }

    private static readonly AttachedProperty<bool> MathBoxIsFocusedProperty =
        AvaloniaProperty.RegisterAttached<MathBox, MathBox, bool>("MathBoxIsFocused");

    private static void SetMathBoxIsFocused(MathBox obj, bool value) => obj.SetValue(MathBoxIsFocusedProperty, value);
    private static bool GetMathBoxIsFocused(MathBox obj) => obj.GetValue(MathBoxIsFocusedProperty);
    #endif
    
    private float Scale = 1;
    private CgMathKeyboard Keyboard { get; set; } = new(new());
    private MathPainter Painter { get; } = new();

    public static readonly DirectProperty<MathBox, float> FontSizeProperty = AvaloniaProperty.RegisterDirect<MathBox, float>(
        nameof(FontSize), o => o.FontSize, (o, v) => o.FontSize = v);


    public static readonly DirectProperty<MathBox, string> ExpressionProperty = AvaloniaProperty.RegisterDirect<MathBox, string>(
        nameof(Expression), o => o.Expression);

    public static readonly DirectProperty<MathBox, bool> IsCorrectProperty = AvaloniaProperty.RegisterDirect<MathBox, bool>(
        nameof(IsCorrect), o => o.IsCorrect);

    public static readonly DirectProperty<MathBox, MathList> MathListProperty =
        AvaloniaProperty.RegisterDirect<MathBox, MathList>(
            nameof(MathList), o => o.MathList, (o, v) => o.MathList = v);

    public static readonly DirectProperty<MathBox, bool> PropertyToTransmitMathInputEventProperty = AvaloniaProperty.RegisterDirect<MathBox, bool>(
        nameof(PropertyToTransmitMathInputEvent), o => o.PropertyToTransmitMathInputEvent, (o, v) => o.PropertyToTransmitMathInputEvent = v);

    public bool PropertyToTransmitMathInputEvent
    {
        get => field;
        set => SetAndRaise(PropertyToTransmitMathInputEventProperty, ref field, value);
    }

    public MathList MathList
    {
        get => Keyboard.MathList;
        set
        {
            Keyboard?.RedrawRequested -= InvokeAsyncInvalidateVisual;
            Keyboard = new(value);
            Keyboard.RedrawRequested += InvokeAsyncInvalidateVisual;
            Keyboard.Font = new Fonts(Keyboard.Font,Scale*FontSize);
            Keyboard.InsertionPositionHighlighted = true;
            
        }
    }

    public bool IsCorrect
    {
        get => field;
        private set => SetAndRaise(IsCorrectProperty, ref field, value);
    } = false;

    private void InvokeAsyncInvalidateVisual(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
    }

    public string Expression
    {
        get => field;
        private set => SetAndRaise(ExpressionProperty, ref field, value);
    } = "";

    //private Fonts Fonts;
    public MathBox():this(1){}
    public MathBox(float scale=1)
    {
        Scale = scale;
        ClipToBounds=false;
        MathList = new();
        //var reader = new OpenFontReader();
        //Typography.OpenFont.Typeface? typeface = null;// reader.Read(SkiaSharp.SKData.Create(SkiaEx.MapleMono.Typeface.OpenStream()).AsStream());
        //Fonts =new Fonts(typeface==null?Fonts.GlobalTypefaces:[typeface],Scale*FontSize);
        //Instances.Add(this);
        RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);
        RenderOptions.SetTextRenderingMode(this,TextRenderingMode.Antialias);
        RenderOptions.SetBitmapInterpolationMode(this,BitmapInterpolationMode.HighQuality);
        VerticalAlignment= Avalonia.Layout.VerticalAlignment.Center;
        Focusable = true;
        Painter.FontSize = FontSize*Scale;
        Painter.LocalTypefaces = new Fonts(Keyboard.Font,Scale*FontSize);
        AffectsMeasure<MathBox>(VisualParentProperty);
        AffectsArrange<MathBox>(VisualParentProperty);
        //PressKey(CgMathKeyboardInput.Sine,CgMathKeyboardInput.SmallX,CgMathKeyboardInput.Slash,CgMathKeyboardInput.Cosine,CgMathKeyboardInput.SmallX);
        InvalidateArrange();
    }

    ~MathBox()
    {
        #if RECORD_INSTANCE
        Instances.Remove(this);
        #endif
    }

    public float FontSize
    {
        get => field;
        set => SetAndRaise(FontSizeProperty, ref field, value);
    } = 15;
    public override void Render(DrawingContext e)
    {
        e.DrawRectangle(new SolidColorBrush(0x00000000),null,Bounds);
        if (!Keyboard.HasText)
        {
            if (Keyboard.ShouldDrawCaret && IsFocused)
            {
                var y = (float)((Bounds.Height - FontSize*2/3) / 2);
                e.DrawLine(new Pen(Brushes.Black,1.5),new(0,y),new(0,y+FontSize*2/3));
                return;
            }
        }
        Painter.LaTeX = Keyboard.LaTeX;
        e.PushTransform(Matrix.CreateTranslation(-CurrentMeasuredRect.Left,-CurrentMeasuredRect.Top+(float)((Bounds.Height-CurrentMeasuredRect.Height)/2)));

        var c = new AvaloniaCanvas(e, (Bounds.Size)*Scale);
        c.Save();
        c.Scale(1/Scale,1/Scale);
        Painter.Draw(c,0f,0f);
        if (Keyboard.ShouldDrawCaret && IsFocused)
        {
            Keyboard.DrawCaret(c, Color.Black, CaretShape.IBeam);
        }
        c.Restore();
    }

    public float CaretPosition=>((Keyboard.Display?.PointForIndex(TypesettingContext.Instance, Keyboard.InsertionIndex) ?? Keyboard.Display?.Position)??new(0,0)).X;
    public float MeasuredWidth=>CurrentMeasuredRect.Width;
    public void PressKey(params CgMathKeyboardInput[] keyboardInputs)
    {
        foreach (var keyboardInput in keyboardInputs)
        {
            Keyboard.KeyPress(keyboardInput);   
        }
        CurrentMeasuredRect = Keyboard.Measure;
        var expres = MathList.Parse();
        expres.Match((string exp) =>
        {
            Expression = exp;
            IsCorrect = true;
        },(exception =>
        {
            IsCorrect = false;
        }));
        InvalidateMeasure();
        InvalidateArrange();
        MathInputted?.Invoke(this, EventArgs.Empty);
        PropertyToTransmitMathInputEvent=!PropertyToTransmitMathInputEvent;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Keyboard.MoveCaretToPoint(e.GetPosition(this).ToSysPointF().Add(CurrentMeasuredRect.Left,CurrentMeasuredRect.Top));
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeySymbol?.Length == 1 && (e.KeySymbol?[0]??0)>=33 && (e.KeySymbol?[0]??0)<127)
        {
            char keyChar = e.KeySymbol[0];
            switch (keyChar)
            {
                case >= 'a' and <= 'z':
                    PressKey(CgMathKeyboardInput.SmallA+(keyChar-'a'));
                    goto HandledEvent;
                case >= 'A' and <= 'Z':
                    PressKey(CgMathKeyboardInput.A+(keyChar-'A'));
                    goto HandledEvent;
                case '/':
                    PressKey(CgMathKeyboardInput.Slash);
                    goto HandledEvent;
                case '|':
                    PressKey(CgMathKeyboardInput.Cup);
                    goto HandledEvent;
                case '&':
                    PressKey(CgMathKeyboardInput.Cap);
                    goto HandledEvent;
                case '%':
                    PressKey(CgMathKeyboardInput.Modulo);
                    goto HandledEvent;
                case '*':
                    PressKey(CgMathKeyboardInput.Multiply);
                    goto HandledEvent;
                case '+':
                    PressKey(CgMathKeyboardInput.Plus);
                    goto HandledEvent;
                case '-':
                    PressKey(CgMathKeyboardInput.Minus);
                    goto HandledEvent;
                case '^':
                    PressKey(CgMathKeyboardInput.Power);
                    goto HandledEvent;
                case >= '0' and <= '9':
                    PressKey(CgMathKeyboardInput.D0+(keyChar-'0'));
                    goto HandledEvent;
                case '(':
                    PressKey(CgMathKeyboardInput.LeftRoundBracket);
                    goto HandledEvent;
                case ')':
                    PressKey(CgMathKeyboardInput.RightRoundBracket);
                    goto HandledEvent;
                case '>':
                    PressKey(CgMathKeyboardInput.GreaterThan);
                    goto HandledEvent;
                case '<':
                    PressKey(CgMathKeyboardInput.LessThan);
                    goto HandledEvent;
                case '=':
                    PressKey(CgMathKeyboardInput.Equals);
                    goto HandledEvent;
                case '.':
                    PressKey(CgMathKeyboardInput.Decimal);
                    goto HandledEvent;
            }
        }

        if (e.KeyModifiers == KeyModifiers.None)
        {
            switch (e.PhysicalKey)
            {
                case PhysicalKey.Backspace:
                    PressKey(CgMathKeyboardInput.Backspace);
                    goto HandledEvent;
                case PhysicalKey.Delete:
                    PressKey(CgMathKeyboardInput.Right,CgMathKeyboardInput.Backspace);
                    goto HandledEvent;
                case PhysicalKey.ArrowLeft:
                    PressKey(CgMathKeyboardInput.Left);
                    goto HandledEvent;
                case PhysicalKey.ArrowRight:
                    PressKey(CgMathKeyboardInput.Right);
                    goto HandledEvent;
                case PhysicalKey.ArrowUp:
                    PressKey(CgMathKeyboardInput.Up);
                    goto HandledEvent;
                case PhysicalKey.ArrowDown:
                    PressKey(CgMathKeyboardInput.Down);
                    goto HandledEvent;
            }
        }

        if (e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case  Key.Z:
                    goto HandledEvent;
                case Key.A:
                    goto HandledEvent;
                case Key.Y:
                    goto HandledEvent;
            }
        }
        return;
        HandledEvent:
        e.Handled = true;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        CurrentMeasuredRect = Keyboard.Measure;
        var size = CurrentMeasuredRect.Size;
        return new Size(Max(size.Width,30), Max(size.Height,30));
    }
}