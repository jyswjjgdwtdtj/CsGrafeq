#define TEST_MEMORY_LEAK
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.ViewModels;
using SkiaSharp;
using static CsGrafeqApplication.Controls.SkiaEx;
using static CsGrafeq.Shapes.GeometryMath;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using AvaSize = Avalonia.Size;
using GeoHalf = CsGrafeq.Shapes.Half;
using static CsGrafeqApplication.AvaloniaMath;
using static CsGrafeqApplication.Properties.Resources;
using static CsGrafeqApplication.Properties.Resources;

namespace CsGrafeqApplication.Addons.GeometryPad;

public partial class GeometryPad : Addon
{
    private static readonly double PointerTouchRange = OS.GetOSType() == OSType.Android ? 15 : 5;
    private readonly List<ActionData> Actions = new();
    private Func<Vec, AvaPoint> MathToPixel;
    protected Func<Vec, SKPoint> MathToPixelSK;
    private GeoPoint? MovingPoint;
    private Vector2<string> MovingPointEndPos;
    private Vector2<string> MovingPointStartPos;
    protected Func<AvaPoint, Vec> PixelToMath;
    protected Func<SKPoint, Vec> PixelToMathSK;
    protected Func<double, double> PixelToMathX, PixelToMathY, MathToPixelX, MathToPixelY;
    private AvaPoint PointerMovedPos;
    private Vec PointerPressedMovingPointPos;
    private AvaPoint PointerPressedPos;
    private PointerPointProperties PointerProperties;
    private AvaPoint PointerReleasedPos;
    private GeometryPadViewModel VM;

    public GeometryPad()
    {
        Setting = new GeometryPadSetting(this);
        InputMethod.SetIsInputMethodEnabled(this, false);
        DataContext=VM = new GeometryPadViewModel();
        Shapes = VM!.Shapes;
        foreach (var i in VM!.Actions)
        foreach (var j in i)
            Actions!.Add(j);
        GeoPadAction = Actions.FirstOrDefault();
        Shapes.CollectionChanged += (s, e) => { Owner?.Invalidate(this); };
        Shapes.OnShapeChanged += () => { Owner?.Invalidate(this); };
#if DEBUG
        var p1 = AddShape(new GeoPoint(new PointGetter_FromLocation((0.5, 0.5))));
        var p2 = AddShape(new GeoPoint(new PointGetter_FromLocation((1.5, 1.5))));
        var s1 = AddShape(new Straight(new LineGetter_Connected(p1, p2)));
        var p3 = AddShape(new GeoPoint(new PointGetter_OnLine(s1, (0, 0))));
        var c1 = AddShape(new Circle(new CircleGetter_FromCenterAndRadius(p1)));
#endif
        InitializeComponent();
    }

    internal ActionData GeoPadAction { get; private set; }

    public ShapeList Shapes { get; init; }

    public override Displayer? Owner
    {
        get => base.Owner;
        set
        {
            if (!(value is CartesianDisplayer))
                throw new Exception();
            base.Owner = value;
            PixelToMathX = Owner.PixelToMathX;
            PixelToMathY = Owner.PixelToMathY;
            PixelToMath = Owner.PixelToMath;
            PixelToMathSK = Owner.PixelToMath;
            MathToPixelX = Owner.MathToPixelX;
            MathToPixelY = Owner.MathToPixelY;
            MathToPixel = Owner.MathToPixel;
            MathToPixelSK = Owner.MathToPixelSK;
#if DEBUG
#endif
        }
    }

    public override string Name => "GeometryPad";

    internal void SetAction(ActionData ad)
    {
        if (GeoPadAction==ad)
            return;
        GeoPadAction = ad;
        Shapes.ClearSelected();
    }

    #region KeyAction

