using CsGrafeq.I18N;

namespace CsGrafeq.Shapes;

public sealed class Value<T> : Shape
{
    private readonly T _Value;

    public Value(T value)
    {
        TypeName = new MultiLanguageData() { Chinese = typeof(T).Name, English = typeof(T).Name };
        _Value = value;
        Name = IsNullRetEmpty(_Value.ToString());
        Visible = false;
    }

    public override void Dispose()
    {
    }


    private string IsNullRetEmpty(string str)
    {
        return str is null ? string.Empty : str;
    }
}