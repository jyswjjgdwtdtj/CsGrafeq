using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using CsGrafeq.I18N;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Utilities.ThrowHelper;
using GeoHalf = CsGrafeq.Shapes.Half;

namespace CsGrafeqApplication.Addons.GeometricPad;

internal static class GeometricActions
{
    /// <summary>
    ///     ����Point,Line,Circle,Polygon��˳��
    /// </summary>
    public static AvaloniaList<HasNameActionList> Actions { get; } = new()
    {
        new HasNameActionList(new MultiLanguageData { English = "Edit", Chinese = "编辑" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Move", Chinese = "移动" },
                Description = new MultiLanguageData { English = "Move the selected point(s)", Chinese = "移动选定的点" },
                GetterConstructor = null,
                Args = [],
                Self = ShownShapeArg.None
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Select", Chinese = "选择" },
                Description = new MultiLanguageData { English = "Select shapes", Chinese = "选择图形" },
                GetterConstructor = null,
                Args = [],
                Self = ShownShapeArg.None
            }
        },

        new HasNameActionList(new MultiLanguageData { English = "Point", Chinese = "点" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Place", Chinese = "放置" },
                Description = new MultiLanguageData { English = "Place a point in the coordinate system", Chinese = "在坐标系中放置一个点" },
                GetterConstructor = null,
                Args = [],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Midpoint", Chinese = "中点" },
                Description = new MultiLanguageData { English = "Place the midpoint of two points", Chinese = "放置两点的中点" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_MiddlePoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Centroid", Chinese = "重心" },
                Description = new MultiLanguageData
                    { English = "Place the centroid of three points", Chinese = "放置三点的重心（质心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_Centroid).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Circumcenter", Chinese = "外心" },
                Description = new MultiLanguageData
                    { English = "Place the circumcenter of three points", Chinese = "放置三点的外心（外接圆心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_Circumcenter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Incenter", Chinese = "内心" },
                Description = new MultiLanguageData
                    { English = "Place the incenter of three points", Chinese = "放置三点的内心（内切圆心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_InCenter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Orthocenter", Chinese = "垂心" },
                Description = new MultiLanguageData
                    { English = "Place the orthocenter of three points", Chinese = "放置三点的垂心" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_OrthoCenter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Axial Symmetry", Chinese = "轴对称" },
                Description = new MultiLanguageData
                    { English = "Place the reflection point across a line", Chinese = "根据直线放置点的轴对称点" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_AxialSymmetryPoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Line)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Nearest Point", Chinese = "最近点" },
                Description = new MultiLanguageData
                    { English = "Place the nearest point from a point to the shape", Chinese = "放置点到图形上的最近点" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_NearestPointOnLine).GetConstructors()
                        .FirstOrDefault()!), // ctor(Line, Point)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Point
            }
        },

        new HasNameActionList(new MultiLanguageData { English = "Line", Chinese = "线" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Straight Line", Chinese = "直线" },
                Description = new MultiLanguageData
                    { English = "Create a straight line from two points", Chinese = "由两点创建直线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Connected).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Half Line", Chinese = "射线" },
                Description = new MultiLanguageData
                    { English = "Create a half line from two points", Chinese = "由两点创建射线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Half).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Half
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Segment", Chinese = "线段" },
                Description = new MultiLanguageData
                    { English = "Create a segment from two points", Chinese = "由两点创建线段" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Segment).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Segment
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Perpendicular Line", Chinese = "垂线" },
                Description = new MultiLanguageData
                    { English = "Create a perpendicular line from a line and a point", Chinese = "根据一条直线和一点创建垂线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Vertical).GetConstructors()
                        .FirstOrDefault()!), // ctor(Line, Point)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Parallel Line", Chinese = "平行线" },
                Description = new MultiLanguageData
                    { English = "Create a parallel line from a line and a point", Chinese = "根据一条直线和一点创建平行线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Parallel).GetConstructors()
                        .FirstOrDefault()!), // ctor(Line, Point)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Fitted Line", Chinese = "拟合直线" },
                Description = new MultiLanguageData
                {
                    English =
                        "Fit a straight line from multiple points\nSelect the first point to finish selecting",
                    Chinese = "由多个点拟合直线\n选中第一个点以结束选择"
                },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Fitted).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point[])
                Args = [ShapeArg.Point],
                IsMultiPoint = true,
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Perpendicular Bisector", Chinese = "垂直平分线" },
                Description = new MultiLanguageData
                    { English = "Create the perpendicular bisector from two points", Chinese = "由两点创建垂直平分线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_PerpendicularBisector).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Straight
            }
        },

        new HasNameActionList(new MultiLanguageData { English = "Polygon", Chinese = "多边形" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Polygon", Chinese = "多边形" },
                Description = new MultiLanguageData
                {
                    English = "Create a polygon\nSelect the first point to finish selecting",
                    Chinese = "创建多边形\n选中第一个点以结束选择"
                },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PolygonGetter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point[])
                Args = [ShapeArg.Point],
                IsMultiPoint = true,
                Self = ShownShapeArg.Polygon
            }
        },

        new HasNameActionList(new MultiLanguageData { English = "Circle", Chinese = "圆" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Three Points", Chinese = "三点圆" },
                Description = new MultiLanguageData
                    { English = "Create a circle through three points", Chinese = "由三点创建圆" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(CircleGetter_FromThreePoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Center and Point", Chinese = "圆心与一点" },
                Description = new MultiLanguageData
                    { English = "Create a circle from its center and a point", Chinese = "由圆心和一点创建圆" },
                GetterConstructor =
                    ConstructorInvoker.Create(
                        typeof(CircleGetter_FromCenterAndPoint).GetConstructors()
                            .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Center and Radius", Chinese = "圆心与半径" },
                Description = new MultiLanguageData
                    { English = "Create a circle from its center and radius", Chinese = "由圆心和半径创建圆" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(CircleGetter_FromCenterAndRadius).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, double)
                Args = [ShapeArg.Point],
                Self = ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Apollonius Circle", Chinese = "阿波罗尼斯圆" },
                Description = new MultiLanguageData
                    { English = "Create an Apollonius circle", Chinese = "创建阿波罗尼斯圆" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(CircleGetter_Apollonius).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Circle
            }
        },

        new HasNameActionList(new MultiLanguageData { English = "Angle", Chinese = "角" })
        {
            new ActionData
            {
                Name = new MultiLanguageData { English = "Angle", Chinese = "角" },
                Description = new MultiLanguageData { English = "Create an angle", Chinese = "创建一个角" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(AngleGetter_FromThreePoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point anglePoint, Point point1, Point point2)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Angle
            }
        }
    };

    public static GeometricShape CreateShape(ShownShapeArg target, Getter getter)
    {
        switch (target)
        {
            case ShownShapeArg.Point:
                return new Point((PointGetter)getter);
            case ShownShapeArg.Straight:
                return new Straight((LineGetter)getter);
            case ShownShapeArg.Half:
                return new GeoHalf((LineGetter_Half)getter);
            case ShownShapeArg.Segment:
                return new Segment((LineGetter_Segment)getter);
            case ShownShapeArg.Circle:
                return new Circle((CircleGetter)getter);
            case ShownShapeArg.Polygon:
                return new Polygon((PolygonGetter)getter);
            case ShownShapeArg.Angle:
                return new Angle((AngleGetter)getter);
            default:
                return Throw<GeometricShape>($"The shape {target} is not supported");
        }
    }
}