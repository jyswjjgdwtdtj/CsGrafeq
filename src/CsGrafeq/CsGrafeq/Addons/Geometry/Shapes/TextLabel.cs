using CsGrafeq.Geometry.Shapes.Getter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Geometry.Shapes
{
    public class PlotTextLabel : Shape
    {
        internal PointGetter PointGetter;
        internal string Text;
        internal Vec OffSet;
        internal Vec Location;
        internal PlotTextLabel(string str, PointGetter p, Vec offSet)
        {
            Text = str;
            PointGetter = p;
            OffSet = offSet;
            p.AddToChangeEvent(RefreshValues, this);
            RefreshValues();
        }
        internal override void RefreshValues()
        {
            Location = PointGetter.GetPoint() + OffSet;
        }
    }
    public class PixelTextLabel : Shape
    {
        internal string Text;
        internal Vec Location;
        internal PixelTextLabel(string str, Vec location)
        {
            Text = str;
            Location=location;
            RefreshValues();
        }
        internal override void RefreshValues()
        {
        }
    }
}
