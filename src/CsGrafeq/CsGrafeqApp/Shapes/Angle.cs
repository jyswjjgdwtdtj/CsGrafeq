using CsGrafeqApp.Classes;
using CsGrafeqApp.Shapes.ShapeGetter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeqApp.Shapes.ShapeGetter.AngleGetter;

namespace CsGrafeqApp.Shapes
{
    public class Angle:Shape
    {
        public AngleData AngleData;
        public AngleGetter AngleGetter;
        public Angle(AngleGetter getter) {
            AngleGetter = getter;
            AngleGetter.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
            
        }
        public override void RefreshValues()
        {
            AngleData= AngleGetter.GetAngle();
        }
        public override Getter Getter => AngleGetter;
        protected override string TypeName => "Angle";
        public override string Description =>"Degreee:"+AngleData.Angle.ToString();
        public override Vec HitTest(Vec vec)
        {
            return Vec.Infinity;
        }
    }
}
