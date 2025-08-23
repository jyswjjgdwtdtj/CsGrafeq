using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using CsGrafeqApp.Controls.Displayers;
using CsGrafeqApp.ViewModels;
using SkiaSharp;
using static CsGrafeqApp.Controls.SkiaEx;
using static CsGrafeq.Shapes.GeometryMath;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using AvaSize = Avalonia.Size;
using GeoHalf = CsGrafeq.Shapes.Half;
using static CsGrafeqApp.AvaloniaMath;

namespace CsGrafeqApp.Addons.GeometryPad;

public partial class GeometryPad : Addon
{
    private readonly List<ActionData> Actions = new();
    protected Func<Vec, AvaPoint> MathToPixel;
    protected Func<Vec, SKPoint> MathToPixelSK;
    private GeoPoint? MovingPoint;
    protected Func<AvaPoint, Vec> PixelToMath;
    protected Func<SKPoint, Vec> PixelToMathSK;
    protected Func<double, double> PixelToMathX, PixelToMathY, MathToPixelX, MathToPixelY;
    private AvaPoint PointerMovedPos;
    private AvaPoint PointerPressedPos;
    private PointerPointProperties PointerProperties;
    private AvaPoint PointerReleasedPos;

    public GeometryPad()
    {
        DataContext = new GeometryPadViewModel();
        Shapes = (DataContext as GeometryPadViewModel)!.Shapes;
        foreach (var i in (DataContext as GeometryPadViewModel)!.Operations)
        foreach (var j in i)
            Actions!.Add(j);

        GeoPadAction = Actions.FirstOrDefault();
        Shapes.CollectionChanged += (s, e) => { Owner?.Invalidate(this); };
        Shapes.OnShapeChanged += () => { Owner?.Invalidate(this); };
#if DEBUG
        var p1 = new GeoPoint(new PointGetter_FromLocation((1, 3)));
        var p2 = new GeoPoint(new PointGetter_FromLocation((3, 4)));
        var p3 = new GeoPoint(new PointGetter_FromLocation((6, 7)));
        Shapes.Add(p1);
        Shapes.Add(p2);
        Shapes.Add(p3);
        Shapes.Add(new GeoPolygon(new PolygonGetter([p1, p2, p3])));
        Shapes.Add(new Angle(new AngleGetter_FromThreePoint(p1, p2, p3)));
        Shapes.Add(new Straight(new LineGetter_Connected(p1, p2)));
        Shapes.Add(new Circle(new CircleGetter_FromCenterAndPoint(p1,p2)));
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
        }
    }

    public override string Name => "GeometryPad";

    public void TextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
            if (!double.TryParse(box.Text, out _))
                box.Text = "0";
    }

    public void TextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is TextBox box)
        {
            var parent = box.Parent;
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
            }
        }
    }

    public void RadioButtonChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
            if (rb.IsChecked == true)
            {
                SetAction(rb.Name);
                Owner.Owner.MsgBox(new TextBlock(){Text=GeoPadAction.Description});
            }
        
    }

    public void SetAction(string geoPadAction)
    {
        if (GeoPadAction.Name == geoPadAction)
            return;
        GeoPadAction = Actions.Find(data => data.Name == geoPadAction);
        Shapes.ClearSelected();
    }

    protected override bool PointerPressed(AddonPointerEventArgs e)
    {
        PointerProperties = e.Properties;
        PointerPressedPos = e.Location;
        if (GeoPadAction.Name == "Select")
        {
            Shapes.ClearSelected();
        }
        else
        {
            MovingPoint = GetShape<GeoPoint>(e.Location);
            if (MovingPoint != null)
                return Intercept;
        }

        MovingPoint = null;
        return DoNext;
    }

    protected override bool PointerMoved(AddonPointerEventArgs e)
    {
        PointerProperties = e.Properties;
        Owner.Suspend();
        PointerMovedPos = e.Location;
        if (GeoPadAction.Name == "Select" && e.Properties.IsLeftButtonPressed)
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
            if (PointerMovedPos != PointerReleasedPos)
                Shapes.ClearSelected();
            if (MovingPoint.PointGetter is PointGetter_Movable mpg)
            {
                mpg.SetControlPoint(Owner.PixelToMath(PointerMovedPos));
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
        if (GeoPadAction.Name == "Select")
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
            return Intercept;
        }

        if (MovingPoint == null)
        {
            Owner.Resume();
            return DoNext;
        }

        if (PointerReleasedPos != PointerPressedPos)
            Shapes.ClearSelected();
        (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(MovingPoint.Location);
        MovingPoint = null;
        Owner.Resume();
        return Intercept;
    }

    protected override bool PointerTapped(AddonPointerEventArgsBase e)
    {
        Owner.Suspend();
        if (GeoPadAction.Name == "Put")
        {
            Shapes.ClearSelected();
            PutPoint(e.Location).Selected = true;
            Owner.Resume();
            return Intercept;
        }

        var first = Shapes.GetSelectedShapes<GeoPoint>().FirstOrDefault();
        var selectfirst = false;
        if (GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Point) &&
            TryGetShape<GeoPoint>(e.Location, out var point))
        {
            if (first == point)
                selectfirst = true;
            else
                point.Selected = !point.Selected;
        }
        else if (GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Line) &&
                 TryGetShape<GeoLine>(e.Location, out var line))
        {
            line.Selected = !line.Selected;
        }
        else if (GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Circle) &&
                 TryGetShape<GeoCircle>(e.Location, out var circle))
        {
            circle.Selected = !circle.Selected;
        }
        else if (GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Polygon) &&
                 TryGetShape<GeoPolygon>(e.Location, out var polygon))
        {
            polygon.Selected = !polygon.Selected;
        }
        else if (GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Point) && GeoPadAction.Name != "Move" &&
                 GeoPadAction.Name != "Select")
        {
            PutPoint(e.Location).Selected = true;
        }

        if (GeoPadAction.Name != "Move" && GeoPadAction.Name != "Select")
        {
            if (!GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Point))
                Shapes.ClearSelected<GeoPoint>();
            if (!GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Line))
                Shapes.ClearSelected<GeoLine>();
            if (!GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Circle))
                Shapes.ClearSelected<GeoCircle>();
            if (!GeoPadAction.ShapesToUse.HasFlag(ActionData.UseShape.Polygon))
                Shapes.ClearSelected<GeoPolygon>();
        }

        var SPoints = Shapes.GetSelectedShapes<GeoPoint>().ToArray();
        var SLines = Shapes.GetSelectedShapes<GeoLine>().ToArray();
        var SCircles = Shapes.GetSelectedShapes<GeoCircle>().ToArray();
        var SPolygons = Shapes.GetSelectedShapes<GeoPolygon>().ToArray();
        var plen = SPoints.Length;
        var llen = SLines.Length;
        var clen = SCircles.Length;
        switch (GeoPadAction.Name)
        {
            case "Select":
            case "Move":
                break;
            case "Put":
            {
                Shapes.ClearSelected();
            }
                break;
            case "Middle":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var newpoint = new GeoPoint(new PointGetter_MiddlePoint(SPoints[0], SPoints[1]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected();
                }

                Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
                if (plen >= 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Median Center":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var newpoint = new GeoPoint(new PointGetter_MedianCenter(SPoints[0], SPoints[1],
                        SPoints[2]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected<GeoPoint>();
                }

                Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
                if (plen >= 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "In Center":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var newpoint = new GeoPoint(new PointGetter_InCenter(SPoints[0], SPoints[1],
                        SPoints[2]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected<GeoPoint>();
                }

                Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
                if (plen >= 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Out Center":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var newpoint = new GeoPoint(new PointGetter_OutCenter(SPoints[0], SPoints[1],
                        SPoints[2]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected<GeoPoint>();
                }

                Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
                if (plen >= 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Ortho Center":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var newpoint = new GeoPoint(new PointGetter_OrthoCenter(SPoints[0], SPoints[1],
                        SPoints[2]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected<GeoPoint>();
                }

                Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
                if (plen >= 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Axial Symmetry":
            {
                if (plen == 1 && llen == 1 && clen == 0)
                {
                    var newpoint =
                        new GeoPoint(new PointGetter_AxialSymmetryPoint(SPoints[0], SLines[0]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected();
                }

                if (plen > 1)
                    Shapes.ClearSelected<GeoPoint>();
                if (llen > 1)
                    Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
            }
                break;
            case "Nearest":
            {
                if (plen == 1 && llen == 1 && clen == 0)
                {
                    var newpoint =
                        new GeoPoint(new PointGetter_NearestPointOnLine(SLines[0], SPoints[0]));
                    Shapes.Add(newpoint);
                    Shapes.ClearSelected();
                }

                if (plen > 1)
                    Shapes.ClearSelected<GeoPoint>();
                if (llen > 1)
                    Shapes.ClearSelected<GeoLine>();
                Shapes.ClearSelected<GeoCircle>();
            }
                break;
            case "Straight":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var line = new Straight(new LineGetter_Connected(SPoints[0], SPoints[1]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }

                if (plen > 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Segment":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var line = new Segment(new LineGetter_Segment(SPoints[0], SPoints[1]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }

                if (plen > 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Half":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var line = new GeoHalf(new LineGetter_Half(SPoints[0], SPoints[1]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }

                if (plen > 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Vertical":
            {
                if (plen == 1 && llen == 1 && clen == 0)
                {
                    var line = new Straight(new LineGetter_Vertical(SLines[0], SPoints[0]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }

                if (plen > 1)
                    Shapes.ClearSelected<GeoPoint>();
                if (llen > 1)
                    Shapes.ClearSelected<GeoLine>();
            }
                break;
            case "Parallel":
            {
                if (plen == 1 && llen == 1 && clen == 0)
                {
                    var line = new Straight(new LineGetter_Parallel(SLines[0], SPoints[0]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }

                if (plen > 1)
                    Shapes.ClearSelected<GeoPoint>();
                if (llen > 1)
                    Shapes.ClearSelected<GeoLine>();
            }
                break;
            case "Perpendicular Bisector":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var line = new Straight(new LineGetter_PerpendicularBisector(SPoints[0], SPoints[1]));
                    Shapes.Add(line);
                    Shapes.ClearSelected();
                }
                else if (plen == 0 && llen == 1 && clen == 0)
                {
                    if (SLines[0] is Segment)
                    {
                        var line = new Straight(new LineGetter_PerpendicularBisector(
                            new GeoPoint(new PointGetter_EndOfLine(SLines[0], true)),
                            new GeoPoint(new PointGetter_EndOfLine(SLines[0], false))));
                        Shapes.Add(line);
                        Shapes.ClearSelected();
                    }
                }

                if (plen > 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Three Points":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var circle = new Circle(new CircleGetter_FromThreePoint(SPoints[0], SPoints[1],
                        SPoints[2]));
                    Shapes.Add(circle);
                    Shapes.ClearSelected<GeoPoint>();
                }

                if (plen > 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Center and Point":
            {
                if (plen == 2 && llen == 0 && clen == 0)
                {
                    var circle =
                        new Circle(new CircleGetter_FromCenterAndPoint(SPoints[0], SPoints[1]));
                    Shapes.Add(circle);
                    Shapes.ClearSelected<GeoPoint>();
                }

                if (plen > 2)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            case "Polygon":
            {
                if (llen == 0 && clen == 0 && plen > 2)
                    if (selectfirst)
                    {
                        var polygon = new Polygon(new PolygonGetter(SPoints));
                        Shapes.Add(polygon);
                        Shapes.ClearSelected();
                    }
                else if(plen<=2&&selectfirst)
                    Shapes.ClearSelected<GeoPoint>();
                }
                break;
            case "Fitted":
            {
                if (llen == 0 && clen == 0 && plen > 2)
                    if (selectfirst)
                    {
                        var fitted = new Straight(new LineGetter_Fitted(SPoints));
                        Shapes.Add(fitted);
                        Shapes.ClearSelected();
                    }
            }
                break;
            case "Angle":
            {
                if (plen == 3 && llen == 0 && clen == 0)
                {
                    var angle = new Angle(new AngleGetter_FromThreePoint(SPoints[0], SPoints[1], SPoints[2]));
                    Shapes.Add(angle);
                    Shapes.ClearSelected<GeoPoint>();
                }

                Shapes.ClearSelected<GeoCircle>();
                Shapes.ClearSelected<GeoLine>();
                if (plen > 3)
                    Shapes.ClearSelected<GeoPoint>();
            }
                break;
            default:
                Extension.Throw<bool>(GeoPadAction.Name);
                Owner.Resume();
                return DoNext;
        }

        Owner.Resume();
        return Intercept;
    }

    protected override bool KeyDown(KeyEventArgs e)
    {
        Owner.Suspend();
        var res = DoNext;
        switch (e.Key)
        {
            case Key.Delete:
            {
                foreach (var shape in Shapes.GetSelectedShapes<GeometryShape>().ToArray())
                    if (shape.Selected)
                    {
                        Shapes.Remove(shape);
                        res &= Intercept;
                    }
            }
                break;
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
            case Key.A:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        res &= Intercept;
                        foreach (var shape in Shapes.OfType<GeometryShape>().ToArray())
                            shape.Selected = true;
                    }
                }
                break;
            case Key.B:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        res &= Intercept;
                        foreach (var shape in Shapes.OfType<GeometryShape>().ToArray())
                            shape.Selected = false;
                    }
                }
                break;

        }

        if (res == Intercept) Owner.Invalidate();
        Owner.Resume();
        return res;
    }

    protected override void Render(SKCanvas dc, SKRect rect)
    {
        rect.Intersect(Owner.ValidRect.ToSKRect());
        dc.Save();
        dc.ClipRect(rect);
        //RenderFunction(dc, new SKRectI((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom));
        RenderShapes(dc, rect, Shapes.GetShape<GeometryShape>());
        if (GeoPadAction.Name == "Select" && PointerProperties.IsLeftButtonPressed)
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
               BubbleBack = new() { Color = cd.AxisPaint1.Color.WithAlpha(90), IsAntialias = true })
        {
            foreach (var shape in shapes)
            {
                if (shape is null)
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
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), FilledPaint);
                        else
                            dc.DrawLine(MathToPixelSK(vs.Item1), MathToPixelSK(vs.Item2), PaintMain);

                        dc.DrawBubble($"Straight:{s.Name}",MathToPixelSK((s.Current.Point1+s.Current.Point2)/2),BubbleBack,PaintMain);
                    }
                        break;
                    case GeoSegment s:
                    {
                        var v1 = s.Current.Point1;
                        var v2 = s.Current.Point2;
                        if (s.Selected)
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(v2), FilledPaint);
                        else
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(v2), PaintMain);

                        dc.DrawBubble($"Segment:{s.Name}", MathToPixelSK((s.Current.Point1 + s.Current.Point2) / 2), BubbleBack, PaintMain);
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
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(p), FilledPaint);
                        else
                            dc.DrawLine(MathToPixelSK(v1), MathToPixelSK(p), PaintMain);

                        dc.DrawBubble($"Half:{h.Name}", MathToPixelSK((h.Current.Point1 + h.Current.Point2) / 2), BubbleBack, PaintMain);
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
                        dc.DrawBubble($"Polygon:{polygon.Name}", MathToPixelSK((polygon.Locations[0] + polygon.Locations[1]) / 2)-new SKPoint(0,20), BubbleBack, PaintMain);
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

                        double r2= cs.Radius * Sqrt(2) / 2;
                        dc.DrawBubble($"Circle:{circle.Name}", MathToPixelSK(circle.InnerCircle.Center+new Vec(-r2,r2)), BubbleBack, PaintMain);
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

            foreach (var p in Shapes.GetShape<GeoPoint>())
            {
                if (p is null)
                    continue;
                if (!p.Visible)
                    continue;
                FilledPaint.Color = new SKColor(p.Color).WithAlpha(255);
                TPFilledPaint.Color = new SKColor(p.Color).WithAlpha(90);
                StrokePaint.Color = new SKColor(p.Color).WithAlpha(255);
                var index = 0;
                var loc = MathToPixelSK(p.Location);
                dc.DrawBubble("Point:" + (p.Name), loc.OffSetBy(2, 2 + 20 * index++),BubbleBack, PaintMain);
                if (p == MovingPoint)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledMedian);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokeMedian);
                    dc.DrawBubble(
                        $"({p.Location.X},{p.Location.Y}) {(p.PointGetter is PointGetter_Movable ? "" : "不可移动")}",
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

    public GeoPoint PutPoint(AvaPoint Location)
    {
        var disp = (Owner as DisplayControl)!;
        var mathcursor = PixelToMath(Location);
        var shapes = new List<(double, Shape)>();
        foreach (var geoshape in Shapes.GetShape<GeometryShape>())
            if (geoshape is Line || geoshape is Circle)
            {
                var dist = (geoshape.HitTest(mathcursor) * disp.UnitLength).GetLength();
                if (dist < 5) shapes.Add((dist, geoshape));
            }

        var ss = shapes.OrderBy(key => key.Item1).ToArray();
        GeoPoint newpoint;
        if (ss.Length == 0)
        {
            newpoint = new GeoPoint(new PointGetter_FromLocation(PixelToMath(Location)));
        }
        else if (ss.Length == 1)
        {
            var shape = ss[0].Item2;
            if (shape is GeoCircle)
                newpoint = new GeoPoint(new PointGetter_OnCircle((Circle)shape, PixelToMath(Location)));
            else if (shape is GeoLine)
                newpoint = new GeoPoint(new PointGetter_OnLine((Line)shape, PixelToMath(Location)));
            else
                newpoint = new GeoPoint(new PointGetter_FromLocation(PixelToMath(Location)));
        }
        else
        {
            var s1 = ss[0].Item2;
            var s2 = ss[1].Item2;
            if (s1 is Line && s2 is Line)
            {
                newpoint = new GeoPoint(new PointGetter_FromTwoLine(s1 as Line, s2 as Line));
            }
            else if (s1 is Line l1 && s2 is Circle c1)
            {
                Vec v1, v2;
                (v1, v2) = GetPointFromCircleAndLine(l1.Current.Point1, l1.Current.Point2, c1.InnerCircle.Center,
                    c1.InnerCircle.Radius);
                newpoint = new GeoPoint(new PointGetter_FromLineAndCircle(l1, c1,
                    (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength()));
            }
            else if (s1 is Circle c2 && s2 is Line l2)
            {
                Vec v1, v2;
                (v1, v2) = GetPointFromCircleAndLine(l2.Current.Point1, l2.Current.Point2, c2.InnerCircle.Center,
                    c2.InnerCircle.Radius);
                newpoint = new GeoPoint(new PointGetter_FromLineAndCircle(l2, c2,
                    (MathToPixel(v1) - Location).GetLength() < (MathToPixel(v2) - Location).GetLength()));
            }
            else
            {
                //暂不支持
                return null;
            }
        }

        Shapes.Add(newpoint);
        return newpoint;
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
                var dis = (tar.HitTest(v) * disp.UnitLength).GetLength();
                if (dis < 5 && dis < distance)
                {
                    distance = dis;
                    target = tar;
                }
            }
        }

        return target;
    }
}