namespace CsGrafeq.Interval.Compiler;

internal struct Token
{
    public TokenType Type;
    public string NameOrValue;

    public override string ToString()
    {
        return Type + " " + NameOrValue;
    }
}