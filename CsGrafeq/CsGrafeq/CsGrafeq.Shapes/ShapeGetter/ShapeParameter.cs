using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public struct ShapeParameter
{
    public readonly MultiLanguageData? Description;
    public readonly GeometryShape Shape;

    public ShapeParameter(GeometryShape shape, MultiLanguageData? description = null)
    {
        Shape = shape;
        Description = description;
    }

    public static implicit operator ShapeParameter(GeometryShape shape)
    {
        return new ShapeParameter(shape);
    }
}