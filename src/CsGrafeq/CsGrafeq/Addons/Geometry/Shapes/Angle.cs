using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes
{
    public class Angle:Shape
    {
        internal PointGetter AnglePointGetter, PointGetter1, PointGetter2;
        internal Vec AnglePoint, Point1, Point2;
        internal Angle(PointGetter anglePoint,PointGetter point1,PointGetter point2) { 
            AnglePointGetter = anglePoint;
            PointGetter1 = point1;
            PointGetter2 = point2;
            AnglePointGetter.AddToChangeEvent(RefreshValues,this);
            PointGetter1.AddToChangeEvent(RefreshValues, this);
            PointGetter2.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            AnglePoint=AnglePointGetter.GetPoint();
            Point1 = PointGetter1.GetPoint();
            Point2 = PointGetter2.GetPoint();
        }

    }
}
