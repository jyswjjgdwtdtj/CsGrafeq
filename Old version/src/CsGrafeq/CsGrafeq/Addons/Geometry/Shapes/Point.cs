using CsGrafeq.Addons.Geometry;
using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsGrafeq.Base;
using System.Drawing;

namespace CsGrafeq.Geometry.Shapes
{
    public class Point:Shape
    {
        internal Vec Location;
        internal PointGetter PointGetter;
        internal readonly DistinctList<TextGetter> TextGetters=new DistinctList<TextGetter>();
        internal Point(PointGetter pointgetter){
            if(pointgetter is PointGetter_FromPoint)
                throw new ArgumentNullException(nameof(pointgetter)+" 不能为指向点");
            if (pointgetter is PointGetter_FromScript)
                _FromScript = true;
            PointGetter = pointgetter;
            PointGetter.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            Location = PointGetter.GetPoint();
            InvokeEvent();
        }
        internal override bool Adjust()
        {
            if (PointGetter.Adjust()){
                RefreshValues();
                return true;
            }
            return false;
        }
    }
}
