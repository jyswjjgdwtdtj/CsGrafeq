using sysMath = System.Math;
using static System.Double;
using static CsGrafeq.TupperInterval.IInterval;

namespace CsGrafeq.TupperInterval;

[Obsolete("Never use methods for interval", true)]
public static class IntervalMath
{
    private static readonly Interval EmptyInterval = new(NaN) { Def = FF, Cont = false };

    static IntervalMath()
    {
    }

    public static Interval New(double num)
    {
        return new Interval(num);
    }

    #region 四则运算

    public static Interval Add(Interval a, Interval b)
    {
        return a.isEmpty() || b.isEmpty()
            ? EmptyInterval
            : new Interval(a.Min + b.Min, a.Max + b.Max) { Def = And(a.Def, b.Def), Cont = a.Cont && b.Cont };
    }

    public static Interval Subtract(Interval a, Interval b)
    {
        return Add(a, Neg(b));
    }

    public static Interval Multiply(Interval i1, Interval i2)
    {
        if (i1.isEmpty() || i2.isEmpty())
            return EmptyInterval;
        if (i1.Min > 0 && i2.Min > 0)
        {
            i1.Min *= i2.Min;
            i1.Max *= i2.Max;
            return i1;
        }

        if (i1.Max < 0 && i2.Max < 0)
            return new Interval(i1.Max * i2.Max, i1.Min * i2.Min)
                { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
        var ds = GetMinMax4(i1.Min * i2.Min, i1.Min * i2.Max, i1.Max * i2.Min, i1.Max * i2.Max);
        return new Interval(ds.Item1, ds.Item2) { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
    }

    public static Interval Divide(Interval m, Interval i)
    {
        //1/i1
        //a/b=a*(1/b)=a*(b=>1/x)
        if (i.isEmpty() || m.isEmpty()) return EmptyInterval;
        if (i.ContainsEqual(0))
            return new Interval(NegativeInfinity, PositiveInfinity)
                { Def = And(i.Def, m.Def), Cont = i.Cont && m.Cont };

        var i1 = new Interval(1 / i.Min, 1 / i.Max);
        return Multiply(m, i1);
    }

    public static Interval Mod(Interval a, Interval b)
    {
        if (b.Min == b.Max)
        {
            var num = b.Min;
            var min = sysMath.Floor(a.Min / num);
            var max = sysMath.Floor(a.Max / num);
            if (min == max)
                return new Interval(NumMod(b.Min, num), NumMod(b.Max, num)) { Def = And(a.Def, b.Def) };
            return new Interval(0, b.Max) { Def = FT };
        }

        return Subtract(a, Multiply(Floor(Divide(a, b)), b)).SetDef(FT);
    }

    private static double NumMod(double a, double b)
    {
        return a - sysMath.Floor(a / b) * b;
    }

    public static Interval Neg(Interval a)
    {
        return new Interval(-a.Max, -a.Min) { Def = a.Def, Cont = a.Cont };
    }

    #endregion

    #region 比较运算

    public static (bool, bool) Equal(Interval i1, Interval i2)
    {
        var def = And(i1.Def, i2.Def);
        if (!def.Item2)
            return def;
        if (i2.Max < i1.Min || i2.Min > i1.Max)
            return FF;
        //if(i2.Max==i1.Max&&i2.Min==i1.Min)
        //    return def;
        return (false, def.Item2);
    }

    public static (bool, bool) Greater(Interval i1, Interval i2)
    {
        var def = And(i1.Def, i2.Def);
        if (i1.isEmpty() || i2.isEmpty())
            return FF;
        if (i1.Max < i2.Min)
            return FF;
        if (i1.Min > i2.Max)
        {
            if (!i1.Def.Item1 || !i2.Def.Item1) return FT;
            return TT;
        }

        return FT;
    }

    public static (bool, bool) Less(Interval i1, Interval i2)
    {
        return Greater(i2, i1);
    }

    public static (bool, bool) LessEqual(Interval i1, Interval i2)
    {
        return Union(Less(i1, i2), Equal(i1, i2));
    }

    public static (bool, bool) GreaterEqual(Interval i1, Interval i2)
    {
        return Union(Greater(i1, i2), Equal(i1, i2));
    }

    #endregion

    #region 数学函数

    public static Interval Sgn(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        i.Min = Sign(i.Min);
        i.Max = Sign(i.Max);
        i.Cont &= i.Min == i.Max;
        return i;
    }

    public static Interval Abs(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.ContainsEqual(0))
        {
            i.Max = sysMath.Max(-i.Min, i.Max);
            i.Min = 0;
            return i;
        }

        if (i.Max < 0)
        {
            (i.Min, i.Max) = (-i.Max, -i.Min);
            return i;
        }

        return i;
    }

    public static Interval Median(Interval i1, Interval i2, Interval i3)
    {
        if (i1.isEmpty() || i2.isEmpty() || i3.isEmpty())
            return EmptyInterval;
        var i = new Interval { Def = And(And(i1.Def, i2.Def), i3.Def), Cont = i1.Cont && i2.Cont && i3.Cont };
        var arr = new double[3] { i1.Min, i2.Min, i3.Min };
        Array.Sort(arr);
        i.Min = arr[1];
        arr = new double[3] { i1.Max, i2.Max, i3.Max };
        Array.Sort(arr);
        i.Max = arr[1];
        return i;
    }

    public static Interval Exp(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        i.Min = sysMath.Pow(E, i.Min);
        i.Max = sysMath.Pow(E, i.Max);
        return i;
    }

    public static Interval Ln(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.Max <= 0)
            return EmptyInterval;
        if (i.Min > 0)
        {
            i.Min = sysMath.Log(i.Min);
            i.Max = sysMath.Log(i.Max);
            return i;
        }

        i.Min = NegativeInfinity;
        i.Max = sysMath.Log(i.Max);
        i.Def = FT;
        return i;
    }

