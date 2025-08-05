using static CsGrafeq.TupperInterval.IInterval;

namespace CsGrafeq.TupperInterval;

public struct Interval : IInterval
{
    public double Min { get; set; }

    public double Max { get; set; }

    public Interval(double num) : this(num, num)
    {
    }

    public Interval(double min, double max)
    {
        if (min > max)
            (min, max) = (max, min);
        Min = min;
        Max = max;
        Def = TT;
        Cont = true;
    }

    public double Length => Max - Min;

    public (bool, bool) Def;

    //指定义域不完整 即定义域的所有值不能一一对应到函数值
    public bool Cont;

    public bool Contains(double num)
    {
        return Min < num && num < Max;
    }

    public bool ContainsEqual(double num)
    {
        return Min <= num && num <= Max;
    }

    public override string ToString()
    {
        return "[" + Min + "," + Max + "]";
    }

    public bool isEmpty()
    {
        return Def.Item2 == false;
    }

    public bool isPartial()
    {
        return Def == FT;
    }

    public bool isNumber()
    {
        return Min == Max;
    }

    IInterval IInterval.SetDef((bool, bool) def)
    {
        Def = def;
        return this;
    }

    IInterval IInterval.SetCont(bool cont)
    {
        Cont = cont;
        return this;
    }

    public Interval SetCont(bool cont)
    {
        Cont = cont;
        return this;
    }

    public Interval SetDef((bool, bool) def)
    {
        Def = def;
        return this;
    }

    public Range ToRange()
    {
        return new Range(Min, Max);
    }
    public static IInterval Create(double num)=>new Interval(num);
}