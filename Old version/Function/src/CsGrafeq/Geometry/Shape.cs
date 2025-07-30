using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry
{
    internal abstract class Shape:IShape
    {
        public Color Color;
        public float Width;
        public abstract void Render(Graphics g, Size s);
    }
}
