using System.Diagnostics.CodeAnalysis;
using CsGrafeq.Interval.Interface;
using static CsGrafeq.Interval.Def;
using static CsGrafeq.Interval.Extensions.IntervalSetExtension;
using sysMath = System.Math;
using Ranges = CsGrafeq.Collections.NativeBuffer<CsGrafeq.Interval.Range>;

namespace CsGrafeq.Interval;

public readonly struct IntervalSet : IInterval<IntervalSet>
{
    #region 定义

    public static readonly IntervalSet Empty = new() { Intervals = new Ranges(0), Def = FF, IsNumber = false };
    public static readonly Range EmptyRange = new(double.NaN);
    internal Ranges Intervals { get; init; }

    public Def Def { get; init; }

    public double Sup { get; init; }

    public double Inf { get; init; }

    internal bool IsNumber { get; init; }

    internal double Number { get; init; }

    public static IntervalSet CreateWithNumber(double num)
    {
        return Create(num);
    }

    public static IntervalSet Create(double num)
    {
        return new IntervalSet
        {
            IsNumber = true,
            Number = num,
            Inf = num,
            Sup = num,
            Def = TT
        };
    }

    public static IntervalSet Create(double min, double max, Def def)
    {
        Math.SwapIfNotLess(ref min, ref max);
        return new IntervalSet
        {
            Intervals = new Ranges(new Range(min, max)),
            Def = TT,
            IsNumber = false,
            Inf = min,
            Sup = max
        };
    }

    public static IntervalSet Create(double[] nums, Def def)
    {
        Array.Sort(nums);
        Ranges intervals = new((nuint)nums.Length);
        var loc = 0;
        foreach (ref var i in intervals) i = new Range(nums[loc++]);
        return new IntervalSet
        {
            Intervals = intervals,
            Def = def,
            IsNumber = false,
            Inf = nums.Length == 0 ? double.NaN : nums[0],
            Sup = nums.Length == 0 ? double.NaN : nums[nums.Length - 1]
        };
    }

    public static IntervalSet Create(Ranges ranges, Def def)
    {
        return new IntervalSet
        {
            Intervals = ranges,
            Def = def,
            IsNumber = false,
            Inf = ranges.Length == 0 ? double.NaN : ranges[0].Inf,
            Sup = ranges.Length == 0 ? double.NaN : ranges[ranges.Length - 1].Sup
        };
    }

    public static IntervalSet Clone(IntervalSet source)
    {
        return Create(source.Intervals.Clone(), source.Def);
    }

    public double Width => Sup - Inf;
    public bool IsEmpty => !Def.Second;
    public bool IsInValid => IsEmpty || Intervals.Length == 0;

    public bool TryGetNumber(out double number)
    {
        number = Number;
        return IsNumber;
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
        foreach (var r in Intervals)
            if (r.Contains(num))
                return true;
        return false;
    }

    public bool ContainsEqual(double num)
    {
        foreach (var r in Intervals)
            if (r.ContainsEqual(num))
                return true;
        return false;
    }

    public override string ToString()
    {
        var result = "";
        foreach (var r in Intervals) result += r + ",";
        return $"{{Def:{Def},Intervals:[{result}]}}";
    }

    #endregion

    #region 四则运算

    public static IntervalSet operator +(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsEmpty || i2.IsEmpty)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Empty;
        }

        if (i1.IsNumber && i2.IsNumber)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Create(i1.Number + i2.Number);
        }

        if (i1.IsNumber)
            return i2 + i1.Number;
        if (i2.IsNumber)
            return i1 + i2.Number;
        unsafe
        {
            return IntervalSetMethod(i1, i2, &RangeAdd);
        }
    }

    private static Range RangeAdd(Range i1, Range i2)
    {
        return new Range { Inf = i1.Inf + i2.Inf, Sup = i1.Sup + i2.Sup };
    }

    public static IntervalSet operator +(IntervalSet i1, double num)
    {
        foreach (ref var i in i1.Intervals) i = new Range(i.Inf + num, i.Sup + num);
        return i1;
    }

    public static IntervalSet operator -(IntervalSet i1, IntervalSet i2)
    {
        return i1 + -i2;
    }

    public static IntervalSet operator -(IntervalSet i1, double i2)
    {
        return i1 + -i2;
    }

    public static IntervalSet operator -(double i2, IntervalSet i1)
    {
        return -i1 + i2;
    }

    public static IntervalSet operator +(double i2, IntervalSet i1)
    {
        return i1 + i2;
    }

    public static IntervalSet operator *(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsEmpty || i2.IsEmpty)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Empty;
        }

        if (i1.IsNumber && i2.IsNumber)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Create(i1.Number * i2.Number);
        }

        if (i1.IsNumber)
            return i2 * i1.Number;
        if (i2.IsNumber)
            return i1 * i2.Number;
        unsafe
        {
            return IntervalSetMethod(i1, i2, &RangeMultiply);
        }
    }

    private static Range RangeMultiply(Range i1, Range i2)
    {
        if (double.IsNaN(i1.Inf) || double.IsNaN(i2.Inf))
            return EmptyRange;
        if (i1.Inf > 0 && i2.Inf > 0) return new Range { Inf = i1.Inf * i2.Inf, Sup = i1.Sup * i2.Sup };
        if (i1.Sup < 0 && i2.Sup < 0) return new Range { Inf = i1.Sup * i2.Sup, Sup = i1.Inf * i2.Inf };
        var res = Math.GetMinMax4(i1.Inf * i2.Inf, i1.Inf * i2.Sup, i1.Sup * i2.Inf, i1.Sup * i2.Sup);
        return new Range(res.Item1, res.Item2);
    }

    public static IntervalSet operator *(double num, IntervalSet i1)
    {
        return i1 * num;
    }

    public static IntervalSet operator *(IntervalSet i1, double num)
    {
        if (num == 0)
        {
            i1.Intervals.Dispose();
            return Create(0);
        }

        if (double.IsInfinity(num))
        {
            i1.Intervals.Dispose();
            return Create(double.NegativeInfinity, double.PositiveInfinity, TT);
        }

        if (num > 0)
        {
            foreach (ref var i in i1.Intervals) i = new Range(i.Inf * num, i.Sup * num);
            return i1;
        }

        return -(i1 * -num);
    }

    public static IntervalSet operator /(IntervalSet i1, IntervalSet i2)
    {
        var i1i = i1.Intervals;
        var i2i = i2.Intervals;
        if (i1.IsNumber && i2.IsNumber)
        {
            i1i.Dispose();
            i2i.Dispose();
            return Create(i1.Number / i2.Number);
        }

        if (i2.IsNumber) return i1 * (1 / i2.Number);
        if (i1.IsNumber) i1 = Create(new Ranges(new Range(i1.Number, i1.Number)), i1.Def);

        if (i1.IsEmpty || i2.IsEmpty)
        {
            i1i.Dispose();
            i2i.Dispose();
            return Empty;
        }

        var ranges = new Ranges(i2.Intervals.Length + 1);
        foreach (ref var ii in i1.Intervals)
        {
            ii.Inf = ii.Inf == 0 ? double.Epsilon : ii.Inf;
            ii.Sup = ii.Sup == 0 ? -double.Epsilon : ii.Sup;
        }

        nuint loc = 0;
        foreach (ref var i in i2.Intervals)
        {
            if (i.ContainsEqual(0))
            {
                ranges[loc++] = new Range { Inf = double.NegativeInfinity, Sup = 1 / i.Inf };
                ranges[loc++] = new Range { Inf = 1 / i.Sup, Sup = double.PositiveInfinity };
                continue;
            }

            ranges[loc++] = new Range(1 / i.Inf, 1 / i.Sup);
        }

        var res = ranges.SliceAndDispose(0, loc);
        return i1 * Create(res, i2.Def);
    }

    public static IntervalSet operator %(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Create(i1.Number % i2.Number);
        }

        if (i1.IsEmpty || i2.IsEmpty)
        {
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return Empty;
        }

        if (i2.IsNumber)
        {
            var num = i2.Number;
            var ranges = new Ranges(i1.Intervals.Length * 2);
            nuint loc = 0;
            foreach (var i in i1.Intervals)
            {
                var min = sysMath.Floor(i.Inf / num);
                var max = sysMath.Floor(i.Sup / num);
                if (min == max)
                {
                    ranges[loc++] = new Range(i.Inf - min * num, i.Sup - max * num);
                }
                else if (min + 1 == max)
                {
                    ranges[loc++] = new Range(i.Inf - min * num, num);
                    ranges[loc++] = new Range(0, i.Sup - max * num);
                }
                else
                {
                    return Create(0, num, i1.Def & i2.Def);
                }
            }

            if (loc == 0)
            {
                ranges.Dispose();
                return Empty;
            }

            var res = ranges.Slice(0, loc);
            ranges.Dispose();
            return Create(res, FT);
        }

        {
            var q = Floor(i1 / i2);
            var min = q.Inf;
            var max = q.Sup;
            var i = i1 - i2 * q;
            var range = new Range(0, 0);
            if (i2.Sup > 0)
                range.Sup = i2.Sup;
            if (i2.Inf < 0)
                range.Inf = i2.Inf;
            return Create(i.Intervals.SetBounds(range), FT);
        }
    }

    public static Def operator ==(IntervalSet i1, IntervalSet i2)
    {
        if (i1.Def == FF || i2.Def == FF) return FF;
        if (i1.IsNumber && i2.IsNumber)
            return i1.Number == i2.Number ? TT : FF;
        if (i1.IsNumber)
            return i2.ContainsEqual(i1.Number) ? FT : FF;
        if (i2.IsNumber)
            return i1.ContainsEqual(i2.Number) ? FT : FF;
        if (i1.Intervals.Length * i2.Intervals.Length == 0)
            return FF;
        foreach (var j1 in (i1 - i2).Intervals)
            if (j1.ContainsEqual(0))
                return FT;
        return FF;
    }

    public static Def operator !=(IntervalSet i1, IntervalSet i2)
    {
        return ThrowWithMessage<NotImplementedException, Def>(new NotImplementedException());
    }

    public static Def operator <(IntervalSet i1, IntervalSet i2)
    {
        return i2 > i1;
    }

    public static Def operator >(IntervalSet i1, IntervalSet i2)
    {
        return Interval.Create(i1.Inf, i1.Sup, i1.Def) > Interval.Create(i2.Inf, i2.Sup, i2.Def);
    }

    public static Def operator <=(IntervalSet i1, IntervalSet i2)
    {
        return (i1 == i2) & (i1 < i2);
    }

    public static Def operator >=(IntervalSet i1, IntervalSet i2)
    {
        return (i1 == i2) & (i1 > i2);
    }

    public static unsafe IntervalSet operator -(IntervalSet i1)
    {
        i1.Intervals.MonotoneTransformOp(&NumNeg);
        return i1;
    }

    private static double NumNeg(double x)
    {
        return -x;
    }

    #endregion

    #region 数学函数

    public static IntervalSet Sgn(IntervalSet i)
    {
        if (i.IsEmpty)
        {
            i.Intervals.Dispose();
            return Empty;
        }

        if (i.IsNumber)
        {
            i.Intervals.Dispose();
            return Create(sysMath.Sign(i.Number));
        }

        var Ranges = new Range[3];
        nuint loc = 0;
        if (i.Inf < 0)
            Ranges[loc++] = new Range(-1);
        foreach (var j in i.Intervals)
            if (j.ContainsEqual(0))
                Ranges[loc++] = new Range(0);

        if (i.Sup < 0)
            Ranges[loc++] = new Range(1);
        var res = new Ranges(loc);
        for (nuint j = 0; j < loc; j++) res[j] = res[j];
        i.Intervals.Dispose();
        return Create(res, FT);
    }

    public static unsafe IntervalSet Abs(IntervalSet i1)
    {
        if (i1.IsEmpty)
            return Empty;
        if (i1.IsNumber) return Create(sysMath.Abs(i1.Inf));
        return IntervalSetMethod(i1, &RangeAbs);
    }

    private static Range RangeAbs(Range i)
    {
        if (i.ContainsEqual(0))
        {
            i.Sup = sysMath.Max(-i.Inf, i.Sup);
            i.Inf = 0;
        }

        if (i.Sup < 0) (i.Inf, i.Sup) = (-i.Sup, -i.Inf);
        return i;
    }

    public static unsafe IntervalSet Min(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(sysMath.Min(i1.Number, i2.Number));
        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, i2, &RangeMin);
    }

    private static Range RangeMin(Range i1, Range i2)
    {
        return new Range { Inf = sysMath.Min(i1.Inf, i2.Inf), Sup = sysMath.Min(i1.Sup, i2.Sup) };
    }

    public static unsafe IntervalSet Max(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(sysMath.Max(i1.Number, i2.Number));
        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, i2, &RangeMax);
    }

    private static Range RangeMax(Range i1, Range i2)
    {
        return new Range { Inf = sysMath.Max(i1.Inf, i2.Inf), Sup = sysMath.Max(i1.Sup, i2.Sup) };
    }

    public static IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
    {
        if (c1.IsNumber && c2.IsNumber && c3.IsNumber) return Create(DoubleMedian(c1.Number, c2.Number, c3.Number));

        if (c1.IsEmpty || c2.IsEmpty || c3.IsEmpty)
        {
            c1.Intervals.Dispose();
            c2.Intervals.Dispose();
            c3.Intervals.Dispose();
            return Empty;
        }

        nuint loc = 0;
        var res = new Ranges(c1.Intervals.Length * c2.Intervals.Length * c3.Intervals.Length);
        foreach (var i1 in c1.Intervals)
        foreach (var i2 in c2.Intervals)
        foreach (var i3 in c3.Intervals)
            res[loc++] = RangeMedian(i1, i2, i3);

        return Create(res.FormatRanges(), c1.Def & c2.Def & c3.Def);
    }

    public static unsafe IntervalSet Exp(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Exp(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&sysMath.Exp);
        return i1;
    }

    public static IntervalSet Ln(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Log(i1.Number));
        //e为底
        if (i1.IsEmpty)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Sup <= 0)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        nuint current = 0;
        for (nuint i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range.Sup < 0)
                continue;
            if (range.Inf < 0)
                i1.Intervals[current++] = new Range(double.NegativeInfinity, sysMath.Log(range.Sup));
            else
                i1.Intervals[current++] = new Range(sysMath.Log(range.Inf), sysMath.Log(range.Sup));
        }

        var res = i1.Intervals.SliceAndDispose(0, current);
        return Create(res, FT);
    }

    public static IntervalSet Lg(IntervalSet i1)
    {
        if (i1.IsNumber)
        {
            i1.Intervals.Dispose();
            return Create(sysMath.Log(i1.Number));
        }

        //e为底
        if (i1.IsEmpty)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Sup <= 0)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        nuint current = 0;
        for (nuint i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range.Sup < 0)
                continue;
            if (range.Inf < 0)
                i1.Intervals[current++] = new Range(double.NegativeInfinity, sysMath.Log10(range.Sup));
            else
                i1.Intervals[current++] = new Range(sysMath.Log10(range.Inf), sysMath.Log10(range.Sup));
        }

        var res = i1.Intervals.SliceAndDispose(0, current);
        return Create(res, FT);
    }

    public static IntervalSet Log(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(sysMath.Log(i1.Number, i2.Number));
        return Ln(i1) / Ln(i2);
    }

    public static IntervalSet Pow(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(sysMath.Pow(i1.Number, i2.Number));
        if (i2.IsNumber)
        {
            var num = i2.Number;
            if (num == (int)num)
            {
                var inum = (int)num;
                if (inum % 2 == 0) //偶数
                {
                    var ranges = new Ranges(i1.Intervals.Length + 1);
                    nuint loc = 0;
                    foreach (var r in i1.Intervals)
                        if (r.Contains(0))
                        {
                            if (inum < 0)
                            {
                                ranges[loc++] = new Range(sysMath.Pow(r.Inf, num), double.NegativeInfinity);
                                ranges[loc++] = new Range(sysMath.Pow(r.Sup, num), double.PositiveInfinity);
                            }
                            else
                            {
                                ranges[loc++] = new Range(0,
                                    sysMath.Max(sysMath.Pow(r.Sup, num), sysMath.Pow(r.Inf, num)));
                            }
                        }
                        else
                        {
                            ranges[loc++] = new Range(sysMath.Pow(r.Inf, num), sysMath.Pow(r.Sup, num));
                        }

                    if (loc == i1.Intervals.Length)
                        ranges[loc] = EmptyRange;
                    i1.Intervals.Dispose();
                    i2.Intervals.Dispose();
                    return Create(ranges.FormatRanges(), i1.Def);
                }

                for (nuint i = 0; i < i1.Intervals.Length; i++)
                {
                    var r = i1.Intervals[i];
                    i1.Intervals[i] = new Range(sysMath.Pow(r.Inf, num), sysMath.Pow(r.Sup, num));
                }

                return i1;
            }
        }

        return Exp(Ln(i1) * i2);
    }

    public static IntervalSet Sqrt(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Sqrt(i1.Number));
        //e为底
        if (i1.IsEmpty)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Sup <= 0)
        {
            i1.Intervals.Dispose();
            return Empty;
        }

        nuint current = 0;
        for (nuint i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range.Sup < 0)
                continue;
            if (range.Inf < 0)
                i1.Intervals[current++] = new Range(0, sysMath.Sqrt(range.Sup));
            else
                i1.Intervals[current++] = new Range(sysMath.Sqrt(range.Inf), sysMath.Sqrt(range.Sup));
        }

        var res = i1.Intervals.SliceAndDispose(0, current);
        return Create(res, FT);
    }

    public static unsafe IntervalSet Cbrt(IntervalSet num)
    {
        if (num.IsNumber)
            return Create(sysMath.Cbrt(num.Number));
        if (num.IsEmpty)
            return Empty;
        num.Intervals.MonotoneTransform(&sysMath.Cbrt);
        return num;
    }

    private static Range RangeCbrt(Range num)
    {
        num.Inf = sysMath.Cbrt(num.Inf);
        num.Sup = sysMath.Cbrt(num.Sup);
        return num;
    }

    private static Range RangeMedian(Range i1, Range i2, Range i3)
    {
        return new Range { Inf = DoubleMedian(i1.Inf, i2.Inf, i3.Inf), Sup = DoubleMedian(i1.Sup, i2.Sup, i3.Sup) };
    }

    private static double DoubleMedian(double t1, double t2, double t3)
    {
        if ((t1 - t2) * (t2 - t3) > 0) return t2;
        if ((t2 - t1) * (t1 - t3) > 0) return t1;
        return t3;
    }

    public static IntervalSet Floor(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Floor(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        nuint loc = 0;
        var Ranges = new Ranges(len * 2 + 1);
        for (nuint i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (loc < 2)
            {
                var min = sysMath.Floor(range.Inf);
                var max = sysMath.Floor(range.Sup);
                if (min == max)
                {
                    Ranges[i] = new Range(min);
                    continue;
                }

                Ranges[i] = new Range(min);
                Ranges[i] = new Range(min + 1);
                if (max > min + 1)
                    Ranges[i] = new Range(min + 2, max);
                continue;
            }

            for (var j = i; j < len; j++)
                Ranges[loc++] = rfloor(i1.Intervals[j]);
            break;
        }

        var res = i1.Intervals.SliceAndDispose(0, loc);
        i1.Intervals.Dispose();
        return Create(res, FT);
    }

    private static Range rfloor(Range r)
    {
        r.Inf = sysMath.Floor(r.Inf);
        r.Sup = sysMath.Floor(r.Sup);
        return r;
    }

    public static IntervalSet Ceil(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Ceiling(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        nuint loc = 0;
        var Ranges = new Ranges(len * 2 + 1);
        for (nuint i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (loc < 2)
            {
                var min = sysMath.Ceiling(range.Inf);
                var max = sysMath.Ceiling(range.Sup);
                if (min == max)
                {
                    Ranges[loc++] = new Range(min);
                    continue;
                }

                Ranges[loc++] = new Range(min);
                Ranges[loc++] = new Range(min + 1);
                if (max > min + 1)
                    Ranges[loc++] = new Range(min + 2, max);
                continue;
            }

            for (var j = i; j < len; j++)
                Ranges[loc++] = rceil(i1.Intervals[j]);
            break;
        }

        var res = i1.Intervals.SliceAndDispose(0, loc);
        return Create(res, FT);
    }

    private static Range rceil(Range r)
    {
        r.Inf = sysMath.Ceiling(r.Inf);
        r.Sup = sysMath.Ceiling(r.Sup);
        return r;
    }

    public static IntervalSet GCD(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Number == (int)i1.Number && i2.Inf == (int)i2.Inf)
                return Create(Math.GCD((int)i1.Number, (int)i2.Inf));
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return Create(Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return Create(new double[]
            {
                Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.GCD((int)i1.Intervals[1].Inf, (int)i2.Intervals[0].Inf)
            }, FT);
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[1].Inf)
            }, FT);
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.GCD((int)i1.Intervals[0].Inf, (int)i2.Intervals[1].Inf),
                Math.GCD((int)i1.Intervals[1].Inf, (int)i2.Intervals[0].Inf),
                Math.GCD((int)i1.Intervals[1].Inf, (int)i2.Intervals[1].Inf)
            }, FT);
        return Create(1, double.PositiveInfinity, FT);
    }

    public static IntervalSet LCM(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Number == (int)i1.Number && i2.Inf == (int)i2.Inf)
                return Create(Math.LCM((int)i1.Number, (int)i2.Inf));
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return Create(Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return Create(new double[]
            {
                Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.LCM((int)i1.Intervals[1].Inf, (int)i2.Intervals[0].Inf)
            }, FT);
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[1].Inf)
            }, FT);
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[0].Inf),
                Math.LCM((int)i1.Intervals[0].Inf, (int)i2.Intervals[1].Inf),
                Math.LCM((int)i1.Intervals[1].Inf, (int)i2.Intervals[0].Inf),
                Math.LCM((int)i1.Intervals[1].Inf, (int)i2.Intervals[1].Inf)
            }, FT);
        return Create(1, double.PositiveInfinity, FT);
    }

    #endregion

    #region 三角

    public static unsafe IntervalSet Sin(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Sin(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, &RangeSin);
    }

    public static IntervalSet Cos(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Cos(i1.Number));
        return Sin(i1 + sysMath.PI / 2);
    }

    private static Range RangeSin(Range i)
    {
        var a = i.Inf;
        var b = i.Sup;
        var minmax = new Range();
        if (sysMath.Floor((a / sysMath.PI - 0.5) / 2) < sysMath.Floor((b / sysMath.PI - 0.5) / 2))
        {
            minmax.Sup = 1;
            i.Sup = 1;
        }

        if (sysMath.Floor((a / sysMath.PI + 0.5) / 2) < sysMath.Floor((b / sysMath.PI + 0.5) / 2))
        {
            minmax.Inf = 1;
            i.Inf = -1;
        }

        if (minmax.Inf == 0) i.Inf = sysMath.Min(sysMath.Sin(a), sysMath.Sin(b));
        if (minmax.Sup == 0) i.Sup = sysMath.Max(sysMath.Sin(a), sysMath.Sin(b));
        return i;
    }

    public static IntervalSet Tan(IntervalSet i1)
    {
        if (i1.IsNumber)
        {
            i1.Intervals.Dispose();
            return Create(sysMath.Tan(i1.Number));
        }

        if (i1.IsEmpty)
            return Empty;
        var ranges = new Ranges(i1.Intervals.Length * 2);
        nuint loc = 0;
        foreach (var i in i1.Intervals)
        {
            var r = (int)sysMath.Floor((i.Sup + sysMath.PI / 2) / sysMath.PI);
            var l = (int)sysMath.Floor((i.Inf + sysMath.PI / 2) / sysMath.PI);
            if (r - l == 1)
            {
                ranges[loc++] = new Range { Sup = sysMath.Tan(i.Sup), Inf = double.NegativeInfinity };
                ranges[loc++] = new Range { Inf = sysMath.Tan(i.Inf), Sup = double.PositiveInfinity };
            }
            else if (l == r)
            {
                ranges[loc++] = new Range { Inf = sysMath.Tan(i.Inf), Sup = sysMath.Tan(i.Sup) };
            }
            else
            {
                return Create(double.NegativeInfinity, double.PositiveInfinity, i1.Def);
            }
        }

        var res = ranges.SliceAndDispose(0, loc).FormatRanges();
        return Create(res, FT);
    }

    public static IntervalSet Cot(IntervalSet i1)
    {
        return -Tan(i1 + sysMath.PI / 2);
    }

    public static unsafe IntervalSet ArcTan(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Atan(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&sysMath.Atan);
        return i1;
    }

    public static unsafe IntervalSet ArcCos(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Acos(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        if (i1.Sup < -1 || i1.Number > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { Inf = -1, Sup = 1 });
        res.MonotoneTransformOp(&sysMath.Acos);
        return Create(res, i1.Sup > 1 || i1.Number < -1 ? FT : i1.Def);
    }

    public static unsafe IntervalSet ArcSin(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Asin(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        if (i1.Sup < -1 || i1.Inf > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { Inf = -1, Sup = 1 });
        res.MonotoneTransform(&sysMath.Asin);
        return Create(res, i1.Sup > 1 || i1.Inf < -1 ? FT : i1.Def);
    }

    public static unsafe IntervalSet Sinh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Sinh(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&sysMath.Sinh);
        return i1;
    }

    public static IntervalSet Cosh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Cosh(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        for (nuint i = 0; i < i1.Intervals.Length; i++)
        {
            ref var r = ref i1.Intervals[i];
            r = r.Contains(0)
                ? new Range { Sup = sysMath.Cosh(sysMath.Max(r.Sup, -r.Inf)), Inf = 1 }
                : new Range(sysMath.Cosh(r.Inf), sysMath.Cosh(r.Sup));
        }

        return Create(i1.Intervals.FormatRanges(), i1.Def);
    }

    public static unsafe IntervalSet Tanh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(sysMath.Tanh(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&sysMath.Tanh);
        return i1;
    }

    public static unsafe IntervalSet ArcCosh(IntervalSet num)
    {
        if (num.IsNumber) return Create(sysMath.Acosh(num.Number));
        var min = num.Inf;
        var res = num.Intervals.SetBounds(new Range(1, double.PositiveInfinity));
        var min1 = double.NaN;
        if (res.Length > 0)
            min1 = res[0].Inf;
        res.MonotoneTransformOp(&sysMath.Acosh);
        return Create(res, min == min1 ? num.Def : FT);
    }

    public static unsafe IntervalSet ArcSinh(IntervalSet num)
    {
        if (num.IsNumber) return Create(sysMath.Asinh(num.Number));
        num.Intervals.MonotoneTransform(&sysMath.Asinh);
        return num;
    }

    public static IntervalSet ArcTanh(IntervalSet num)
    {
        if (num.IsNumber) return Create(sysMath.Atanh(num.Number));
        //e为底
        if (num.IsEmpty)
        {
            num.Intervals.Dispose();
            return Empty;
        }

        var len = num.Intervals.Length;
        if (num.Sup <= -1 || num.Inf >= 1)
        {
            num.Intervals.Dispose();
            return Empty;
        }

        nuint current = 0;
        for (nuint i = 0; i < len; i++)
        {
            var range = num.Intervals[i];
            if (range.Sup < -1 || range.Inf > 1)
                continue;
            range.Inf = sysMath.Max(-1, range.Inf);
            range.Sup = sysMath.Min(1, range.Sup);
            num.Intervals[current++] = new Range(sysMath.Atanh(range.Inf), sysMath.Atanh(range.Sup));
        }

        var res = num.Intervals.SliceAndDispose(0, current);
        return Create(res, FT);
    }

    #endregion

    [DoesNotReturn]
    private static TResult ThrowWithMessage<TException, TResult>(TException exception) where TException : Exception
    {
        throw exception;
    }
}