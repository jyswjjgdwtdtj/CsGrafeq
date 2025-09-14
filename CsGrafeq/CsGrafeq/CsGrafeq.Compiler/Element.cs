namespace CsGrafeq.Compiler;

public struct Element
{
    public ElementType Type;
    public string NameOrValue;
    public int ArgCount;

    public Element(ElementType type, string nameOrValue, int argCount)
    {
        Type = type;
        NameOrValue = nameOrValue;
        ArgCount = argCount;
    }

    public override string ToString()
    {
        return Type + " " + NameOrValue + " " + ArgCount;
    }
}