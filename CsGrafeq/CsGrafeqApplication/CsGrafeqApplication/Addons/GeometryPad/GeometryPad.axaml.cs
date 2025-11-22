using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using CsGrafeq.Interval;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using CsGrafeqApplication.Controls.Displayers;
using DialogHostAvalonia;
using SkiaSharp;
using static CsGrafeq.Shapes.GeometryMath;
using static CsGrafeqApplication.AvaloniaMath;
using static CsGrafeqApplication.Controls.SkiaEx;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using AvaSize = Avalonia.Size;
using GeoHalf = CsGrafeq.Shapes.Half;
using static CsGrafeqApplication.Extension;
using static CsGrafeq.Utilities.ThrowHelper;
using static CsGrafeqApplication.GlobalSetting;

namespace CsGrafeqApplication.Addons.GeometryPad;

public partial class GeometryPad : Addon
{
    /// <summary>
    ///     所有几何图形的渲染目标
    /// </summary>
    private readonly Renderable MainRenderTarget = new();

    /// <summary>
    ///     ViewModel
    /// </summary>
    private readonly GeometryPadViewModel VM;

    public GeometryPad()
    {
        Setting = new GeometryPadSetting(this);
        DataContext = VM = new GeometryPadViewModel();
        Shapes = VM.Shapes;
        CurrentAction = GeometryActions.Actions.FirstOrDefault()?.FirstOrDefault()!;
        Shapes.CollectionChanged += (s, e) =>
        {
            // 添加隐函数时启用移动缩放优化
            if (Shapes.GetShapes<ImplicitFunction>().Count() > 0)
            {
                Owner?.MovingOptimization = true;
                Owner?.ZoomingOptimization = true;
            }

            var newshapes = e.NewItems?.OfType<GeoShape>() ?? [];
            var newfuncs = newshapes.OfType<ImplicitFunction>();
            // 设置隐函数渲染目标大小
            foreach (var fn in newfuncs)
            {
                fn.RenderTarget.Changed = true;
                fn.RenderTarget.SetBitmapSize(MainRenderTarget.GetSize());
            }

            // 主渲染目标标记为已更改
            if (newshapes.Any(o => o is GeometryShape)) MainRenderTarget.Changed = true;
            Owner?.AskForRender();
        };
        Shapes.OnShapeChanged += sp =>
        {
            if (sp is ImplicitFunction impf)
                impf.RenderTarget.Changed = true;
            else if (sp is GeometryShape gs)
                MainRenderTarget.Changed = true;
            Owner?.AskForRender();
        };
        Layers.Add(MainRenderTarget);
        MainRenderTarget.OnRender += Renderable_OnRender;
#if DEBUG

        var p1 = AddShape(new GeoPoint(new PointGetter_FromLocation((0.5, 0.5))));
        var p2 = AddShape(new GeoPoint(new PointGetter_FromLocation((1.5, 1.5))));
        var s1 = AddShape(new Straight(new LineGetter_Connected(p1, p2)));
        var p3 = AddShape(new GeoPoint(new PointGetter_OnLine(s1, (0, 0))));
        var c1 = AddShape(new Circle(new CircleGetter_FromCenterAndRadius(p1)));
        for (var i = 0; i < 1; i++)
        {
            var ip1 = AddShape(CreateImpFunc("y=x+1"));
        }
#endif
        InitializeComponent();
    }

    /// <summary>
    ///     正在被移动的点
    /// </summary>
    private GeoPoint? MovingPoint { get; set; }

    /// <summary>
    ///     点拖动操作的起始点位置 字符串形式
    /// </summary>
    private Vector2<string> PointMovementEndPositionStr { get; set; } = new("0", "0");

    /// <summary>
    ///     点拖动操作的结束点位置 字符串形式
    /// </summary>
    private Vector2<string> PointMovementStartPositionStr { get; set; } = new("0", "0");

    /// <summary>
    ///     移动操作正在进行时的指针位置
    /// </summary>
    private AvaPoint PointerMovedPosition { get; set; }

    /// <summary>
    ///     移动操作开始时的指针位置
    /// </summary>
    private AvaPoint PointerPressedPosition { get; set; }

    /// <summary>
    ///     上一次指针操作的属性
    /// </summary>
    private PointerPointProperties LastPointerProperties { get; set; }

    /// <summary>
    ///     移动结束时的指针位置
    /// </summary>
    private AvaPoint PointerReleasedPosition { get; set; }

    /// <summary>
    ///     当前几何操作
    /// </summary>
    internal ActionData CurrentAction { get; private set; }

    /// <summary>
    ///     图形的集合
    /// </summary>
    public ShapeList Shapes { get; init; }

    /// <summary>
    ///     所有者
    /// </summary>
    public override Displayer? Owner
    {
        get => base.Owner;
        set
        {
            if (!(value is CartesianDisplayer))
                throw new Exception();
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            base.Owner = value;
            PixelToMathX = value.PixelToMathX;
            PixelToMathY = value.PixelToMathY;
            PixelToMath = value.PixelToMath;
            PixelToMathSK = value.PixelToMath;
            MathToPixelX = value.MathToPixelX;
            MathToPixelY = value.MathToPixelY;
            MathToPixel = value.MathToPixel;
            MathToPixelSK = value.MathToPixelSK;
            if (Shapes.GetShapes<ImplicitFunction>().Count() > 0)
            {
                Owner?.MovingOptimization = true;
                Owner?.ZoomingOptimization = true;
            }
        }
    }

    public override string AddonName => "GeometryPad";


    /// <summary>
    ///     设置Action
    /// </summary>
    /// <param name="ad"></param>
    internal void SetAction(ActionData ad)
    {
        if (CurrentAction == ad)
            return;
        CurrentAction = ad;
        Shapes.ClearSelected();
    }

