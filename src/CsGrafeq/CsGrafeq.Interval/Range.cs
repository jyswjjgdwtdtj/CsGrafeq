using CsGrafeq.Interval.Interface;
using CsGrafeq;

namespace CsGrafeq.Interval;

public struct Range : IRange
{
    public double Inf { get; set; }

    public double Sup { get; set; }

    public Range(double num) : this(num, num)
    {
    }

    public Range(double min, double max)
    {
        Math.SwapIfNotLess(ref min, ref max);
        Inf = min;
        Sup = max;
    }

    public bool IsInValid=>(double.IsNaN(Inf) || double.IsNaN(Sup));

    public double Width => Sup - Inf;

    public bool Contains(double num)
    {
        return Inf < num && num < Sup;
    }

    public bool ContainsEqual(double num)
    {
        return Inf <= num && num <= Sup;
    }

    public override string ToString()
    {
        return "[" + Inf + "," + Sup + "]";
    }

    public bool IsEmpty => false;
    public bool TryGetNumber(out double number)
    {
        number = Inf;
        return Inf == Sup;
    }

    public bool TryGetInteger(out int integer)
    {
        integer = 0;
        if (TryGetNumber(out double number))
        {
            integer = (int)number;
            if(number ==integer)
                return true;
        }
        return false;
    }
}