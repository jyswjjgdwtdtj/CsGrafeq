using System.Linq;
using System.Reflection;
using Avalonia.Collections;
using CsGrafeq.I18N;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;
using static CsGrafeq.Utilities.ThrowHelper;

namespace CsGrafeqApplication.Addons.GeometryPad;

internal static class GeometryActions
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
                Description = new MultiLanguageData { English = "Move the selected point", Chinese = "移动选定的点" },
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
                Name = new MultiLanguageData { English = "Put", Chinese = "放置" },
                Description = new MultiLanguageData { English = "Put a point to axis", Chinese = "在坐标系中放置一个点" },
                GetterConstructor = null,
                Args = [],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Middle", Chinese = "中点" },
                Description = new MultiLanguageData { English = "Put the middle of two points", Chinese = "放置两点的中点" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_MiddlePoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Median Center", Chinese = "重心" },
                Description = new MultiLanguageData
                    { English = "Put the median center of three points", Chinese = "放置三点的重心（质心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_Centroid).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Out Center", Chinese = "外心" },
                Description = new MultiLanguageData
                    { English = "Put the median center of three points", Chinese = "放置三点的外心（外接圆心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_Circumcenter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "In Center", Chinese = "内心" },
                Description = new MultiLanguageData
                    { English = "Put the in center of three points", Chinese = "放置三点的内心（内切圆心）" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_InCenter).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Ortho Center", Chinese = "垂心" },
                Description = new MultiLanguageData
                    { English = "Put the ortho center of three points", Chinese = "放置三点的垂心" },
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
                    { English = "Put the axial symmetry of a point from a line", Chinese = "根据直线放置点的轴对称点" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(PointGetter_AxialSymmetryPoint).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Line)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Point
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Nearest", Chinese = "最近点" },
                Description = new MultiLanguageData
                    { English = "Put the nearest point of a shape from a points", Chinese = "放置点到图形上的最近点" },
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
                Name = new MultiLanguageData { English = "Straight", Chinese = "直线" },
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
                Name = new MultiLanguageData { English = "Half", Chinese = "射线" },
                Description = new MultiLanguageData
                    { English = "Create a half line from two points", Chinese = "由两点创建半直线" },
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
                    { English = "Create a line segment from two points", Chinese = "由两点创建线段" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Segment).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point, Point)
                Args = [ShapeArg.Point, ShapeArg.Point],
                Self = ShownShapeArg.Segment
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Vertical", Chinese = "垂线" },
                Description = new MultiLanguageData
                    { English = "Create a vertical line of a line from a point", Chinese = "根据一条直线和一点创建垂线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Vertical).GetConstructors()
                        .FirstOrDefault()!), // ctor(Line, Point)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Parallel", Chinese = "平行线" },
                Description = new MultiLanguageData
                    { English = "Create a parallel line of a line from a point", Chinese = "根据一条直线和一点创建平行线" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(LineGetter_Parallel).GetConstructors()
                        .FirstOrDefault()!), // ctor(Line, Point)
                Args = [ShapeArg.Point, ShapeArg.Line],
                Self = ShownShapeArg.Straight
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Fitted", Chinese = "拟合直线" },
                Description = new MultiLanguageData
                {
                    English =
                        "Create a fitted straight line from points \n select the first selected point to finish choosing",
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
                    { English = "Create a perpendicular bisector from two points", Chinese = "由两点创建垂直平分线" },
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
                    English = "Create a polygon\nselect the first selected point to finish choosing",
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
                    { English = "Create a circle from three points", Chinese = "由三点创建圆" },
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
                    { English = "Create a circle from a center and a point", Chinese = "由圆心和一点创建圆" },
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
                    { English = "Create a circle from center and radius", Chinese = "由圆心和半径创建圆" },
                GetterConstructor =
                    ConstructorInvoker.Create(typeof(CircleGetter_FromCenterAndRadius).GetConstructors()
                        .FirstOrDefault()!), // ctor(Point)
                Args = [ShapeArg.Point],
                Self = ShownShapeArg.Circle
            },
            new ActionData
            {
                Name = new MultiLanguageData { English = "Apollonius", Chinese = "阿波罗尼斯圆" },
                Description = new MultiLanguageData
                    { English = "Create a circle through Apollonius Circle", Chinese = "创建阿波罗尼斯圆" },
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
            case ShownShapeArg.Angle:
                return new Angle((AngleGetter)getter);
            default:
                return Throw<GeometryShape>("Shape not supported:" + target);
        }
    }
}