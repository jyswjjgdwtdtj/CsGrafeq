using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Shapes.ShapeGetter
{
    public class NullGeometryGetter:GeometryGetter
    {
        public override void Attach(Action handler, GeometryShape subShape)
        {
            
        }
        public override void UnAttach(Action handler, GeometryShape subShape)
        {
            
        }
        public override string ActionName => "null getter";
        public override GeometryShape[] Parameters => [];
    }
}