    public static Interval Lg(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.Max <= 0)
            return EmptyInterval;
        if (i.Min > 0)
        {
            i.Min = Log10(i.Min);
            i.Max = Log10(i.Max);
            return i;
        }

        i.Min = NegativeInfinity;
        i.Max = Log10(i.Max);
        i.Def = FT;
        return i;
    }

    public static Interval Log(Interval i1, Interval i2)
    {
        return Divide(Ln(i1), Ln(i2));
    }

    public static Interval Pow(Interval i1, Interval i2)
    {
        return Exp(Multiply(Ln(i1), i2));
    }

    public static Interval Sqrt(Interval i)
    {
        if (i.isEmpty())
            return i;
        if (i.Max < 0)
            return EmptyInterval;
        if (i.Min >= 0)
        {
            i.Min = sysMath.Sqrt(i.Min);
            i.Max = sysMath.Sqrt(i.Max);
            return i;
        }

        i.Min = 0;
        i.Max = sysMath.Sqrt(i.Max);
        i.Def = FT;
        return i;
    }

    public static Interval Root(Interval i, Interval i2)
    {
        if (i2.Min != i2.Max) return EmptyInterval;
        if (i.isEmpty()) return i;
        var num = i2.Max;
        if ((int)num == num && (int)(num / 2) != num / 2)
            return new Interval(sysMath.Pow(i.Min, 1 / num), sysMath.Pow(i.Max, 1 / num));

        if (i.Max < 0) return EmptyInterval;

        if (i.Contains(0))
        {
            i = new Interval(0, sysMath.Pow(i.Max, 1 / num));
            i.Def = FT;
            return i;
        }

        return new Interval(sysMath.Pow(i.Min, 1 / num), sysMath.Pow(i.Max, 1 / num));
    }

    public static Interval Min(Interval i1, Interval i2)
    {
        return new Interval(sysMath.Min(i1.Min, i2.Min), sysMath.Min(i1.Max, i2.Max))
            { Def = And(i1.Def, i2.Def), Cont = i1.Cont & i2.Cont };
    }

    public static Interval Max(Interval i1, Interval i2)
    {
        return new Interval(sysMath.Max(i1.Min, i2.Min), sysMath.Max(i1.Max, i2.Max))
            { Def = And(i1.Def, i2.Def), Cont = i1.Cont & i2.Cont };
    }

    public static Interval Floor(Interval i1)
    {
        i1.Min = sysMath.Floor(i1.Min);
        i1.Max = sysMath.Floor(i1.Max);
        i1.Cont &= i1.Min == i1.Max;
        return i1;
    }

    public static Interval Ceil(Interval i1)
    {
        i1.Min = sysMath.Ceiling(i1.Min);
        i1.Max = sysMath.Ceiling(i1.Max);
        i1.Cont &= i1.Min == i1.Max;
        return i1;
    }

    public static Interval GCD(Interval i1, Interval i2)
    {
        if (i1.isEmpty() || i2.isEmpty())
            return EmptyInterval;
        if (!(i1.IsInterger() && i2.IsInterger()))
            return new Interval(NegativeInfinity, PositiveInfinity);
        return new Interval(GCDBase((int)i1.Min, (int)i2.Min)) { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
    }

    public static Interval LCM(Interval i1, Interval i2)
    {
        if (i1.isEmpty() || i2.isEmpty())
            return EmptyInterval;
        if (!(i1.IsInterger() && i2.IsInterger()))
            return new Interval(NegativeInfinity, PositiveInfinity);
        return new Interval(LCMBase((int)i1.Min, (int)i2.Min)) { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
    }

    private static int GCDBase(int a, int b)
    {
        return a == 0 || b == 0 ? 0 : GCDForInt(a, b);
    }

    private static int GCDForInt(int a, int b)
    {
        return b == 0 ? sysMath.Abs(a) : GCDForInt(b, a % b);
    }

    private static int LCMBase(int a, int b)
    {
        return a == 0 || b == 0 ? 0 : a * b / GCDForInt(a, b);
    }

    public static Interval Factorial(Interval i1)
    {
        if (i1.isEmpty())
            return EmptyInterval;
        if (i1.Max < 0)
            return EmptyInterval;
        if (!i1.IsInterger())
            return new Interval(NegativeInfinity, PositiveInfinity);
        return new Interval(FactorialBase((int)i1.Min)) { Def = i1.Def, Cont = i1.Cont };
    }

    private static double FactorialBase(int num)
    {
        if (num >= FactorialValue.Values.Length)
            return MaxValue;
        return FactorialValue.Values[num];
    }

    public static (bool, bool) Union((bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 || b.Item1, a.Item2 || b.Item2);
    }

    public static (bool, bool) Intersect((bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 && b.Item1, a.Item2 && b.Item2);
    }

    #endregion

    #region 三角函数

    public static Interval Sin(Interval i)
    {
        if (i.isEmpty()) return i;
        var a = i.Min;
        var b = i.Max;
        var minmax = new Interval();
        if (sysMath.Floor((a / sysMath.PI - 0.5) / 2) < sysMath.Floor((b / sysMath.PI - 0.5) / 2))
        {
            minmax.Max = 1;
            i.Max = 1;
        }

        if (sysMath.Floor((a / sysMath.PI + 0.5) / 2) < sysMath.Floor((b / sysMath.PI + 0.5) / 2))
        {
            minmax.Min = 1;
            i.Min = -1;
        }

        if (minmax.Min == 0) i.Min = sysMath.Min(sysMath.Sin(a), sysMath.Sin(b));
        if (minmax.Max == 0) i.Max = sysMath.Max(sysMath.Sin(a), sysMath.Sin(b));
        return i;
    }

    public static Interval Cos(Interval i)
    {
        i.Min += sysMath.PI / 2;
        i.Max += sysMath.PI / 2;
        return Sin(i);
    }

    public static Interval Tan(Interval i)
    {
        if (i.isEmpty()) return EmptyInterval;
        var l = sysMath.Floor((i.Max + sysMath.PI / 2) / sysMath.PI);
        var r = sysMath.Floor((i.Min + sysMath.PI / 2) / sysMath.PI);
        if (l - r == 1) return new Interval(NegativeInfinity, PositiveInfinity) { Def = i.Def, Cont = false };

        if (l - r == 0)
        {
            i.Min = sysMath.Tan(i.Min);
            i.Max = sysMath.Tan(i.Max);
            return i;
        }

        return new Interval(NegativeInfinity, PositiveInfinity) { Def = i.Def, Cont = false };
    }

    public static Interval Cot(Interval i)
    {
        return Tan(Subtract(new Interval(sysMath.PI / 2), i));
    }

    public static Interval ArcTan(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        i.Min = sysMath.Atan(i.Min);
        i.Max = sysMath.Atan(i.Max);
        return i;
    }

    public static Interval ArcSin(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.Max < -1 || i.Min > 1)
            return EmptyInterval;
        if (i.Min < -1 || i.Max > 1)
            i.Def = FT;
        i.Min = sysMath.Asin(sysMath.Max(i.Min, -1));
        i.Min = sysMath.Asin(sysMath.Min(i.Max, 1));
        return i;
    }

    public static Interval ArcCos(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.Max < -1 || i.Min > 1)
            return EmptyInterval;
        if (i.Min < -1 || i.Max > 1)
            i.Def = FT;
        var max = i.Max;
        i.Max = sysMath.Acos(sysMath.Max(i.Min, -1));
        i.Min = sysMath.Acos(sysMath.Min(max, 1));
        return i;
    }

    public static Interval Sinh(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        i.Min = sysMath.Sinh(i.Min);
        i.Min = sysMath.Sinh(i.Max);
        return i;
    }

    public static Interval Cosh(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        if (i.ContainsEqual(0))
        {
            var max = sysMath.Max(i.Max, -i.Min);
            i.Min = 1;
            i.Max = sysMath.Cosh(max);
            return i;
        }

        return new Interval(sysMath.Cosh(i.Min), sysMath.Cosh(i.Max)) { Def = i.Def, Cont = i.Cont };
    }

    public static Interval Tanh(Interval i)
    {
        if (i.isEmpty())
            return EmptyInterval;
        i.Min = sysMath.Tanh(i.Min);
        i.Min = sysMath.Tanh(i.Max);
        return i;
    }

    #endregion

    #region 其他计算

    private static (bool, bool) And((bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 && b.Item1, a.Item2 && b.Item2);
    }

    private static (double, double) GetMinMax4(double n1, double n2, double n3, double n4)
    {
        var minnum = n1;
        var maxnum = n1;
        minnum = minnum < n2 ? minnum : n2;
        maxnum = maxnum > n2 ? maxnum : n2;
        minnum = minnum < n3 ? minnum : n3;
        maxnum = maxnum > n3 ? maxnum : n3;
        minnum = minnum < n4 ? minnum : n4;
        maxnum = maxnum > n4 ? maxnum : n4;
        //若出现NaN 则应当使用以下代码
        /*minnum = Math.Min(minnum, n2);
        maxnum = Math.Max(maxnum, n2);
        minnum = Math.Min(minnum, n3);
        maxnum = Math.Max(maxnum, n3);
        minnum = Math.Min(minnum, n4);
        maxnum = Math.Max(maxnum, n4);*/
        return (minnum, maxnum);
    }

    #endregion
}

internal static class ExMethods
{
    public static bool IsInterger(this Interval Range)
    {
        if (IsInfinity(Range.Min) || IsInfinity(Range.Max))
            return false;
        if (IsNaN(Range.Min) || IsNaN(Range.Max))
            return false;
        return Range.Min == Range.Max && Range.Min == (int)Range.Min;
    }
}