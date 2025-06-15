using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes
{
    public delegate void ShapeChangeHandler();
    public abstract class Shape:IShape
    {
        internal abstract void RefreshValues();
        internal virtual void InvokeEvent()
        {
            Changed?.Invoke();
        }
        internal event ShapeChangeHandler Changed;
        internal bool Visible = true;
        internal string Name = null;
        internal List<Shape> SubShapes = new List<Shape>();
    }
}
