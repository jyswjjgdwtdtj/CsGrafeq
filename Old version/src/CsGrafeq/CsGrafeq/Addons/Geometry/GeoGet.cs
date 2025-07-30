using CsGrafeq.Geometry;
using CsGrafeq.Geometry.Shapes;
using CsGrafeq.Geometry.Shapes.Getter;
using ScriptCompilerEngine.ScriptNative.ScriptNativeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Addons.Geometry
{
    public static class GeoGet
    {
        public static GeometryPad GeoPad;
        public static object GetPoint(object index)
        {
            int idx;
            if (index is string str)
            {
                foreach (var i in GeoPad.Shapes)
                {
                    if (i is Point p)
                    {
                        if (p.Name == str&&!(p.PointGetter is PointGetter_FromScript))
                        {
                            return new object[] { p.Location.X, p.Location.Y };
                        }
                    }
                }
            }
            else if (index is long l)
            {
                idx = (int)l;
                var po = GeoPad.Shapes.FromIndex(idx);
                if(!(po.PointGetter is PointGetter_FromScript))
                    return new object[] { po.Location.X, po.Location.Y };
            }
            return Empty.Instance;
        }
       
    }
}
