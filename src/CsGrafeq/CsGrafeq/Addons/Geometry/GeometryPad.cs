using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using CsGrafeq.Base;
using CsGrafeq.Geometry.Shapes.Getter;
using CsGrafeq.Geometry.Shapes;
using static CsGrafeq.Base.Values;
using static CsGrafeq.Geometry.GeometryMath;
using System.Runtime.InteropServices.ComTypes;
using CsGrafeq.Addons.Implicit;
using System.Reflection.Emit;
using CsGrafeq.Addons.Geometry;
using CsGrafeq.Addons;
using Microsoft.VisualBasic;


namespace CsGrafeq.Geometry
{
    public class GeometryPad:Addon
    {
        private ShapeList Shapes=new ShapeList();
        public readonly DistinctList<Point> SelectedPoints = new DistinctList<Point>();
        public readonly DistinctList<Circle> SelectedCircles= new DistinctList<Circle>();
        public readonly DistinctList<Line> SelectedLines= new DistinctList<Line>();
        internal string GeoPadAction="Move";
        public GeometryPad()
        {
            Enabled = true;
            _RenderMode = RenderMode.All;
            OpControl = new Addons.Geometry.OpControl(this);
            
        }
        Point MovingPoint = null;
        System.Drawing.Point DownPoint=new System.Drawing.Point(-1,-1);
        System.Drawing.Point MovePoint;
        bool mousedown=false;
        protected override bool OnMouseDown(MouseEventArgs e)
        {
            mousedown=true;
            DownPoint = e.Location;
            if (GeoPadAction != "Choose")
            {
                foreach (Shape shape in Shapes)
                {
                    if (shape is Point point)
                        if (Math.Pow(MathToPixelX(point.Location.X) - e.X, 2) + Math.Pow(MathToPixelY(point.Location.Y) - e.Y, 2) < 25 && shape.Visible)
                        {
                            MovingPoint = point;
                            return Intercept;
                        }
                }
            }
            else
            {
                ClearSelect();
            }
            MovingPoint = null;
            return DoNext;
        }
        protected override bool OnMouseMove(AddonMouseMoveEventArgs e)
        {
            MovePoint=e.MouseEventArgs.Location;
            if (GeoPadAction == "Choose"&&DownPoint!=new System.Drawing.Point(-1,-1))
            {
                AskForRender();
                return Intercept;
            }
            if (MovingPoint == null)
                return DoNext;
            if(MovePoint!=DownPoint)
                ClearSelect();
            (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(new Vec(PixelToMathX(e.X),PixelToMathY(e.Y)));
            MovingPoint.RefreshValues();
            AskForRender();
            return Intercept;
        }
        protected override bool OnMouseUp(MouseEventArgs e)
        {
            mousedown = false;
            if (GeoPadAction == "Choose")
            {
                AskForRender();
                RefreshOwnerArguments();
                var rect = RegulateRectangle(new System.Drawing.Rectangle(DownPoint, new System.Drawing.Size(MovePoint.X - DownPoint.X, MovePoint.Y - DownPoint.Y)));
                var mathrectloc = PixelToMathForPoint(new System.Drawing.Point(rect.Left,rect.Top+rect.Height));
                var mathrectsize = new Vec(((double)rect.Width)/UnitLengthX,((double)rect.Height)/UnitLengthY);
                DownPoint = new System.Drawing.Point(-1, -1);
                foreach(var s in Shapes)
                {
                    switch (s)
                    {
                        case Point p:
                            {
                                Vec v = p.Location - mathrectloc;
                                if (InRange(0, mathrectsize.X, v.X) && InRange(0, mathrectsize.Y, v.Y))
                                    SelectedPoints.Add(p);
                            }
                            break;
                        case Line l:
                            {
                                Vec v1 = l.Point1;
                                Vec v2 = l.Point2;
                                Vec LT = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Bottom));
                                Vec RT = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Bottom));
                                Vec RB = new Vec(PixelToMathX(rect.Right), PixelToMathY(rect.Top));
                                Vec LB = new Vec(PixelToMathX(rect.Left), PixelToMathY(rect.Top));
                                (Vec, Vec) vs = GetValidVec(
                                    GetIntersectionLSAndSL(LT, RT, v1, v2),
                                    GetIntersectionLSAndSL(RT, RB, v1, v2),
                                    GetIntersectionLSAndSL(RB, LB, v1, v2),
                                    GetIntersectionLSAndSL(LB, LT, v1, v2)
                                );
                                vs.Item1 = l.CheckIsValid(vs.Item1) ? vs.Item1 : Vec.InvalidVec;
                                vs.Item2 = l.CheckIsValid(vs.Item2) ? vs.Item2 : Vec.InvalidVec;
                                
                                if (!(vs.Item1.IsInValidVec() && vs.Item2.IsInValidVec()))
                                    SelectedLines.Add(l);
                                else if (InRange(mathrectloc.X, mathrectloc.X + mathrectsize.X, ((l.Point1 + l.Point2) / 2).X) && InRange(mathrectloc.Y, mathrectloc.Y + mathrectsize.Y, ((l.Point1 + l.Point2) / 2).Y))
                                {
                                    SelectedLines.Add(l);
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
                                    SelectedCircles.Add(c);
                            }
                            break;
                    }
                }
                return Intercept;
            }
            if (MovingPoint == null)
                return DoNext;
            if(e.Location!=DownPoint)
                ClearSelect();
            (MovingPoint.PointGetter as PointGetter_Movable)?.SetControlPoint(MovingPoint.Location);
            MovingPoint = null;
            AskForRender();
            DownPoint = new System.Drawing.Point(-1, -1);
            return Intercept;
        }
        protected override bool OnMouseClick(MouseEventArgs e)
        {
            if (GeoPadAction == "PutPoint")
            {
                ClearSelect();
                PutPoint(e.Location);
                AskForRender();
                return Intercept;
            }
            Point first=SelectedPoints.Count>0?SelectedPoints[0]:null;
            if (TryGetPoint(e.Location,out Point point))
            {
                if(SelectedPoints.Contains(point))
                    SelectedPoints.Remove(point);
                else
                    SelectedPoints.Add(point);
            }else if(TryGetLine(e.Location,out Line line))
            {
                if(SelectedLines.Contains(line))
                    SelectedLines.Remove(line);
                else
                    SelectedLines.Add(line);
            }
            else if(TryGetCircle(e.Location,out Circle circle))
            {
                if(SelectedCircles.Contains(circle))
                    SelectedCircles.Remove(circle);
                else
                    SelectedCircles.Add(circle);
            }else if (GeoPadAction != "Move"&&GeoPadAction!="Choose")
            {
                SelectedPoints.Add(PutPoint(e.Location));
            }
            int plen = SelectedPoints.Count;
            int llen=SelectedLines.Count;
            int clen=SelectedCircles.Count;
            switch (GeoPadAction)
            {
                case "Choose":
                case "Move":
                    {
                    }
                    break;
                case "PutPoint":
                    {
                        ClearSelect();
                    }
                    break;
                case "MiddlePoint":
                    {
                        if ((plen == 2)&&(llen==0)&&(clen==0))
                        {
                            Point newpoint = new Point(new PointGetter_MiddlePoint(SelectedPoints[0], SelectedPoints[1]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        else if (plen==0&&llen==1&&clen==0)
                        {
                            if (SelectedLines[0] is LineSegment)
                            {
                                Point newpoint= new Point(new PointGetter_MiddlePoint(
                                    new Point(new PointGetter_PointOfLine(SelectedLines[0],1)),
                                    new Point(new PointGetter_PointOfLine(SelectedLines[0], 2))
                                ));
                                AddShape(newpoint);
                                SelectedLines.Clear();
                            }
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if(plen>=2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "MedianCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_MedianCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "InCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_InCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "OutCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_OutCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "OrthoCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_OrthoCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "AxialSymmetryPoint":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_AxialSymmetryPoint(SelectedPoints[0], SelectedLines[0]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();

                        }
                        if(plen>1)
                            SelectedPoints.Clear();
                        if(llen>1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "NearestPoint":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_NearestPointOnLine( SelectedLines[0], SelectedPoints[0]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "StraightLine":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new StraightLine(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "LineSegment":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new LineSegment(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "HalfLine":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new HalfLine(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "VerticalLine":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            var line = new VerticalLine(new PointGetter_FromPoint(SelectedPoints[0]), SelectedLines[0]);
                            AddShape(line);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "ParallelLine":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            var line = new ParallelLine(new PointGetter_FromPoint(SelectedPoints[0]), SelectedLines[0]);
                            AddShape(line);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "PerpendicularBisector":
                    {
                        if ((plen == 2) && (llen == 0) && (clen == 0))
                        {
                            var line = new PerpendicularBisector(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint((SelectedPoints[1])));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        else if (plen == 0 && llen == 1 && clen == 0)
                        {
                            if (SelectedLines[0] is LineSegment)
                            {
                                var line = new PerpendicularBisector(SelectedLines[0] as LineSegment);
                                AddShape(line);
                                SelectedLines.Clear();
                            }
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "ThreePointCircle":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Circle circle = new Circle(new CircleGetter_FromThreePoint(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(circle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "TwoPointCircle":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            Circle circle = new Circle(new CircleGetter_FromCenterAndPoint(SelectedPoints[0], SelectedPoints[1]));
                            AddShape(circle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "TextBoxOnPlot":
                    {
                        if(plen == 1 &&llen== 0 && clen == 0)
                        {
                            string text = Interaction.InputBox("文本内容");
                            if (!string.IsNullOrEmpty(text))
                            {
                                SelectedPoints[0].TextGetters.Add(new TextGetter_FromString(text));
                            }
                        }
                        ClearSelect();
                    }
                    break;
                case "Polygon":
                    {
                        if (llen == 0 && clen == 0&&plen>2)
                        {
                            if (first == GetPoint(e.Location))
                            {
                                Polygon polygon= new Polygon(SelectedPoints.ToArray().ToPointGetters(),new PointGetter_FromPoint(first));
                                AddShape(polygon);
                                SelectedPoints.Clear();
                            }
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "Angle":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Angle angle = new Angle(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]);
                            AddShape(angle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                default:
                    MessageBox.Show(GeoPadAction);
                    return DoNext;
            }
            AskForRender();
            return Intercept;
        }
        //部分逻辑不同 难以重用
        internal void CreateShapeFromSelects()
        {
            int plen = SelectedPoints.Count;
            int llen = SelectedLines.Count;
            int clen = SelectedCircles.Count;
            switch (GeoPadAction)
            {
                case "Choose":
                case "PutPoint":
                case "Move":
                case "TextBoxOnPixel":
                    ClearSelect();
                    break;
                case "MiddlePoint":
                    {
                        if ((plen == 2) && (llen == 0) && (clen == 0))
                        {
                            Point newpoint = new Point(new PointGetter_MiddlePoint(SelectedPoints[0], SelectedPoints[1]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        else if (plen == 0 && llen == 1 && clen == 0)
                        {
                            if (SelectedLines[0] is LineSegment)
                            {
                                Point newpoint = new Point(new PointGetter_MiddlePoint(
                                    new Point(new PointGetter_PointOfLine(SelectedLines[0], 1)),
                                    new Point(new PointGetter_PointOfLine(SelectedLines[0], 2))
                                ));
                                AddShape(newpoint);
                                SelectedLines.Clear();
                            }
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen >= 2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "MedianCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_MedianCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "InCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_InCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "OutCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_OutCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "OrthoCenter":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_OrthoCenter(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "AxialSymmetryPoint":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_AxialSymmetryPoint(SelectedPoints[0], SelectedLines[0]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();

                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "NearestPoint":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            Point newpoint = new Point(new PointGetter_NearestPointOnLine(SelectedLines[0], SelectedPoints[0]));
                            AddShape(newpoint);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "StraightLine":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new StraightLine(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "LineSegment":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new LineSegment(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "HalfLine":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            var line = new HalfLine(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint(SelectedPoints[1]));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        if (plen > 2)
                            SelectedPoints.Clear();
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "VerticalLine":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            var line = new VerticalLine(new PointGetter_FromPoint(SelectedPoints[0]), SelectedLines[0]);
                            AddShape(line);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "ParallelLine":
                    {
                        if (plen == 1 && llen == 1 && clen == 0)
                        {
                            var line = new ParallelLine(new PointGetter_FromPoint(SelectedPoints[0]), SelectedLines[0]);
                            AddShape(line);
                            SelectedPoints.Clear();
                            SelectedLines.Clear();
                        }
                        if (plen > 1)
                            SelectedPoints.Clear();
                        if (llen > 1)
                            SelectedLines.Clear();
                        SelectedCircles.Clear();
                    }
                    break;
                case "PerpendicularBisector":
                    {
                        if ((plen == 2) && (llen == 0) && (clen == 0))
                        {
                            var line = new PerpendicularBisector(new PointGetter_FromPoint(SelectedPoints[0]), new PointGetter_FromPoint((SelectedPoints[1])));
                            AddShape(line);
                            SelectedPoints.Clear();
                        }
                        else if (plen == 0 && llen == 1 && clen == 0)
                        {
                            if (SelectedLines[0] is LineSegment)
                            {
                                var line = new PerpendicularBisector(SelectedLines[0] as LineSegment);
                                AddShape(line);
                                SelectedLines.Clear();
                            }
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "ThreePointCircle":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Circle circle = new Circle(new CircleGetter_FromThreePoint(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]));
                            AddShape(circle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                case "TwoPointCircle":
                    {
                        if (plen == 2 && llen == 0 && clen == 0)
                        {
                            Circle circle = new Circle(new CircleGetter_FromCenterAndPoint(SelectedPoints[0], SelectedPoints[1]));
                            AddShape(circle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 2)
                            SelectedPoints.Clear();
                    }
                    break;
                case "TextBoxOnPlot":
                    {
                        if (plen == 1 && llen == 0 && clen == 0)
                        {
                            string text = Interaction.InputBox("文本内容");
                            if (!string.IsNullOrEmpty(text))
                            {
                                SelectedPoints[0].TextGetters.Add(new TextGetter_FromString(text));
                            }
                        }
                        ClearSelect();
                    }
                    break;
                case "Polygon":
                    {
                        if (llen == 0 && clen == 0 && plen >= 3)
                        {
                            Polygon polygon = new Polygon(SelectedPoints.ToArray().ToPointGetters());
                            AddShape(polygon);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                    }
                    break;
                case "Angle":
                    {
                        if (plen == 3 && llen == 0 && clen == 0)
                        {
                            Angle angle = new Angle(SelectedPoints[0], SelectedPoints[1], SelectedPoints[2]);
                            AddShape(angle);
                            SelectedPoints.Clear();
                        }
                        SelectedCircles.Clear();
                        SelectedLines.Clear();
                        if (plen > 3)
                            SelectedPoints.Clear();
                    }
                    break;
                default:
                    MessageBox.Show(GeoPadAction);
                    break;
            }
        }
        private void ClearSelect()
        {
            SelectedCircles.Clear();
            SelectedLines.Clear();
            SelectedPoints.Clear();
        }
        protected override bool OnMouseDoubleClick(MouseEventArgs e)
        {
            SelectedCircles.Clear();
            SelectedLines.Clear();
            SelectedPoints.Clear();
            Point point;
            if(TryGetPoint(e.Location,out point)&&point.PointGetter is PointGetter_Movable)
            {
                PropertyDialog dlg = new PropertyDialog(new Dictionary<string, (string init, Type t)>() {
                        { "名字:",(point.Name,typeof(string))},
                        { "X:",(point.Location.X.ToString(),typeof(double))},
                        { "Y:",(point.Location.Y.ToString(),typeof(double))}
                    });
                dlg.ShowDialog();
                if (dlg.OK)
                {
                    var dic = dlg.returndic;
                    point.Name = dic["名字:"];
                    (point.PointGetter as PointGetter_Movable).SetControlPoint(new Vec(double.Parse(dic["X:"]), double.Parse(dic["Y:"])));
                    point.RefreshValues();
                    AskForRender();
                }
                return DoNext;
            }
            return Intercept;
        }
        protected override bool OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    {
                        if(SelectedCircles.Count+SelectedLines.Count+SelectedPoints.Count>2)
                            if(MessageBox.Show("是否删除？","",MessageBoxButtons.YesNo)!=DialogResult.Yes)
                                break;
                        foreach (var i in SelectedPoints)
                            Shapes.Remove(i);
                        SelectedPoints.Clear();
                        foreach (var i in SelectedCircles)
                            Shapes.Remove(i);
                        SelectedCircles.Clear();
                        foreach (var i in SelectedLines)
                            Shapes.Remove(i);
                        SelectedLines.Clear();
                        AskForRender();
                    }
                    break;
            }
            return DoNext ;
        }
        private System.Drawing.Font font = new System.Drawing.Font("Microsoft Yahei", 8);
        private System.Drawing.SolidBrush GTBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(70, System.Drawing.Color.Gray));
        protected override void Render(System.Drawing.Graphics g, System.Drawing.Rectangle r)
        {
            UnitLengthX = OwnerArguments.GetUX();
            UnitLengthY = OwnerArguments.GetUY();
            Vec LT= new Vec(PixelToMathX(r.Left),PixelToMathY(r.Bottom));
            Vec RT= new Vec(PixelToMathX(r.Right), PixelToMathY(r.Bottom));
            Vec RB= new Vec(PixelToMathX(r.Right), PixelToMathY(r.Top));
            Vec LB= new Vec(PixelToMathX(r.Left), PixelToMathY(r.Top));
            for(int i = 0; i < Shapes.Count; i++)
            {
                Shape shape = Shapes[i];
                if (!shape.Visible)
                    continue;
                switch (shape)
                {
                    case Point p:
                        {
                            int index = 0;
                            if (string.IsNullOrEmpty(shape.Name))
                                g.DrawBubblePopup("Point:" + (char)(Shapes.GetIndex(shape as Point) + 65), font, MathToPixelForPoint(p.Location).OffSetBy(2, 2+20*(index++)));
                            else
                                g.DrawBubblePopup("Point:" + (char)(Shapes.GetIndex(shape as Point) + 65) + " Name:" + shape.Name, font, MathToPixelForPoint(p.Location).OffSetBy(2, 2+20*(index++)));
                            if (SelectedPoints.Contains(p))
                            {
                                g.FillEllipse(Brush_Red, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 3, (float)MathToPixelY(p.Location.Y) - 3, 6, 6));
                            }
                            else if (p == MovingPoint)
                            {
                                g.FillEllipse(Brush_Blue, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 3, (float)MathToPixelY(p.Location.Y) - 3, 6, 6));
                                g.DrawBubblePopup($"({p.Location.X},{p.Location.Y}) {((p.PointGetter is PointGetter_Movable) ? "" : "不可移动")}", font, MathToPixelForPoint(p.Location).OffSetBy(2, 2 + 20*(index++)));
                            }
                            else
                                g.FillEllipse(Brush_Black, new System.Drawing.RectangleF((float)MathToPixelX(p.Location.X) - 2, (float)MathToPixelY(p.Location.Y) - 2, 4, 4));
                            foreach(var t in p.TextGetters)
                            {
                                g.DrawBubblePopup(t.GetText(), font, MathToPixelForPoint(p.Location).OffSetBy(2, 2 + 20 * (index++)));
                            }
                        }
                        break;
                    case StraightLine s:
                        {
                            Vec v1 = s.Point1;
                            Vec v2 = s.Point2;
                            (Vec, Vec) vs = GetValidVec(
                                GetIntersectionLSAndSL(LT, RT, v1, v2),
                                GetIntersectionLSAndSL(RT, RB, v1, v2),
                                GetIntersectionLSAndSL(RB, LB, v1, v2),
                                GetIntersectionLSAndSL(LB, LT, v1, v2)
                            );
                            vs.Item1.X = MathToPixelX(vs.Item1.X);
                            vs.Item2.X = MathToPixelX(vs.Item2.X);
                            vs.Item1.Y = MathToPixelY(vs.Item1.Y);
                            vs.Item2.Y = MathToPixelY(vs.Item2.Y);
                            if (SelectedLines.Contains(s))
                                g.DrawLine(Pen_Red, vs.Item1.ToPointF(), vs.Item2.ToPointF());
                            else
                                g.DrawLine(Pen_Black, vs.Item1.ToPointF(), vs.Item2.ToPointF());
                        }
                        break;
                    case LineSegment s:
                        {
                            Vec v1 = s.Point1;
                            Vec v2 = s.Point2;

                            v1.X = MathToPixelX(v1.X);
                            v2.X = MathToPixelX(v2.X);
                            v1.Y = MathToPixelY(v1.Y);
                            v2.Y = MathToPixelY(v2.Y);
                            if (SelectedLines.Contains(s))
                                g.DrawLine(Pen_Red, v1.ToPointF(), v2.ToPointF());
                            else
                                g.DrawLine(Pen_Black, v1.ToPointF(), v2.ToPointF());
                        }
                        break;
                    case HalfLine h:
                        {
                            Vec v1 = h.Point1;
                            Vec v2 = h.Point2;
                            (Vec, Vec) vs = GetValidVec(
                                GetIntersectionLSAndSL(LT, RT, v1, v2),
                                GetIntersectionLSAndSL(RT, RB, v1, v2),
                                GetIntersectionLSAndSL(RB, LB, v1, v2),
                                GetIntersectionLSAndSL(LB, LT, v1, v2)
                            );
                            Vec p;
                            if (v1.X == v2.X)
                            {
                                if ((vs.Item1.Y - v1.Y)/Math.Sign(v2.Y - v1.Y)> (vs.Item2.Y - v1.Y) / Math.Sign(v2.Y - v1.Y))
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
                            Console.WriteLine(p.ToString()); 
                            v1.X = MathToPixelX(v1.X);
                            p.X = MathToPixelX(p.X);
                            v1.Y = MathToPixelY(v1.Y);
                            p.Y = MathToPixelY(p.Y);
                            if (SelectedLines.Contains(h))
                                g.DrawLine(Pen_Red, v1.ToPointF(), p.ToPointF());
                            else
                                g.DrawLine(Pen_Black, v1.ToPointF(), p.ToPointF());

                        }
                        break;
                    case Polygon polygon:
                        {
                            System.Drawing.PointF[] ps = new System.Drawing.PointF[polygon.Locations.Length];
                            for (int j = 0; j < ps.Length; j++)
                            {
                                ps[j] = MathToPixelForPointF(polygon.Locations[j]);
                            }
                            if (polygon.Fill)
                            {
                                g.FillPolygon(GTBrush, ps);
                            }
                            else
                            {
                                g.DrawPolygon(Pen_Black, ps);
                            }
                        }
                        break;
                    case Circle circle:
                        {
                            CircleStruct cs = circle.InnerCircle;
                            System.Drawing.PointF pf = MathToPixelForPointF(new Vec(cs.Center.X - cs.Radius, cs.Center.Y + cs.Radius));
                            System.Drawing.SizeF s = new System.Drawing.SizeF((float)(2 * cs.Radius * UnitLengthX), (float)(2 * cs.Radius * UnitLengthY));
                            if (SelectedCircles.Contains(circle))
                                g.DrawEllipse(Pen_Red, new System.Drawing.RectangleF(pf, s));
                            else
                                g.DrawEllipse(Pen_Black, new System.Drawing.RectangleF(pf, s));
                        }
                        break;
                    case Angle angle:
                        {
                            System.Drawing.PointF pf = MathToPixelForPointF(angle.AnglePoint);
                            double arg1 = ((MathToPixelForPoint(angle.Point1).Sub(MathToPixelForPoint(angle.AnglePoint))).Arg() / Math.PI * 180).Mod(360);
                            double arg2 = ((MathToPixelForPoint(angle.Point2).Sub(MathToPixelForPoint(angle.AnglePoint))).Arg() / Math.PI * 180).Mod(360);
                            double aa = (((angle.Point2 - angle.AnglePoint).Arg2() - (angle.Point1 - angle.AnglePoint).Arg2())) / Math.PI * 180;
                            aa = aa.Mod(360);
                            if (aa > 180)
                                aa = 360 - aa;
                            double a = arg2 - arg1;
                            a = a.Mod(360);
                            if (a > 180)
                                a -= 360;
                            g.DrawPie(Pen_Black, new System.Drawing.RectangleF(pf.X - 20, pf.Y - 20, 40, 40), (float)arg1, (float)(a));
                            g.DrawBubblePopup($"{Math.Abs(aa).ToString("0.00")}°", font, MathToPixelForPoint(angle.AnglePoint).OffSetBy(2, 2 - 20));
                        }
                        break;
                }
            }
            if (GeoPadAction == "Choose"&&mousedown)
            {
                var rect =RegulateRectangle( new System.Drawing.Rectangle(DownPoint, new System.Drawing.Size(MovePoint.X - DownPoint.X, MovePoint.Y - DownPoint.Y)));
                g.DrawRectangle(Pen_Blue,rect);
                g.FillRectangle(Brush_Blue_Trans,rect);
            }
        }
        private System.Drawing.SolidBrush Brush_Blue_Trans = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb( 70, Color_Blue));
        private System.Drawing.Point MathToPixelForPoint(Vec vec)
        {
            return new System.Drawing.Point((int)MathToPixelX(vec.X), (int)MathToPixelY(vec.Y));
        }
        private Vec PixelToMathForPoint(System.Drawing.Point p)
        {
            return new Vec(PixelToMathX(p.X), PixelToMathY(p.Y));
        }
        private System.Drawing.PointF MathToPixelForPointF(Vec vec)
        {
            return new System.Drawing.PointF((float)MathToPixelX(vec.X), (float)MathToPixelY(vec.Y));
        }
        private class ShapeList : List<Shape>
        {
            private Point[] PointCounter = new Point[100];
            public ShapeList()
            {
            }
            public new void Add(Shape shape)
            {
                if (Contains(shape))
                    return;
                base.Add(shape);
                if (shape is Point)
                {
                    for(int i = 0; i < 100; i++)
                    {
                        if (PointCounter[i] == null)
                        {
                            PointCounter[i]=shape as Point;
                            return;
                        }
                    }
                    throw new IndexOutOfRangeException();
                }
            }
            public new void Remove(Shape shape)
            {
                if (!Contains(shape))
                    return;
                base.Remove(shape);
                foreach(var i in shape.SubShapes)
                    Remove(i);
                if(shape is Point p)
                {
                    PointCounter[GetIndex(p)] = null;
                }
            }
            public new void AddRange(IEnumerable<Shape> shapes)
            {
                foreach (Shape shape in shapes)
                {
                    Add(shape);
                }
            }
            
            public int GetIndex(Point p)
            {
                for(int i = 0; i < 100; i++)
                {
                    if (PointCounter[i]==p)
                        return i;
                }
                return -1;
            }
            public Point FromIndex(int index)
            {
                return PointCounter[index];
            }
        }
        public Point PutPoint(System.Drawing.Point Location)
        {
            RefreshOwnerArguments();
            Vec cursor = new Vec(Location.X,Location.Y);
            Vec mathcursor = PixelToMathForPoint(Location);
            List<(double,Shape)> shapes = new List<(double,Shape)> ();
            foreach (var s in Shapes)
            {
                if (s is Line Line)
                {
                    Vec v1 = Line.Point1;
                    Vec v2 = Line.Point2;
                    double dx = v2.X - v1.X;
                    double dy = v2.Y - v1.Y;
                    double t = ((mathcursor.X - v1.X) * dx + (mathcursor.Y - v1.Y) * dy) / (dx * dx + dy * dy);
                    Vec mathnv = new Vec(v1.X + t * dx, v1.Y + t * dy);
                    Vec pixelnv = new Vec(MathToPixelForPoint(mathnv));
                    if (!Line.CheckIsValid(mathnv))
                        continue;
                    double distance = (pixelnv - cursor).GetLength();
                    if (distance<5)
                    {
                        shapes.Add((distance,s));
                    }
                }else if(s is Circle c)
                {
                    Vec vvv = (mathcursor - c.InnerCircle.Center)- (mathcursor - c.InnerCircle.Center).Unit()* c.InnerCircle.Radius;
                    double distance = new Vec(vvv.X * UnitLengthX , vvv.Y * UnitLengthY).GetLength();
                    if (distance <5)
                    {
                        shapes.Add((distance, s));
                    }
                }
            }
            (double,Shape)[] ss=shapes.OrderBy(key => key.Item1).ToArray();
            Point newp;
            if (ss.Length == 0)
            {
                newp = new Point(new PointGetter_FromLocation(PixelToMathForPoint(Location)));
            }
            else if (ss.Length == 1)
            {
                Shape shape = ss[0].Item2;
                if (shape is Circle)
                {
                    newp = new Point(new PointGetter_OnCircle((Circle)shape, PixelToMathForPoint(Location)));
                }
                else if (shape is Line)
                {
                    newp = new Point(new PointGetter_OnLine((Line)shape, PixelToMathForPoint(Location)));
                }
                else
                    throw new Exception();
            }
            else
            {
                Shape s1 = ss[0].Item2;
                Shape s2 = ss[1].Item2;
                if (s1 is Line && s2 is Line)
                    newp = new Point(new PointGetter_FromTwoLine(s1 as Line, s2 as Line));
                else if (s1 is Line l1 && s2 is Circle c1)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l1.Point1, l1.Point2, c1.InnerCircle.Center, c1.InnerCircle.Radius);
                    v1 = new Vec(MathToPixelForPoint(v1));
                    v2 = new Vec(MathToPixelForPoint(v2));
                    newp = new Point(new PointGetter_FromLineAndCircle(l1, c1, (v1 - cursor).GetLength() < (v2 - cursor).GetLength()));
                }
                else if (s1 is Circle c2 && s2 is Line l2)
                {
                    Vec v1, v2;
                    (v1, v2) = GetPointFromCircleAndLine(l2.Point1, l2.Point2, c2.InnerCircle.Center, c2.InnerCircle.Radius);
                    v1 = new Vec(MathToPixelForPoint(v1));
                    v2 = new Vec(MathToPixelForPoint(v2));
                    newp = new Point(new PointGetter_FromLineAndCircle(l2, c2, (v1 - cursor).GetLength() < (v2 -cursor).GetLength()));
                }
                else
                {
                    //暂不支持
                    return null;
                }
            }
            Shapes.Add(newp);
            return newp;
        }
        public bool TryGetPoint(System.Drawing.Point Location,out Point point)
        {
            point=GetPoint(Location);
            return point != null;
        }
        public Point GetPoint(System.Drawing.Point Location)
        {
            Vec v = PixelToMathForPoint(Location);
            double distance = 1000;
            Point p = null;
            foreach (var s in Shapes)
            {
                if (s is Point)
                {
                    Point pp = (Point)s;
                    double dis = (MathToPixelForPoint(pp.Location).Sub(Location).GetLength());
                    if (dis < 5 && dis < distance)
                    {
                        distance = dis;
                        p = pp;
                    }
                }
            }
            return p;
        }
        public bool TryGetLine(System.Drawing.Point Location, out Line line)
        {
            line = GetLine(Location);
            return line != null;
        }
        public Line GetLine(System.Drawing.Point Location)
        {
            Vec v = PixelToMathForPoint(Location);
            double distance = 1000;
            Line p = null;
            foreach (var s in Shapes)
            {
                if (!(s is Line)) { continue; }
                Line Line = (Line)s;
                Vec v1 = Line.Point1;
                Vec v2 = Line.Point2;
                double dx = v2.X - v1.X;
                double dy = v2.Y - v1.Y;
                double t = ((v.X - v1.X) * dx + (v.Y - v1.Y) * dy) / (dx * dx + dy * dy);
                Vec nv1 = new Vec(v1.X + t * dx, v1.Y + t * dy);
                var nv = MathToPixelForPoint(nv1);
                if (!Line.CheckIsValid(nv1))
                    continue;
                double dis = nv.Sub(Location).GetLength();
                if (dis < 5&&dis<distance)
                {
                    p = Line;
                    distance= dis;
                }
            }
            return p;
        }
        public bool TryGetCircle(System.Drawing.Point Location, out Circle circle)
        {
            circle = GetCircle(Location);
            return circle != null;
        }
        public Circle GetCircle(System.Drawing.Point p)
        {
            Vec v=PixelToMathForPoint(p);
            double distance = 1000;
            Circle res = null;
            foreach (var s in Shapes)
            {
                if (!(s is Circle)) continue;
                Circle c = s as Circle;
                double dis = (MathToPixelForPoint((v - c.InnerCircle.Center).Unit()*c.InnerCircle.Radius).Sub(p)).GetLength();
                if (dis < 5 && dis < distance)
                {
                    res = c;
                    distance = dis;
                }

            }
            return res;
        }
        public void AddShape(Shape s)
        {
            Shapes.Add(s);
        }
    }
    
    internal static partial class GeometryMath
    {
        public static System.Drawing.Rectangle RegulateRectangle(System.Drawing.Rectangle rectangle)
        {
            var size= rectangle.Size;
            var loc= rectangle.Location;
            if(size.Width<0)
                loc.X+=size.Width;
            if(size.Height<0)
                loc.Y+=size.Height;
            size.Width = Math.Abs(size.Width);
            size.Height = Math.Abs(size.Height);
            return new System.Drawing.Rectangle(loc,size);
        }
        public static System.Drawing.Point Sub(this System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return new System.Drawing.Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static double GetLength(this System.Drawing.Point p1)
        {
            return Math.Sqrt(p1.X*p1.X+p1.Y*p1.Y);
        }
        public static double Arg(this System.Drawing.Point p)
        {
            return Math.Atan2(p.Y,p.X);
        }
        public static (Vec,Vec) GetPointFromCircleAndLine(Vec v1,Vec v2,Vec cp,double radius)
        {
            double dx = v2.X - v1.X;
            double dy = v2.Y - v1.Y;
            double t = ((cp.X - v1.X) * dx + (cp.Y - v1.Y) * dy) / (dx * dx + dy * dy);
            Vec nv = new Vec(v1.X + t * dx, v1.Y + t * dy);
            Vec m = new Vec(dx, dy).Unit() * Math.Sqrt(radius * radius - Math.Pow((cp - nv).GetLength(), 2));
            v1 = nv - m;
            v2 = nv + m;
            return (v1, v2);
        }
        public static (Vec, Vec) GetValidVec(Vec v1, Vec v2, Vec v3, Vec v4)
        {
            Vec[] vs = new Vec[4] { v1, v2, v3, v4 };
            Vec[] vs2 = new Vec[4];
            int i = 0;
            foreach (var v in vs)
            {
                if (v.IsInValidVec())
                    continue;
                vs2[i] = v;
                i++;
            }
            if (i == 2)
                return (vs2[0], vs2[1]);
            if (i == 0)
                return (Vec.InvalidVec, Vec.InvalidVec);
            return (vs2[0], vs2[2]);

        }
        public static Vec SolveFunction(double a, double b, double c, double d, double e, double f)
        {
            double det = a * e - b * d;
            if (det == 0)
                return Vec.InvalidVec;
            double[,] mat = new double[2,2];
            mat[0, 0] = e / det;
            mat[0, 1] = -b / det;
            mat[1, 0] = -d / det;
            mat[1, 1] = a / det;
            double x = mat[0, 0] * c + mat[0, 1] * f;
            double y = mat[1, 0] * c + mat[1, 1] * f;
            return new Vec(x, y);
        }
        public static PointGetter[] ToPointGetters(this Point[] ps)
        {
            PointGetter[] points = new PointGetter[ps.Length];
            for(int i = 0; i < ps.Length; i++)
            {
                points[i] = new PointGetter_FromPoint(ps[i]);
            }
            return points;
        }
    }
}
