using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes.Getter
{
    internal abstract class Getter
    {
        public abstract void AddToChangeEvent(ShapeChangeHandler handler,Shape subShape);
        public virtual bool Adjust()
        {
            return false;
        }
    }
}
