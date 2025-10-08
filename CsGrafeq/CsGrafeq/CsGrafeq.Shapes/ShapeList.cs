using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using DynamicData;

namespace CsGrafeq.Shapes;

public class ShapeList:ObservableCollection<Shape>
{
    private static readonly StringBuilder sb = new();
    public event Action? OnShapeChanged;
    private List<GeometryShape> SelectedShapes= new();
    public ShapeList() : base()
    {

    }

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

    public new void Add(Shape shape)
    {
        shape.ShapeChanged += ShapeChanged;
        if (shape is GeometryShape s)
            AddGeometry(s);
        else
            AddNotGeometry(shape);
    }

    public void Delete(Shape shape)
    {
        if (shape is GeometryShape s)
        {
            DeleteGeometry(s);
        }
        else
        {
            shape.IsDeleted = true;
        }
    }

    private void DeleteGeometry(GeometryShape shape)
    {
        shape.IsDeleted = true;
        foreach (var i in shape.SubShapes)
        {
            DeleteGeometry(i);
        }
    }
    public new void Remove(Shape shape)
    {
        if (shape is GeometryShape s)
            RemoveGeometry(s);
        else
            RemoveNotGeometry(shape);
    }

    private void AddGeometry(GeometryShape shape)
    {
        base.Add(shape);
        shape.Name = GetFirstNameNotDistributed().ToUpper();
        shape.SelectedChanged += (s, e) =>
        {
            if (e)
                SelectedShapes.Add(shape);
            else
                SelectedShapes.Remove(shape);
        };
    }

    private void AddNotGeometry(Shape shape)
    {
        base.Add(shape);
    }

    private void RemoveGeometry(GeometryShape shape)
    {
        base.Remove(shape);
        shape.Selected = false;
        foreach (var i in shape.SubShapes)
            RemoveGeometry(i);
    }
    private void RemoveNotGeometry(Shape shape)
    {
        base.Remove(shape);
    }
    public static string GetNameFromIndex(int index)
    {
        if (index < 0)
            return "";
        sb.Clear();
        if (index<26)
        {
            return ((char)('A' + index)).ToString();
        }
        index -= 26;
        if(index==0)
            return "A";
        while (index != 0)
        {
            var remainder = index % 26;
            sb.Insert(0, (char)('A' + remainder));
            index-=remainder;
            index /= 26;
        }

        if (sb.Length == 1)
        {
            sb.Insert(0, 'A');
        }
        return sb.ToString();
    }
    public void Invalidate()
    {
        Shape[] newarr=new  Shape[this.Count];
        this.CopyTo(newarr,0);
        this.ClearItems();
        this.AddRange(newarr);
    }
    public IEnumerable<T> GetShapes<T>()
    {
        return this.OfType<T>();
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
    public IEnumerable<T> GetSelectedShapes<T>() where T : GeometryShape
    {
        foreach (var i in SelectedShapes.OfType<T>())
            yield return i;
    }

    public static IEnumerable<GeometryShape> GetAllChildren(GeometryShape shape)
    {
        if (!shape.IsDeleted)
        {
            yield return shape;
            foreach (var s in shape.SubShapes)
            foreach (var i in GetAllChildren(s)) 
                yield return i;
        }
    }
    public Shape? GetShapeByName(string name)
    {
        name = name.ToLower();
        return this.FirstOrDefault((shape) => (!shape.IsDeleted)&&shape.Name.ToLower() == name, null);
    }

    public string GetFirstNameNotDistributed()
    {
        for(var i = 0;;i++)
        {
            string name=GetNameFromIndex(i);
            if (GetShapeByName(name) == null)
                return name;
        }
    }
}