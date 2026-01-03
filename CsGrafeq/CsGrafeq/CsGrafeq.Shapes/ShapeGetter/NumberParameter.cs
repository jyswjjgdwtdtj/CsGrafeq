using CsGrafeq.I18N;

namespace CsGrafeq.Shapes.ShapeGetter;

public struct NumberParameter
{
    public readonly MultiLanguageData Description;
    public readonly ExpNumber Number;

    public NumberParameter(ExpNumber number, MultiLanguageData description)
    {
        Number = number;
        Description = description;
    }
}