using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public class ValueGetter:Getter
    {
        public override string ActionName => throw new NotImplementedException();
        public override void AddToChangeEvent(ShapeChangedHandler handler, Shape subShape)
        {
            throw new NotImplementedException();
        }
        public override bool Adjust()
        {
            throw new NotImplementedException();
        }
        public override Shape[] Parameters => throw new NotImplementedException();
    }
}
