using System.Collections.ObjectModel;
using System.Text;
using GeoPoint = CsGrafeq.Shapes.Point;
using GeoShape = CsGrafeq.Shapes.Shape;

namespace CsGrafeq.Shapes;

public class ShapeList : ObservableCollection<GeoShape>
{
    private static readonly StringBuilder sb = new();
    private readonly List<GeoShape?> ElseShapes = new();
    private readonly List<GeoPoint?> Points = new();
    public event Action? OnShapeChanged;

    private void ShapeChanged()
    {
        OnShapeChanged?.Invoke();
    }

    public void ClearSelected()
    {
        ClearSelected<GeometryShape>();
    }

    public void ClearSelected<T>() where T : GeometryShape
    {
        foreach (var i in this.OfType<T>()) 
            i.Selected = false;
    }

    public new void Add(GeoShape geoShape)
    {
        geoShape.ShapeChanged += ShapeChanged;
        if (geoShape is GeoPoint p)
            AddPoint(p);
        else
            AddShape(geoShape);
    }

    public new void Remove(GeoShape geoShape)
    {
        if (geoShape is GeoPoint p)
            RemovePoint(p);
        else
            RemoveShape(geoShape);
    }

    private void AddPoint(GeoPoint point)
    {
        base.Add(point);
        for (var i = 0; i < Points.Count; i++)
            if (Points[i] == null)
            {
                Points[i] = point;
                return;
            }

        Points.Add(point);
        point.Name = GetNameFromIndex(GetIndex(point)).ToUpper();
    }

    private void AddShape(GeoShape Shape)
    {
        base.Add(Shape);
        if (Shape is GeoPoint point)
        {
            AddPoint(point);
            return;
        }

        for (var i = 0; i < ElseShapes.Count; i++)
            if (ElseShapes[i] == null)
            {
                ElseShapes[i] = Shape;
                return;
            }

        ElseShapes.Add(Shape);
        Shape.Name = GetNameFromIndex(GetIndex(Shape)).ToLower();
    }

    private void RemoveShape(GeoShape shape)
    {
        base.Remove(shape);
        if (shape is GeoPoint point)
        {
            RemovePoint(point);
            return;
        }

        var index = ElseShapes.IndexOf(shape);
        if (index != -1) ElseShapes[index] = null;
        foreach (var i in (shape as GeometryShape)?.SubShapes)
            RemoveShape(i);
    }

    private void RemovePoint(GeoPoint shape)
    {
        base.Remove(shape);
        var index = Points.IndexOf(shape);
        if (index != -1) Points[index] = null;
        foreach (var i in shape.SubShapes)
            RemoveShape(i);
    }

    public GeoPoint? FindPointByIndex(int index)
    {
        if (index >= Points.Count)
            return null;
        return Points[index];
    }

    public GeoShape? FindShapeByIndex(int index)
    {
        if (index >= ElseShapes.Count)
            return null;
        return ElseShapes[index];
    }

    public int GetIndex(GeoShape? Shape)
    {
        if (Shape is null)
            return -1;
        return ElseShapes.IndexOf(Shape);
    }

    public int GetIndex(GeoPoint? Point)
    {
        if (Point is null)
            return -1;
        return Points.IndexOf(Point);
    }

    public static string GetNameFromIndex(int index)
    {
        if (index < 0)
            return "";
        sb.Clear();
        if (index == 0)
            return "A";
        while (index != 0)
        {
            var remainder = index % 26;
            sb.Insert(0, (char)('A' + remainder));
            index /= 26;
        }

        return sb.ToString();
    }

    public static int GetIndexFromString(string str)
    {
        var res = 0;
        str = str.Trim().ToUpper();
        foreach (var i in str)
        {
            if (i < 'A' || i > 'Z') return -1;
            res = res * 26 + (i - 'A');
        }

        return res;
    }

    public IEnumerable<GeoPoint?> GetPoints()
    {
        return Points.ToArray();
    }

    public IEnumerable<GeoShape?> GetElseShapes()
    {
        return ElseShapes.ToArray();
    }

    public IEnumerable<T> GetSelectedShapes<T>() where T : GeoShape
    {
        foreach (var i in this.OfType<GeometryShape>())
            if (i is T t && i.Selected)
                yield return t;
    }
    public IEnumerable<T> GetShape<T>() where T : GeoShape
    {
        return this.OfType<T>();
    }
}