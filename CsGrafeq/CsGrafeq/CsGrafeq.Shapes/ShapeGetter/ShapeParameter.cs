using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public struct ShapeParameter
{
    public readonly MultiLanguageData? Description;
    public readonly GeometricShape Shape;

    public ShapeParameter(GeometricShape shape, MultiLanguageData? description = null)
    {
        Shape = shape;
        Description = description;
    }

    public static implicit operator ShapeParameter(GeometricShape shape)
    {
        return new ShapeParameter(shape);
    }
}