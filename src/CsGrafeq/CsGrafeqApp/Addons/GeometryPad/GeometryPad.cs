using Avalonia;
using CsGrafeqApp.Controls;
using Publics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using static CsGrafeqApp.Controls.SkiaEx;
using static CsGrafeqApp.Shapes.GeometryMath;
using AvaPoint = Avalonia.Point;
using AvaRect = Avalonia.Rect;
using AvaSize = Avalonia.Size;
using GeoCircle = CsGrafeqApp.Shapes.Circle;
using GeoLine = CsGrafeqApp.Shapes.Line;
using GeoLineSegment = CsGrafeqApp.Shapes.Segment;
using GeoPoint = CsGrafeqApp.Shapes.Point;
using GeoPolygon = CsGrafeqApp.Shapes.Polygon;
using GeoShape = CsGrafeqApp.Shapes.Shape;
using GeoHalf = CsGrafeqApp.Shapes.Half;
using CsGrafeqApp.Shapes;
using CsGrafeqApp.Shapes.ShapeGetter;
using CsGrafeqApp.Classes;
using CsGrafeqApp.Controls.Displayers;
using static CsGrafeqApp.Shapes.GeometryMath;
using static CsGrafeqApp.InternalMath;

namespace CsGrafeqApp.Addons.GeometryPad
{
    public class GeometryPad:Addon
    {
        private readonly ShapeList Shapes=new ShapeList();
        private GeoPoint? MovingPoint;
        internal string GeoPadAction = "Move";
        public GeometryPad()
        {
            OperationControl = new OpControl(Shapes, SetAction);
            Shapes.CollectionChanged += (s, e) =>
            {
                Owner?.Invalidate(this);
            };
            Shapes.OnShapeChanged += () =>
            {
                Owner?.Invalidate(this);
                Console.WriteLine(123);
            };
            #if DEBUG
            var p1 = new GeoPoint(new PointGetter_FromLocation((1, 3)));
            var p2 = new GeoPoint(new PointGetter_FromLocation((3, 4)));
            var p3 = new GeoPoint(new PointGetter_FromLocation((6, 7)));
            Shapes.Add(p1);
            Shapes.Add(p2);
            Shapes.Add(p3);
            Shapes.Add(new GeoPolygon(new PolygonGetter(p1,p2,p3)));
            Shapes.Add(new Angle(new AngleGetter_FromThreePoint(p1, p2, p3)));
            Shapes.Add(new Straight(new LineGetter_Connected(p1,p2)));
            #endif
        }
        public void SetAction(string geoPadAction)
        {
            if(GeoPadAction==geoPadAction)
                return;
            GeoPadAction=geoPadAction;
            ClearSelect();
        }

        protected AvaPoint DownPoint=new AvaPoint(-1,-1),MovePoint=new(-1,-1);
        protected override bool PointerPressed(AddonPointerEventArgs e)
        {
            DownPoint = e.Location;
            if (GeoPadAction != "Select")
            {
                MovingPoint = GetShape<GeoPoint>(DownPoint);
                if(MovingPoint!=null)
                    return Intercept;
            }
            else
            {
                ClearSelect();
            }
            MovingPoint = null;
            return DoNext;
        }

