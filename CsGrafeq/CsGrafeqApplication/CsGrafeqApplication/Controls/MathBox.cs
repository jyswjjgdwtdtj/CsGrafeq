//#define RECORD_INSTANCE
using System;
using System.Collections.Generic;
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
    private CgMathKeyboard Keyboard { get; } = new();
    private MathPainter Painter { get; } = new();
    
    public static readonly DirectProperty<MathBox, string> LaTeXProperty =
        AvaloniaProperty.RegisterDirect<MathBox, string>(
            nameof(LaTeX), o => o.LaTeX, (o, v) => o.LaTeX = v,"",BindingMode.OneWay,false);

    public static readonly DirectProperty<MathBox, float> FontSizeProperty = AvaloniaProperty.RegisterDirect<MathBox, float>(
        nameof(FontSize), o => o.FontSize, (o, v) => o.FontSize = v);

    //private Fonts Fonts;
    public MathBox()
    {
        ClipToBounds=false;
        var reader = new OpenFontReader();
        //Typography.OpenFont.Typeface? typeface = null;// reader.Read(SkiaSharp.SKData.Create(SkiaEx.MapleMono.Typeface.OpenStream()).AsStream());
        //Fonts =new Fonts(typeface==null?Fonts.GlobalTypefaces:[typeface],Scale*FontSize);
        ClipToBounds = true;
        //Instances.Add(this);
        RenderOptions.SetEdgeMode(this, EdgeMode.Antialias);
        RenderOptions.SetTextRenderingMode(this,TextRenderingMode.Antialias);
        RenderOptions.SetBitmapInterpolationMode(this,BitmapInterpolationMode.HighQuality);
        VerticalAlignment= Avalonia.Layout.VerticalAlignment.Center;
        Keyboard.Font = new Fonts(Keyboard.Font,Scale*FontSize);
        Focusable = true;
        Painter.FontSize = FontSize*Scale;
        Painter.LocalTypefaces = Keyboard.Font;
        AffectsArrange<MathBox>(LaTeXProperty);
        AffectsMeasure<MathBox>(LaTeXProperty);
        AffectsMeasure<MathBox>(VisualParentProperty);
        AffectsArrange<MathBox>(VisualParentProperty);
        InvalidateArrange();
        PressKey(CgMathKeyboardInput.Sine);
        PressKey(CgMathKeyboardInput.Sine);
        PressKey(CgMathKeyboardInput.Sine);
        PressKey(CgMathKeyboardInput.Sine);
        Keyboard.RedrawRequested += (_,_) =>
        {
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual);
        };
    }

    ~MathBox()
    {
        #if RECORD_INSTANCE
        Instances.Remove(this);
        #endif
    }

    public string LaTeX
    {
        get => field;
        private set => SetAndRaise(LaTeXProperty, ref field, value);
    } = "";

    public float FontSize
    {
        get => field;
        set => SetAndRaise(FontSizeProperty, ref field, value);
    } = 15;
    public override void Render(DrawingContext e)
    {
        Painter.LaTeX = LaTeX;;
        e.PushTransform(Matrix.CreateTranslation(-CurrentMeasuredRect.Left,-CurrentMeasuredRect.Top));
        var c = new AvaloniaCanvas(e, (Bounds.Size+new Size(20,20))*Scale);
        c.Save();
        c.Scale(1/Scale,1/Scale);
        Painter.Draw(c,0,0);
        c.Restore();
        if (Keyboard.ShouldDrawCaret&&IsFocused)
            Keyboard.DrawCaret(c, Color.Black, CaretShape.IBeam);
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
        Keyboard.InsertionPositionHighlighted = true;
        LaTeX = Keyboard.LaTeX;
        InvalidateMeasure();
        InvalidateArrange();
        MathInputted?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        //MathBox.FocusedInstance = this;
        Console.WriteLine(e.GetPosition(this));
        Keyboard.MoveCaretToPoint(e.GetPosition(this).ToSysPointF().Add(CurrentMeasuredRect.Left,CurrentMeasuredRect.Top));
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeySymbol?.Length == 1 && (e.KeySymbol?[0]??0)>=33 && (e.KeySymbol?[0]??0)<127)
        {
            char keyChar = e.KeySymbol[0];
            switch (keyChar)
            {
                case >= 'a' and <= 'z':
                {
                    PressKey(CgMathKeyboardInput.SmallA+(keyChar-'a'));
                }
                    return;
                case >= 'A' and <= 'Z':
                {
                    PressKey(CgMathKeyboardInput.A+(keyChar-'A'));
                }
                    return;
                case '/':
                    PressKey(CgMathKeyboardInput.Slash);
                    return;
                case '|':
                    PressKey(CgMathKeyboardInput.Cup);
                    return;
                case '&':
                    PressKey(CgMathKeyboardInput.Cap);
                    return;
                case '%':
                    PressKey(CgMathKeyboardInput.Modulo);
                    return;
                case '*':
                    PressKey(CgMathKeyboardInput.Multiply);
                    return;
                case '+':
                    PressKey(CgMathKeyboardInput.Plus);
                    return;
                case '-':
                    PressKey(CgMathKeyboardInput.Minus);
                    return;
                case '^':
                    PressKey(CgMathKeyboardInput.Power);
                    return;
                case >= '0' and <= '9':
                    PressKey(CgMathKeyboardInput.D0+(keyChar-'0'));
                    return;
                case '(':
                    PressKey(CgMathKeyboardInput.LeftRoundBracket);
                    return;
                case ')':
                    PressKey(CgMathKeyboardInput.RightRoundBracket);
                    return;
                case '>':
                    PressKey(CgMathKeyboardInput.GreaterThan);
                    return;
                case '<':
                    PressKey(CgMathKeyboardInput.LessThan);
                    return;
                case '=':
                    PressKey(CgMathKeyboardInput.Equals);
                    return;
            }
        }
        switch (e.PhysicalKey)
        {
            case PhysicalKey.Backspace:
                PressKey(CgMathKeyboardInput.Backspace);
                return;
            case PhysicalKey.Delete:
                PressKey(CgMathKeyboardInput.Right,CgMathKeyboardInput.Backspace);
                return;
            case PhysicalKey.ArrowLeft:
                PressKey(CgMathKeyboardInput.Left);
                return;
            case PhysicalKey.ArrowRight:
                PressKey(CgMathKeyboardInput.Right);
                return;
            case PhysicalKey.ArrowUp:
                PressKey(CgMathKeyboardInput.Up);
                return;
            case PhysicalKey.ArrowDown:
                PressKey(CgMathKeyboardInput.Down);
                return;
            case PhysicalKey.Enter:
            {
                var (math, error) = Evaluate(Keyboard.MathList);
                Console.WriteLine(error+" "+LaTeX);
                switch (math)
                {
                    case MathItem.Entity { Content: var entity }:
                        // entity is an AngouriMath.Entity
                        var simplifiedEntity = entity.Simplify();
                        Console.WriteLine(simplifiedEntity);
                        break;
                    case MathItem.Comma comma:
                        // comma is a System.Collections.Generic.IEnumerable<CSharpMath.Evaluation.MathItem>
                        break;
                    case MathItem.Set { Content: var set }:
                        // set is an AngouriMath.Core.Set
                        break;
                }
            }
                return;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        CurrentMeasuredRect = Keyboard.Measure;
        var size = CurrentMeasuredRect.Size;
        Console.WriteLine("Measured");
        return new Size(Max(size.Width,this.GetVisualParent()?.Bounds.Width??0), size.Height);
    }
}