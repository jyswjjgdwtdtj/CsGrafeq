using CsGrafeq.Shapes.ShapeGetter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Shapes
{
    public class NullGeometryShape:GeometryShape
    {
        public override string TypeName => "null";
        public override GeometryGetter Getter => new NullGeometryGetter();
        public override void InvokeEvent()
        {
            base.InvokeEvent();
        }
        public override void RefreshValues()
        {
            
        }
        public override Vec NearestOf(Vec vec)
        {
            return Vec.Empty;
        }

    }
}
