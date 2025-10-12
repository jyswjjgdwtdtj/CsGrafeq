using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Collections;
using CsGrafeq.Collections;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal static class GeometryActions
{
    /// <summary>
    /// ����Point,Line,Circle,Polygon��˳��
    /// </summary>
    public static AvaloniaList<HasNameActionList> Actions { get; } = new()
    {
        new HasNameActionList("Edit")
        {
            new ActionData
            {
                Name = "Move",
                Description = "Move the selected point",
                GetterConstructor = null,
                Args = [],
                Self=ShownShapeArg.None
            },
            new ActionData
            {
                Name = "Select",
                Description = "Select shapes",
                GetterConstructor = null,
                Args = [],
                Self=ShownShapeArg.None
            }
        },

        new HasNameActionList("Point")
        {
            new ActionData
            {
                Name = "Put",
                Description = "Put a point to axis",
                GetterConstructor = null,
                Args = [],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Middle",
                Description = "Put the middle of two points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_MiddlePoint).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Median Center",
                Description = "Put the median center of three points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_MedianCenter).GetConstructors().FirstOrDefault()), // ctor(Point, Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Out Center",
                Description = "Put the median center of three points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_OutCenter).GetConstructors().FirstOrDefault()), // ctor(Point, Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "In Center",
                Description = "Put the in center of three points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_InCenter).GetConstructors().FirstOrDefault()), // ctor(Point, Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Ortho Center",
                Description = "Put the ortho center of three points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_OrthoCenter).GetConstructors().FirstOrDefault()), // ctor(Point, Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Axial Symmetry",
                Description = "Put the axial symmetry of a point from a line",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_AxialSymmetryPoint).GetConstructors().FirstOrDefault()), // ctor(Point, Line)
                Args = [ ShapeArg.Point, ShapeArg.Line ],
                Self=ShownShapeArg.Point
            },
            new ActionData
            {
                Name = "Nearest",
                Description = "Put the nearest point of a shape from a points",
                GetterConstructor = ConstructorInvoker.Create(typeof(PointGetter_NearestPointOnLine).GetConstructors().FirstOrDefault()), // ctor(Line, Point)
                Args = [  ShapeArg.Point, ShapeArg.Line ],
                Self=ShownShapeArg.Point
            }
        },

        new HasNameActionList("Line")
        {
            new ActionData
            {
                Name = "Straight",
                Description = "Create a straight line from two points",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Connected).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = "Half",
                Description = "Create a half line from two points",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Half).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Half
            },
            new ActionData
            {
                Name = "Segment",
                Description = "Create a line segment from two points",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Segment).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Segment
            },
            new ActionData
            {
                Name = "Vertical",
                Description = "Create a vertical line of a line from a point",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Vertical).GetConstructors().FirstOrDefault()), // ctor(Line, Point)
                Args = [ ShapeArg.Point,ShapeArg.Line ],
                Self=ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = "Parallel",
                Description = "Create a parallel line of a line from a point",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Parallel).GetConstructors().FirstOrDefault()), // ctor(Line, Point)
                Args = [ ShapeArg.Point,ShapeArg.Line ],
                Self=ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = "Fitted",
                Description = "Create a fitted straight line from points \n select the first selected point to finish choosing",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_Fitted).GetConstructors().FirstOrDefault()), // ctor(Point[])
                Args = [ ShapeArg.Point ],
                IsMultiPoint = true,
                Self=ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = "Perpendicular Bisector",
                Description = "Create a perpendicular bisector from two points",
                GetterConstructor = ConstructorInvoker.Create(typeof(LineGetter_PerpendicularBisector).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Straight
            }
        },

        new HasNameActionList("Polygon")
        {
            new ActionData
            {
                Name = "Polygon",
                Description = "Create a polygon\nselect the first selected point to finish choosing",
                GetterConstructor = ConstructorInvoker.Create(typeof(PolygonGetter).GetConstructors().FirstOrDefault()), // ctor(Point[])
                Args = [ ShapeArg.Point ] ,
                IsMultiPoint = true,
                Self=ShownShapeArg.Polygon
            }
        },

        new HasNameActionList("Circle")
        {
            new ActionData
            {
                Name = "Three Points",
                Description = "Create a circle from three points",
                GetterConstructor = ConstructorInvoker.Create(typeof(CircleGetter_FromThreePoint).GetConstructors().FirstOrDefault()), // ctor(Point, Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = "Center and Point",
                Description = "Create a circle from a center and a point",
                GetterConstructor = ConstructorInvoker.Create(typeof(CircleGetter_FromCenterAndPoint).GetConstructors().FirstOrDefault()), // ctor(Point, Point)
                Args = [ ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = "Center and Radius",
                Description = "Create a circle from center and radius",
                GetterConstructor = ConstructorInvoker.Create(typeof(CircleGetter_FromCenterAndRadius).GetConstructors().FirstOrDefault()), // ctor(Point)
                Args = [ ShapeArg.Point],
                Self=ShownShapeArg.Circle
            },
        },

        new HasNameActionList("Angle")
        {
            new ActionData
            {
                Name = "Angle",
                Description = "Create an angle",
                GetterConstructor = ConstructorInvoker.Create(typeof(AngleGetter_FromThreePoint).GetConstructors().FirstOrDefault()), // ctor(Point anglePoint, Point point1, Point point2)
                Args = [ ShapeArg.Point, ShapeArg.Point, ShapeArg.Point ],
                Self=ShownShapeArg.Angle
            }
        }
    };
    public static GeometryShape CreateShape(ShownShapeArg target, Getter getter)
    {
        switch (target)
        {
            case ShownShapeArg.Point:
                return new GeoPoint((PointGetter)getter);
            case ShownShapeArg.Straight:
                return new Straight((LineGetter)getter);
            case ShownShapeArg.Half:
                return new Half((LineGetter_Half)getter);
            case ShownShapeArg.Segment:
                return new Segment((LineGetter_Segment)getter);
            case ShownShapeArg.Circle:
                return new Circle((CircleGetter)getter);
            case ShownShapeArg.Polygon:
                return new Polygon((PolygonGetter)getter);
            default:
                return CsGrafeq.Extension.Throw<GeometryShape>("");
        }
    }
}