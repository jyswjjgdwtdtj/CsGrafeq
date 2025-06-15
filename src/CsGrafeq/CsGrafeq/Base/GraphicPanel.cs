using CsGrafeq.Implicit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.Base.Values;
using System.Resources;
using CsGrafeq.Geometry;
using CsGrafeq.Geometry.Shapes.Getter;
using CsGrafeq.Geometry.Shapes;
using System.Reflection;
using Microsoft.VisualBasic;


namespace CsGrafeq.Base
{
    public partial class GraphicPanel : UserControl
    {
        private DisplayerBase Player=new DisplayerBase();
        private OperationPanel OpPanel=new OperationPanel();
        private DragPanel DgPanel=new DragPanel();
        private GeometryPad GP;
        private WhatToDraw wd=WhatToDraw.None;

        public GraphicPanel()
        {
            InitializeComponent();
            SuspendLayout();
            Player.Dock = DockStyle.Fill;
            Player.PanelWidth = 200;
            OpPanel.Location = System.Drawing.Point.Empty;
            OpPanel.Width = Player.PanelWidth;
            OpPanel.Height = Height;
            OpPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            OpPanel.BackColor = Color.White;
            DgPanel.Width = 3;
            DgPanel.Left = Player.PanelWidth;
            DgPanel.Padding = new Padding(0);
            DgPanel.Margin = new Padding(0);
            DgPanel.BackColor = Color.FromArgb(200, 200, 200);
            DgPanel.Height = Height;
            DgPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            DgPanel.Cursor = Cursors.SizeWE;
            Controls.Add(DgPanel);
            Controls.Add(OpPanel);
            Controls.Add(Player);
            Player.SendToBack();
            Player.AppendAddon(new GeometryPad());
            GP = Player.GetAddon<GeometryPad>(0);
            DgPanel.BringToFront();
            OpPanel.BringToFront();

            ResumeLayout(false);
            int DownX = 0;
            bool IsDown = false;
            DgPanel.MouseDown += (s, e) =>
            {
                DownX = e.X;
                IsDown = true;
            };
            DgPanel.MouseMove += (s, e) =>
            {
                if (IsDown)
                {
                    int a = DgPanel.Left + e.X - DownX;
                    if (a > Width / 2)
                        a = Width / 2;
                    if (a < 20)
                        a = 20;
                    if (a >= 20 && a <= Width / 2 && a != DgPanel.Left)
                    {
                        SuspendLayout();
                        DgPanel.Left = a;
                        OpPanel.Width = DgPanel.Left;
                        Player.PanelWidth = OpPanel.Width;
                        Player.ReRenderAxis();
                        OpPanel.Refresh();
                        ResumeLayout();
                    }
                }
            };
            DgPanel.MouseUp += (s, e) =>
            {
                IsDown = false;
            };
            Player.MouseDoubleClick += (s, e) =>
            {
                GP.SelectedCircles.Clear();
                GP.SelectedLines.Clear();
                GP.SelectedPoints.Clear();
                var p = GP.GetPoint(e.Location);
                if (p != null&&p.PointGetter is PointGetter_Movable)
                {
                    PropertyDialog dlg = new PropertyDialog(new Dictionary<string, (string init, Type t)>() {
                        { "名字:",(p.Name,typeof(string))},
                        { "X:",(p.Location.X.ToString(),typeof(double))},
                        { "Y:",(p.Location.Y.ToString(),typeof(double))}
                    });
                    dlg.ShowDialog();
                    if (dlg.OK)
                    {
                        var dic = dlg.returndic;
                        p.Name = dic["名字:"];
                        (p.PointGetter as PointGetter_Movable).SetControlPoint(new Vec(double.Parse(dic["X:"]), double.Parse(dic["Y:"])));
                        p.RefreshValues();
                        Player.Render();
                    }
                }
            };
            Player.MouseClick += (s, e) =>
            {
                if (wd == WhatToDraw.PT)
                {
                    GP.PutPoint(e.Location);
                    GP.SelectedCircles.Clear();
                    GP.SelectedLines.Clear();
                    GP.SelectedPoints.Clear();
                    Player.Render();
                    return;
                }
                var p = GP.GetPoint(e.Location);
                if (wd == WhatToDraw.Polygon)
                {
                    GP.SelectedLines.Clear();
                    if (GP.SelectedPoints.Count > 1)
                    {
                        if (p == GP.SelectedPoints[0])
                        {
                            if (GP.SelectedPoints.Count < 3)
                            {
                                GP.SelectedPoints.Clear();
                                MessageBox.Show("边数需大于三");
                            }
                            else
                            {
                                PointGetter[] pgs = new PointGetter[GP.SelectedPoints.Count];
                                for (int i = 0; i < GP.SelectedPoints.Count; i++)
                                {
                                    pgs[i] = GP.SelectedPoints[i];
                                }
                                GP.AddShape(new Polygon(pgs));
                                GP.SelectedPoints.Clear();
                                Player.Render();
                                return;
                            }
                        }
                    }
                }
                if (p != null)
                {
                    if (GP.SelectedPoints.Contains(p))
                    {
                        GP.SelectedPoints.Remove(p);
                    }
                    else
                        GP.SelectedPoints.Add(p);
                }
                else
                {
                    var l = GP.GetLine(e.Location);
                    if(l!=null)
                        if (GP.SelectedLines.Contains(l))
                            GP.SelectedLines.Remove(l);
                        else
                            GP.SelectedLines.Add(l);
                }
                int plen = GP.SelectedPoints.Count;
                int llen = GP.SelectedLines.Count;
                PropertyDialog pd;
                switch (wd)
                {
                    case WhatToDraw.TXTP:
                        GP.SelectedLines.Clear();
                        if (plen >= 1)
                        {
                            Geometry.Shapes.Point poi = GP.SelectedPoints[0];
                            pd = new PropertyDialog(new Dictionary<string, (string init, Type t)>()
                            {
                                {"显示:",("",typeof(string))},
                                { "X偏移:",("0",typeof(double))},
                                { "Y偏移:",("0",typeof(double))}
                            });
                            pd.ShowDialog();
                            if (pd.OK)
                            {
                                GP.AddShape(new PlotTextLabel(pd.returndic["显示:"], poi, new Vec(double.Parse(pd.returndic["X偏移:"]), double.Parse(pd.returndic["Y偏移:"]))));
                            }
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.PB:
                        if (llen > 0&&plen==0)
                        {
                            if (llen == 1 && GP.SelectedLines[0] is LineSegment ls)
                            {
                                GP.AddShape(new Geometry.Shapes.PerpendicularBisector(ls));
                                GP.SelectedPoints.Clear();
                            }
                            GP.SelectedLines.Clear();
                            break;
                        }
                        if (plen >= 2&&llen==0)
                        {
                            GP.AddShape(new Geometry.Shapes.PerpendicularBisector(GP.SelectedPoints[0], GP.SelectedPoints[1]));
                            GP.SelectedPoints.Clear();
                        }
                        GP.SelectedLines.Clear();
                        break;
                    case WhatToDraw.SL:
                        GP.SelectedLines.Clear();
                        if (plen >= 2)
                        {
                            GP.AddShape(new Geometry.Shapes.StraightLine(GP.SelectedPoints[0], GP.SelectedPoints[1]));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.LS:
                        GP.SelectedLines.Clear();
                        if (plen >= 2)
                        {
                            GP.AddShape(new Geometry.Shapes.LineSegment(GP.SelectedPoints[0], GP.SelectedPoints[1]));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.HL:
                        GP.SelectedLines.Clear();
                        if (plen >= 2)
                        {
                            GP.AddShape(new Geometry.Shapes.HalfLine(GP.SelectedPoints[0], GP.SelectedPoints[1]));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.CLTWO:
                        GP.SelectedLines.Clear();
                        if (plen >= 2)
                        {
                            GP.AddShape(new Geometry.Shapes.Circle(new CircleGetter_FromCenterAndPoint(GP.SelectedPoints[0], GP.SelectedPoints[1])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.CLTHREE:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Circle(new CircleGetter_FromThreePoint(GP.SelectedPoints[0], GP.SelectedPoints[1], GP.SelectedPoints[2])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.MIDPT:
                        GP.SelectedLines.Clear();
                        if (plen >= 2)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_MiddlePoint(GP.SelectedPoints[0], GP.SelectedPoints[1])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.OutP:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_OutCenter(GP.SelectedPoints[0], GP.SelectedPoints[1], GP.SelectedPoints[2])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.InP:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_InCenter(GP.SelectedPoints[0], GP.SelectedPoints[1], GP.SelectedPoints[2])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.OrthoP:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_OrthoCenter(GP.SelectedPoints[0], GP.SelectedPoints[1], GP.SelectedPoints[2])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.MedianP:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_MedianCenter(GP.SelectedPoints[0], GP.SelectedPoints[1], GP.SelectedPoints[2])));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.Angle:
                        GP.SelectedLines.Clear();
                        if (plen >= 3)
                        {
                            GP.AddShape(new Geometry.Shapes.Angle( GP.SelectedPoints[1], GP.SelectedPoints[0], GP.SelectedPoints[2]));
                            GP.SelectedPoints.Clear();
                        }
                        break;
                    case WhatToDraw.VL:
                        if (plen == 1 && llen == 1)
                        {
                            GP.AddShape(new Geometry.Shapes.VerticalLine(GP.SelectedPoints[0], GP.SelectedLines[0]));
                            GP.SelectedPoints.Clear();
                            GP.SelectedLines.Clear();
                        }
                        if (plen > 1)
                        {
                            var po = GP.SelectedPoints[GP.SelectedPoints.Count - 1];
                            GP.SelectedPoints.Clear();
                            GP.SelectedPoints.Add(po);
                        }
                        if (llen > 1)
                        {
                            var po = GP.SelectedLines[GP.SelectedLines.Count - 1];
                            GP.SelectedLines.Clear();
                            GP.SelectedLines.Add(po);
                        }
                        break;
                    case WhatToDraw.PL:
                        if (plen == 1 && llen == 1)
                        {
                            GP.AddShape(new Geometry.Shapes.ParallelLine(GP.SelectedPoints[0], GP.SelectedLines[0]));
                            GP.SelectedPoints.Clear();
                            GP.SelectedLines.Clear();
                        }
                        if (plen > 1)
                        {
                            var po = GP.SelectedPoints[GP.SelectedPoints.Count - 1];
                            GP.SelectedPoints.Clear();
                            GP.SelectedPoints.Add(po);
                        }
                        if (llen > 1)
                        {
                            var po = GP.SelectedLines[GP.SelectedLines.Count - 1];
                            GP.SelectedLines.Clear();
                            GP.SelectedLines.Add(po);
                        }
                        break;
                    case WhatToDraw.AP:
                        if (plen == 1 && llen == 1)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_AxialSymmetryPoint(GP.SelectedPoints[0], GP.SelectedLines[0])));
                            GP.SelectedPoints.Clear();
                            GP.SelectedLines.Clear();
                        }
                        if (plen > 1)
                        {
                            var po = GP.SelectedPoints[GP.SelectedPoints.Count - 1];
                            GP.SelectedPoints.Clear();
                            GP.SelectedPoints.Add(po);
                        }
                        if (llen > 1)
                        {
                            var po = GP.SelectedLines[GP.SelectedLines.Count - 1];
                            GP.SelectedLines.Clear();
                            GP.SelectedLines.Add(po);
                        }
                        break;
                    case WhatToDraw.NP:
                        if (plen == 1 && llen == 1)
                        {
                            GP.AddShape(new Geometry.Shapes.Point(new PointGetter_NearestPointOnLine(GP.SelectedLines[0],GP.SelectedPoints[0])));
                            GP.SelectedPoints.Clear();
                            GP.SelectedLines.Clear();
                        }
                        if (plen > 1)
                        {
                            var po = GP.SelectedPoints[GP.SelectedPoints.Count - 1];
                            GP.SelectedPoints.Clear();
                            GP.SelectedPoints.Add(po);
                        }
                        if (llen > 1)
                        {
                            var po = GP.SelectedLines[GP.SelectedLines.Count - 1];
                            GP.SelectedLines.Clear();
                            GP.SelectedLines.Add(po);
                        }
                        break;
                }
                Player.Render();
            };
            EventHandler b7click = (object s, EventArgs e) =>
            {
                MakeAllUnchecked(OpPanel.tableLayoutPanel1, s as CheckBox);
                wd = WhatToDraw.None;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button1.Click += (s, e) =>
            {
                CheckBox cb=s as CheckBox;
                if (!cb.Checked)
                {
                    b7click(OpPanel.button7,null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.PT;
                Player.CanMoveOrZoom = false;
            };
            OpPanel.button2.Click += (s, e) => {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.SL;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button3.Click += (s, e) => {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.HL;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button4.Click += (s, e) => {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.LS;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button5.Click += (s, e) => {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.VL;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button6.Click += (s, e) => {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.PL;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button7.Click += b7click;
            OpPanel.button8.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.PB;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button9.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.MIDPT;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button10.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.MedianP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button11.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.OutP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button12.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.InP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button13.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.OrthoP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button14.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.CLTHREE;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button15.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.CLTWO;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button16.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1,s as CheckBox);
                wd = WhatToDraw.AP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.button17.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1, s as CheckBox);
                wd = WhatToDraw.NP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.checkBox4.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1, s as CheckBox);
                wd = WhatToDraw.TXTP;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.checkBox5.Click += (s, e) =>
            {
                OpPanel.checkBox5.Checked = false;
                GP.SelectedLines.Clear();
                GP.SelectedPoints.Clear();
                PropertyDialog pd = new PropertyDialog(new Dictionary<string, (string init, Type t)>()
                            {
                                {"显示:",("",typeof(string))},
                                { "X:",("0",typeof(double))},
                                { "Y:",("0",typeof(double))}
                            });
                pd.ShowDialog();
                if (pd.OK)
                {
                    GP.AddShape(new PixelTextLabel(pd.returndic["显示:"], new Vec(double.Parse(pd.returndic["X:"]), double.Parse(pd.returndic["Y:"]))));
                }
            };
            OpPanel.checkBox6.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1, s as CheckBox);
                wd = WhatToDraw.Polygon;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.checkBox7.Click += (s, e) =>
            {
                CheckBox cb = s as CheckBox;
                if (!cb.Checked)
                {
                    cb.Checked = false;
                    b7click(OpPanel.button7, null);
                    return;
                }
                MakeAllUnchecked(OpPanel.tableLayoutPanel1, s as CheckBox);
                wd = WhatToDraw.Angle;
                Player.CanMoveOrZoom = true;
            };
            OpPanel.checkBox1.CheckedChanged += CheckBox_CheckedChanged;
            OpPanel.checkBox2.CheckedChanged += CheckBox_CheckedChanged;
            OpPanel.checkBox3.CheckedChanged += CheckBox_CheckedChanged;
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Player.DrawAxisLine=OpPanel.checkBox1.Checked;
            Player.DrawAxisNumber = OpPanel.checkBox2.Checked;
            Player.DrawAxisGrid = OpPanel.checkBox3.Checked;
            Player.ReRenderAxis();
        }
        private void MakeAllUnchecked(Control c,CheckBox cc)
        {
            foreach(Control c2 in c.Controls)
            {
                if (c2 is CheckBox cb&&c2!=cc)
                    cb.CheckState=CheckState.Unchecked;
                if(c2==cc)
                    (c2 as CheckBox).CheckState = CheckState.Checked;
                MakeAllUnchecked(c2,cc);
            }
        }

        private enum WhatToDraw
        {
            None,
            SL,LS,HL,PT,CLTHREE,CLTWO,CLONE,MIDPT,VL,PL,PB,MedianP,OutP,InP,OrthoP,
            AP,NP,TXTP,TXTPP,Polygon,Angle
        }
        private static void ForceToPaint(Control c)
        {
            c.GetType().GetMethod("OnPaint", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(c, new object[] { new PaintEventArgs(c.CreateGraphics(), c.ClientRectangle) });
            foreach (Control i in c.Controls)
                ForceToPaint(i);
        }
        private class DragPanel : Panel
        {
        }
    }
}
