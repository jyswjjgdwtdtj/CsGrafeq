namespace CsGrafeq.TupperInterval;

public struct Range : IInterval
{
    public double Min { get; set; }

    public double Max { get; set; }

    public Range(double num) : this(num, num)
    {
    }

    public Range(double min, double max)
    {
        if (min > max)
            (min, max) = (max, min);
        Min = min;
        Max = max;
    }

    public double Length => Max - Min;

    public bool Contains(double num)
    {
        return Min < num && num < Max;
    }

    public bool ContainsEqual(double num)
    {
        return Min <= num && num <= Max;
    }

    public bool Exists()
    {
        return double.IsNaN(Min);
    }

    public override string ToString()
    {
        return "[" + Min + "," + Max + "]";
    }

    public IInterval SetDef((bool, bool) def)
    {
        throw new NotImplementedException();
    }

    public IInterval SetCont(bool cont)
    {
        throw new NotImplementedException();
    }

    public Interval ToInterval()
    {
        return new Interval(Min, Max);
    }

    public Interval ToInterval((bool, bool) def, bool cont)
    {
        return new Interval(Min, Max) { Def = def, Cont = cont };
    }

    public bool Equals(Range obj)
    {
        return Min == obj.Min && Max == obj.Max;
    }

    public static implicit operator Range((double, double) tuple)
    {
        return new Range(tuple.Item1, tuple.Item2);
    }

    public Range Copy()
    {
        return new Range(Min, Max);
    }
    public static IInterval Create(double num)=>new Range(num);
}