    public override void Delete()
    {
        List<GeometryShape> todelete = new();
        foreach (var shape in Shapes.GetSelectedShapes<GeometryShape>().ToArray())
            if (shape.Selected)
            {
                todelete.Add(shape);
            }

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
    protected override bool KeyDown(KeyEventArgs e)
    {
        Owner.Suspend();
        var res = DoNext;
        switch (e.Key)
        {
            case Key.Tab:
            {
                foreach (var shape in Shapes.GetSelectedShapes<GeometryShape>().ToArray())
                    if (shape.Selected)
                    {
                        res &= Intercept;
                        shape.Selected = false;
                        foreach (var subshape in shape.SubShapes) subshape.Selected = true;
                    }
            }
                break;
        }

        if (res == Intercept) Owner.Invalidate();
        Owner.Resume();
        return res;
    }

    #endregion

    public T? AddShape<T>(T shape) where T : Shape
    {
        if (shape.IsDeleted)
            return null;
        Shapes.Add(shape);
        DoShapeAdd(shape);
        return shape;
    }

    private class TextChangedCommand : CommandManager.Command
    {
        public readonly string PreviousText;

        public TextChangedCommand(string previous, string current, ExpNumber target, TextBox tb)
            : base(null, _ => { }, _ => { }, _ => { })
        {
            PreviousText = previous;
            CurrentText = current;
            Number = target;
            Do = _ =>
            {
                target.ValueStr = current;
                (tb.Tag as GeoPoint)?.RefreshValues();
                if (target.IsError)
                    DataValidationErrors.SetError(tb, new Exception());
                else
                    DataValidationErrors.ClearErrors(tb);
            };
            UnDo = _ =>
            {
                target.ValueStr = previous;
                (tb.Tag as GeoPoint)?.RefreshValues();
                if (target.IsError)
                    DataValidationErrors.SetError(tb, new Exception());
                else
                    DataValidationErrors.ClearErrors(tb);
            };
            Clear = _ => { };
        }

        public string CurrentText { get; init; }
        public ExpNumber Number { get; init; }
    }


    #region PointerAction

    private bool IsMovingPointMovable;

    protected override bool PointerPressed(AddonPointerEventArgs e)
    {
        PointerProperties = e.Properties;
        PointerPressedPos = e.Location;
        if (GeoPadAction.Name.English == "Select")
        {
            Shapes.ClearSelected();
        }
        else
        {
            MovingPoint = GetShape<GeoPoint>(e.Location);
            if (MovingPoint != null)
            {
                PointerPressedMovingPointPos = MovingPoint.Location;
                if (MovingPoint.PointGetter is PointGetter_Movable pgm && MovingPoint.IsUserEnabled)
                {
                    IsMovingPointMovable = true;
                    MovingPointStartPos = new Vector2<string>(pgm.PointX.ValueStr, pgm.PointY.ValueStr);
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

    protected override bool PointerMoved(AddonPointerEventArgs e)
    {
        PointerProperties = e.Properties;
        Owner.Suspend();
        PointerMovedPos = e.Location;
        if (GeoPadAction.Name.English == "Select" && e.Properties.IsLeftButtonPressed)
        {
            Owner.Resume(false);
            Owner.Invalidate(this);
            return Intercept;
        }

        if (MovingPoint == null)
        {
            Owner.Resume(false);
            return DoNext;
        }

        if (e.Properties.IsLeftButtonPressed)
        {
            if (OS.GetOSType() == OSType.Android && (PointerMovedPos - PointerPressedPos).GetLength() < 10)
            {
            }
            else if (PointerMovedPos == PointerPressedPos)
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
                        Console.WriteLine(123);
                        DoPointGetterChange(MovingPoint, previous, getter);
                    }
                }
            }*/
            if (MovingPoint.PointGetter is PointGetter_Movable pg)
            {
                pg.SetPoint(Owner.PixelToMath(PointerMovedPos));
                if (MovingPoint.PointGetter is PointGetter_FromLocation)
                {
                    pg.SetPoint(
                        Owner.PixelToMath(FindNearestPointOnTwoAxisLine(PointerMovedPos)));
                }
                else if (MovingPoint.PointGetter is PointGetter_OnLine)
                {
                    var newp = FindNearestPointOnTwoAxisLine(MathToPixel(pg.GetPoint()));
                    if (newp.X != PointerMovedPos.X)
                        pg.PointX.SetNumber(PixelToMathX(newp.X));
                    else if (newp.Y != PointerMovedPos.Y)
                        pg.PointY.SetNumber(PixelToMathY(newp.Y));
                    else
                        pg?.SetPoint(Owner.PixelToMath(newp));
                }

                MovingPoint.RefreshValues();
            }

            Owner.Resume();
            return Intercept;
        }

        Owner.Resume(false);
        return DoNext;
    }

    protected override bool PointerReleased(AddonPointerEventArgs e)
    {
        PointerProperties = e.Properties;
        Owner.Suspend();
        PointerReleasedPos = e.Location;
        var disp = (Owner as DisplayControl)!;
        if (GeoPadAction.Name.English == "Select")
        {
            var rect = RegulateRectangle(new AvaRect(PointerPressedPos,
                new AvaSize(PointerReleasedPos.X - PointerPressedPos.X, PointerReleasedPos.Y - PointerPressedPos.Y)));
            var mathrectloc = Owner.PixelToMath(new AvaPoint(rect.Left, rect.Top + rect.Height));
            var mathrectsize = new Vec(rect.Width, rect.Height) / disp.UnitLength;
            foreach (var s in Shapes)
                switch (s)
                {
                    case GeoPoint p:
                    {
                        var v = p.Location - mathrectloc;
                        if (RangeIn(0, mathrectsize.X, v.X) && RangeIn(0, mathrectsize.Y, v.Y))
                            p.Selected = true;
                    }
                        break;
                    case Line l:
                    {
                        var v1 = l.Current.Point1;
                        var v2 = l.Current.Point2;
                        var LT = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Bottom));
                        var RT = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Bottom));
                        var RB = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Top));
                        var LB = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Top));
                        var vs = GetValidVec(
                            GetIntersectionLSAndSL(LT, RT, v1, v2),
                            GetIntersectionLSAndSL(RT, RB, v1, v2),
                            GetIntersectionLSAndSL(RB, LB, v1, v2),
                            GetIntersectionLSAndSL(LB, LT, v1, v2)
                        );
                        vs.Item1 = l.CheckIsValid(vs.Item1) ? vs.Item1 : Vec.Invalid;
                        vs.Item2 = l.CheckIsValid(vs.Item2) ? vs.Item2 : Vec.Invalid;

                        if (!(vs.Item1.IsInvalid() && vs.Item2.IsInvalid()))
                            l.Selected = true;
                        else if (RangeIn(mathrectloc.X, mathrectloc.X + mathrectsize.X,
                                     ((l.Current.Point1 + l.Current.Point2) / 2).X) && RangeIn(mathrectloc.Y,
                                     mathrectloc.Y + mathrectsize.Y, ((l.Current.Point1 + l.Current.Point2) / 2).Y))
                            l.Selected = true;
                    }
                        break;
                    case Circle c:
                    {
                        var o = mathrectloc + mathrectsize / 2;
                        var cc = c.InnerCircle.Center - o;
                        o = mathrectsize / 2;
                        cc.X = Abs(cc.X);
                        cc.Y = Abs(cc.Y);
                        var bc = cc - o;
                        bc.X = Max(bc.X, 0);
                        bc.Y = Max(bc.Y, 0);
                        if (bc.GetLength() <= c.InnerCircle.Radius)
                            c.Selected = true;
                    }
                        break;
                    case Polygon p:
                    {
                        foreach (var point in p.Locations)
                        {
                            var v = point - mathrectloc;
                            if (RangeIn(0, mathrectsize.X, v.X) && RangeIn(0, mathrectsize.Y, v.Y)) p.Selected = true;

                            break;
                        }
                    }
                        break;
                    case Angle a:
                    {
                        var v = a.AngleData.AnglePoint - mathrectloc;
                        if (RangeIn(0, mathrectsize.X, v.X) && RangeIn(0, mathrectsize.Y, v.Y)) a.Selected = true;
                    }
                        break;
                }

            Owner.Resume();
            PointerPressedPos = new AvaPoint(double.NaN, double.NaN);
            return Intercept;
        }

        if (MovingPoint == null)
        {
            Owner.Resume();
            return DoNext;
        }

        if (PointerReleasedPos != PointerPressedPos)
            Shapes.ClearSelected();
        if (IsMovingPointMovable)
        {
            var pgm = (PointGetter_Movable)MovingPoint.PointGetter;
            DoPointMove(MovingPoint, MovingPointStartPos,
                new Vector2<string>(pgm.PointX.ValueStr, pgm.PointY.ValueStr));
        }

        MovingPoint = null;
        Owner.Resume();
        return Intercept;
    }

    protected override bool PointerTapped(AddonPointerEventArgsBase e)
    {
        Owner.Suspend();
        using (var ec=new ExitController(() => Owner.Resume()))
        {
            if (GeoPadAction.Name.English == "Put")
            {
                Shapes.ClearSelected();
                PutPoint(e.Location).Selected = true;
                return Intercept;
            }

            var first = Shapes.GetSelectedShapes<GeoPoint>().FirstOrDefault();
            var selectfirst = false;
            if (GeoPadAction.Args.Contains(ShapeArg.Point) &&
                TryGetShape<GeoPoint>(e.Location, out var point))
            {
                if (first == point)
                    selectfirst = true;
                else
                    point.Selected = !point.Selected;
            }
            else if (GeoPadAction.Args.Contains(ShapeArg.Line) &&
                     TryGetShape<GeoLine>(e.Location, out var line))
            {
                line.Selected = !line.Selected;
            }
            else if (GeoPadAction.Args.Contains(ShapeArg.Circle) &&
                     TryGetShape<GeoCircle>(e.Location, out var circle))
            {
                circle.Selected = !circle.Selected;
            }
            else if (GeoPadAction.Args.Contains(ShapeArg.Polygon) &&
                     TryGetShape<GeoPolygon>(e.Location, out var polygon))
            {
                polygon.Selected = !polygon.Selected;
            }
            else if (GeoPadAction.Args.Contains(ShapeArg.Point) && GeoPadAction.Name.English != "Move" &&
                     GeoPadAction.Name.English != "Select")
            {
                PutPoint(e.Location).Selected = true;
            }

            if (GeoPadAction.Name.English != "Move" && GeoPadAction.Name.English != "Select")
            {
                if (!GeoPadAction.Args.Contains(ShapeArg.Point))
                    Shapes.ClearSelected<GeoPoint>();
                if (!GeoPadAction.Args.Contains(ShapeArg.Line))
                    Shapes.ClearSelected<GeoLine>();
                if (!GeoPadAction.Args.Contains(ShapeArg.Circle))
                    Shapes.ClearSelected<GeoCircle>();
                if (!GeoPadAction.Args.Contains(ShapeArg.Polygon))
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
            var needplen = GeoPadAction.Args.Count(i => i == ShapeArg.Point);
            var needllen = GeoPadAction.Args.Count(i => i == ShapeArg.Line);
            var needclen = GeoPadAction.Args.Count(i => i == ShapeArg.Circle);
            var needpolen = GeoPadAction.Args.Count(i => i == ShapeArg.Circle);
            if (GeoPadAction.IsMultiPoint)
            {
                if (plen > 2)
                    if (selectfirst)
                    {
                        var shape = GeometryActions.CreateShape(GeoPadAction.Self, (Getter)GeoPadAction.GetterConstructor.Invoke(SPoints.ToArray()));
                        AddShape(shape);
                        Shapes.ClearSelected();
                    }
                    else if (plen <= 2 && selectfirst)
                    {
                        Shapes.ClearSelected<GeoPoint>();
                    }
                return Intercept;
            }
            if (GeoPadAction.Name.English == "Select" || GeoPadAction.Name.English == "Move")
            {
                return Intercept;
            }
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
                        CsGrafeq.Extension.Throw("");
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
                var shape=GeometryActions.CreateShape(GeoPadAction.Self,(Getter)GeoPadAction.GetterConstructor.Invoke(SAll.Select(o => (object)o).ToArray()));
                AddShape(shape);
                Shapes.ClearSelected();
            }
            return Intercept;
        }
    }

    #endregion

    #region RenderAction

    protected override void Render(SKCanvas dc, SKRect rect)
    {
        rect.Intersect(Owner.ValidRect.ToSKRect());
        dc.Save();
        dc.ClipRect(rect);
        //RenderFunction(dc, new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom));
        RenderShapes(dc, rect, Shapes.GetShapes<GeometryShape>());
        if (GeoPadAction.Name.English == "Select" && PointerProperties.IsLeftButtonPressed)
        {
            var selrect = RegulateRectangle(new AvaRect(PointerPressedPos,
                new AvaSize(PointerMovedPos.X - PointerPressedPos.X, PointerMovedPos.Y - PointerPressedPos.Y)));
            dc.DrawRect(selrect.ToSKRect(), FilledTpMedian);
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
        var cd = Owner as CartesianDisplayer;
        using (SKPaint TPFilledPaint = new() { IsAntialias = true },
               FilledPaint = new() { IsAntialias = true },
               StrokePaint = new() { IsStroke = true, IsAntialias = true },
               StrokeMain = new() { Color = cd.AxisPaintMain.Color, IsStroke = true, IsAntialias = true },
               PaintMain = new() { Color = cd.AxisPaintMain.Color, IsAntialias = true },
               StrokePaintMain = new() { Color = cd.AxisPaintMain.Color, IsAntialias = true, IsStroke = true },
               BubbleBack = new() { Color = cd.AxisPaint1.Color.WithAlpha(90), IsAntialias = true })
        {
            /*switch (OS.GetOSType())
            {
                case OSType.Android:
                {
                    StrokePaint.StrokeWidth = 2;
                    StrokeMain.StrokeWidth = 2;
                    StrokePaintMain.StrokeWidth = 2;
                }
                    break;
            }*/

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
                            GetIntersectionLSAndSL(LT, RT, v1, v2),
                            GetIntersectionLSAndSL(RT, RB, v1, v2),
                            GetIntersectionLSAndSL(RB, LB, v1, v2),
                            GetIntersectionLSAndSL(LB, LT, v1, v2)
                        );
                        if (s.Selected)
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), StrokePaint);
                        else
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), StrokePaintMain);

                        dc.DrawBubble($"{Properties.Resources.StraightText}:{s.Name}", MathToPixelSK((s.Current.Point1 + s.Current.Point2) / 2),
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

                        dc.DrawBubble($"{SegmentText}:{s.Name}", MathToPixelSK((s.Current.Point1 + s.Current.Point2) / 2),
                            BubbleBack, PaintMain);
                    }
                        break;
                    case GeoHalf h:
                    {
                        var v1 = h.Current.Point1;
                        var v2 = h.Current.Point2;
                        var vs = GetValidVec(
                            GetIntersectionLSAndSL(LT, RT, v1, v2),
                            GetIntersectionLSAndSL(RT, RB, v1, v2),
                            GetIntersectionLSAndSL(RB, LB, v1, v2),
                            GetIntersectionLSAndSL(LB, LT, v1, v2)
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

                        dc.DrawBubble($"{HalfLineText}:{h.Name}", MathToPixelSK((h.Current.Point1 + h.Current.Point2) / 2),
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

                        dc.DrawBubble($"{PolygonText}:{polygon.Name}",
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
                        dc.DrawBubble($"{CircleText}:{circle.Name}",
                            MathToPixelSK(circle.InnerCircle.Center + new Vec(-r2, r2)), BubbleBack, PaintMain);
                    }
                        break;
                    case Angle ang:
                    {
                        var angle = ang.AngleData;
                        var pf = MathToPixelSK(angle.AnglePoint);
                        var arg1 =
                            PosMod(MathToPixel(angle.Point1).Sub(MathToPixel(angle.AnglePoint)).Arg() / PI * 180,
                                360);
                        var arg2 =
                            PosMod(MathToPixel(angle.Point2).Sub(MathToPixel(angle.AnglePoint)).Arg() / PI * 180,
                                360);
                        var aa = angle.Angle;
                        var a = arg2 - arg1;
                        a = PosMod(a, 360);
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
                StrokePaint.Color = new SKColor(p.Color).WithAlpha(255);
                var index = 0;
                var loc = MathToPixelSK(p.Location);
                dc.DrawBubble($"{PointText}:" + p.Name, loc.OffSetBy(2, 2 + 20 * index++), BubbleBack, PaintMain);
                if (p == MovingPoint)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledMedian);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokeMedian);
                    dc.DrawBubble(
                        $"({System.Math.Round(MovingPoint.Location.X,8)},{System.Math.Round(MovingPoint.Location.Y,8)}) {(MovingPoint.PointGetter is PointGetter_Movable ? "" : CantBeMovedText)}",
                        loc.OffSetBy(2, 2 + 20 * index++), BubbleBack, PaintMain);
                }
                else if (p.Selected)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledPaint);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokePaint);
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

    /*private void RenderFunction(SKCanvas dc, SKRectI rect,IEnumerable<ImplicitFunction> funcs)
    {
        foreach (var impfunc in funcs)
        {
            var RectToCalc = new ConcurrentBag<SKRectI> { rect };
            var RectToRenderRect = new ConcurrentBag<SKRectI>();
            var RectToRenderPoint = new ConcurrentBag<SKPoint>();
            var paint = new SKPaint { Color = new SKColor(impfunc.Color) };
            var func = impfunc.Function;
            do
            {
                var paint1 = new SKPaint { Color = ColorExtension.GetRandomColor() };
                var rs = RectToCalc.ToArray();
                RectToCalc.Clear();
                Action<int> atn = idx => RenderRectIntervalSet(dc, rs[idx], RectToCalc, RectToRenderRect,
                    RectToRenderPoint, paint1, func, false);
                for (var i = 0; i < rs.Length; i += 100)
                {
                    Parallel.For(i, Min(i + 100, rs.Length), atn);
                    foreach (var r in RectToRenderRect) dc.DrawRect(r, paint);
                    foreach (var point in RectToRenderPoint) dc.DrawRect(CreateSKRectWH(point.X, point.Y, 1, 1), paint);
                    //dc.DrawPoint(point, paint);
                    RectToRenderRect.Clear();
                    RectToRenderPoint.Clear();
                }
            } while (RectToCalc.Count != 0);
        }
    }*/

    /*private void RenderRectIntervalSet(SKCanvas dc, SKRectI r, ConcurrentBag<SKRectI> RectToCalc,
        ConcurrentBag<SKRectI> RectToRender, ConcurrentBag<SKPoint> RectToRenderPoint, SKPaint paint,
        IntervalHandler<IntervalSet> func, bool checkpixel)
    {
        //dc.DrawRect(r,paint);
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
            var xi =IntervalSet.Create(xmin, xmax,Def.TT);
            for (var j = r.Top; j < r.Bottom; j += dy)
            {
                var dj = j;
                var ymin = PixelToMathY(j);
                var ymax = PixelToMathY(j + dy);
                var yi = IntervalSet.Create(ymin, ymax,Def.TT);
                Def result = func.Invoke(xi, yi);
                if (result ==Def.TT)
                {
                    if (isPixel)
                        RectToRenderPoint.Add(new SKPoint(di, dj));
                    else
                        RectToRender.Add(new SKRectI(di, dj, di + dx, dj + dy));
                }
                else if (result == Def.FT)
                {
                    if (isPixel)
                        RectToRenderPoint.Add(new SKPoint(di, dj));
                    else
                        RectToCalc.Add(new SKRectI(i, j, Min(i + dx, r.Right),
                            Min(j + dy, r.Bottom)));
                }
            }
        }
    }*/

    #endregion

    #region ShapeAction

    public GeoPoint? PutPoint(AvaPoint Location)
    {
        var getter = GetNewPointGetterFromLocation(Location);
        if (getter is null)
            return null;
        var p = new GeoPoint(getter);
        AddShape(p);
        return p;
    }

    protected PointGetter? GetNewPointGetterFromLocation(AvaPoint Location)
    {
        var disp = (Owner as DisplayControl)!;
        var mathcursor = PixelToMath(Location);
        var shapes = new List<(double, Shape)>();
        foreach (var geoshape in Shapes.GetShapes<GeometryShape>())
            if (geoshape is Line || geoshape is Circle)
            {
                var dist = ((geoshape.NearestOf(mathcursor) - mathcursor) * disp.UnitLength).GetLength();
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
                return new PointGetter_OnLine((Line)shape, PixelToMath(Location));
            return new PointGetter_FromLocation(PixelToMath(Location));
        }

        var s1 = ss[0].Item2;
        var s2 = ss[1].Item2;
        if (s1 is Line && s2 is Line) return new PointGetter_FromTwoLine(s1 as Line, s2 as Line);

        if (s1 is Line l1 && s2 is Circle c1)
        {
            Vec v1, v2;
            (v1, v2) = GetPointFromCircleAndLine(l1.Current.Point1, l1.Current.Point2, c1.InnerCircle.Center,
                c1.InnerCircle.Radius);
            return new PointGetter_FromLineAndCircle(l1, c1,
                (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength());
        }

        if (s1 is Circle c2 && s2 is Line l2)
        {
            Vec v1, v2;
            (v1, v2) = GetPointFromCircleAndLine(l2.Current.Point1, l2.Current.Point2, c2.InnerCircle.Center,
                c2.InnerCircle.Radius);
            return new PointGetter_FromLineAndCircle(l2, c2,
                (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength());
        }

        //暂不支持
        return null;
    }

    protected enum PGType
    {
        None,
        Location,
        OnLine,
        OnCircle,
        Fixed
    }

    protected PGType GetNewPointGetterTypeFromLocation(AvaPoint Location)
    {
        var disp = (Owner as DisplayControl)!;
        var mathcursor = PixelToMath(Location);
        var shapes = new List<(double, Shape)>();
        foreach (var geoshape in Shapes.GetShapes<GeometryShape>())
            if (geoshape is Line || geoshape is Circle)
            {
                var dist = ((geoshape.NearestOf(mathcursor) - mathcursor) * disp.UnitLength).GetLength();
                if (dist < 5) shapes.Add((dist, geoshape));
            }

        var ss = shapes.OrderBy(key => key.Item1).ToArray();
        if (ss.Length == 0) return PGType.Location;

        if (ss.Length == 1)
        {
            var shape = ss[0].Item2;
            if (shape is GeoCircle)
                return PGType.OnCircle;
            if (shape is GeoLine)
                return PGType.OnLine;
            return PGType.Location;
        }

        var s1 = ss[0].Item2;
        var s2 = ss[1].Item2;
        if (s1 is Line && s2 is Line) return PGType.Fixed;

        if (s1 is Line l1 && s2 is Circle c1)
        {
            Vec v1, v2;
            (v1, v2) = GetPointFromCircleAndLine(l1.Current.Point1, l1.Current.Point2, c1.InnerCircle.Center,
                c1.InnerCircle.Radius);
            return PGType.Fixed;
        }

        if (s1 is Circle c2 && s2 is Line l2)
        {
            Vec v1, v2;
            (v1, v2) = GetPointFromCircleAndLine(l2.Current.Point1, l2.Current.Point2, c2.InnerCircle.Center,
                c2.InnerCircle.Radius);
            return PGType.Fixed;
        }

        //暂不支持
        return PGType.None;
    }

    protected PGType GetPGType(PointGetter pg)
    {
        if (pg is PointGetter_FromLocation)
            return PGType.Location;
        if (pg is PointGetter_OnLine)
            return PGType.OnLine;
        //if (pg is PointGetter_OnCircle)
        //    return PGType.OnCircle;
        return PGType.Fixed;
    }

    /// <summary>
    ///     当于xy两个方向的线距离均小于5时 会吸附到交点上
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

    public bool TryGetShape<T>(AvaPoint Location, out T shape) where T : GeometryShape
    {
        shape = GetShape<T>(Location);
        return shape != null;
    }

    public T? GetShape<T>(AvaPoint Location) where T : GeometryShape
    {
        var disp = (Owner as DisplayControl)!;
        var v = Owner!.PixelToMath(Location);
        var distance = double.PositiveInfinity;
        T? target = null;
        foreach (var s in Shapes)
        {
            if (!s.Visible)
                continue;
            if (s is T tar)
            {
                var dis = ((tar.NearestOf(v) - v) * disp.UnitLength).GetLength();
                if (dis < PointerTouchRange && dis < distance)
                {
                    distance = dis;
                    target = tar;
                }
            }
        }

        return target;
    }

    #endregion

    #region Do

    private void DoShapeAdd(Shape shape)
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
            o => { Shapes.Remove(shape); }, true
        );
    }

    private void DoGeoShapesDelete(IEnumerable<GeometryShape> shapes)
    {
        var ss = shapes.Select(s => ShapeList.GetAllChildren(s)).JoinToOne().Distinct().ToArray();
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
            o => { }, true
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
            previous.UnAttach(point.RefreshValues, point);
            point.PointGetter = next;
            next.Attach(point.RefreshValues, point);
            point.RefreshValues();
        }, o =>
        {
            next.UnAttach(point.RefreshValues, point);
            point.PointGetter = previous;
            previous.Attach(point.RefreshValues, point);
            point.RefreshValues();
        }, o => { next = null; }, true);
    }

    #endregion

    #region ControlAction

    private void TextBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
    }

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

    private void TextBoxGetFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox tb)
        {
        }
    }

    private void EventHandledTrue(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
            if (btn.Tag is GeometryShape shape)
                DoGeoShapesDelete([shape]);
    }

    private void NumberTextBox_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) tb.AddHandler(KeyDownEvent, TunnelTextBoxKeyDown, RoutingStrategies.Tunnel);
    }

    public void TextBoxPixelXLostFocus(object? sender, RoutedEventArgs e)
    {
    }

    public void TextBoxPixelYLostFocus(object? sender, RoutedEventArgs e)
    {
    }

    public void NumberLostFocus(object? sender, RoutedEventArgs e)
    {
    }

    public void HandleEvent(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

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

    public void TunnelTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
        {
            if (e.Key == Key.A)
            {
            }
            else
            {
                e.Handled = true;
            }
        }
    }

    public void RadioButtonChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
            if (rb.IsChecked == true)
            {
                SetAction((ActionData)rb.Tag);
                Static.Info(new TextBlock { Text = GeoPadAction.Description.Data });
            }
    }

    #endregion
}