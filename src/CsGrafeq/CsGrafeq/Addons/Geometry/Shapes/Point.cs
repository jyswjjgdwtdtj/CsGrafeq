using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq.Geometry.Shapes
{
    public class Point:Shape
    {
        internal Vec Location;
        internal PointGetter PointGetter;
        internal Point(PointGetter pointgetter){
            if(pointgetter is PointGetter_FromPoint)
                throw new ArgumentNullException(nameof(pointgetter)+" 不能为指向点");
            PointGetter = pointgetter;
            PointGetter.AddToChangeEvent(RefreshValues,this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            Location = PointGetter.GetPoint();
            InvokeEvent();
        }
    }
}
