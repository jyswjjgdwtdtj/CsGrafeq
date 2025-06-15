using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes
{
    public class Circle:Shape
    {
        private CircleGetter CircleGetter;
        internal Circle(CircleGetter cg) { 
            CircleGetter= cg;
            cg.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            InnerCircle=CircleGetter.GetCircle();
            InvokeEvent();
        }
        internal CircleStruct InnerCircle;
    }
    internal struct CircleStruct
    {
        public Vec Center;
        public double Radius;
    }
}
