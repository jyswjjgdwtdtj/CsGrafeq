namespace CsGrafeq.Shapes;

public sealed class Value<T> : Shape
{
    private readonly T _Value;

    public Value(T value)
    {
        _Value = value;
        Name = IsNullRetEmpty(_Value.ToString());
        Visible = false;
    }
    public override void Dispose()
    {
        
    }

    public override string TypeName { get; } = typeof(T).GetType().Name;

    public override void InvokeEvent()
    {
        throw new NotImplementedException();
    }


    private string IsNullRetEmpty(string str)
    {
        return str is null ? string.Empty : str;
    }
}