    /// <summary>
    ///     添加图形
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="shape"></param>
    /// <returns></returns>
    public T? AddShape<T>(T shape) where T : GeoShape
    {
        if (shape.IsDeleted)
            return null;
        Shapes.Add(shape);
        DoShapeAdd(shape);
        if (shape is ImplicitFunction impf) Layers.Add(impf.RenderTarget);
        return shape;
    }

    private void NewFuncOnClick(object? sender, KeyEventArgs e)
    {
        /*if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None &&
            !string.IsNullOrWhiteSpace(NewFuncTextBox.Text) && IntervalCompiler.TryCompile(NewFuncTextBox.Text))
        {
            AddShape(CreateImpFunc(NewFuncTextBox.Text));
            NewFuncTextBox.Text = "";
        }*/
    }

    private void ImpFuncOnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border border) FlyoutBase.ShowAttachedFlyout(border);
    }

    /// <summary>
    ///     创建新的隐函数图形
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private ImplicitFunction CreateImpFunc(string expression)
    {
        var ins = new ImplicitFunction(expression);
        ins.RenderTarget.OnRender += (dc, s) =>
            RenderFunction(dc, new SKRectI((int)s.Left, (int)s.Top, (int)s.Right, (int)s.Bottom), ins);
        return ins;
    }

    private void NewFuncPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty)
            {
                if (tb.Text == "" || IntervalCompiler.TryCompile(tb.Text))
                    DataValidationErrors.ClearErrors(tb);
                else
                    DataValidationErrors.SetError(tb, new Exception());
            }
    }

    /// <summary>
    ///     阻止默认的Redo Undo操作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImpFuncLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) tb.AddHandler(KeyDownEvent, TunnelTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    ///     监听隐函数控件表达式变化 设置是否错误
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ImpFuncPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty)
                if ((string)e.NewValue != (string)e.OldValue)
                {
                    if (tb.Text == "" || IntervalCompiler.TryCompile(tb.Text))
                        DataValidationErrors.ClearErrors(tb);
                    else
                        DataValidationErrors.SetError(tb, new Exception());
                    DoFuncTextChange(tb, (string)e.NewValue, (string)e.OldValue);
                }
    }

    private void AddFuncTapped(object? sender, TappedEventArgs e)
    {
    }

    private void NewFuncTextBoxTemplateApplied(object? s, TemplateAppliedEventArgs e)
    {
        var tb = s as TextBox;
        var borderelement = e.NameScope.Find<Border>("PART_BorderElement");
        borderelement.CornerRadius = new CornerRadius(0);
        borderelement.BorderThickness = new Thickness(0, 0, 0, 2);
        borderelement.Background = Brushes.Transparent;
        tb.LostFocus += (s, e) => { borderelement.Background = Brushes.Transparent; };
        tb.GotFocus += (s, e) => { borderelement.Background = Brushes.Transparent; };
    }

    private void Expander_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is Expander expander)
        {
            Console.WriteLine(123);
            expander.TemplateApplied += (_, te) =>
            {
                var togglebtn= te.NameScope.Find<ToggleButton>("PART_ToggleButton");
                togglebtn.TemplateApplied += (_, tte) =>
                {
                    var path= tte.NameScope.Find<Path>("PART_ExpandIcon");
                    path.Bind(Path.FillProperty, Resources.GetResourceObservable("CgForegroundBrush"));
                };
            };
        }
    }

    private void AddFuncQuestionsClicked(object? sender, RoutedEventArgs e)
    {
        var tp = TopLevel.GetTopLevel(this);
        if (tp != null)
        {
            var lb = new ListBox { MaxHeight = tp.Height * 2 / 3, MaxWidth = tp.Width * 2 / 3 };
            var examples = new List<string>(ImplicitFunctionExamples.Examples);
            for (var i = 0; i < examples.Count; i++) examples[i] = examples[i].Split(";")[0];
            lb.ItemsSource = examples;
            lb.SelectionMode = SelectionMode.Single;
            lb.SelectionChanged += (s, se) =>
            {
                /*NewFuncTextBox.Text = lb?.SelectedItem?.ToString() ?? NewFuncTextBox.Text;
                DialogHost.Close("dialog");
                NewFuncTextBox.Focus();*/
            };
            var sv = new ScrollViewer
            {
                MaxHeight = tp.Height * 2 / 3, MaxWidth = tp.Width * 2 / 3,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
            };
            sv.Content = lb;
            DialogHost.Show(sv, "dialog");
        }
    }

    #region CoordinateTransformFuncs

    /// <summary>
    ///     将数学坐标转换为像素坐标（Avalonia.Point） 来自Owner
    /// </summary>
    private Func<Vec, AvaPoint> MathToPixel = VoidFunc<Vec, AvaPoint>;

    /// <summary>
    ///     将数学坐标转换为像素坐标（SKPoint） 来自Owner
    /// </summary>
    protected Func<Vec, SKPoint> MathToPixelSK = VoidFunc<Vec, SKPoint>;

    /// <summary>
    ///     将像素坐标转换为数学坐标（Avalonia.Point） 来自Owner
    /// </summary>
    protected Func<AvaPoint, Vec> PixelToMath = VoidFunc<AvaPoint, Vec>;

    /// <summary>
    ///     将像素坐标转换为数学坐标（SKPoint） 来自Owner
    /// </summary>
    protected Func<SKPoint, Vec> PixelToMathSK = VoidFunc<SKPoint, Vec>;

    protected Func<double, double> PixelToMathX = VoidFunc<double, double>,
        PixelToMathY = VoidFunc<double, double>,
        MathToPixelX = VoidFunc<double, double>,
        MathToPixelY = VoidFunc<double, double>;

    #endregion


    #region KeyAction

    public override void Delete()
    {
        List<GeometryShape> todelete = new();
        foreach (var shape in Shapes.GetSelectedShapes<GeometryShape>().ToArray())
            if (shape.Selected)
                todelete.Add(shape);

        if (todelete.Count > 0) DoGeoShapesDelete(todelete.ToArray());
    }

    public override void SelectAll()
    {
        foreach (var shape in Shapes.OfType<GeometryShape>().ToArray())
            shape.Selected = true;
    }

    public override void DeSelectAll()
    {
        foreach (var shape in Shapes.OfType<GeometryShape>().ToArray())
            shape.Selected = false;
    }

    protected override bool AddonKeyDown(KeyEventArgs e)
    {
        if (Owner == null) return DoNext;
        var res = DoNext;
        switch (e.Key)
        {
            case Key.Tab:
            {
                foreach (var shape in Shapes.GetSelectedShapes<GeometryShape>().ToArray())
                    if (shape.Selected)
                    {
                        res = Intercept;
                        shape.Selected = false;
                        foreach (var subshape in shape.SubShapes) subshape.Selected = true;
                    }
            }
                break;
        }

        if (res == Intercept) MainRenderTarget.Changed = true;
        return res;
    }

    #endregion


    #region PointerAction

    /// <summary>
    ///     指示试图被移动的点是否可移动
    /// </summary>
    private bool IsMovingPointMovable { get; set; }

    protected override bool AddonPointerPressed(AddonPointerEventArgs e)
    {
        if (Owner == null) return DoNext;
        LastPointerProperties = e.Properties;
        PointerPressedPosition = e.Location;
        if (CurrentAction.Name.English == "Select")
        {
            Shapes.ClearSelected();
        }
        else
        {
            TryGetShape<GeoPoint>(e.Location, out var mp);
            MovingPoint = mp;
            if (MovingPoint != null)
            {
                if (MovingPoint.PointGetter is PointGetter_Movable pgm && MovingPoint.IsUserEnabled)
                {
                    IsMovingPointMovable = true;
                    PointMovementStartPositionStr = new Vector2<string>(pgm.PointX.ValueStr, pgm.PointY.ValueStr);
                }
                else
                {
                    IsMovingPointMovable = false;
                }

                return Intercept;
            }
        }

        MovingPoint = null;
        return DoNext;
    }

    protected override bool AddonPointerMoved(AddonPointerEventArgs e)
    {
        if (Owner == null) return DoNext;
        LastPointerProperties = e.Properties;
        PointerMovedPosition = e.Location;
        if (CurrentAction.Name.English == "Select" && e.Properties.IsLeftButtonPressed)
        {
            MainRenderTarget.Changed = true;
            return Intercept;
        }

        if (MovingPoint == null) return DoNext;

        if (e.Properties.IsLeftButtonPressed)
        {
            if (OS.GetOSType() == OSType.Android && (PointerMovedPosition - PointerPressedPosition).GetLength() < 10)
            {
            }
            else if (PointerMovedPosition == PointerPressedPosition)
            {
            }
            else
            {
                Shapes.ClearSelected();
            }

            var previous = MovingPoint.PointGetter;
            //如点附着在基于这个点的几何图形上 会造成无限递归 使程序崩溃 同时破坏了图形的树的单向结构
            /*if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var previoustype = GetPGType(previous);
                var newtype=GetNewPointGetterTypeFromLocation(e.Location);
                if (previoustype!=newtype&&newtype!=PGType.None)
                {
                    var getter = GetNewPointGetterFromLocation(e.Location);
                    if (getter.GetType() != previous.GetType() ||
                        !Extension.ArrayEqual(getter.Parameters, previous.Parameters))
                    {
                        DoPointGetterChange(MovingPoint, previous, getter);
                    }
                }
            }*/
            if (MovingPoint.PointGetter is PointGetter_Movable pg && MovingPoint.IsUserEnabled)
            {
                pg.SetPoint(Owner.PixelToMath(PointerMovedPosition));
                if (MovingPoint.PointGetter is PointGetter_FromLocation)
                {
                    pg.SetPoint(
                        Owner.PixelToMath(FindNearestPointOnTwoAxisLine(PointerMovedPosition)));
                }
                else if (MovingPoint.PointGetter is PointGetter_OnLine)
                {
                    var newp = FindNearestPointOnTwoAxisLine(MathToPixel(pg.GetPoint()));
                    if (newp.X != PointerMovedPosition.X)
                        pg.PointX.SetNumber(PixelToMathX(newp.X));
                    else if (newp.Y != PointerMovedPosition.Y)
                        pg.PointY.SetNumber(PixelToMathY(newp.Y));
                    else
                        pg?.SetPoint(Owner.PixelToMath(newp));
                }

                MovingPoint.RefreshValues();
            }

            MainRenderTarget.Changed = true;
            return Intercept;
        }

        return DoNext;
    }

    protected override bool AddonPointerReleased(AddonPointerEventArgs e)
    {
        if (Owner == null) return DoNext;
        LastPointerProperties = e.Properties;
        PointerReleasedPosition = e.Location;
        var disp = (Owner as DisplayControl)!;
        if (CurrentAction.Name.English == "Select")
        {
            var rect = RegulateRectangle(new AvaRect(PointerPressedPosition,
                new AvaSize(PointerReleasedPosition.X - PointerPressedPosition.X,
                    PointerReleasedPosition.Y - PointerPressedPosition.Y)));
            var mathrect = new CgRectangle(Owner.PixelToMath(new AvaPoint(rect.Left, rect.Top + rect.Height)),
                new Vec(rect.Width, rect.Height) / disp.UnitLength);
            foreach (var s in Shapes.GetShapes<GeometryShape>())
                s.Selected = s.IsIntersectedWithRect(mathrect);
            MainRenderTarget.Changed = true;
            PointerPressedPosition = new AvaPoint(double.NaN, double.NaN);
            return Intercept;
        }

        if (MovingPoint == null) return DoNext;

        if (PointerReleasedPosition != PointerPressedPosition)
            Shapes.ClearSelected();
        if (IsMovingPointMovable)
        {
            var pgm = (PointGetter_Movable)MovingPoint.PointGetter;
            DoPointMove(MovingPoint, PointMovementStartPositionStr,
                new Vector2<string>(pgm.PointX.ValueStr, pgm.PointY.ValueStr));
        }

        MovingPoint = null;
        return Intercept;
    }

    protected override bool AddonPointerTapped(AddonPointerEventArgsBase e)
    {
        if (Owner == null) return DoNext;
        {
            // 放置点
            if (CurrentAction.Name.English == "Put")
            {
                Shapes.ClearSelected();
                PutPoint(e.Location)?.Selected = true;
                return Intercept;
            }

            var first = Shapes.GetSelectedShapes<GeoPoint>().FirstOrDefault();
            var selectfirst = false;
            if (CurrentAction.Args.Contains(ShapeArg.Point) &&
                TryGetShape<GeoPoint>(e.Location, out var point))
            {
                if (first == point)
                    selectfirst = true;
                else
                    point.Selected = !point.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Line) &&
                     TryGetShape<GeoLine>(e.Location, out var line))
            {
                line.Selected = !line.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Circle) &&
                     TryGetShape<GeoCircle>(e.Location, out var circle))
            {
                circle.Selected = !circle.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Polygon) &&
                     TryGetShape<GeoPolygon>(e.Location, out var polygon))
            {
                polygon.Selected = !polygon.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Point) && CurrentAction.Name.English != "Move" &&
                     CurrentAction.Name.English != "Select")
            {
                PutPoint(e.Location)?.Selected = true;
            }

            if (CurrentAction.Name.English != "Move" && CurrentAction.Name.English != "Select")
            {
                if (!CurrentAction.Args.Contains(ShapeArg.Point))
                    Shapes.ClearSelected<GeoPoint>();
                if (!CurrentAction.Args.Contains(ShapeArg.Line))
                    Shapes.ClearSelected<GeoLine>();
                if (!CurrentAction.Args.Contains(ShapeArg.Circle))
                    Shapes.ClearSelected<GeoCircle>();
                if (!CurrentAction.Args.Contains(ShapeArg.Polygon))
                    Shapes.ClearSelected<GeoPolygon>();
            }

            var SAll = Shapes.GetSelectedShapes<GeometryShape>().ToList();
            var SPoints = Shapes.GetSelectedShapes<GeoPoint>().ToArray();
            var SLines = Shapes.GetSelectedShapes<GeoLine>().ToArray();
            var SCircles = Shapes.GetSelectedShapes<GeoCircle>().ToArray();
            var SPolygons = Shapes.GetSelectedShapes<GeoPolygon>().ToArray();
            var plen = SPoints.Length;
            var llen = SLines.Length;
            var clen = SCircles.Length;
            var polen = SPolygons.Length;
            var needplen = CurrentAction.Args.Count(i => i == ShapeArg.Point);
            var needllen = CurrentAction.Args.Count(i => i == ShapeArg.Line);
            var needclen = CurrentAction.Args.Count(i => i == ShapeArg.Circle);
            var needpolen = CurrentAction.Args.Count(i => i == ShapeArg.Circle);
            if (CurrentAction.IsMultiPoint)
            {
                if (plen > 2)
                    if (selectfirst)
                    {
                        var shape = GeometryActions.CreateShape(CurrentAction.Self,
                            (Getter)CurrentAction.GetterConstructor.Invoke(SPoints.ToArray()));
                        AddShape(shape);
                        Shapes.ClearSelected();
                    }
                    else if (plen <= 2 && selectfirst)
                    {
                        Shapes.ClearSelected<GeoPoint>();
                    }

                return Intercept;
            }

            if (CurrentAction.Name.English == "Select" || CurrentAction.Name.English == "Move") return Intercept;
            //==========================
            if (needplen < plen || needclen < clen || needllen < llen || needpolen < polen)
            {
                Shapes.ClearSelected();
                return Intercept;
            }

            int GetIndex(GeometryShape s)
            {
                switch (s)
                {
                    case GeoPoint _:
                        return 0;
                    case GeoLine _:
                        return 5;
                    case GeoCircle _:
                        return 10;
                    case GeoPolygon _:
                        return 15;
                    default:
                        Throw("");
                        return 20;
                }
            }

            if (needplen == plen && needclen == clen && needllen == llen && needpolen == polen)
            {
                SAll.Sort((s1, s2) =>
                {
                    var i1 = GetIndex(s1);
                    var i2 = GetIndex(s2);
                    if (i1 < i2)
                        return -1;
                    if (i1 == i2)
                        return 0;
                    return 1;
                });
                var shape = GeometryActions.CreateShape(CurrentAction.Self,
                    (Getter)CurrentAction.GetterConstructor.Invoke(SAll?.Select(o => (object?)o)?.ToArray() ?? []));
                AddShape(shape);
                Shapes.ClearSelected();
            }

            return Intercept;
        }
    }

    #endregion

    #region RenderAction

    protected void Renderable_OnRender(SKCanvas dc, SKRect rect)
    {
        if (Owner is null) return;
        dc.Save();
        dc.ClipRect(rect);
        RenderShapes(dc, rect, Shapes.GetShapes<GeometryShape>());
        if (CurrentAction.Name.English == "Select" && LastPointerProperties.IsLeftButtonPressed)
        {
            var selrect = RegulateRectangle(new AvaRect(PointerPressedPosition,
                new AvaSize(PointerMovedPosition.X - PointerPressedPosition.X,
                    PointerMovedPosition.Y - PointerPressedPosition.Y)));
            dc.DrawRect(selrect.ToSKRect(), StrokeMedian);
        }

        dc.Restore();
    }

    private void RenderShapes(SKCanvas dc, SKRect rect, IEnumerable<GeometryShape> shapes)
    {
        var UnitLength = (Owner as CartesianDisplayer)!.UnitLength;
        var LT = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Bottom));
        var RT = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Bottom));
        var RB = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Top));
        var LB = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Top));
        var cd = (CartesianDisplayer)Owner;
        using (SKPaint TPFilledPaint = new() { IsAntialias = true },
               FilledPaint = new() { IsAntialias = true },
               StrokePaint = new() { IsStroke = true, IsAntialias = true, StrokeWidth = 2 },
               StrokePaint1 = new() { IsStroke = true, IsAntialias = true, StrokeWidth = 1 },
               StrokeMain = new() { Color = cd.AxisPaintMain.Color, IsStroke = true, IsAntialias = true },
               PaintMain = new() { Color = cd.AxisPaintMain.Color, IsAntialias = true },
               StrokePaintMain = new() { Color = cd.AxisPaintMain.Color, IsAntialias = true, IsStroke = true },
               BubbleBack = new() { Color = cd.AxisPaint1.Color.WithAlpha(90), IsAntialias = true })
        {
            foreach (var shape in shapes)
            {
                if (shape is null)
                    continue;
                if (shape.IsDeleted)
                    continue;
                if (!shape.Visible)
                    continue;
                if (shape is GeoPoint)
                    continue;
                FilledPaint.Color = new SKColor(shape.Color).WithAlpha(255);
                TPFilledPaint.Color = new SKColor(shape.Color).WithAlpha(90);
                StrokePaint.Color = new SKColor(shape.Color).WithAlpha(255);
                switch (shape)
                {
                    case Straight s:
                    {
                        var v1 = s.Current.Point1;
                        var v2 = s.Current.Point2;
                        var vs = GetValidVec(
                            GetIntersectionOfSegmentAndLine(LT, RT, v1, v2),
                            GetIntersectionOfSegmentAndLine(RT, RB, v1, v2),
                            GetIntersectionOfSegmentAndLine(RB, LB, v1, v2),
                            GetIntersectionOfSegmentAndLine(LB, LT, v1, v2)
                        );
                        if (s.Selected)
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), StrokePaint);
                        else
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), StrokePaintMain);

                        dc.DrawBubble($"{Properties.Resources.StraightText}:{s.Name}",
                            MathToPixelSK((s.Current.Point1 + s.Current.Point2) / 2),
                            BubbleBack, PaintMain);
                    }
                        break;
                    case GeoSegment s:
                    {
                        var v1 = s.Current.Point1;
                        var v2 = s.Current.Point2;
                        if (s.Selected)
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(v2), StrokePaint);
                        else
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(v2), StrokePaintMain);

                        dc.DrawBubble($"{MultiLanguageResources.Instance.SegmentText}:{s.Name}",
                            MathToPixelSK((s.Current.Point1 + s.Current.Point2) / 2),
                            BubbleBack, PaintMain);
                    }
                        break;
                    case GeoHalf h:
                    {
                        var v1 = h.Current.Point1;
                        var v2 = h.Current.Point2;
                        var vs = GetValidVec(
                            GetIntersectionOfSegmentAndLine(LT, RT, v1, v2),
                            GetIntersectionOfSegmentAndLine(RT, RB, v1, v2),
                            GetIntersectionOfSegmentAndLine(RB, LB, v1, v2),
                            GetIntersectionOfSegmentAndLine(LB, LT, v1, v2)
                        );
                        Vec p;
                        if (v1.X == v2.X)
                        {
                            if ((vs.Item1.Y - v1.Y) / Sign(v2.Y - v1.Y) > (vs.Item2.Y - v1.Y) / Sign(v2.Y - v1.Y))
                                p = vs.Item1;
                            else
                                p = vs.Item2;
                        }
                        else
                        {
                            if ((vs.Item1.X - v1.X) / Sign(v2.X - v1.X) > (vs.Item2.X - v1.X) / Sign(v2.X - v1.X))
                                p = vs.Item1;
                            else
                                p = vs.Item2;
                        }

                        if (h.Selected)
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(p), StrokePaint);
                        else
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(p), StrokePaintMain);

                        dc.DrawBubble($"{MultiLanguageResources.Instance.HalfLineText}:{h.Name}",
                            MathToPixelSK((h.Current.Point1 + h.Current.Point2) / 2),
                            BubbleBack, PaintMain);
                    }
                        break;
                    case GeoPolygon polygon:
                    {
                        var ps = new SKPoint[polygon.Locations.Length + 1];
                        for (var j = 0; j < ps.Length - 1; j++) ps[j] = MathToPixelSK(polygon.Locations[j]);

                        ps[polygon.Locations.Length] = ps[0];
                        var path = new SKPath();
                        path.AddPoly(ps);
                        if (polygon.Filled)
                        {
                            if (polygon.Selected)
                            {
                                dc.DrawPath(path, TPFilledPaint);
                                dc.DrawPath(path, StrokePaint);
                            }
                            else
                            {
                                dc.DrawPath(path, FilledTranparentGrey);
                                dc.DrawPath(path, StrokeMain);
                            }
                        }
                        else
                        {
                            if (polygon.Selected)
                                dc.DrawPath(path, StrokePaint);
                            else
                                dc.DrawPath(path, StrokeMain);
                        }

                        dc.DrawBubble($"{MultiLanguageResources.Instance.PolygonText}:{polygon.Name}",
                            MathToPixelSK((polygon.Locations[0] + polygon.Locations[1]) / 2) - new SKPoint(0, 20),
                            BubbleBack, PaintMain);
                    }
                        break;
                    case GeoCircle circle:
                    {
                        var cs = circle.InnerCircle;
                        var pf = MathToPixelSK(cs.Center);
                        var s = new SKSize((float)(cs.Radius * UnitLength), (float)(cs.Radius * UnitLength));
                        if (circle.Selected)
                            dc.DrawOval(pf, s, StrokePaint);
                        else
                            dc.DrawOval(pf, s, StrokeMain);

                        var r2 = cs.Radius * Sqrt(2) / 2;
                        dc.DrawBubble($"{MultiLanguageResources.Instance.CircleText}:{circle.Name}",
                            MathToPixelSK(circle.InnerCircle.Center + new Vec(-r2, r2)), BubbleBack, PaintMain);
                    }
                        break;
                    case Angle ang:
                    {
                        var angle = ang.AngleData;
                        var pf = MathToPixelSK(angle.AnglePoint);
                        var arg1 =
                            CustomMod(MathToPixel(angle.Point1).Sub(MathToPixel(angle.AnglePoint)).Arg() / PI * 180,
                                360);
                        var arg2 =
                            CustomMod(MathToPixel(angle.Point2).Sub(MathToPixel(angle.AnglePoint)).Arg() / PI * 180,
                                360);
                        var aa = angle.Angle;
                        var a = arg2 - arg1;
                        a = CustomMod(a, 360);
                        if (a > 180)
                            a -= 360;
                        if (ang.Selected)
                            dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true,
                                StrokePaint);
                        else
                            dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true,
                                StrokeMain);
                        dc.DrawBubble($"{Abs(aa).ToString("0.00")}°", pf.OffSetBy(2, 2 - 20), BubbleBack,
                            PaintMain);
                    }
                        break;
                }
            }

            foreach (var p in Shapes.GetShapes<GeoPoint>())
            {
                if (p is null)
                    continue;
                if (p.IsDeleted)
                    continue;
                if (!p.Visible)
                    continue;
                FilledPaint.Color = new SKColor(p.Color).WithAlpha(255);
                TPFilledPaint.Color = new SKColor(p.Color).WithAlpha(90);
                StrokePaint1.Color = new SKColor(p.Color).WithAlpha(255);
                var index = 0;
                var loc = MathToPixelSK(p.Location);
                dc.DrawBubble($"{MultiLanguageResources.Instance.PointText}:" + p.Name,
                    loc.OffSetBy(2, 2 + 20 * index++), BubbleBack, PaintMain);
                if (p == MovingPoint)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledMedian);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokeMedian);
                    dc.DrawBubble(
                        $"({Round(MovingPoint.Location.X, 8)},{Round(MovingPoint.Location.Y, 8)}) {(MovingPoint.PointGetter is PointGetter_Movable && MovingPoint.IsUserEnabled ? "" : MultiLanguageResources.Instance.CantBeMovedText)}",
                        loc.OffSetBy(2, 2 + 20 * index++), BubbleBack, PaintMain);
                }
                else if (p.Selected)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledPaint);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokePaint1);
                }
                else if (p.PointerOver)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledPaint);
                }
                else
                {
                    dc.DrawOval(loc, new SKSize(3, 3), PaintMain);
                }

                foreach (var t in p.TextGetters)
                    if (!string.IsNullOrEmpty(t.GetText()))
                        dc.DrawBubble(t.GetText(), loc.OffSetBy(2, 2 + 20 * index++), BubbleBack, PaintMain);
            }
        }
    }

    private void RenderFunction(SKCanvas dc, SKRectI rect, ImplicitFunction impFunc)
    {
        if (!impFunc.IsCorrect)
            return;
        if (impFunc.IsDeleted)
            return;
        var RectToCalc = new ConcurrentBag<SKRectI> { rect };
        var Points = new ConcurrentBag<SKPoint>();
        var Rects = new ConcurrentBag<SKRectI>();
        var paint = new SKPaint { Color = new SKColor(impFunc.Color).WithAlpha(impFunc.Opacity) };
        var func = impFunc.Function.Function;
        do
        {
            var rs = RectToCalc.ToArray();
            RectToCalc.Clear();
            Action<int> atn = idx => RenderRectIntervalSet(rs[idx], RectToCalc, func, false, Points, Rects);
            for (var i = 0; i < rs.Length; i += 100)
            {
                //总会有莫名的问题 
                //Parallel.For(i, Min(i + 100, rs.Length), atn);
                for (var j = i; j < Min(i + 100, rs.Length); j++) atn(j);
                foreach (var rectToDraw in Rects) dc.DrawRect(rectToDraw, paint);
                Rects.Clear();
                dc.DrawPoints(SKPointMode.Points, Points.ToArray(), paint);
                Points.Clear();
            }
        } while (RectToCalc.Count != 0);

        dc.Flush();
    }

    private void RenderRectIntervalSet(SKRectI r, ConcurrentBag<SKRectI> RectToCalc, IntervalHandler<IntervalSet> func,
        bool checkpixel, ConcurrentBag<SKPoint> Points, ConcurrentBag<SKRectI> Rects)
    {
        if (r.Height == 0 || r.Width == 0)
            return;
        int xtimes = 2, ytimes = 2;
        if (r.Width > r.Height)
            ytimes = 1;
        else if (r.Width < r.Height)
            xtimes = 1;
        var dx = (int)Ceiling((double)r.Width / xtimes);
        var dy = (int)Ceiling((double)r.Height / ytimes);
        var isPixel = dx == 1 && dy == 1;
        for (var i = r.Left; i < r.Right; i += dx)
        {
            var di = i;
            var xmin = PixelToMathX(i);
            var xmax = PixelToMathX(i + dx);
            var xi = IntervalSet.Create(xmin, xmax, Def.TT);
            for (var j = r.Top; j < r.Bottom; j += dy)
            {
                var dj = j;
                var ymin = PixelToMathY(j);
                var ymax = PixelToMathY(j + dy);
                var yi = IntervalSet.Create(ymin, ymax, Def.TT);
                var result = func(xi, yi);
                if (result == Def.TT)
                {
                    if (isPixel)
                        Points.Add(new SKPoint(di, dj));
                    else
                        Rects.Add(new SKRectI(di, dj, di + dx, dj + dy));
                }
                else if (result == Def.FT)
                {
                    if (isPixel)
                        Points.Add(new SKPoint(di, dj));
                    else
                        RectToCalc.Add(new SKRectI(i, j, Min(i + dx, r.Right),
                            Min(j + dy, r.Bottom)));
                }
            }
        }
    }

    #endregion

    #region ShapeAction

    /// <summary>
    ///     在指定位置放置点
    /// </summary>
    /// <param name="Location"></param>
    /// <returns></returns>
    public GeoPoint? PutPoint(AvaPoint Location)
    {
        var getter = GetNewPointGetterFromLocation(Location);
        if (getter is null)
            return null;
        var p = new GeoPoint(getter);
        AddShape(p);
        return p;
    }

    /// <summary>
    ///     获取指定位置的点获取器
    /// </summary>
    /// <param name="Location"></param>
    /// <returns></returns>
    protected PointGetter? GetNewPointGetterFromLocation(AvaPoint Location)
    {
        var disp = (Owner as DisplayControl)!;
        var mathcursor = PixelToMath(Location);
        var shapes = new List<(double, GeoShape)>();
        foreach (var geoshape in Shapes.GetShapes<GeometryShape>())
            if (geoshape is GeoLine || geoshape is Circle)
            {
                var dist = ((geoshape.DistanceTo(mathcursor) - mathcursor) * disp.UnitLength).GetLength();
                if (dist < 5) shapes.Add((dist, geoshape));
            }

        var ss = shapes.OrderBy(key => key.Item1).ToArray();
        if (ss.Length == 0) return new PointGetter_FromLocation(PixelToMath(Location));

        if (ss.Length == 1)
        {
            var shape = ss[0].Item2;
            if (shape is GeoCircle)
                return new PointGetter_OnCircle((Circle)shape, PixelToMath(Location));
            if (shape is GeoLine)
                return new PointGetter_OnLine((GeoLine)shape, PixelToMath(Location));
            return new PointGetter_FromLocation(PixelToMath(Location));
        }

        var s1 = ss[0].Item2;
        var s2 = ss[1].Item2;
        if (s1 is GeoLine && s2 is GeoLine) return new PointGetter_FromTwoLine(s1 as GeoLine, s2 as GeoLine);

        if (s1 is GeoLine l1 && s2 is Circle c1) (s1, s2) = (s2, s1);

        if (s1 is Circle c2 && s2 is GeoLine l2)
        {
            Vec v1, v2;
            (v1, v2) = IntersectionMath.FromLineAndCircle(l2.Current, c2.InnerCircle);
            return new PointGetter_FromLineAndCircle(l2, c2,
                (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength());
        }

        if (s1 is Circle c3 && s2 is Circle c4)
        {
            Vec v1, v2;
            (v1, v2) = IntersectionMath.FromTwoCircle(c3.InnerCircle, c4.InnerCircle);
            return new PointGetter_FromTwoCircle((Circle)s1, (Circle)s2,
                (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength());
        }

        return null;
    }

    /// <summary>
    ///     点获取器的类型
    /// </summary>
    protected enum PGType
    {
        None,
        Location,
        OnLine,
        OnCircle,
        Fixed
    }

    /// <summary>
    ///     当xy两个方向的线距离均小于5时 会吸附到交点上
    /// </summary>
    protected AvaPoint FindNearestPointOnTwoAxisLine(AvaPoint location)
    {
        double nearestX = double.NaN, nearestY = double.NaN;
        var distance = double.PositiveInfinity;
        var car = (Owner as DisplayControl)!;
        foreach (var tuple in car.AxisX)
        {
            var dist = Abs(location.X - tuple.Item1);
            if (dist < distance && dist < 5)
            {
                distance = dist;
                nearestX = tuple.Item1;
            }
        }

        distance = double.PositiveInfinity;
        foreach (var tuple in car.AxisY)
        {
            var dist = Abs(location.Y - tuple.Item1);
            if (dist < distance && dist < 5)
            {
                distance = dist;
                nearestY = tuple.Item1;
            }
        }

        Vec ret;
        ret.X = double.IsNaN(nearestX) ? location.X : nearestX;
        ret.Y = double.IsNaN(nearestY) ? location.Y : nearestY;
        return ret.ToAvaPoint();
    }

    /// <summary>
    ///     当于xy两个方向的线距离均小于5时 会吸附到更近的点
    /// </summary>
    protected AvaPoint FindNearestPointOnAxisLine(AvaPoint location)
    {
        var nearest = location;
        var distance = double.PositiveInfinity;
        var car = (Owner as DisplayControl)!;
        foreach (var tuple in car.AxisX)
        {
            var dist = Abs(location.X - tuple.Item1);
            if (dist < distance && dist < 5)
            {
                distance = dist;
                nearest = new AvaPoint(tuple.Item1, location.Y);
            }
        }

        foreach (var tuple in car.AxisY)
        {
            var dist = Abs(location.Y - tuple.Item1);
            if (dist < distance && dist < 5)
            {
                distance = dist;
                nearest = new AvaPoint(location.X, tuple.Item1);
            }
        }

        return nearest;
    }

    /// <summary>
    ///     试着从指定位置获取图形
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="Location"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public bool TryGetShape<T>(AvaPoint Location, out T shape) where T : GeometryShape
    {
        var disp = (Owner as DisplayControl)!;
        var v = Owner!.PixelToMath(Location);
        var distance = double.PositiveInfinity;
        shape = null;
        foreach (var s in Shapes)
        {
            if (!s.Visible)
                continue;
            if (s is T tar)
            {
                var dis = ((tar.DistanceTo(v) - v) * disp.UnitLength).GetLength();
                if (dis < PointerTouchRange && dis < distance)
                {
                    distance = dis;
                    shape = tar;
                }
            }
        }

        return shape != null;
    }

    #endregion

    #region Do

    /// <summary>
    ///     添加“添加图形”操作到CmdManager
    /// </summary>
    /// <param name="shape"></param>
    private void DoShapeAdd(GeoShape shape)
    {
        CmdManager.Do(
            shape,
            o =>
            {
                shape.IsDeleted = false;
                ShapeItemsControl?.InvalidateArrange();
            },
            o =>
            {
                shape.IsDeleted = true;
                ShapeItemsControl?.InvalidateArrange();
            },
            o =>
            {
                Shapes.Remove(shape);
                if (o is ImplicitFunction im) Layers.Remove(im.RenderTarget);
            }, true
        );
    }

    /// <summary>
    ///     添加“删除图形”操作到CmdManager
    /// </summary>
    /// <param name="shape"></param>
    private void DoShapeDelete(GeoShape shape)
    {
        if (shape is GeometryShape geo)
            DoGeoShapesDelete([geo]);
        else
            CmdManager.Do(
                shape,
                o =>
                {
                    shape.IsDeleted = true;
                    ShapeItemsControl.InvalidateArrange();
                },
                o =>
                {
                    shape.IsDeleted = false;
                    ShapeItemsControl.InvalidateArrange();
                },
                o =>
                {
                    Shapes.Remove(shape);
                    if (shape is ImplicitFunction impf) Layers.Remove(impf.RenderTarget);
                    shape.Dispose();
                }, true
            );
    }

    private void DoFuncTextChange(TextBox tb, string newtext, string oldtext)
    {
        CmdManager.Do<object?>(null, o => { tb.Text = newtext; }, o => { tb.Text = oldtext; }, o => { });
    }

    private void DoGeoShapesDelete(IEnumerable<GeometryShape> shapes)
    {
        var ss = shapes.Select(s => ShapeList.GetAllChildren(s)).SelectMany(o => o).Distinct().ToArray();
        CmdManager.Do(
            ss,
            o =>
            {
                foreach (var sh in ss)
                {
                    sh.IsDeleted = true;
                    sh.Selected = false;
                }

                ShapeItemsControl.InvalidateArrange();
            },
            o =>
            {
                foreach (var sh in ss)
                {
                    sh.IsDeleted = false;
                    sh.Selected = false;
                }

                ShapeItemsControl.InvalidateArrange();
            },
            o =>
            {
                foreach (var geometryShape in ss)
                {
                    Shapes.Remove(geometryShape);
                    geometryShape.Dispose();
                }
            }, true
        );
    }

    private void DoPointMove(GeoPoint point, Vector2<string> previous, Vector2<string> next)
    {
        if (point.PointGetter is PointGetter_Movable pg)
            CmdManager.Do(pg, o =>
            {
                pg.SetStringPoint(next);
                point.RefreshValues();
            }, o =>
            {
                pg.SetStringPoint(previous);
                point.RefreshValues();
            }, o => { });
    }

    private void DoPointGetterChange(GeoPoint point, PointGetter previous, PointGetter next)
    {
        CmdManager.Do<object?>(null, o =>
        {
            previous.UnAttach(point);
            point.PointGetter = next;
            next.Attach(point);
            point.RefreshValues();
        }, o =>
        {
            next.UnAttach(point);
            point.PointGetter = previous;
            previous.Attach(point);
            point.RefreshValues();
        }, o => { }, true);
    }

    #endregion

    #region ControlAction

    /// <summary>
    ///     监听数字输入框的变化 设置是否出错 并将操作添加入CmdManager
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Number_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox tb)
            if (e.Property == TextBox.TextProperty)
            {
                var n = tb.Tag as ExpNumber;
                if (n is not null && tb.IsFocused)
                {
                    if (n.IsError)
                    {
                        DataValidationErrors.SetError(tb, new Exception());
                    }
                    else
                    {
                        (n.Owner as GeometryShape)?.RefreshValues();
                        DataValidationErrors.ClearErrors(tb);
                    }

                    CmdManager.Do(
                        new TextChangedCommand((string?)e.OldValue ?? "", (string?)e.NewValue ?? "", n, tb));
                }
            }
    }

    /// <summary>
    ///     拦截操作的通用方法
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EventHandledTrue(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    /// <summary>
    ///     删除按钮
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
            if (btn.Tag is GeometryShape shape)
                DoGeoShapesDelete([shape]);
            else if (btn.Tag is GeoShape s) DoShapeDelete(s);

        e.Handled = true;
    }

    /// <summary>
    ///     绑定拦截操作
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NumberTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) tb.AddHandler(KeyDownEvent, TunnelTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    ///     使在TextBox中可以通过左右键来移动到同级TextBox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox box)
        {
            var parent = box.Parent;
            if (e.KeyModifiers != KeyModifiers.None)
            {
                e.Handled = true;
                return;
            }

            if (parent != null)
            {
                if (e.Key == Key.Left)
                {
                    var ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    var index = ls.IndexOf(box);
                    if (index > 0)
                        ls[index - 1].Focus();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    var ls = new List<TextBox>();
                    foreach (var i in parent.GetLogicalChildren())
                        if (i is TextBox tb)
                            ls.Add(tb);
                    var index = ls.IndexOf(box);
                    if (index < ls.Count - 1)
                        ls[index + 1].Focus();
                    e.Handled = true;
                }
                else if (e.Key >= Key.D0 && e.Key <= Key.D9)
                {
                }
                else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                {
                }
                else if (e.Key == Key.OemMinus || e.Key == Key.OemPeriod || e.Key == Key.Subtract ||
                         e.Key == Key.Decimal || e.Key == Key.Add || e.Key == Key.Divide || e.Key == Key.Multiply ||
                         e.Key == Key.OemPlus || e.Key == Key.OemQuestion)
                {
                }
                else if (e.Key >= Key.A && e.Key <= Key.Z)
                {
                }
                else
                {
                    e.Handled = true;
                }
            }
        }
    }

    /// <summary>
    ///     拦截除了部分操作以外的键盘快捷键输入
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TunnelTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
        {
            if (e.Key == Key.A)
            {
            }
            else if (e.Key == Key.C)
            {
            }
            else if (e.Key == Key.V)
            {
            }
            else
            {
                e.Handled = true;
            }
        }
        else if (e.KeyModifiers != KeyModifiers.None)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    ///     几何操作RadioButton被选择
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void RadioButtonChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
            if (rb.IsChecked == true && rb.Tag is ActionData ad)
            {
                SetAction(ad);
                Static.Info(new TextBlock { Text = CurrentAction.Description.Data }, Static.InfoType.Information);
            }
    }

    #endregion

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        foreach (var control in this.GetTemplateChildren().OfType<TextBox>())
        {
            control.Styles.Add(Static.FluentTheme);
        }
    }
}