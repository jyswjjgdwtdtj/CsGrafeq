using CsGrafeq.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq.Base;
using CsGrafeq.Geometry.Shapes;
using CsGrafeq.Geometry.Shapes.Getter;

namespace CsGrafeq.Addons.Geometry
{
    public partial class OpControl : UserControl
    {
        public GeometryPad GP;
        private List<CheckBox> btnlist;
        public OpControl(GeometryPad gp)
        {
            InitializeComponent();
            GP = gp;
            btn_Move.Click += Btn_Click;
            btn_Angle.Click += Btn_Click;
            btn_AxialSymmetryPoint.Click += Btn_Click;
            btn_HalfLine.Click += Btn_Click;
            btn_InCenter.Click += Btn_Click;
            btn_LineSegment.Click += Btn_Click;
            btn_MedianCenter.Click += Btn_Click;
            btn_MiddlePoint.Click += Btn_Click;
            btn_NearestPoint.Click += Btn_Click;
            btn_OrthoCenter.Click += Btn_Click;
            btn_OutCenter.Click += Btn_Click;
            btn_ParallelLine.Click += Btn_Click;
            btn_PerpendicularBisector.Click += Btn_Click;
            btn_Polygon.Click += Btn_Click;
            btn_PutPoint.Click += Btn_Click;
            btn_StraightLine.Click += Btn_Click;
            btn_TextBoxOnPlot.Click += Btn_Click;
            btn_ThreePointCircle.Click += Btn_Click;
            btn_TwoPointCircle.Click += Btn_Click;
            btn_VerticalLine.Click += Btn_Click;
            btn_Choose.Click += Btn_Click;
            btn_FittedLine.Click += Btn_Click;
            btnlist = new List<CheckBox>() {
            btn_Move,
            btn_Angle,
            btn_AxialSymmetryPoint,
            btn_HalfLine,
            btn_InCenter,
            btn_LineSegment,
            btn_MedianCenter,
            btn_MiddlePoint,
            btn_NearestPoint,
            btn_OrthoCenter,
            btn_OutCenter,
            btn_ParallelLine,
            btn_PerpendicularBisector,
            btn_Polygon,
            btn_PutPoint,
            btn_StraightLine,
            btn_TextBoxOnPlot,
            btn_ThreePointCircle,
            btn_TwoPointCircle,
            btn_VerticalLine,
            btn_Choose,
            btn_FittedLine,
            };
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            CheckBox cb = (sender as CheckBox);
            if (cb.Checked == false)
            {
                btn_Move.Checked = true;
                GP.GeoPadAction=btn_Move.Name.Replace("btn_","");
            }
            else
            {
                foreach (var i in btnlist)
                {
                    if(i!=cb)
                        i.Checked = false;
                }
                GP.GeoPadAction = (sender as Control).Name.Replace("btn_", "");
                GP.CreateShapeFromSelects();
                GP.AskForRender();
            }

        }

        private void btn_PointScript_CheckedChanged(object sender, EventArgs e)
        {
            Button cb = (sender as Button);
            GP.AddShape(new CsGrafeq.Geometry.Shapes.Point(new PointGetter_FromScript()));
        }
    }
}
