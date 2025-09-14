using _Interval = CsGrafeq.Interval.Interface.IInterval<CsGrafeq.Interval.Interval>;
using static CsGrafeq.Interval.Def;
using static CsGrafeq.Interval.Extensions.IntervalExtension;
using sysMath = System.Math;

namespace CsGrafeq.Interval;

public readonly struct Interval : _Interval
{
    public Interval()
    {
    }

    public double Sup { get; init; }
    public double Inf { get; init; }
    public Def Def { get; init; } = TT;
    public double Width => Sup - Inf;

    public static Interval Create(double Inf, double Sup, Def def)
    {
        Math.SwapIfNotLess(ref Inf, ref Sup);
        return new Interval { Inf = Inf, Sup = Sup, Def = def };
    }

    public bool IsValid => !(double.IsNaN(Inf) || double.IsNaN(Sup) || Def == FF);
    public bool IsInValid => double.IsNaN(Inf) || double.IsNaN(Sup) || Def == FF;
    public bool IsEmpty => Def == FF;

    public static Interval CreateFromDouble(double num)
    {
        return Create(num, num, TT);
    }

    public static Interval Clone(Interval i)
    {
        return Create(i.Inf, i.Sup, i.Def);
    }

    public bool TryGetNumber(out double number)
    {
        number = Inf;
        return Inf == Sup;
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

    public Range ToRange()
    {
        return new Range(Inf, Sup);
    }

    public static Interval operator +(Interval left, Interval right)
    {
        return left.IsInValid || left.IsInValid
            ? _Interval.InValid
            : new Interval { Inf = left.Inf + right.Inf, Sup = left.Sup + right.Sup, Def = left.Def & right.Def };
    }

    public static Interval operator -(Interval num)
    {
        if (num.IsInValid) return _Interval.InValid;
        return Create(-num.Sup, -num.Inf, num.Def);
    }

    public static Interval operator -(Interval left, Interval right)
    {
        return left + -right;
    }

    public static Interval operator *(Interval left, Interval right)
    {
        if (left.IsInValid || right.IsInValid) return _Interval.InValid;
        var def = left.Def & right.Def;
        if (left.Inf > 0 && right.Inf > 0)
            return new Interval { Inf = left.Inf * right.Inf, Sup = left.Sup * right.Sup, Def = def };
        if (left.Sup < 0 && right.Sup < 0)
            return new Interval { Inf = left.Sup * right.Sup, Sup = left.Inf * right.Inf, Def = def };
        var res = Math.GetMinMax4(left.Inf * right.Inf, left.Inf * right.Sup, left.Sup * right.Inf,
            left.Sup * right.Sup);
        return new Interval { Inf = res.Item1, Sup = res.Item2, Def = def };
    }

    public static Interval operator /(Interval left, Interval right)
    {
        if (left.IsInValid || right.IsInValid) return _Interval.InValid;
        if (right.ContainsEqual(0))
            return new Interval
                { Inf = double.NegativeInfinity, Sup = double.PositiveInfinity, Def = left.Def & right.Def };
        return Create(1 / right.Inf, 1 / right.Sup, right.Def) * left;
    }

    public static Interval operator %(Interval a, Interval b)
    {
        if (b.Inf == b.Sup)
        {
            var num = b.Inf;
            var Inf = sysMath.Floor(a.Inf / num);
            var Sup = sysMath.Floor(a.Sup / num);
            if (Inf == Sup)
                return Create(Math.NumMod(b.Inf, num), Math.NumMod(b.Sup, num), a.Def & b.Def);
            return Create(0, b.Sup, FT);
        }

        return a - b * Floor(a / b);
    }

    public static Interval Pow(Interval i1, Interval i2)
    {
        return Exp(Ln(i1) * i2);
    }

    public static Interval Floor(Interval i1)
    {
        return new Interval { Inf = sysMath.Floor(i1.Inf), Sup = sysMath.Floor(i1.Sup), Def = i1.Def };
    }

    public static Interval Ceil(Interval i1)
    {
        return new Interval { Inf = sysMath.Ceiling(i1.Inf), Sup = sysMath.Ceiling(i1.Sup), Def = i1.Def };
    }

    public static Def operator ==(Interval i1, Interval i2)
    {
        var def = And(i1.Def, i2.Def);
        if (def == FF)
            return def;
        if (i2.Sup < i1.Inf || i2.Inf > i1.Sup)
            return FF;
        return def.Second ? FT : FF;
    }

    public static Def operator !=(Interval left, Interval right)
    {
        throw new NotImplementedException();
    }

    public static Def operator >(Interval i1, Interval i2)
    {
        var def = And(i1.Def, i2.Def);
        if (i1.IsInValid || i2.IsInValid)
            return FF;
        if (i1.Sup < i2.Inf)
            return FF;
        if (i1.Inf > i2.Sup)
        {
            if (!i1.Def.First || !i2.Def.First) return FT;
            return TT;
        }

        return FT;
    }

    public static Def operator <(Interval i1, Interval i2)
    {
        return i2 > i1;
    }

    public static Def operator <=(Interval i1, Interval i2)
    {
        return (i1 < i2) | (i1 == i2);
    }

    public static Def operator >=(Interval i1, Interval i2)
    {
        return (i1 > i2) | (i1 == i2);
    }

    public static Interval Sgn(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Sign(i.Inf), Sup = sysMath.Sign(i.Sup), Def = i.Def };
    }

    public static Interval Abs(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.ContainsEqual(0)) return new Interval { Inf = 0, Sup = sysMath.Max(-i.Inf, i.Sup), Def = i.Def };

        if (i.Sup < 0) return new Interval { Inf = -i.Sup, Sup = -i.Inf, Def = i.Def };

        return i;
    }

    public static Interval Median(Interval i1, Interval i2, Interval i3)
    {
        if (i1.IsInValid || i2.IsInValid || i3.IsInValid)
            return _Interval.InValid;
        return new Interval
        {
            Def = i1.Def & i2.Def & i3.Def,
            Inf = Math.DoubleMedian(i1.Inf, i2.Inf, i3.Inf),
            Sup = Math.DoubleMedian(i1.Sup, i2.Sup, i3.Sup)
        };
    }

    public static Interval Exp(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Pow(double.E, i.Inf), Sup = sysMath.Pow(double.E, i.Sup), Def = i.Def };
    }

    public static Interval Ln(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.Sup <= 0)
            return _Interval.InValid;
        if (i.Inf > 0)
        {
            return new Interval { Inf = sysMath.Log(i.Inf), Sup = sysMath.Log(i.Sup), Def = i.Def };
            ;
        }

        return new Interval { Inf = double.NegativeInfinity, Sup = sysMath.Log(i.Sup), Def = FT };
    }

    public static Interval Lg(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.Sup <= 0)
            return _Interval.InValid;
        if (i.Inf > 0)
        {
            return new Interval { Inf = sysMath.Log10(i.Inf), Sup = sysMath.Log10(i.Sup), Def = i.Def };
            ;
        }

        return new Interval { Inf = double.NegativeInfinity, Sup = sysMath.Log10(i.Sup), Def = FT };
    }

    public static Interval Log(Interval i1, Interval i2)
    {
        return Ln(i1) / Ln(i2);
    }

    public static Interval Sqrt(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.Sup <= 0)
            return _Interval.InValid;
        if (i.Inf > 0)
        {
            return new Interval { Inf = sysMath.Sqrt(i.Inf), Sup = sysMath.Sqrt(i.Sup), Def = i.Def };
            ;
        }

        return new Interval { Inf = 0, Sup = sysMath.Sqrt(i.Sup), Def = FT };
    }

    public static Interval Cbrt(Interval i)
    {
        if (i.IsInValid)
            return i;
        return new Interval { Inf = sysMath.Pow(i.Inf, 1d / 3), Sup = sysMath.Pow(i.Sup, 1d / 3), Def = i.Def };
    }

    public static Interval Min(Interval i1, Interval i2)
    {
        return new Interval
            { Inf = sysMath.Min(i1.Inf, i2.Inf), Sup = sysMath.Min(i1.Sup, i2.Sup), Def = And(i1.Def, i2.Def) };
    }

    public static Interval Max(Interval i1, Interval i2)
    {
        return new Interval
            { Inf = sysMath.Max(i1.Inf, i2.Inf), Sup = sysMath.Max(i1.Sup, i2.Sup), Def = And(i1.Def, i2.Def) };
    }

    public static Interval GCD(Interval i1, Interval i2)
    {
        if (i1.IsInValid || i2.IsInValid)
            return _Interval.InValid;
        if (!(i1.TryGetInteger(out _) && i2.TryGetInteger(out _)))
            return new Interval { Inf = double.NegativeInfinity, Sup = double.PositiveInfinity, Def = i1.Def & i2.Def };
        double value = Math.GCD((int)i1.Inf, (int)i2.Inf);
        return new Interval { Inf = value, Sup = value, Def = i1.Def & i2.Def };
    }

    public static Interval LCM(Interval i1, Interval i2)
    {
        if (i1.IsInValid || i2.IsInValid)
            return _Interval.InValid;
        if (!(i1.TryGetInteger(out _) && i2.TryGetInteger(out _)))
            return new Interval { Inf = double.NegativeInfinity, Sup = double.PositiveInfinity, Def = i1.Def & i2.Def };
        double value = Math.LCM((int)i1.Inf, (int)i2.Inf);
        return new Interval { Inf = value, Sup = value, Def = i1.Def & i2.Def };
    }

    public static Interval Sin(Interval i)
    {
        if (i.IsInValid) return i;
        var a = i.Inf;
        var b = i.Sup;
        var minmax = new Range();
        var res = new Range { Inf = 0, Sup = 0 };
        if (sysMath.Floor((a / sysMath.PI - 0.5) / 2) < sysMath.Floor((b / sysMath.PI - 0.5) / 2))
        {
            minmax.Inf = 1;
            res.Sup = 1;
        }

        if (sysMath.Floor((a / sysMath.PI + 0.5) / 2) < sysMath.Floor((b / sysMath.PI + 0.5) / 2))
        {
            minmax.Inf = 1;
            res.Inf = -1;
        }

        if (minmax.Inf == 0) res.Inf = sysMath.Min(sysMath.Sin(a), sysMath.Sin(b));
        if (minmax.Sup == 0) res.Sup = sysMath.Max(sysMath.Sin(a), sysMath.Sin(b));
        return new Interval { Inf = res.Inf, Sup = res.Sup, Def = i.Def };
    }

    public static Interval Cos(Interval i)
    {
        return Sin(new Interval { Inf = i.Inf + sysMath.PI / 2, Sup = i.Sup + sysMath.PI / 2, Def = i.Def });
    }

    public static Interval Tan(Interval i)
    {
        if (i.IsInValid) return _Interval.InValid;
        var l = sysMath.Floor((i.Sup + sysMath.PI / 2) / sysMath.PI);
        var r = sysMath.Floor((i.Inf + sysMath.PI / 2) / sysMath.PI);
        if (l - r == 1)
            return new Interval { Inf = double.NegativeInfinity, Sup = double.PositiveInfinity, Def = i.Def };
        if (l - r == 0) return new Interval { Inf = sysMath.Tan(i.Inf), Sup = sysMath.Tan(i.Sup), Def = i.Def };
        return new Interval { Inf = double.NegativeInfinity, Sup = double.PositiveInfinity, Def = i.Def };
    }

    public static Interval Cot(Interval i)
    {
        return Tan(Create<Interval>(sysMath.PI / 2) - i);
    }

    public static Interval ArcTan(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Atan(i.Inf), Sup = sysMath.Atan(i.Sup), Def = i.Def };
        ;
    }

    public static Interval ArcSin(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.Sup < -1 || i.Inf > 1)
            return _Interval.InValid;
        return new Interval
        {
            Inf = sysMath.Asin(sysMath.Max(i.Inf, -1)),
            Sup = sysMath.Asin(sysMath.Min(i.Sup, 1)),
            Def = i.Inf < -1 || i.Sup > 1 ? FT : i.Def
        };
    }

    public static Interval ArcCos(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.Sup < -1 || i.Inf > 1)
            return _Interval.InValid;
        return new Interval
        {
            Inf = sysMath.Acos(sysMath.Min(i.Sup, 1)),
            Sup = sysMath.Acos(sysMath.Max(i.Inf, -1)),
            Def = i.Inf < -1 || i.Sup > 1 ? FT : i.Def
        };
    }

    public static Interval Sinh(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Sinh(i.Inf), Sup = sysMath.Sinh(i.Sup), Def = i.Def };
        ;
        ;
    }

    public static Interval Cosh(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        if (i.ContainsEqual(0))
            return new Interval { Inf = 1, Sup = sysMath.Cosh(sysMath.Max(i.Sup, -i.Inf)), Def = i.Def };

        return new Interval { Inf = sysMath.Cosh(i.Inf), Sup = sysMath.Cosh(i.Sup), Def = i.Def };
    }

    public static Interval Tanh(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Tanh(i.Inf), Sup = sysMath.Tanh(i.Sup), Def = i.Def };
    }

    public static Interval ArcTanh(Interval i)
    {
        if (i.IsInValid || i.Sup < -1 || i.Inf > 1)
            return _Interval.InValid;
        return new Interval
        {
            Inf = i.Inf < -1 ? double.NegativeInfinity : sysMath.Atanh(i.Inf),
            Sup = i.Sup > 1 ? double.PositiveInfinity : sysMath.Atanh(i.Sup), Def = i.Def
        };
    }

    public static Interval ArcSinh(Interval i)
    {
        if (i.IsInValid)
            return _Interval.InValid;
        return new Interval { Inf = sysMath.Asinh(i.Inf), Sup = sysMath.Asinh(i.Sup), Def = i.Def };
    }

    public static Interval ArcCosh(Interval i)
    {
        if (i.IsInValid || i.Sup < 1)
            return _Interval.InValid;
        var res = new Range { Inf = i.Inf, Sup = i.Sup };
        var def = i.Def;
        if (res.Inf < 1)
        {
            res.Inf = 1;
            def = FT;
        }

        res.Inf = sysMath.Acosh(i.Inf);
        res.Inf = sysMath.Acosh(i.Sup);
        return new Interval { Inf = res.Inf, Sup = res.Sup, Def = def };
    }
}