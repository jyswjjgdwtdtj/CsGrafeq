namespace CsGrafeq.Interval.Compiler;

internal static class Extension
{
    public static Element ToElement(this OperatorType t)
    {
        return new Element(ElementType.Operator, t.ToString(), 0);
    }

    public static OperatorType ToOper(this Token t)
    {
        return (OperatorType)t.Type;
    }

    public static Type[] GetTypeArray(Type type, int arg)
    {
        var types = new Type[arg];
        for (var i = 0; i < types.Length; i++)
            types[i] = type;
        return types;
    }
}