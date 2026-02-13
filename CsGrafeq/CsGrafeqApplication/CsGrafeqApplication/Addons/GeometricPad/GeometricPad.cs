using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using CsGrafeq.I18N;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using CsGrafeqApplication.Controls.Displayers;
using CsGrafeqApplication.Events;
using CsGrafeqApplication.Utilities;
using SkiaSharp;
using static CsGrafeq.Shapes.GeometryMath;
using static CsGrafeqApplication.Core.Utils.PointRectHelper;
using static CsGrafeqApplication.SkiaHelper;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using AvaSize = Avalonia.Size;
using GeoHalf = CsGrafeq.Shapes.Half;
using static CsGrafeqApplication.Core.Utils.StaticSkiaResources;

namespace CsGrafeqApplication.Addons.GeometricPad;

public class GeometricPad : Addon
{
    /// <summary>
    ///     所有几何图形的渲染目标
    /// </summary>
    private readonly Renderable _mainRenderTarget = new();

    public GeometricPad()
    {
        AddonName = MultiLanguageResources.Instance.GeometricPadText;
        var host = new Control();
        object? obj;
        host.Resources.MergedDictionaries.Add(
            new ResourceInclude(new Uri("avares://CsGrafeqApplication/"))
            {
                Source = new Uri("avares://CsGrafeqApplication/Addons/GeometricPad/GeometricResources.axaml")
            });
        host.TryFindResource("GeometricPadViewTemplate", out obj);
        MainTemplate = (IDataTemplate)obj!;
        CurrentAction = GeometricActions.Actions.FirstOrDefault()?.FirstOrDefault()!;
        Layers.Add(_mainRenderTarget);
        _mainRenderTarget.OnRenderCanvas += Renderable_OnRender;
        Shapes.CollectionChanged += (_,_) =>
        {
            _mainRenderTarget.Changed = true;
            Owner?.AskForRender();
        };
        Shapes.OnShapeChanged += _ =>
        {
            _mainRenderTarget.Changed = true;
            Owner?.AskForRender();
        };
#if DEBUG
        var p1 = AddShape(new GeoPoint(new PointGetter_FromLocation((0.5, 0.5))));
        var p2 = AddShape(new GeoPoint(new PointGetter_FromLocation((1.5, 1.5))));
        var s1 = AddShape(new Straight(new LineGetter_Connected(p1!, p2!)));
        var p3 = AddShape(new GeoPoint(new PointGetter_OnLine(s1!, (0, 0))));
        var p4 = AddShape(new GeoPoint(new PointGetter_MiddlePoint(p1!, p2!)));
        var c1 = AddShape(new Circle(new CircleGetter_FromCenterAndRadius(p1!)));
#endif
    }

    internal ShapeList Shapes { get; } = new();

    /// <summary>
    ///     正在被移动的点
    /// </summary>
    private GeoPoint? MovingPoint { get; set; }

