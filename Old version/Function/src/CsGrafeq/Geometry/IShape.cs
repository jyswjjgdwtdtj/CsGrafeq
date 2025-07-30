using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry
{
    internal interface IShape
    {
        void Render(Graphics graphics, Size size);
    }
}
