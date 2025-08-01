using CsGrafeqApp.Classes;
using CsGrafeqApp.Shapes.ShapeGetter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Shapes
{
    public class Circle:FilledShape
    {
        private CircleGetter CircleGetter;
        public Circle(CircleGetter cg) { 
            CircleGetter= cg;
            cg.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
        }
        public override void RefreshValues()
        {
            InnerCircle=CircleGetter.GetCircle();
            InvokeEvent();
        }
        public CircleStruct InnerCircle;
        protected override string TypeName
        {
            get => "Circle";
        }
        public double Radius => InnerCircle.Radius;
        public double LocY => InnerCircle.Center.Y;
        public double LocX => InnerCircle.Center.X;
        public override CircleGetter Getter => CircleGetter;
        public override string Description => $"Center:({LocX},{LocY}),Radius:{Radius}";
        public override Vec HitTest(Vec vec)
        {
            if (Filled)
                if ((vec - InnerCircle.Center).GetLength() - InnerCircle.Radius < 0)
                    return Vec.Empty;
                else
                    return ((vec - InnerCircle.Center).Unit() * InnerCircle.Radius) - vec;
            else
                return ((vec - InnerCircle.Center).Unit() * InnerCircle.Radius) - vec;

        }
    }
    public struct CircleStruct
    {
        public Vec Center;
        public double Radius;
    }
}
