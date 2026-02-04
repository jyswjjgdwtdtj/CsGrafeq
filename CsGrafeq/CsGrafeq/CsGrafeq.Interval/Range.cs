using CsGrafeq.Interval.Interface;
using CsGrafeq.Numeric;

namespace CsGrafeq.Interval;

public struct Range : IRange
{
    public readonly double Inf => _Inf;
    public double _Inf;

    public readonly double Sup => _Sup;
    public double _Sup;

    public Range(double num) : this(num, num)
    {
    }

    public Range(double min, double max)
    {
        CsGrafeqMath.SwapIfNotLess(ref min, ref max);
        _Inf = min;
        _Sup = max;
    }

    public bool IsInValid => double.IsNaN(_Inf) || double.IsNaN(_Sup);

    public double Width => _Sup - _Inf;

    public bool Contains(double num)
    {
        return _Inf < num && num < _Sup;
    }

    public bool ContainsEqual(double num)
    {
        return _Inf <= num && num <= _Sup;
    }

    public override string ToString()
    {
        return "[" + _Inf + "," + _Sup + "]";
    }

    public bool IsEmpty => false;

    public bool TryGetNumber(out double number)
    {
        number = _Inf;
        return _Inf == _Sup;
    }

    public bool TryGetInteger(out int integer)
    {
        integer = 0;
        if (TryGetNumber(out var number))
        {
            integer = (int)number;
            if (number == integer)
                return true;
        }

        return false;
    }
}