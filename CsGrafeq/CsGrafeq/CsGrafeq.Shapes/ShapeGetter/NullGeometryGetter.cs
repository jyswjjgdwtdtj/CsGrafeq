using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public class NullGeometryGetter : GeometryGetter
{
    public override MultiLanguageData ActionName => MultiLanguageResources.NullText;

    public override void Attach(GeometricShape subShape)
    {
    }

    public override void UnAttach(GeometricShape subShape)
    {
    }
}