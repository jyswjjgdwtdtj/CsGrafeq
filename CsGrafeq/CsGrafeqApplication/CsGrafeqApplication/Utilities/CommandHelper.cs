using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CsGrafeq.Command;
using CsGrafeq.Shapes;
using CsGrafeq.Shapes.ShapeGetter;

namespace CsGrafeqApplication.Utilities;

public static class CommandHelper
{
    public static CommandManager CommandManager { get; } = new();
    
    /// <summary>
    ///     添加“添加图形”操作到CommandManager
    /// </summary>
    /// <param name="shape"></param>
    public static void DoShapeAdd(ShapeList sl,GeoShape shape)
    {
        CommandManager.Do(
            shape,
            _ =>
            {
                sl.TryAdd(shape);  
            },
            o =>
            {
                shape.IsDeleted = false;
            },
            o =>
            {
                shape.IsDeleted = true;
            },
            o =>
            {
                sl.Remove(shape);
                shape.Dispose();
            }, true
        );
    }

    /// <summary>
    ///     添加“删除图形”操作到CommandManager
    /// </summary>
    /// <param name="shape"></param>
    public static void DoShapeDelete(GeoShape shape)
    {
        if (shape is GeometryShape geo)
            DoGeoShapesDelete([geo]);
        else
            CommandManager.Do(
                shape,
                _=>
                {
                    shape.IsDeleted = true;
                },
                o =>
                {
                    shape.IsDeleted = true;
                },
                o =>
                {
                    shape.IsDeleted = false;
                },
                o =>
                {
                    shape.Owner?.Remove(shape);
                    shape.Dispose();
                }, false
            );
    }

    public static void DoTextBoxTextChange(TextBox tb, string newtext, string oldtext)
    {
        CommandManager.Do(null, o => { },o => { tb.Text = newtext; }, o => { tb.Text = oldtext; }, o => { });
    }

    public static void DoGeoShapesDelete(IEnumerable<GeometryShape> shapes)
    {
        var ss = shapes.Select(s => ShapeList.GetAllChildren(s)).SelectMany(o => o).Distinct().ToArray();
        CommandManager.Do(
            ss,
            o =>
            {
                  
            },
            o =>
            {
                foreach (var sh in ss)
                {
                    sh.IsDeleted = true;
                    sh.Selected = false;
                }
            },
            o =>
            {
                foreach (var sh in ss)
                {
                    sh.IsDeleted = false;
                    sh.Selected = false;
                }
            },
            o =>
            {
                foreach (var geometryShape in ss)
                {
                    geometryShape.Owner?.Remove(geometryShape);
                    geometryShape.Dispose();
                }
            }, true
        );
    }

    public static void DoPointMove(GeoPoint point, Vector2<string> previous, Vector2<string> next)
    {
        if (point.PointGetter is PointGetter_Movable pg)
            CommandManager.Do(pg,static _ =>{} ,o =>
            {
                pg.SetStringPoint(next);
                point.RefreshValues();
            }, o =>
            {
                pg.SetStringPoint(previous);
                point.RefreshValues();
            }, o => { });
    }
    
}