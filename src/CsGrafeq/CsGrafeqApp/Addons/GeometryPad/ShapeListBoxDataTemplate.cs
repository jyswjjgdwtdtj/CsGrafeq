using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CsGrafeqApp.Shapes;
using CsGrafeqApp.Shapes.ShapeGetter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeqApp.Addons.GeometryPad
{
    public class ShapeListBoxDataTemplate:IDataTemplate
    {
        public required IDataTemplate? IsFunction { get; set; }
        public required IDataTemplate? IsCommonPoint { get; set; }
        public required IDataTemplate? IsOnShapePoint { get; set; }
        public required IDataTemplate? IsLocationPoint { get; set; }
        public required IDataTemplate? IsPolygon { get; set; }
        public required IDataTemplate? IsCircle { get; set; }
        public required IDataTemplate? IsLine { get; set; }
        public required IDataTemplate? Common { get; set; }
        public Control? Build(object? param)
        {
            if (param is Shapes.Shape item)
            {
                switch (item)
                {
                    case Point point:
                        {
                            if(point.PointGetter is PointGetter_FromLocation)
                                return IsLocationPoint?.Build(param);
                            else if(point.PointGetter is PointGetter_Movable)
                                return IsOnShapePoint?.Build(param);
                            return IsCommonPoint?.Build(param);
                        }
                    case Line _:
                    case Circle _:
                    case Polygon _:
                    case Angle _:
                        return Common?.Build(param);
                }

            }
            return new Control();
        }

        public bool Match(object? data)
        {
            return data is Shapes.Shape;
        }
    }
}
