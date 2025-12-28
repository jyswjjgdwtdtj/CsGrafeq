namespace CsGrafeq.Shapes;

public static class ShapeCreaterHelper
{
    public static int GetIndex(GeometryShape s)
    {
        switch (s)
        {
            case Point _:
                return 0;
            case Line _:
                return 5;
            case Circle _:
                return 10;
            case Polygon _:
                return 15;
            case Angle _:
                return 20;
            default:
                throw new Exception("Unknown shape type");
                return 25;
        }
    }

    public static IEnumerable<GeometryShape> SortShape(this IEnumerable<GeometryShape> shapes)
    {
        var list = new List<GeometryShape>(shapes);
        list.Sort((s1, s2) =>
        {
            var i1 = GetIndex(s1);
            var i2 = GetIndex(s2);
            if (i1 < i2)
                return -1;
            if (i1 == i2)
                return 0;
            return 1;
        });
        return list;
    }
}