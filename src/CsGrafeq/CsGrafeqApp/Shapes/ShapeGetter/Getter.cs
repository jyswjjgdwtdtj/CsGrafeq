using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Shapes.ShapeGetter
{
    public abstract class Getter
    {
        public abstract void AddToChangeEvent(ShapeChangedHandler handler,Shape subShape);
        public virtual bool Adjust()
        {
            return false;
        }
        public abstract string ActionName { get; }
        public abstract Shape[] Parameters { get; }
    }
}