    /// <summary>
    ///     点拖动操作的起始点位置 字符串形式
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
        CommandHelper.DoShapeAdd(Shapes, shape);
        return shape;
    }


    #region KeyAction

    public override void Delete()
    {
        List<GeometricShape> todelete = new();
        foreach (var shape in Shapes.GetSelectedShapes<GeometricShape>().ToArray())
            if (shape.Selected)
                todelete.Add(shape);

        if (todelete.Count > 0) CommandHelper.DoGeoShapesDelete(todelete.ToArray());
    }

    public override void SelectAll()
    {
        foreach (var shape in Shapes.OfType<GeometricShape>().ToArray())
            shape.Selected = true;
    }

    public override void DeselectAll()
    {
        foreach (var shape in Shapes.OfType<GeometricShape>().ToArray())
            shape.Selected = false;
    }

    protected override bool OnKeyDown(KeyEventArgs e)
    {
        if (Owner == null) return DoNext;
        var res = DoNext;
        switch (e.Key)
        {
            case Key.Tab:
            {
                foreach (var shape in Shapes.GetSelectedShapes<GeometricShape>().ToArray())
                    if (shape.Selected)
                    {
                        res = Intercept;
                        shape.Selected = false;
                        foreach (var subshape in shape.SubShapes) subshape.Selected = true;
                    }
            }
                break;
        }

        if (res == Intercept) _mainRenderTarget.Changed = true;
        return res;
    }

    #endregion


    #region PointerAction

    /// <summary>
    ///     指示试图被移动的点是否可移动
    /// </summary>
    private bool IsMovingPointMovable { get; set; }

    protected override bool OnPointerPressed(MouseEventArgs e)
    {
        if (Owner == null) return DoNext;
        LastPointerProperties = e.Properties;
        PointerPressedPosition = e.Position;
        if (CurrentAction.Name.English == "Select")
        {
            Shapes.ClearSelected();
        }
        else
        {
            TryGetShape<GeoPoint>(e.Position, out var mp);
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

    protected override void OnPointerMoved(MouseEventArgs e)
    {
        if (Owner == null) return;
        LastPointerProperties = e.Properties;
        PointerMovedPosition = e.Position;
        if (CurrentAction.Name.English == "Select" && e.Properties.IsLeftButtonPressed)
        {
            _mainRenderTarget.Changed = true;
            return;
        }

        if (MovingPoint == null) return;

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

            
            //如点附着在基于这个点的几何图形上 会造成无限递归 使程序崩溃 同时破坏了图形的树的单向结构
            /*
            var previous = MovingPoint.PointGetter; 
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
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
                        pg.SetPoint(Owner.PixelToMath(newp));
                }

                MovingPoint.RefreshValues();
            }

            _mainRenderTarget.Changed = true;
        }
    }

    protected override void OnPointerReleased(MouseEventArgs e)
    {
        if (Owner == null) return;
        LastPointerProperties = e.Properties;
        PointerReleasedPosition = e.Position;
        var disp = (Owner as DisplayControl)!;
        if (CurrentAction.Name.English == "Select")
        {
            var rect = new AvaRect(PointerPressedPosition,
                new AvaSize(PointerReleasedPosition.X - PointerPressedPosition.X,
                    PointerReleasedPosition.Y - PointerPressedPosition.Y)).RegulateRectangle();
            var mathrect = new CgRectangle(Owner.PixelToMath(new AvaPoint(rect.Left, rect.Top + rect.Height)),
                new Vec(rect.Width, rect.Height) / disp.UnitLength);
            foreach (var s in Shapes.GetShapes<GeometricShape>())
                s.Selected = s.IsIntersectedWithRect(mathrect);
            _mainRenderTarget.Changed = true;
            PointerPressedPosition = new AvaPoint(double.NaN, double.NaN);
            return;
        }

        if (MovingPoint == null) return;

        if (PointerReleasedPosition != PointerPressedPosition)
            Shapes.ClearSelected();
        if (IsMovingPointMovable)
        {
            var pgm = (PointGetter_Movable)MovingPoint.PointGetter;
            CommandHelper.DoPointMove(MovingPoint, PointMovementStartPositionStr,
                new Vector2<string>(pgm.PointX.ValueStr, pgm.PointY.ValueStr));
        }

        MovingPoint = null;
    }

    protected override bool OnPointerTapped(MouseEventArgs e)
    {
        if (Owner == null) return DoNext;
        {
            // 放置点
            if (CurrentAction.Name.English=="Place")
            {
                Shapes.ClearSelected();
                PutPoint(e.Position)?.Selected = true;
                return Intercept;
            }

            var first = Shapes.GetSelectedShapes<GeoPoint>().FirstOrDefault();
            var selectfirst = false;
            if (CurrentAction.Args.Contains(ShapeArg.Point) &&
                TryGetShape<GeoPoint>(e.Position, out var point))
            {
                if (first == point)
                    selectfirst = true;
                else
                    point.Selected = !point.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Line) &&
                     TryGetShape<GeoLine>(e.Position, out var line))
            {
                line.Selected = !line.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Circle) &&
                     TryGetShape<GeoCircle>(e.Position, out var circle))
            {
                circle.Selected = !circle.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Polygon) &&
                     TryGetShape<GeoPolygon>(e.Position, out var polygon))
            {
                polygon.Selected = !polygon.Selected;
            }
            else if (CurrentAction.Args.Contains(ShapeArg.Point) && CurrentAction.Name.English != "Move" &&
                     CurrentAction.Name.English != "Select")
            {
                PutPoint(e.Position)?.Selected = true;
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

            var all = Shapes.GetSelectedShapes<GeometricShape>().ToArray();
            var points = Shapes.GetSelectedShapes<GeoPoint>().ToArray();
            var lines = Shapes.GetSelectedShapes<GeoLine>().ToArray();
            var circles = Shapes.GetSelectedShapes<GeoCircle>().ToArray();
            var polygons = Shapes.GetSelectedShapes<GeoPolygon>().ToArray();
            var pointsLength = points.Length;
            var linesLength = lines.Length;
            var circlesLength = circles.Length;
            var polygonsLength = polygons.Length;
            var pointsNeeded = CurrentAction.Args.Count(i => i == ShapeArg.Point);
            var linesNeeded = CurrentAction.Args.Count(i => i == ShapeArg.Line);
            var circleNeeded = CurrentAction.Args.Count(i => i == ShapeArg.Circle);
            var polygonsNeeded = CurrentAction.Args.Count(i => i == ShapeArg.Polygon);
            if (CurrentAction.IsMultiPoint)
            {
                if (pointsLength > 2)
                {
                    if (selectfirst)
                    {
                        var shape = GeometricActions.CreateShape(CurrentAction.Self,
                            (Getter)CurrentAction.GetterConstructor.Invoke(points.ToArray()));
                        AddShape(shape);
                        Shapes.ClearSelected();
                    }
                }
                else if (selectfirst)
                {
                    Shapes.ClearSelected<GeoPoint>();
                }
                return Intercept;
            }

            if (CurrentAction.Name.English == "Select" || CurrentAction.Name.English == "Move") return Intercept;
            //==========================
            if (pointsNeeded < pointsLength || circleNeeded < circlesLength || linesNeeded < linesLength || polygonsNeeded < polygonsLength)
            {
                Shapes.ClearSelected();
                return Intercept;
            }


            if (pointsNeeded == pointsLength && circleNeeded == circlesLength && linesNeeded == linesLength && polygonsNeeded == polygonsLength)
            {
                var shape = GeometricActions.CreateShape(CurrentAction.Self,
                    (Getter)CurrentAction.GetterConstructor.Invoke(
                        all.SortShape().Select(o => (object?)o).ToArray()));
                AddShape(shape);
                Shapes.ClearSelected();
            }

            return Intercept;
        }
    }

    #endregion

    #region RenderAction

    protected void Renderable_OnRender(SKCanvas? dc, SKRect rect,CancellationToken ct)
    {
        if (Owner is null || dc is null) return;
        dc.Save();
        dc.ClipRect(rect);
        RenderShapes(dc, rect, Shapes.GetShapes<GeometricShape>());
        if (CurrentAction.Name.English == "Select" && LastPointerProperties.IsLeftButtonPressed)
        {
            var selrect = new AvaRect(PointerPressedPosition,
                new AvaSize(PointerMovedPosition.X - PointerPressedPosition.X,
                    PointerMovedPosition.Y - PointerPressedPosition.Y)).RegulateRectangle();
            dc.DrawRect(selrect.ToSKRect(), StrokeMid);
        }

        dc.Restore();
    }

    private void RenderShapes(SKCanvas dc, SKRect rect, IEnumerable<GeometricShape> shapes)
    {
        var unitLength = (Owner as CartesianDisplayer)!.UnitLength;
        var lt = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Bottom));
        var rt = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Bottom));
        var rb = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Top));
        var lb = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Top));
        var cd = (CartesianDisplayer)Owner;
        using SKPaint tpFilledPaint = new(),
            filledPaint = new(),
            strokePaint = new(),
            strokePaint1 = new(),
            strokeMain = new(),
            paintMain = new(),
            strokePaintMain = new(),
            bubbleBack = new();
        bubbleBack.Color = cd.AxisPaint1.Color.WithAlpha(90);
        bubbleBack.IsAntialias = true;
        strokePaintMain.Color = cd.AxisPaintMain.Color;
        strokePaintMain.IsAntialias = true;
        strokePaintMain.IsStroke = true;
        strokePaintMain.StrokeWidth = 1;
        paintMain.Color = cd.AxisPaintMain.Color;
        paintMain.IsAntialias = true;
        paintMain.StrokeWidth = 1;
        strokeMain.Color = cd.AxisPaintMain.Color;
        strokeMain.IsStroke = true;
        strokeMain.IsAntialias = true;
        strokeMain.StrokeWidth = 1;
        strokePaint1.IsStroke = true;
        strokePaint1.IsAntialias = true;
        strokePaint1.StrokeWidth = 1;
        strokePaint.IsStroke = true;
        strokePaint.IsAntialias = true;
        strokePaint.StrokeWidth = 2;
        filledPaint.IsAntialias = true;
        tpFilledPaint.IsAntialias = true;
        
        foreach (var shape in shapes)
        {
            if (shape.IsDeleted)
                continue;
            if (!shape.Visible)
                continue;
            if (shape is GeoPoint)
                continue;
            filledPaint.Color = new SKColor(shape.Color).WithAlpha(255);
            tpFilledPaint.Color = new SKColor(shape.Color).WithAlpha(90);
            strokePaint.Color = new SKColor(shape.Color).WithAlpha(255);
            switch (shape)
            {
                case Straight s:
                {
                    var v1 = s.Current.Point1;
                    var v2 = s.Current.Point2;
                    var rs = TryGetValidVec(
                        GetIntersectionOfSegmentAndLine(lt, rt, v1, v2),
                        GetIntersectionOfSegmentAndLine(rt, rb, v1, v2),
                        GetIntersectionOfSegmentAndLine(rb, lb, v1, v2),
                        GetIntersectionOfSegmentAndLine(lb, lt, v1, v2)
                    );
                    if (rs.IsError)
                        continue;
                    rs.Success(out var vs,out _);
                    var p1 = MathToPixelSk(vs.Item1);
                    var p2 = MathToPixelSk(vs.Item2);
                    if (s.Selected)
                        dc.DrawLine(p1,p2, strokePaint);
                    else
                        dc.DrawLine(p1,p2, strokePaintMain);

                    dc.DrawBubble(
                        $"{MultiLanguageResources.Instance.StraightText}:{s.Name}",
                        MathToPixelSk((s.Current.Point1 + s.Current.Point2) / 2), bubbleBack, paintMain);
                }
                    break;
                case GeoSegment s:
                {
                    var v1 = s.Current.Point1;
                    var v2 = s.Current.Point2;
                    if (s.Selected)
                        dc.DrawLine(MathToPixelSk(v1), MathToPixelSk(v2), strokePaint);
                    else
                        dc.DrawLine(MathToPixelSk(v1), MathToPixelSk(v2), strokePaintMain);

                    dc.DrawBubble($"{MultiLanguageResources.Instance.SegmentText}:{s.Name}",
                        MathToPixelSk((s.Current.Point1 + s.Current.Point2) / 2),
                        bubbleBack, paintMain);
                }
                    break;
                case GeoHalf h:
                {
                    var v1 = h.Current.Point1;
                    var v2 = h.Current.Point2;
                    var rs = TryGetValidVec(
                        GetIntersectionOfSegmentAndLine(lt, rt, v1, v2),
                        GetIntersectionOfSegmentAndLine(rt, rb, v1, v2),
                        GetIntersectionOfSegmentAndLine(rb, lb, v1, v2),
                        GetIntersectionOfSegmentAndLine(lb, lt, v1, v2)
                    );
                    if(rs.IsError)
                        continue;
                    rs.Success(out var vs,out _);
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
                        dc.DrawLine(MathToPixelSk(v1), MathToPixelSk(p), strokePaint);
                    else
                        dc.DrawLine(MathToPixelSk(v1), MathToPixelSk(p), strokePaintMain);

                    dc.DrawBubble($"{MultiLanguageResources.Instance.HalfLineText}:{h.Name}",
                        MathToPixelSk((h.Current.Point1 + h.Current.Point2) / 2),
                        bubbleBack, paintMain);
                }
                    break;
                case GeoPolygon polygon:
                {
                    var ps = new SKPoint[polygon.Locations.Length + 1];
                    for (var j = 0; j < ps.Length - 1; j++) ps[j] = MathToPixelSk(polygon.Locations[j]);

                    ps[polygon.Locations.Length] = ps[0];
                    var path = new SKPath();
                    path.AddPoly(ps);
                    if (polygon.Filled)
                    {
                        if (polygon.Selected)
                        {
                            dc.DrawPath(path, tpFilledPaint);
                            dc.DrawPath(path, strokePaint);
                        }
                        else
                        {
                            dc.DrawPath(path, FilledTranparentGrey);
                            dc.DrawPath(path, strokeMain);
                        }
                    }
                    else
                    {
                        if (polygon.Selected)
                            dc.DrawPath(path, strokePaint);
                        else
                            dc.DrawPath(path, strokeMain);
                    }

                    dc.DrawBubble($"{MultiLanguageResources.Instance.PolygonText}:{polygon.Name}",
                        MathToPixelSk((polygon.Locations[0] + polygon.Locations[1]) / 2) - new SKPoint(0, 20),
                        bubbleBack, paintMain);
                }
                    break;
                case GeoCircle circle:
                {
                    var cs = circle.Current;
                    var pf = MathToPixelSk(cs.Center);
                    var s = new SKSize((float)(cs.Radius * unitLength), (float)(cs.Radius * unitLength));
                    if (circle.Selected)
                        dc.DrawOval(pf, s, strokePaint);
                    else
                        dc.DrawOval(pf, s, strokeMain);

                    var r2 = cs.Radius * Sqrt(2) / 2;
                    dc.DrawBubble($"{MultiLanguageResources.Instance.CircleText}:{circle.Name}",
                        MathToPixelSk(circle.Current.Center + new Vec(-r2, r2)), bubbleBack, paintMain);
                }
                    break;
                case Angle ang:
                {
                    var angle = ang.AngleData;
                    var pf = MathToPixelSk(angle.AnglePoint);
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
                            strokePaint);
                    else
                        dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true,
                            strokeMain);
                    dc.DrawBubble($"{Abs(aa).ToString("0.00")}°", pf.OffSetBy(2, 2 - 20), bubbleBack,
                        paintMain);
                }
                    break;
            }
        }

        foreach (var p in Shapes.GetShapes<GeoPoint>())
        {
            if (p.IsDeleted)
                continue;
            if (!p.Visible)
                continue;
            filledPaint.Color = new SKColor(p.Color).WithAlpha(255);
            tpFilledPaint.Color = new SKColor(p.Color).WithAlpha(90);
            strokePaint1.Color = new SKColor(p.Color).WithAlpha(255);
            var index = 0;
            var loc = MathToPixelSk(p.Location);
            dc.DrawBubble($"{MultiLanguageResources.Instance.PointText}:" + p.Name,
                loc.OffSetBy(2, 2 + 20 * index++), bubbleBack, paintMain);
            if (p == MovingPoint)
            {
                dc.DrawOval(loc, new SKSize(4, 4), FilledMid);
                dc.DrawOval(loc, new SKSize(7, 7), StrokeMid);
                dc.DrawBubble(
                    $"({Round(MovingPoint.Location.X, 8)},{Round(MovingPoint.Location.Y, 8)}) {(MovingPoint.PointGetter is PointGetter_Movable && MovingPoint.IsUserEnabled ? "" : MultiLanguageResources.Instance.CantBeMovedText)}",
                    loc.OffSetBy(2, 2 + 20 * index++), bubbleBack, paintMain);
            }
            else if (p.Selected)
            {
                dc.DrawOval(loc, new SKSize(4, 4), filledPaint);
                dc.DrawOval(loc, new SKSize(7, 7), strokePaint1);
            }
            else if (p.PointerOver)
            {
                dc.DrawOval(loc, new SKSize(4, 4), filledPaint);
            }
            else
            {
                dc.DrawOval(loc, new SKSize(3, 3), paintMain);
            }

            foreach (var t in p.TextGetters)
                if (!string.IsNullOrEmpty(t.GetText()))
                    dc.DrawBubble(t.GetText(), loc.OffSetBy(2, 2 + 20 * index++), bubbleBack, paintMain);
        }
    }

    #endregion

    #region ShapeAction

    /// <summary>
    ///     在指定位置放置点
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public GeoPoint? PutPoint(AvaPoint location)
    {
        var getter = GetNewPointGetterFromLocation(location);
        if (getter is null)
            return null;
        var p = new GeoPoint(getter);
        AddShape(p);
        return p;
    }

    /// <summary>
    ///     获取指定位置的点获取器
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    protected PointGetter? GetNewPointGetterFromLocation(AvaPoint location)
    {
        var disp = (Owner as DisplayControl)!;
        var mathcursor = PixelToMath(location);
        var shapes = new List<(double, GeoShape)>();
        foreach (var geoshape in Shapes.GetShapes<GeometricShape>())
            if ((!geoshape.IsDeleted)&&(geoshape is GeoLine || geoshape is Circle))
            {
                var dist = ((geoshape.DistanceTo(mathcursor) - mathcursor) * disp.UnitLength).GetLength();
                if (dist < 5) shapes.Add((dist, geoshape));
            }

        var ss = shapes.OrderBy(key => key.Item1).ToArray();
        if (ss.Length == 0) return new PointGetter_FromLocation(PixelToMath(location));

        if (ss.Length == 1)
        {
            var shape = ss[0].Item2;
            if (shape is GeoCircle)
                return new PointGetter_OnCircle((Circle)shape, PixelToMath(location));
            if (shape is GeoLine)
                return new PointGetter_OnLine((GeoLine)shape, PixelToMath(location));
            return new PointGetter_FromLocation(PixelToMath(location));
        }

        var s1 = ss[0].Item2;
        var s2 = ss[1].Item2;
        {
            if (s1 is GeoLine l1 && s2 is GeoLine l2) return new PointGetter_FromTwoLine(l1,l2);
        }
        {
            if (s1 is GeoLine && s2 is Circle) (s1, s2) = (s2, s1);
        }
        {
            if (s1 is Circle c2 && s2 is GeoLine l2)
            {
                Vec v1, v2;
                (v1, v2) = IntersectionMath.FromLineAndCircle(l2.Current, c2.Current);
                return new PointGetter_FromLineAndCircle(l2, c2,
                    (MathToPixel(v1) - location).GetLength() < (MathToPixel(v2) - location).GetLength());
            }
        }

        if (s1 is Circle c3 && s2 is Circle c4)
        {
            Vec v1, v2;
            (v1, v2) = IntersectionMath.FromTwoCircle(c3.Current, c4.Current);
            return new PointGetter_FromTwoCircle((Circle)s1, (Circle)s2,
                (MathToPixel(v1) - location).GetLength() < (MathToPixel(v2) - location).GetLength());
        }

        return null;
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
    /// <param name="location"></param>
    /// <param name="shape"></param>
    /// <returns></returns>
    public bool TryGetShape<T>(AvaPoint location, out T shape) where T : GeometricShape
    {
        var disp = (Owner as DisplayControl)!;
        var v = Owner!.PixelToMath(location);
        var distance = double.PositiveInfinity;
        shape = null!;
        foreach (var s in Shapes)
        {
            if (!s.Visible)
                continue;
            if (s is T tar)
            {
                var dis = ((tar.DistanceTo(v) - v) * disp.UnitLength).GetLength();
                if (dis < Setting.PointerTouchRange && dis < distance)
                {
                    distance = dis;
                    shape = tar;
                }
            }
        }

        return shape != null!;
    }

    #endregion
}