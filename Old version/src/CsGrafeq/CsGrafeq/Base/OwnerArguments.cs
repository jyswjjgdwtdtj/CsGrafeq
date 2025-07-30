using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Addons
{
    internal struct OwnerArguments
    {
        public Action AskForRender;
        public Func<double> GetUX, GetUY;
        public Func<double, double> PMX, PMY, MPX, MPY;
        public Func<double[]> GetConstants;
        public Func<PointL> GetZero;
        public Func<Size> GetSize;
        public Func<int> GetPanelWidth;
    }
}
