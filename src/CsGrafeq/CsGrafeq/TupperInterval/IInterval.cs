namespace CsGrafeq.TupperInterval;

public interface IInterval
{
    public static readonly (bool, bool) TT = (true, true);
    public static readonly (bool, bool) FT = (false, true);
    public static readonly (bool, bool) FF = (false, false);
    double Max { get; }
    double Min { get; }
    double Length { get; }
    bool Contains(double n);
    bool ContainsEqual(double n);
    IInterval SetDef((bool, bool) def);
    IInterval SetCont(bool cont);
    static abstract IInterval Create(double num);
}