        protected override bool PointerMoved(AddonPointerEventArgs e)
        {
            Owner.Suspend();
            MovePoint=e.Location;
            if (GeoPadAction == "Select"&&DownPoint!=new AvaPoint(-1,-1))
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
                if(MovePoint!=DownPoint)
                    ClearSelect();
                if(MovingPoint.PointGetter is PointGetter_Movable mpg)
                {
                    mpg.SetControlPoint(Owner.PixelToMath(MovePoint));
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
            Owner.Suspend();
            DisplayControl disp=(Owner as DisplayControl)!;
            if (GeoPadAction == "Select")
            {
                var rect = RegulateRectangle(new AvaRect(DownPoint, new AvaSize(MovePoint.X - DownPoint.X, MovePoint.Y - DownPoint.Y)));
                var mathrectloc = Owner.PixelToMath(new AvaPoint(rect.Left,rect.Top+rect.Height));
                var mathrectsize = new Vec(((double)rect.Width),((double)rect.Height))/disp.UnitLength;
                DownPoint = new AvaPoint(-1, -1);
                foreach(var s in Shapes)
                {
                    switch (s)
                    {
                        case GeoPoint p:
                            {
                                Vec v = p.Location - mathrectloc;
                                if (InRange(0, mathrectsize.X, v.X) && InRange(0, mathrectsize.Y, v.Y))
                                    p.Selected = true;
                            }
                            break;
                        case Line l:
                            {
                                Vec v1 = l.Current.Point1;
                                Vec v2 = l.Current.Point2;
                                Vec LT = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Bottom));
                                Vec RT = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Bottom));
                                Vec RB = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Top));
                                Vec LB = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Top));
                                (Vec, Vec) vs = GetValidVec(
                                    GetIntersectionLSAndSL(LT, RT, v1, v2),
                                    GetIntersectionLSAndSL(RT, RB, v1, v2),
                                    GetIntersectionLSAndSL(RB, LB, v1, v2),
                                    GetIntersectionLSAndSL(LB, LT, v1, v2)
                                );
                                vs.Item1 = l.CheckIsValid(vs.Item1) ? vs.Item1 : Vec.Invalid;
                                vs.Item2 = l.CheckIsValid(vs.Item2) ? vs.Item2 : Vec.Invalid;
                                
                                if (!(vs.Item1.IsInvalid() && vs.Item2.IsInvalid()))
                                    l.Selected = true;
                                else if (InRange(mathrectloc.X, mathrectloc.X + mathrectsize.X, ((l.Current.Point1 + l.Current.Point2) / 2).X) && InRange(mathrectloc.Y, mathrectloc.Y + mathrectsize.Y, ((l.Current.Point1 + l.Current.Point2) / 2).Y))
                                {
                                    l.Selected = true;
                                }
                            }
                            break;
                        case Circle c:
                            {
                                var o = mathrectloc + mathrectsize / 2;
                                var cc = c.InnerCircle.Center - o;
                                o = mathrectsize / 2;
                                cc.X = Math.Abs(cc.X);
                                cc.Y=Math.Abs(cc.Y);
                                var bc = cc - o;
                                bc.X = Math.Max(bc.X, 0);
                                bc.Y=Math.Max(bc.Y, 0);
                                if (bc.GetLength() <= c.InnerCircle.Radius)
                                    c.Selected = true;
                            }
                            break;
                        case Polygon p:
                        {
                            foreach (var point in p.Locations)
                            {
                                Vec v=point-mathrectloc;
                                if (InRange(0, mathrectsize.X, v.X) && InRange(0, mathrectsize.Y, v.Y))
                                {
                                    p.Selected = true;
                                }
                                break;
                            }
                        }
                            break;
                        case Angle a:
                        {
                            Vec v=a.AngleData.AnglePoint-mathrectloc;
                            if (InRange(0, mathrectsize.X, v.X) && InRange(0, mathrectsize.Y, v.Y))
                            {
                                a.Selected = true;
                            }
                        }
                            break;
                    }
                }
                Owner.Resume();
                return Intercept;
            }

            if (MovingPoint == null)
            {
                Owner.Resume();
                return DoNext;
            }
            if(e.Location!=DownPoint)
                ClearSelect();
            (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(MovingPoint.Location);
            MovingPoint = null;
            DownPoint = new AvaPoint(-1, -1);
            Owner.Resume();
            return Intercept;
        }

        protected override bool PointerTapped(AddonPointerEventArgsBase e)
        {
            Owner.Suspend();
            if (GeoPadAction == "Put")
            {
                ClearSelect();
                PutPoint(e.Location);
                Owner.Resume();
                return Intercept;
            }
            GeoPoint? first =Shapes.GetSelectedShapes<GeoPoint>().FirstOrDefault();
            if (TryGetShape<GeoPoint>(e.Location, out var point))
            {
                point.Selected=!point.Selected;
            }
            else if (TryGetShape<GeoLine>(e.Location, out var line))
            {
                line.Selected = !line.Selected;
            }
            else if (TryGetShape<GeoCircle>(e.Location, out var circle))
            {
                circle.Selected = !circle.Selected; 
            }
            else if (GeoPadAction != "Move" && GeoPadAction != "Select")
            {
                PutPoint(e.Location).Selected = true;
            }
            GeoPoint[] SPoints = Shapes.GetSelectedShapes<GeoPoint>().ToArray();
            GeoLine[] SLines = Shapes.GetSelectedShapes<GeoLine>().ToArray();
            GeoCircle[] SCircles = Shapes.GetSelectedShapes<GeoCircle>().ToArray();
            GeoPolygon[] SPolygons = Shapes.GetSelectedShapes<GeoPolygon>().ToArray();
            int plen = SPoints.Length;
            int llen = SLines.Length;
            int clen = SCircles.Length;
            switch (GeoPadAction)
            {
                case "Select":
                case "Move":
                    break;
                case "Put":
                {
                    ClearSelect();
                }
                    break;
                case "Middle":
                {
                    if ((plen == 2) && (llen == 0) && (clen == 0))
                    {
                        GeoPoint newpoint = new GeoPoint(new PointGetter_MiddlePoint(SPoints[0], SPoints[1]));
                        Shapes.Add(newpoint);
                        ClearSelect();
                    }
                    else if (plen == 0 && llen == 1 && clen == 0)
                    {
                        if (SLines[0] is Segment)
                        {
                            GeoPoint newpoint = new GeoPoint(new PointGetter_MiddlePoint(
                                new GeoPoint(new PointGetter_EndOfLine(SLines[0], true)),
                                new GeoPoint(new PointGetter_EndOfLine(SLines[0], false))
                            ));
                            Shapes.Add(newpoint);
                            ClearSelect();
                        }
                    }
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                    if (plen >= 2)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Median Center":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        GeoPoint newpoint = new GeoPoint(new PointGetter_MedianCenter(SPoints[0], SPoints[1],
                            SPoints[2]));
                        Shapes.Add(newpoint);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                    if (plen >= 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "In Center":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        GeoPoint newpoint = new GeoPoint(new PointGetter_InCenter(SPoints[0], SPoints[1],
                            SPoints[2]));
                        Shapes.Add(newpoint);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                    if (plen >= 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Out Center":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        GeoPoint newpoint = new GeoPoint(new PointGetter_OutCenter(SPoints[0], SPoints[1],
                            SPoints[2]));
                        Shapes.Add(newpoint);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                    if (plen >= 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Ortho Center":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        GeoPoint newpoint = new GeoPoint(new PointGetter_OrthoCenter(SPoints[0], SPoints[1],
                            SPoints[2]));
                        Shapes.Add(newpoint);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                    if (plen >= 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Axial Symmetry":
                {
                    if (plen == 1 && llen == 1 && clen == 0)
                    {
                        GeoPoint newpoint =
                            new GeoPoint(new PointGetter_AxialSymmetryPoint(SPoints[0], SLines[0]));
                        Shapes.Add(newpoint);
                        ClearSelect();
                    }
                    if (plen > 1)
                        ClearSelect<GeoPoint>();
                    if (llen > 1)
                        ClearSelect<GeoLine>();
                    ClearSelect<Circle>();
                }
                    break;
                case "Nearest":
                {
                    if (plen == 1 && llen == 1 && clen == 0)
                    {
                        GeoPoint newpoint =
                            new GeoPoint(new PointGetter_NearestPointOnLine(SLines[0], SPoints[0]));
                        Shapes.Add(newpoint);
                        ClearSelect();
                    }
                    if (plen > 1)
                        ClearSelect<GeoPoint>();
                    if (llen > 1)
                        ClearSelect<GeoLine>();
                    ClearSelect<Circle>();
                }
                    break;
                case "Straight":
                {
                    if (plen == 2 && llen == 0 && clen == 0)
                    {
                        var line = new Straight(new LineGetter_Connected(SPoints[0],SPoints[1]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    if (plen > 2)
                        ClearSelect<GeoPoint>();
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                }
                    break;
                case "Segment":
                {
                    if (plen == 2 && llen == 0 && clen == 0)
                    {
                        var line = new Segment(new LineGetter_Segment(SPoints[0], SPoints[1]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    if (plen > 2)
                        ClearSelect<GeoPoint>();
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                }
                    break;
                case "Half":
                {
                    if (plen == 2 && llen == 0 && clen == 0)
                    {
                        var line = new GeoHalf(new LineGetter_Half(SPoints[0], SPoints[1]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    if (plen > 2)
                        ClearSelect<GeoPoint>();
                    ClearSelect<GeoLine>();
                    ClearSelect<GeoCircle>();
                }
                    break;
                case "Vertical":
                {
                    if (plen == 1 && llen == 1 && clen == 0)
                    {
                        var line = new Straight(new LineGetter_Vertical(SLines[0],SPoints[0]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    if (plen > 1)
                        ClearSelect<GeoPoint>();
                    if (llen > 1)
                        ClearSelect<GeoLine>();
                    ClearSelect<Circle>();
                }
                    break;
                case "Parallel":
                {
                    if (plen == 1 && llen == 1 && clen == 0)
                    {
                        var line = new Straight(new LineGetter_Parallel(SLines[0],SPoints[0]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    if (plen > 1)
                        ClearSelect<GeoPoint>();
                    if (llen > 1)
                        ClearSelect<GeoLine>();
                    ClearSelect<Circle>();
                }
                    break;
                case "Perpendicular Bisector":
                {
                    if ((plen == 2) && (llen == 0) && (clen == 0))
                    {
                        var line = new Straight(new LineGetter_PerpendicularBisector(SPoints[0],SPoints[1]));
                        Shapes.Add(line);
                        ClearSelect();
                    }
                    else if (plen == 0 && llen == 1 && clen == 0)
                    {
                        if (SLines[0] is Segment)
                        {
                            var line = new Straight(new LineGetter_PerpendicularBisector(
                                new GeoPoint(new PointGetter_EndOfLine(SLines[0], true)),
                                new GeoPoint(new PointGetter_EndOfLine(SLines[0], false))));
                            Shapes.Add(line);
                            ClearSelect();
                        }
                    }

                    ClearSelect<Circle>();
                    ClearSelect<GeoLine>();
                    if (plen > 2)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Three Points":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        Circle circle = new Circle(new CircleGetter_FromThreePoint(SPoints[0], SPoints[1],
                            SPoints[2]));
                        Shapes.Add(circle);
                        ClearSelect<GeoPoint>();
                    }

                    ClearSelect<Circle>();
                    ClearSelect<GeoLine>();
                    if (plen > 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Center and Point":
                {
                    if (plen == 2 && llen == 0 && clen == 0)
                    {
                        Circle circle =
                            new Circle(new CircleGetter_FromCenterAndPoint(SPoints[0], SPoints[1]));
                        Shapes.Add(circle);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<Circle>();
                    ClearSelect<GeoLine>();
                    if (plen > 2)
                        ClearSelect<GeoPoint>();
                }
                    break;
                case "Polygon":
                {
                    if (llen == 0 && clen == 0 && plen > 2)
                    {
                        if (TryGetShape<GeoPoint>(e.Location,out GeoPoint p)&&first==p)
                        {
                            Polygon polygon = new Polygon(new PolygonGetter(first,SPoints));
                            Shapes.Add(polygon);
                            ClearSelect();
                        }
                    }
                    ClearSelect<Circle>();
                    ClearSelect<GeoLine>();
                }
                    break;
                case "Angle":
                {
                    if (plen == 3 && llen == 0 && clen == 0)
                    {
                        Angle angle = new Angle(new AngleGetter_FromThreePoint(SPoints[0], SPoints[1], SPoints[2]));
                        Shapes.Add(angle);
                        ClearSelect<GeoPoint>();
                    }
                    ClearSelect<Circle>();
                    ClearSelect<GeoLine>();
                    if (plen > 3)
                        ClearSelect<GeoPoint>();
                }
                    break;
                default:
                    Console.WriteLine(GeoPadAction);
                    Owner.Resume();
                    return DoNext;
            }
            Owner.Resume();
            return Intercept;
        }

        protected override bool KeyDown(KeyEventArgs e)
        {
            bool res=DoNext;
            switch (e.Key)
            {
                case Key.Delete:
                {
                    foreach (var shape in Shapes)
                    {
                        if (shape.Selected)
                        {
                            Shapes.Remove(shape);
                            res &= Intercept;
                        }
                    }
                }
                    break;
                case Key.Tab:
                {
                    foreach (var shape in Shapes)
                    {
                        if (shape.Selected)
                        {
                            shape.Selected = false;
                            foreach (var subshape in shape.SubShapes)
                            {
                                subshape.Selected = true;
                            }
                        }
                    }
                }
                    break;
            }
            return res; 
        }

        public void ClearSelect()
        {
            Shapes.ClearSelected();
        }
        public void ClearSelect<T>() where T : GeoShape
        {
            Shapes.ClearSelected<T>();
        }
        public override Displayer? Owner { 
            get =>base.Owner;
            set
            {
                if (!(value is CartesianDisplayer))
                    throw new Exception();
                base.Owner = value;
            }
        }
        public override string Name => "GeometryPad";
        protected override void Render(SKCanvas dc, SKRect rect)
        {
            RenderShapes(dc, rect);
            if (GeoPadAction == "Select"&&DownPoint!=new AvaPoint(-1,-1))
            {
                var selrect =RegulateRectangle( new Rect(DownPoint, new AvaSize(MovePoint.X - DownPoint.X, MovePoint.Y - DownPoint.Y)));
                dc.DrawRect(selrect.ToSKRect(),FilledTpMedian);
                dc.DrawRect(selrect.ToSKRect(),StrokeMedian);
            }
        }
        private void RenderShapes(SKCanvas dc,SKRect rect)
        {
            dc.Save();
            AvaRect Bounds = Owner!.Bounds;
            dc.ClipRect(rect);
            double UnitLength = (Owner as CartesianDisplayer)!.UnitLength;
            foreach(var shape in Shapes.GetElseShapes())
            {
                if (shape is null)
                    continue;
                if (!shape.Visible)
                    continue;
                Vec LT = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Bottom));
                Vec RT = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Bottom));
                Vec RB = new Vec(Owner.PixelToMathX(rect.Right), Owner.PixelToMathY(rect.Top));
                Vec LB = new Vec(Owner.PixelToMathX(rect.Left), Owner.PixelToMathY(rect.Top));
                switch (shape)
                {
                    case Straight s:
                        {
                            Vec v1 = s.Current.Point1;
                            Vec v2 = s.Current.Point2;
                            (Vec, Vec) vs = GetValidVec(
                                GetIntersectionLSAndSL(LT, RT, v1, v2),
                                GetIntersectionLSAndSL(RT, RB, v1, v2),
                                GetIntersectionLSAndSL(RB, LB, v1, v2),
                                GetIntersectionLSAndSL(LB, LT, v1, v2)
                            );
                            if (s.Selected)
                                dc.DrawLine(Owner.MathToPixelSK(vs.Item1), Owner.MathToPixelSK(vs.Item2), FilledMedian);
                            else if(s.PointerOver)
                                dc.DrawLine(Owner.MathToPixelSK(vs.Item1), Owner.MathToPixelSK(vs.Item2), UnshadyFilledMedian);
                            else
                                dc.DrawLine(Owner.MathToPixelSK(vs.Item1), Owner.MathToPixelSK(vs.Item2), FilledBlack);
                        }
                        break;
                    case GeoLineSegment s:
                        {
                            Vec v1 = s.Current.Point1;
                            Vec v2 = s.Current.Point2;
                            if (s.Selected)
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(v2), FilledMedian);
                            else if (s.PointerOver)
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(v2), UnshadyFilledMedian);
                            else
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(v2), FilledBlack);
                        }
                        break;
                    case GeoHalf h:
                        {
                            Vec v1 = h.Current.Point1;
                            Vec v2 = h.Current.Point2;
                            (Vec, Vec) vs = GetValidVec(
                                GetIntersectionLSAndSL(LT, RT, v1, v2),
                                GetIntersectionLSAndSL(RT, RB, v1, v2),
                                GetIntersectionLSAndSL(RB, LB, v1, v2),
                                GetIntersectionLSAndSL(LB, LT, v1, v2)
                            );
                            Vec p;
                            if (v1.X == v2.X)
                            {
                                if ((vs.Item1.Y - v1.Y) / Math.Sign(v2.Y - v1.Y) > (vs.Item2.Y - v1.Y) / Math.Sign(v2.Y - v1.Y))
                                    p = vs.Item1;
                                else
                                    p = vs.Item2;
                            }
                            else
                            {
                                if ((vs.Item1.X - v1.X) / Math.Sign(v2.X - v1.X) > (vs.Item2.X - v1.X) / Math.Sign(v2.X - v1.X))
                                    p = vs.Item1;
                                else
                                    p = vs.Item2;
                            }
                            if (h.Selected)
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(p), FilledMedian);
                            else if (h.Selected)
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(p), UnshadyFilledMedian);
                            else
                                dc.DrawLine(Owner.MathToPixelSK(v1), Owner.MathToPixelSK(p), FilledBlack);

                        }
                        break;
                    case GeoPolygon polygon:
                        {
                            SKPoint[] ps = new SKPoint[polygon.Locations.Length+1];
                            for (int j = 0; j < ps.Length-1; j++)
                            {
                                ps[j] = Owner.MathToPixelSK(polygon.Locations[j]);
                            }
                            ps[polygon.Locations.Length] = ps[0];
                            SKPath path = new SKPath();
                            path.AddPoly(ps, true);
                            if (polygon.Filled)
                            {
                                if (polygon.Selected)
                                {
                                    dc.DrawPath(path, FilledTpMedian);
                                    dc.DrawPath(path, StrokeMedian);
                                }
                                else if (polygon.PointerOver)
                                {
                                    dc.DrawPath(path, FilledTpMedian);
                                    dc.DrawPath(path, UnshadyStrokeMedian);
                                }
                                else
                                {
                                    dc.DrawPath(path, FilledTranparentGrey);
                                    dc.DrawPath(path, StrokeBlack);
                                }
                            }
                            else
                            {
                                if (polygon.Selected)
                                    dc.DrawPath(path, StrokeMedian);
                                else if (polygon.PointerOver)
                                    dc.DrawPath(path, UnshadyStrokeMedian);
                                else
                                    dc.DrawPath(path, StrokeBlack);

                            }
                        }
                        break;
                    case GeoCircle circle:
                        {
                            CircleStruct cs = circle.InnerCircle;
                            SKPoint pf = Owner.MathToPixelSK(cs.Center);
                            SKSize s = new SKSize((float)(cs.Radius * UnitLength), (float)(cs.Radius * UnitLength));
                            if (circle.Selected)
                                dc.DrawOval(pf, s, StrokeMedian);
                            if (circle.PointerOver)
                                dc.DrawOval(pf, s, UnshadyStrokeMedian);
                            else
                                dc.DrawOval(pf, s, StrokeBlack);
                        }
                        break;
                    case Angle ang:
                        {
                            var angle = ang.AngleData;
                            SKPoint pf = Owner.MathToPixelSK(angle.AnglePoint);
                            double arg1 = (Owner.MathToPixel(angle.Point1).Sub(Owner.MathToPixel(angle.AnglePoint)).Arg() / Math.PI * 180).Mod(360);
                            double arg2 = (Owner.MathToPixel(angle.Point2).Sub(Owner.MathToPixel(angle.AnglePoint)).Arg() / Math.PI * 180).Mod(360);
                            double aa = angle.Angle;
                            double a = arg2 - arg1;
                            a = a.Mod(360);
                            if (a > 180)
                                a -= 360;
                            if (ang.Selected)
                                dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true, StrokeMedian);
                            else if (ang.PointerOver)
                                dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true, UnshadyStrokeMedian);
                            else
                                dc.DrawArc(CreateSKRectWH(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)a, true, StrokeBlack);
                            dc.DrawBubble($"{Math.Abs(aa).ToString("0.00")}°", pf.OffSetBy(2, 2 - 20));
                        }
                        break;
                }
            }
            foreach (var p in Shapes.GetPoints())
            {
                if (p is null)
                    continue;
                if (!p.Visible)
                    continue;
                int index = 0;
                SKPoint loc = Owner.MathToPixelSK(p.Location);
                if (string.IsNullOrEmpty(p.Name))
                    dc.DrawBubble("Point:" + (char)(Shapes.GetIndex(p) + 65), loc.OffSetBy(2, 2 + 20 * index++));
                else
                    dc.DrawBubble("Point:" + (char)(Shapes.GetIndex(p) + 65) + " Name:" + p.Name, loc.OffSetBy(2, 2 + 20 * index++));


                if (p == MovingPoint)
                {
                    dc.DrawOval(loc,new SKSize(4,4), FilledMedian);
                    dc.DrawOval(loc,new SKSize(7,7), StrokeMedian);
                    dc.DrawBubble($"({p.Location.X},{p.Location.Y}) {(p.PointGetter is PointGetter_Movable ? "" : "不可移动")}", loc.OffSetBy(2, 2 + 20 * index++));
                }
                else if (p.Selected)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledMedian);
                    dc.DrawOval(loc, new SKSize(7, 7), StrokeMedian);
                }
                else if (p.PointerOver)
                {
                    dc.DrawOval(loc, new SKSize(4, 4), FilledMedian);
                }
                else
                    dc.DrawOval(loc, new SKSize(3, 3), FilledBlack);
                foreach (var t in p.TextGetters)
                {
                    if (!string.IsNullOrEmpty(t.GetText()))
                        dc.DrawBubble(t.GetText(), loc.OffSetBy(2, 2 + 20 * index++));
                }
            }
            dc.ClipRect(rect);
            dc.Restore();
        }
        public GeoPoint PutPoint(AvaPoint Location)
        {
            DisplayControl disp=(Owner as DisplayControl)!;
            Vec mathcursor = Owner.PixelToMath(Location);
            List<(double,Shape)> shapes = new List<(double,Shape)> ();
            foreach (var s in Shapes)
            {
                if (s is Line || s is Circle)
                {
                    double dist = (s.HitTest(mathcursor)*disp.UnitLength).GetLength();
                    if (dist<5)
                    {
                        shapes.Add((dist,s));
                    }
                }
            }
            (double,Shape)[] ss=shapes.OrderBy(key => key.Item1).ToArray();
            GeoPoint newpoint;
            if (ss.Length == 0)
            {
                newpoint = new GeoPoint(new PointGetter_FromLocation(Owner.PixelToMath(Location)));
            }
            else if (ss.Length == 1)
            {
                Shape shape = ss[0].Item2;
                if (shape is Circle)
                {
                    newpoint = new GeoPoint(new PointGetter_OnCircle((Circle)shape, Owner.PixelToMath(Location)));
                }
                else if (shape is Line)
                {
                    newpoint = new GeoPoint(new PointGetter_OnLine((Line)shape, Owner.PixelToMath(Location)));
                }
                else
                    newpoint = new GeoPoint(new PointGetter_FromLocation(Owner.PixelToMath(Location)));
            }
            else
            {
                Shape s1 = ss[0].Item2;
                Shape s2 = ss[1].Item2;
                if (s1 is Line && s2 is Line)
                    newpoint = new GeoPoint(new PointGetter_FromTwoLine(s1 as Line, s2 as Line));
                else if (s1 is Line l1 && s2 is Circle c1)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l1.Current.Point1, l1.Current.Point2, c1.InnerCircle.Center, c1.InnerCircle.Radius);
                    newpoint = new GeoPoint(new PointGetter_FromLineAndCircle(l1, c1, (Owner.MathToPixel(v1) - Location).GetLength() < (Owner.MathToPixel(v2) - Location).GetLength()));
                }
                else if (s1 is Circle c2 && s2 is Line l2)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l2.Current.Point1, l2.Current.Point2, c2.InnerCircle.Center, c2.InnerCircle.Radius);
                    newpoint = new GeoPoint(new PointGetter_FromLineAndCircle(l2, c2, (Owner.MathToPixel(v1) - Location).GetLength() < (Owner.MathToPixel(v2) - Location).GetLength()));
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

        public bool TryGetShape<T>(AvaPoint Location, out T shape) where T : Shape
        {
            shape=GetShape<T>(Location);
            return shape != null;
        }
        public T? GetShape<T>(AvaPoint Location) where T : GeoShape
        {
            DisplayControl disp=(Owner as DisplayControl)!;
            Vec v = Owner!.PixelToMath(Location);
            double distance = Double.PositiveInfinity;
            T? target = null;
            foreach (var s in Shapes)
            {
                if(!s.Visible)
                    continue;
                if (s is T tar)
                {
                    double dis = (tar.HitTest(v)*disp.UnitLength).GetLength();
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
}
