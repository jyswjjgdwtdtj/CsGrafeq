using static CsGrafeq.TupperInterval.IInterval;
using sysMath = System.Math;

namespace CsGrafeq.TupperInterval;

/// <summary>
///     1.对于所能达到的最小区间，计算结果必须绝对精确
///     2.对于任何区间计算，计算的结果必须包含实际结果
/// </summary>
public static class IntervalSetMath
{
    private const double PI = sysMath.PI;
    private const double E = sysMath.E;
    private static readonly IntervalSet EmptyIntervalSet = new(double.NaN) { Def = FF, IsNumber = false };
    private static readonly double neginf = double.NegativeInfinity;
    private static readonly double posinf = double.PositiveInfinity;
    private static Range EmptyRange => new(double.NaN);

    public static IntervalSet New(double num)
    {
        return new IntervalSet(num);
    }

    private static unsafe Range[] MonotoneTransform(Range[] ranges, Func<double, double> callback, bool dr)
    {
        var Ranges = new Range[ranges.Length];
        var loc = 0;
        fixed (Range* first = ranges, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + ranges.Length; i++)
                *(Rangesfirst + loc++) =
                    dr
                        ? new Range { Max = callback.Invoke(i->Max), Min = callback.Invoke(i->Min) }
                        : new Range { Min = callback.Invoke(i->Max), Max = callback.Invoke(i->Min) };
        }

        return Ranges;
    }

    private static (bool, bool) And((bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 && b.Item1, a.Item2 && b.Item2);
    }

    private static Range GetMinMax4(double n1, double n2, double n3, double n4)
    {
        var minnum = n1;
        var maxnum = n1;
        minnum = minnum < n2 ? minnum : n2;
        maxnum = maxnum > n2 ? maxnum : n2;
        minnum = minnum < n3 ? minnum : n3;
        maxnum = maxnum > n3 ? maxnum : n3;
        minnum = minnum < n4 ? minnum : n4;
        maxnum = maxnum > n4 ? maxnum : n4;
        /*minnum = sysMath.Min(minnum, n2);
        maxnum = sysMath.Max(maxnum, n2);
        minnum = sysMath.Min(minnum, n3);
        maxnum = sysMath.Max(maxnum, n3);
        minnum = sysMath.Min(minnum, n4);
        maxnum = sysMath.Max(maxnum, n4);*/
        return new Range { Min = minnum, Max = maxnum };
    }

    public static IntervalSet GetIntervalSetFromRangeArray(Range[] Ranges)
    {
        return GetIntervalSetFromRangeArray(Ranges, TT, true);
    }

    public static unsafe IntervalSet GetIntervalSetFromRangeArray(Range[] Ranges, (bool, bool) def, bool cont)
    {
        //删除空区间
        var nRanges = new Range[Ranges.Length];
        Array.Copy(Ranges, nRanges, Ranges.Length);
        var count = 0;
        fixed (Range* first = nRanges, Rangesfirst = Ranges)
        {
            for (var f = first; f < first + Ranges.Length; f++)
                if (!(double.IsNaN(f->Min) || double.IsNaN(f->Max)))
                    *(Rangesfirst + count++) = *f;
        }

        if (count == 0)
            return EmptyIntervalSet;
        //对区间排序
        for (var i = 0; i < count - 1; i++)
        for (var j = 0; j < count - i - 1; j++)
            if (Ranges[j].Min > Ranges[j + 1].Min)
                (Ranges[j], Ranges[j + 1]) = (Ranges[j + 1], Ranges[j]);

        var latest = Ranges[0];
        var intervals = new Range[count];
        var loc = 0;
        for (var i = 1; i < count; i++)
        {
            var interval = Ranges[i];
            if (latest.ContainsEqual(interval.Min))
            {
                if (!latest.ContainsEqual(interval.Max)) latest.Max = interval.Max;
            }
            else
            {
                intervals[loc++] = latest;
                latest = Ranges[i];
            }
        }

        intervals[loc++] = latest;
        Array.Resize(ref intervals, loc);
        return new IntervalSet(intervals, def, cont);
    }

    private static IntervalSet IntervalSetMethod(IntervalSet i1, IntervalSet i2, RangeHandler2 handler)
    {
        var Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
        var loc = 0;
        unsafe
        {
            fixed (Range* ptr = Ranges)
            {
                foreach (var r1 in i1.Intervals)
                foreach (var r2 in i2.Intervals)
                    *(ptr + loc++) = handler(&r1, &r2);
            }
        }

        i1.Intervals = [];
        i2.Intervals = [];
        return GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
    }

    private static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, RangeHandler1 handler)
    {
        var Ranges = new Range[i1.Intervals.Length];
        var loc = 0;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
                *(Rangesfirst + loc++) = handler(i);
        }

        i1.Intervals = [];
        return GetIntervalSetFromRangeArray(Ranges, i1.Def, i1.Cont);
    }

    private unsafe delegate Range RangeHandler3(Range* r1, Range* r2, Range* r3);

    private unsafe delegate Range RangeHandler2(Range* r1, Range* r2);

    private unsafe delegate Range RangeHandler1(Range* r1);

    #region 四则运算

    public static unsafe IntervalSet Add(this IntervalSet i1, IntervalSet i2)
    {
        if (!(i1.Def.Item2 && i2.Def.Item2))
            return EmptyIntervalSet;
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(i1.Min + i2.Min);

        return IntervalSetMethod(i1, i2,
            (i1, i2) => new Range { Min = i1->Min + i2->Min, Max = i1->Max + i2->Max });
    }

    public static IntervalSet Neg(IntervalSet i)
    {
        if (i.IsNumber)
            return new IntervalSet(-i.Min);
        var len = i.Intervals.Length;
        var Ranges = new Range[i.Intervals.Length];
        for (var j = 0; j < i.Intervals.Length; j++) Ranges[j] = RangeNeg(i.Intervals[len - 1 - j]);
        i.Intervals = Ranges;
        return i;
    }

    private static Range RangeNeg(Range i)
    {
        return new Range { Max = -i.Min, Min = -i.Max };
    }

    public static IntervalSet Subtract(this IntervalSet i1, IntervalSet i2)
    {
        return Add(i1, Neg(i2));
    }

    public static unsafe IntervalSet Multiply(this IntervalSet i1, IntervalSet i2)
    {
        if (!(i1.Def.Item2 && i2.Def.Item2))
            return EmptyIntervalSet;
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(i1.Min * i2.Min);
        if (i1.IsNumber)
            return NumberMultiply(i2, i1.Min);
        if (i2.IsNumber)
            return NumberMultiply(i1, i2.Min);
        return IntervalSetMethod(i1, i2, RangeMultiply);
    }

    public static IntervalSet NumberMultiply(IntervalSet i1, double num)
    {
        if (num == 0)
            return new IntervalSet(0);
        if (double.IsInfinity(num)) return new IntervalSet(neginf, posinf);
        var rs = new Range[i1.Intervals.Length];
        for (var i = 0; i < i1.Intervals.Length; i++)
        {
            var r = i1.Intervals[i];
            rs[i] = new Range(r.Min * num, r.Max * num);
        }

        i1.Intervals = rs;
        return i1;
    }

    private static unsafe Range RangeMultiply(Range* i1, Range* i2)
    {
        if (double.IsNaN(i1->Min) || double.IsNaN(i2->Min))
            return EmptyRange;
        (double Min, double Max) res;
        if (i1->Min > 0 && i2->Min > 0) return new Range(i1->Min * i2->Min, i1->Max * i2->Max);
        if (i1->Max < 0 && i2->Max < 0) return new Range(i1->Max * i2->Max, i1->Min * i2->Min);

        return GetMinMax4(i1->Min * i2->Min, i1->Min * i2->Max, i1->Max * i2->Min, i1->Max * i2->Max);
    }

    public static IntervalSet Divide(this IntervalSet i1, IntervalSet i2)
    {
        //a/b=>a*(1/b)
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(i1.Min / i2.Min);
        if (!(i1.Def.Item2 && i2.Def.Item2))
            return EmptyIntervalSet;
        if (i2.Intervals.Length == 1)
            if (i2.Intervals[0].Min == 0 && i2.Intervals[0].Max == 0)
                return EmptyIntervalSet;
        var ranges = new OnlyAddList<Range>(5);
        Range Range1, Range2;
        for (var ii = 0; ii < i1.Intervals.Length; ii++)
        {
            i1.Intervals[ii].Max = i1.Intervals[ii].Max == 0 ? -double.Epsilon : i1.Intervals[ii].Max;
            i1.Intervals[ii].Min = i1.Intervals[ii].Min == 0 ? double.Epsilon : i1.Intervals[ii].Min;
        }

        unsafe
        {
            foreach (var i in i2.Intervals)
            {
                if (i.ContainsEqual(0))
                {
                    Range1 = new Range { Min = neginf, Max = 1 / i.Min };
                    Range2 = new Range { Min = 1 / i.Max, Max = posinf };
                    foreach (var j in i1.Intervals)
                    {
                        ranges.Add(RangeMultiply(&Range1, &j));
                        ranges.Add(RangeMultiply(&Range2, &j));
                    }

                    continue;
                }

                Range1 = new Range(1 / i.Min, 1 / i.Max);
                foreach (var j in i1.Intervals) ranges.Add(RangeMultiply(&Range1, &j));
            }
        }

        var iss = GetIntervalSetFromRangeArray(ranges.GetArray(), And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        return iss;
    }

    public static IntervalSet Mod(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(i1.Min % i2.Min);
        if (!i1.Def.Item2 || !i2.Def.Item2)
            return EmptyIntervalSet;
        if (i2.IsNumber)
        {
            var num = i2.Min;
            var ranges = new Range[i1.Intervals.Length * 2];
            var loc = 0;
            foreach (var i in i1.Intervals)
            {
                var min = sysMath.Floor(i.Min / num);
                var max = sysMath.Floor(i.Max / num);
                if (min == max)
                {
                    ranges[loc++] = new Range(i.Min - min * num, i.Max - max * num);
                }
                else if (min + 1 == max)
                {
                    ranges[loc++] = new Range(i.Min - min * num, num);
                    ranges[loc++] = new Range(0, i.Max - max * num);
                }
                else
                {
                    return new IntervalSet(0, num);
                }
            }

            Array.Resize(ref ranges, loc);
            if (loc == 0)
                return EmptyIntervalSet;
            return new IntervalSet(ranges, FT, false);
        }

        {
            var q = Floor(Divide(i1, i2));
            var min = q.Min;
            var max = q.Max;
            var i = Subtract(i1, Multiply(i2, q));
            var range = new Range(0, 0);
            if (i2.Max > 0)
                range.Max = i2.Max;
            if (i2.Min < 0)
                range.Min = i2.Min;
            return SetBound(i, range);
        }
    }

    private static IntervalSet SetBound(IntervalSet set, Range r)
    {
        var rs = new Range[set.Intervals.Length];
        var loc = 0;
        foreach (var i in set.Intervals)
        {
            if (i.Max <= r.Min || i.Min >= r.Max)
                continue;
            rs[loc++] = new Range(sysMath.Max(i.Min, r.Min), sysMath.Min(i.Max, r.Max));
        }

        Array.Resize(ref rs, loc);
        set.Intervals = rs;
        if (rs.Length == 0)
            return EmptyIntervalSet;
        return set;
    }

    private static Range[] SetBound(Range[] set, Range r)
    {
        var rs = new Range[set.Length];
        var loc = 0;
        foreach (var i in set)
        {
            if (i.Max <= r.Min || i.Min >= r.Max)
                continue;
            rs[loc++] = new Range(sysMath.Max(i.Min, r.Min), sysMath.Min(i.Max, r.Max));
        }

        Array.Resize(ref rs, loc);
        return rs;
    }

    #endregion

    #region 数学函数

    public static unsafe IntervalSet Sgn(this IntervalSet i)
    {
        if (!i.Def.Item2)
            return EmptyIntervalSet;
        if (i.IsNumber) return new IntervalSet(Math.Sgn(i.Min));
        var Ranges = new Range[3];
        var loc = 0;
        fixed (Range* first = i.Intervals)
        {
            if (first->Min < 0)
                Ranges[loc++] = new Range(-1);
            for (var f = first; f < first + i.Intervals.Length; f++)
                if (f->Min <= 0 && f->Max >= 0)
                {
                    Ranges[loc++] = new Range(0);
                    break;
                }

            if ((first + i.Intervals.Length - 1)->Max > 0)
                Ranges[loc++] = new Range(1);
            while (loc < 3)
                Ranges[loc++] = EmptyRange;
        }

        i.Intervals = Ranges;
        return i;
    }

    public static unsafe IntervalSet Abs(this IntervalSet i1)
    {
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        if (i1.IsNumber) return new IntervalSet(sysMath.Abs(i1.Min));

        return IntervalSetMethod(i1, RangeAbs);
    }

    private static unsafe Range RangeAbs(Range* i)
    {
        if (i->ContainsEqual(0))
        {
            i->Max = sysMath.Max(-i->Min, i->Max);
            i->Min = 0;
        }

        if (i->Max < 0) (i->Min, i->Max) = (-i->Max, -i->Min);
        return *i;
    }

    public static unsafe IntervalSet Min(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(sysMath.Min(i1.Min, i2.Min));
        if (!(i1.Def.Item2 && i2.Def.Item2))
            return EmptyIntervalSet;
        return IntervalSetMethod(i1, i2, RangeMin);
    }

    private static unsafe Range RangeMin(Range* i1, Range* i2)
    {
        return new Range { Min = sysMath.Min(i1->Min, i2->Min), Max = sysMath.Min(i1->Max, i2->Max) };
    }

    public static unsafe IntervalSet Max(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(sysMath.Max(i1.Min, i2.Min));
        if (!(i1.Def.Item2 && i2.Def.Item2))
            return EmptyIntervalSet;
        return IntervalSetMethod(i1, i2, RangeMax);
    }

    private static unsafe Range RangeMax(Range* i1, Range* i2)
    {
        return new Range { Min = sysMath.Max(i1->Min, i2->Min), Max = sysMath.Max(i1->Max, i2->Max) };
    }

    public static unsafe IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
    {
        if (c1.IsNumber && c2.IsNumber && c3.IsNumber)
        {
            var da = new[] { c1.Min, c2.Min, c3.Min };
            Array.Sort(da);
            return new IntervalSet(da[1]);
        }

        if (!(c1.Def.Item2 && c2.Def.Item2 && c3.Def.Item2))
            return EmptyIntervalSet;
        var Ranges = new Range[5];
        var len = 5;
        var loc = 0;
        var c1len = c1.Intervals.Length;
        var c2len = c2.Intervals.Length;
        var c3len = c3.Intervals.Length;
        fixed (Range* c1first = c1.Intervals, c2first = c2.Intervals, c3first = c3.Intervals)
        {
            for (var j1 = c1first; j1 < c1first + c1len; j1++)
            for (var j2 = c2first; j2 < c2first + c2len; j2++)
            for (var j3 = c3first; j3 < c3first + c3len; j3++)
            {
                Ranges[loc++] = RangeMedian(j1, j2, j3);
                if (loc == len)
                {
                    len *= 2;
                    Array.Resize(ref Ranges, len);
                }
            }
        }

        Array.Resize(ref Ranges, loc);
        return GetIntervalSetFromRangeArray(Ranges, And(And(c1.Def, c2.Def), c3.Def), c1.Cont && c2.Cont && c3.Cont);
    }

    public static IntervalSet Exp(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Exp(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        i1.Intervals = MonotoneTransform(i1.Intervals, sysMath.Exp, true);
        return i1;
    }

    public static unsafe IntervalSet Ln(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Log(i1.Min));
        //e为底
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Max <= 0)
            return EmptyIntervalSet;
        var Ranges = new Range[len];
        var loc = 0;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
            {
                if (i->Max <= 0)
                    continue;
                if (i->Min <= 0)
                {
                    *(Rangesfirst + loc++) = new Range(neginf, sysMath.Log(i->Max));
                    i1.Def.Item1 = false;
                }
                else
                {
                    *(Rangesfirst + loc++) = new Range(sysMath.Log(i->Min), sysMath.Log(i->Max));
                }
            }
        }

        i1.Intervals = Ranges;
        return i1;
    }

    public static unsafe IntervalSet Lg(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Log10(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Max <= 0)
            return EmptyIntervalSet;
        var Ranges = new Range[len];
        var loc = 0;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
            {
                if (i->Max <= 0)
                    continue;
                if (i->Min <= 0)
                {
                    *(Rangesfirst + loc++) = new Range(neginf, sysMath.Log10(i->Max));
                    i1.Def.Item1 = false;
                }
                else
                {
                    *(Rangesfirst + loc++) = new Range(sysMath.Log10(i->Min), sysMath.Log10(i->Max));
                }
            }
        }

        i1.Intervals = Ranges;
        return i1;
    }

    public static IntervalSet Log(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(sysMath.Log(i1.Min, i2.Min));
        return Divide(Ln(i1), Ln(i2));
    }

    public static IntervalSet Pow(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return new IntervalSet(sysMath.Pow(i1.Min, i2.Min));
        if (i2.IsNumber)
        {
            var num = i2.Min;
            if (num == (int)num)
            {
                var inum = (int)num;
                if (inum % 2 == 0) //偶数
                {
                    var ranges = new Range[i1.Intervals.Length + 1];
                    var loc = 0;
                    foreach (var r in i1.Intervals)
                        if (r.Contains(0))
                        {
                            if (inum < 0)
                            {
                                ranges[loc++] = new Range(sysMath.Pow(r.Min, num), neginf);
                                ranges[loc++] = new Range(sysMath.Pow(r.Max, num), posinf);
                            }
                            else
                            {
                                ranges[loc++] = new Range(0,
                                    sysMath.Max(sysMath.Pow(r.Max, num), sysMath.Pow(r.Min, num)));
                            }
                        }
                        else
                        {
                            ranges[loc++] = new Range(sysMath.Pow(r.Min, num), sysMath.Pow(r.Max, num));
                        }

                    Array.Resize(ref ranges, loc);
                    return GetIntervalSetFromRangeArray(ranges);
                }
                else
                {
                    var ranges = new Range[i1.Intervals.Length + 1];
                    var loc = 0;
                    foreach (var r in i1.Intervals)
                        ranges[loc++] = new Range(sysMath.Pow(r.Min, num), sysMath.Pow(r.Max, num));
                    return new IntervalSet(ranges);
                }
            }
        }

        return Exp(Multiply(Ln(i1), i2));
    }

    public static unsafe IntervalSet Sqrt(this IntervalSet i1)
    {
        if (i1.IsNumber)
        {
            if (i1.Min < 0)
                return EmptyIntervalSet;
            return new IntervalSet(sysMath.Sqrt(i1.Min));
        }

        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Max <= 0)
            return EmptyIntervalSet;
        var Ranges = new Range[len];
        var loc = 0;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
            {
                if (i->Max <= 0)
                    continue;
                if (i->Min <= 0)
                {
                    *(Rangesfirst + loc++) = new Range(0, sysMath.Sqrt(i->Max));
                    i1.Def.Item1 = false;
                }
                else
                {
                    *(Rangesfirst + loc++) = new Range(sysMath.Sqrt(i->Min), sysMath.Sqrt(i->Max));
                }
            }
        }

        i1.Intervals = Ranges;
        if (i1.Intervals.Length == 0)
            i1.Def.Item2 = false;
        return i1;
    }

    public static IntervalSet Root(this IntervalSet i1, IntervalSet i2)
    {
        if (!i2.IsNumber) throw new ArgumentException(nameof(i2) + " should be a positive integer");
        var d = i2.Min;
        if (d <= 0 || d != (int)d) throw new ArgumentException(nameof(i2) + " should be a positive integer");
        var r = (int)d;
        if (i1.IsNumber)
        {
            if (i1.Min < 0)
                return EmptyIntervalSet;
            return new IntervalSet(sysMath.Pow(i1.Min, 1 / d));
        }

        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1].Max <= 0)
            return EmptyIntervalSet;
        i1.Intervals =
            r / 2 == d / 2
                ? MonotoneTransform(SetBound(i1.Intervals, new Range(0, posinf)), num => sysMath.Pow(num, 1 / d), true)
                : MonotoneTransform(i1.Intervals, num => sysMath.Pow(num, 1 / d), true);
        return i1;
    }

    private static unsafe Range RangeMedian(Range* i1, Range* i2, Range* i3)
    {
        var i = new Range();
        var arr = new double[3] { i1->Min, i2->Min, i3->Min };
        Array.Sort(arr);
        i.Min = arr[1];
        arr = new double[3] { i1->Max, i2->Max, i3->Max };
        Array.Sort(arr);
        i.Max = arr[1];
        return i;
    }

    public static unsafe IntervalSet Floor(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Floor(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        var loc = 0;
        var Ranges = new Range[len * 2 + 1];
        fixed (Range* first = i1.Intervals)
        {
            for (var i = 0; i < len; i++)
            {
                if (loc < 2)
                {
                    var min = sysMath.Floor((first + i)->Min);
                    var max = sysMath.Floor((first + i)->Max);
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
                    Ranges[loc++] = rfloor(*(first + j));
                break;
            }
        }

        Array.Resize(ref Ranges, loc);
        i1.Intervals = Ranges;
        return i1;
    }

    private static Range rfloor(Range r)
    {
        r.Min = sysMath.Floor(r.Min);
        r.Max = sysMath.Floor(r.Max);
        return r;
    }

    public static unsafe IntervalSet Ceil(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Ceiling(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var len = i1.Intervals.Length;
        var loc = 0;
        var Ranges = new Range[len * 2 + 1];
        fixed (Range* first = i1.Intervals)
        {
            for (var i = 0; i < len; i++)
            {
                if (loc < 2)
                {
                    var min = sysMath.Ceiling((first + i)->Min);
                    var max = sysMath.Ceiling((first + i)->Max);
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
                    Ranges[loc++] = rceil(*(first + j));
                break;
            }
        }

        Array.Resize(ref Ranges, loc);
        i1.Intervals = Ranges;
        return i1;
    }

    private static Range rceil(Range r)
    {
        r.Min = sysMath.Ceiling(r.Min);
        r.Max = sysMath.Ceiling(r.Max);
        return r;
    }

    public static IntervalSet GCD(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Min == (int)i1.Min && i2.Min == (int)i2.Min)
                return new IntervalSet(GCDBase((int)i1.Min, (int)i2.Min));
            throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        }

        if (!i1.Def.Item2 || !i2.Def.Item2)
            return EmptyIntervalSet;
        if (!(i1.Intervals[0].IsInterger() && i2.Intervals[0].IsInterger()))
            throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return new IntervalSet(GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return new IntervalSet(new double[]
            {
                GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min)
            });
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return new IntervalSet(new double[]
            {
                GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min)
            });
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return new IntervalSet(new double[]
            {
                GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min),
                GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min),
                GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[1].Min)
            });
        return new IntervalSet(1, posinf);
    }

    public static IntervalSet LCM(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Min == (int)i1.Min && i2.Min == (int)i2.Min)
                return new IntervalSet(LCMBase((int)i1.Min, (int)i2.Min));
            throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        }

        if (!i1.Def.Item2 || !i2.Def.Item2)
            return EmptyIntervalSet;
        if (!(i1.Intervals[0].IsInterger() && i2.Intervals[0].IsInterger()))
            throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return new IntervalSet(LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return new IntervalSet(new double[]
            {
                LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min)
            });
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return new IntervalSet(new double[]
            {
                LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min)
            });
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return new IntervalSet(new double[]
            {
                LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min),
                LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min),
                LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[1].Min)
            });
        return new IntervalSet(1, posinf);
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

    public static IntervalSet Factorial(this IntervalSet i1)
    {
        if (i1.IsNumber)
        {
            var num = i1.Min;
            if (num < 0)
                throw new Exception();
            if (num == (int)num)
                return new IntervalSet(FactorialBase((int)num));
            throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        }

        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        if (i1.Max < 0)
            return EmptyIntervalSet;
        if (!i1.Intervals[0].IsInterger()) throw new ArgumentException("参数需经过Floor,Ceil函数处理");
        if (i1.Intervals.Length == 1) return new IntervalSet(FactorialBase((int)i1.Intervals[0].Min));
        if (i1.Intervals.Length == 2)
            return new IntervalSet(new[]
            {
                FactorialBase((int)i1.Intervals[0].Min),
                FactorialBase((int)i1.Intervals[1].Min)
            });
        return new IntervalSet(1, posinf);
    }


    private static double FactorialBase(int num)
    {
        if (num >= FactorialValue.Values.Length)
            return double.MaxValue;
        return FactorialValue.Values[num];
    }

    internal static unsafe IntervalSet AddNumber(IntervalSet i1, double i2)
    {
        var len = i1.Intervals.Length;
        var Ranges = new Range[len];
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = 0; i < len; i++) *(Rangesfirst + i) = new Range((first + i)->Min + i2, (first + i)->Max + i2);
        }

        i1.Intervals = Ranges;
        return i1;
    }

    #endregion

    #region 三角函数

    public static unsafe IntervalSet Sin(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Sin(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        return IntervalSetMethod(i1, RangeSin);
    }

    public static IntervalSet Cos(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Cos(i1.Min));
        return Sin(AddNumber(i1, PI / 2));
    }

    private static unsafe Range RangeSin(Range* i)
    {
        var a = i->Min;
        var b = i->Max;
        var minmax = new Range();
        if (sysMath.Floor((a / PI - 0.5) / 2) < sysMath.Floor((b / PI - 0.5) / 2))
        {
            minmax.Max = 1;
            i->Max = 1;
        }

        if (sysMath.Floor((a / PI + 0.5) / 2) < sysMath.Floor((b / PI + 0.5) / 2))
        {
            minmax.Min = 1;
            i->Min = -1;
        }

        if (minmax.Min == 0) i->Min = sysMath.Min(sysMath.Sin(a), sysMath.Sin(b));
        if (minmax.Max == 0) i->Max = sysMath.Max(sysMath.Sin(a), sysMath.Sin(b));
        return *i;
    }

    public static unsafe IntervalSet Tan(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Tan(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var Ranges = new Range[5];
        var loc = 0;
        var len = 5;
        fixed (Range* first = i1.Intervals)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
            {
                var l = sysMath.Floor((i->Max + PI / 2) / PI);
                var r = sysMath.Floor((i->Min + PI / 2) / PI);
                if (l - r == 1)
                {
                    Ranges[loc++] = new Range { Min = sysMath.Tan(i->Min), Max = posinf };
                    if (loc == len)
                    {
                        len *= 2;
                        Array.Resize(ref Ranges, len);
                    }

                    Ranges[loc++] = new Range { Max = sysMath.Tan(i->Max), Min = neginf };
                    if (loc == len)
                    {
                        len *= 2;
                        Array.Resize(ref Ranges, len);
                    }

                    continue;
                }

                if (l == r)
                {
                    Ranges[loc++] = new Range { Min = sysMath.Tan(i->Min), Max = sysMath.Tan(i->Max) };
                    if (loc == len)
                    {
                        len *= 2;
                        Array.Resize(ref Ranges, len);
                    }

                    continue;
                }

                return new IntervalSet(neginf, posinf) { Def = i1.Def, Cont = false };
            }
        }

        Array.Resize(ref Ranges, loc);
        return GetIntervalSetFromRangeArray(Ranges, i1.Def, false);
    }

    public static unsafe IntervalSet Cot(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(1 / sysMath.Tan(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var Ranges = new Range[i1.Intervals.Length];
        var loc = 0;
        var p = PI / 2;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
                *(Rangesfirst + loc++) = new Range { Max = p - i->Min, Min = p - i->Max };
        }

        i1.Intervals = Ranges;
        return Tan(i1);
    }

    public static IntervalSet ArcTan(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Atan(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        i1.Intervals = MonotoneTransform(i1.Intervals, sysMath.Atan, true);
        return i1;
    }

    public static IntervalSet ArcCos(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Acos(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        if (i1.Max < -1 || i1.Min > 1)
            return EmptyIntervalSet;
        if (i1.Max > 1 || i1.Min < -1)
            i1.Def = FT;
        i1.Intervals = MonotoneTransform(SetBound(i1.Intervals, new Range { Min = -1, Max = 1 }), sysMath.Acos, false);
        return i1;
    }

    public static IntervalSet ArcSin(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Asin(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        if (i1.Max < -1 || i1.Min > 1)
            return EmptyIntervalSet;
        if (i1.Max > 1 || i1.Min < -1)
            i1.Def = FT;
        i1.Intervals = MonotoneTransform(SetBound(i1.Intervals, new Range { Min = -1, Max = 1 }), sysMath.Asin, true);
        return i1;
    }

    public static IntervalSet Sinh(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Sinh(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        i1.Intervals = MonotoneTransform(i1.Intervals, sysMath.Sinh, true);
        return i1;
    }

    public static unsafe IntervalSet Cosh(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Cosh(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        var Ranges = new Range[i1.Intervals.Length];
        var loc = 0;
        fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
        {
            for (var i = first; i < first + i1.Intervals.Length; i++)
                *(Rangesfirst + loc++) = i->ContainsEqual(0)
                    ? new Range { Max = sysMath.Cosh(sysMath.Max(i->Max, -i->Min)), Min = 1 }
                    : new Range(sysMath.Cosh(i->Min), sysMath.Cosh(i->Max));
        }

        return GetIntervalSetFromRangeArray(Ranges, i1.Def, i1.Cont);
    }

    public static IntervalSet Tanh(this IntervalSet i1)
    {
        if (i1.IsNumber) return new IntervalSet(sysMath.Tanh(i1.Min));
        if (!i1.Def.Item2)
            return EmptyIntervalSet;
        i1.Intervals = MonotoneTransform(i1.Intervals, sysMath.Tanh, true);
        return i1;
    }

    #endregion

    #region 比较运算

    public static (bool, bool) Equal(this IntervalSet i1, IntervalSet i2)
    {
        if (i1.Def == FF || i2.Def == FF) return FF;
        if (i1.Intervals.Length * i2.Intervals.Length == 0)
            return FF;
        foreach (var j1 in Subtract(i1, i2).Intervals)
            if (j1.ContainsEqual(0))
                return FT;
        return FF;
    }

    public static (bool, bool) Greater(this IntervalSet i1, IntervalSet i2)
    {
        var result = Greater(
            new Interval(i1.Min, i1.Max)
            {
                Def = i1.Def,
                Cont = i1.Cont
            },
            new Interval(i2.Min, i2.Max)
            {
                Def = i2.Def,
                Cont = i2.Cont
            }
        );
        return result;
    }

    private static (bool, bool) Greater(Interval i1, Interval i2)
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

    public static (bool, bool) Less(this IntervalSet i1, IntervalSet i2)
    {
        return Greater(i2, i1);
    }

    private static bool RangeEqual(Range i1, Range i2)
    {
        if (double.IsNaN(i1.Min) || double.IsNaN(i2.Min))
            return false;
        return !(i2.Max < i1.Min || i2.Min > i1.Max);
    }

    public static (bool, bool) LessEqual(this IntervalSet i1, IntervalSet i2)
    {
        return Union(Less(i1, i2), Equal(i1, i2));
    }

    public static (bool, bool) GreaterEqual(this IntervalSet i1, IntervalSet i2)
    {
        return Union(Greater(i1, i2), Equal(i1, i2));
    }

    public static (bool, bool) Union(this (bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 || b.Item1, a.Item2 || b.Item2);
    }

    public static (bool, bool) Intersect(this (bool, bool) a, (bool, bool) b)
    {
        return (a.Item1 && b.Item1, a.Item2 && b.Item2);
    }

    #endregion
}

internal static class Extension
{
    public static bool IsInterger(this Range Range)
    {
        if (double.IsInfinity(Range.Min) || double.IsInfinity(Range.Max))
            return false;
        if (double.IsNaN(Range.Min) || double.IsNaN(Range.Max))
            return false;
        return Range.Min == Range.Max && Range.Min == (int)Range.Min;
    }

    public static bool IsNumber(this Range Range)
    {
        return Range.Min == Range.Max;
    }
}

internal struct OnlyAddList<T> //不可被赋值！ 
{
    public OnlyAddList(int len)
    {
        this.len = len;
        loc = 0;
        values = new T[len];
    }

    public void Add(T t)
    {
        if (loc == len)
        {
            len *= 2;
            Array.Resize(ref values, len);
        }

        values[loc] = t;
        loc++;
    }

    public T[] GetArray()
    {
        var array = values;
        values = null;
        Array.Resize(ref array, loc);
        return array;
    }

    private int loc;
    private int len;
    private T[] values;
}

internal class FactorialValue
{
    internal static double[] Values =
    {
        1,
        1,
        2,
        6,
        24,
        120,
        720,
        5040,
        40320,
        362880,
        3628800,
        39916800,
        479001600,
        6227020800,
        87178291200,
        1307674368000d,
        20922789888000d,
        355687428096000d,
        6402373705728000d,
        121645100408832000d,
        2432902008176640000d,
        51090942171709440000d,
        1124000727777607680000d,
        25852016738884976640000d,
        620448401733239439360000d,
        15511210043330985984000000d,
        403291461126605635584000000d,
        10888869450418352160768000000d,
        304888344611713860501504000000d,
        8841761993739701954543616000000d,
        265252859812191058636308480000000d,
        8222838654177922817725562880000000d,
        263130836933693530167218012160000000d,
        8683317618811886495518194401280000000d,
        295232799039604140847618609643520000000d,
        10333147966386144929666651337523200000000d,
        371993326789901217467999448150835200000000d,
        13763753091226345046315979581580902400000000d,
        523022617466601111760007224100074291200000000d,
        20397882081197443358640281739902897356800000000d,
        815915283247897734345611269596115894272000000000d,
        33452526613163807108170062053440751665152000000000d,
        1405006117752879898543142606244511569936384000000000d,
        60415263063373835637355132068513997507264512000000000d,
        2658271574788448768043625811014615890319638528000000000d,
        119622220865480194561963161495657715064383733760000000000d,
        5502622159812088949850305428800254892961651752960000000000d,
        258623241511168180642964355153611979969197632389120000000000d,
        12413915592536072670862289047373375038521486354677760000000000d,
        608281864034267560872252163321295376887552831379210240000000000d,
        30414093201713378043612608166064768844377641568960512000000000000d,
        1551118753287382280224243016469303211063259720016986112000000000000d,
        80658175170943878571660636856403766975289505440883277824000000000000d,
        4274883284060025564298013753389399649690343788366813724672000000000000d,
        230843697339241380472092742683027581083278564571807941132288000000000000d,
        12696403353658275925965100847566516959580321051449436762275840000000000000d,
        710998587804863451854045647463724949736497978881168458687447040000000000000d,
        40526919504877216755680601905432322134980384796226602145184481280000000000000d,
        2350561331282878571829474910515074683828862318181142924420699914240000000000000d,
        138683118545689835737939019720389406345902876772687432540821294940160000000000000d,
        8320987112741390144276341183223364380754172606361245952449277696409600000000000000d,
        507580213877224798800856812176625227226004528988036003099405939480985600000000000000d,
        31469973260387937525653122354950764088012280797258232192163168247821107200000000000000d,
        1982608315404440064116146708361898137544773690227268628106279599612729753600000000000000d,
        126886932185884164103433389335161480802865516174545192198801894375214704230400000000000000d,
        8247650592082470666723170306785496252186258551345437492922123134388955774976000000000000000d,
        544344939077443064003729240247842752644293064388798874532860126869671081148416000000000000000d,
        36471110918188685288249859096605464427167635314049524593701628500267962436943872000000000000000d,
        2480035542436830599600990418569171581047399201355367672371710738018221445712183296000000000000000d,
        171122452428141311372468338881272839092270544893520369393648040923257279754140647424000000000000000d,
        11978571669969891796072783721689098736458938142546425857555362864628009582789845319680000000000000000d,
        850478588567862317521167644239926010288584608120796235886430763388588680378079017697280000000000000000d,
        61234458376886086861524070385274672740778091784697328983823014963978384987221689274204160000000000000000d,
        4470115461512684340891257138125051110076800700282905015819080092370422104067183317016903680000000000000000d,
        330788544151938641225953028221253782145683251820934971170611926835411235700971565459250872320000000000000000d,
        24809140811395398091946477116594033660926243886570122837795894512655842677572867409443815424000000000000000000d,
        1885494701666050254987932260861146558230394535379329335672487982961844043495537923117729972224000000000000000000d,
        145183092028285869634070784086308284983740379224208358846781574688061991349156420080065207861248000000000000000000d,
        11324281178206297831457521158732046228731749579488251990048962825668835325234200766245086213177344000000000000000000d,
        894618213078297528685144171539831652069808216779571907213868063227837990693501860533361810841010176000000000000000000d,
        71569457046263802294811533723186532165584657342365752577109445058227039255480148842668944867280814080000000000000000000d,
        5797126020747367985879734231578109105412357244731625958745865049716390179693892056256184534249745940480000000000000000000d,
        475364333701284174842138206989404946643813294067993328617160934076743994734899148613007131808479167119360000000000000000000d,
        39455239697206586511897471180120610571436503407643446275224357528369751562996629334879591940103770870906880000000000000000000d,
        3314240134565353266999387579130131288000666286242049487118846032383059131291716864129885722968716753156177920000000000000000000d,
        281710411438055027694947944226061159480056634330574206405101912752560026159795933451040286452340924018275123200000000000000000000d,
        24227095383672732381765523203441259715284870552429381750838764496720162249742450276789464634901319465571660595200000000000000000000d,
        2107757298379527717213600518699389595229783738061356212322972511214654115727593174080683423236414793504734471782400000000000000000000d,
        185482642257398439114796845645546284380220968949399346684421580986889562184028199319100141244804501828416633516851200000000000000000000d,
        16507955160908461081216919262453619309839666236496541854913520707833171034378509739399912570787600662729080382999756800000000000000000000d,
        1485715964481761497309522733620825737885569961284688766942216863704985393094065876545992131370884059645617234469978112000000000000000000000d,
        135200152767840296255166568759495142147586866476906677791741734597153670771559994765685283954750449427751168336768008192000000000000000000000d,
        12438414054641307255475324325873553077577991715875414356840239582938137710983519518443046123837041347353107486982656753664000000000000000000000d,
        1156772507081641574759205162306240436214753229576413535186142281213246807121467315215203289516844845303838996289387078090752000000000000000000000d,
        108736615665674308027365285256786601004186803580182872307497374434045199869417927630229109214583415458560865651202385340530688000000000000000000000d,
        10329978488239059262599702099394727095397746340117372869212250571234293987594703124871765375385424468563282236864226607350415360000000000000000000000d,
        991677934870949689209571401541893801158183648651267795444376054838492222809091499987689476037000748982075094738965754305639874560000000000000000000000d,
        96192759682482119853328425949563698712343813919172976158104477319333745612481875498805879175589072651261284189679678167647067832320000000000000000000000d,
        9426890448883247745626185743057242473809693764078951663494238777294707070023223798882976159207729119823605850588608460429412647567360000000000000000000000d,
        933262154439441526816992388562667004907159682643816214685929638952175999932299156089414639761565182862536979208272237582511852109168640000000000000000000000d,
        93326215443944152681699238856266700490715968264381621468592963895217599993229915608941463976156518286253697920827223758251185210916864000000000000000000000000d,
        9425947759838359420851623124482936749562312794702543768327889353416977599316221476503087861591808346911623490003549599583369706302603264000000000000000000000000d,
        961446671503512660926865558697259548455355905059659464369444714048531715130254590603314961882364451384985595980362059157503710042865532928000000000000000000000000d,
        99029007164861804075467152545817733490901658221144924830052805546998766658416222832141441073883538492653516385977292093222882134415149891584000000000000000000000000d,
        10299016745145627623848583864765044283053772454999072182325491776887871732475287174542709871683888003235965704141638377695179741979175588724736000000000000000000000000d,
        1081396758240290900504101305800329649720646107774902579144176636573226531909905153326984536526808240339776398934872029657993872907813436816097280000000000000000000000000d,
        114628056373470835453434738414834942870388487424139673389282723476762012382449946252660360871841673476016298287096435143747350528228224302506311680000000000000000000000000d,
        12265202031961379393517517010387338887131568154382945052653251412013535324922144249034658613287059061933743916719318560380966506520420000368175349760000000000000000000000000d,
        1324641819451828974499891837121832599810209360673358065686551152497461815091591578895743130235002378688844343005686404521144382704205360039762937774080000000000000000000000000d,
        144385958320249358220488210246279753379312820313396029159834075622223337844983482099636001195615259277084033387619818092804737714758384244334160217374720000000000000000000000000d,
        15882455415227429404253703127090772871724410234473563207581748318444567162948183030959960131517678520479243672638179990208521148623422266876757623911219200000000000000000000000000d,
        1762952551090244663872161047107075788761409536026565516041574063347346955087248316436555574598462315773196047662837978913145847497199871623320096254145331200000000000000000000000000d,
        197450685722107402353682037275992488341277868034975337796656295094902858969771811440894224355027779366597957338237853638272334919686385621811850780464277094400000000000000000000000000d,
        22311927486598136465966070212187151182564399087952213171022161345724023063584214692821047352118139068425569179220877461124773845924561575264739138192463311667200000000000000000000000000d,
        2543559733472187557120132004189335234812341496026552301496526393412538629248600474981599398141467853800514886431180030568224218435400019580180261753940817530060800000000000000000000000000d,
        292509369349301569068815180481773552003419272043053514672100535242441942363589054622883930786268803187059211939585703515345785120071002251720730101703194015956992000000000000000000000000000d,
        33931086844518982011982560935885732032396635556994207701963662088123265314176330336254535971207181169698868584991941607780111073928236261199604691797570505851011072000000000000000000000000000d,
        3969937160808720895401959629498630647790406360168322301129748464310422041758630649341780708631240196854767624444057168110272995649603642560353748940315749184568295424000000000000000000000000000d,
        468452584975429065657431236280838416439267950499862031533310318788629800927518416622330123618486343228862579684398745837012213486653229822121742374957258403779058860032000000000000000000000000000d,
        55745857612076058813234317117419771556272886109483581752463927935846946310374691578057284710599874844234646982443450754604453404911734348832487342619913750049708004343808000000000000000000000000000d,
        6689502913449127057588118054090372586752746333138029810295671352301633557244962989366874165271984981308157637893214090552534408589408121859898481114389650005964960521256960000000000000000000000000000d,
        809429852527344373968162284544935082997082306309701607045776233628497660426640521713391773997910182738287074185078904956856663439318382745047716214841147650721760223072092160000000000000000000000000000d,
        98750442008336013624115798714482080125644041369783596059584700502676714572050143649033796427745042294071023050579626404736512939596842694895821378210620013388054747214795243520000000000000000000000000000d,
        12146304367025329675766243241881295855454217088483382315328918161829235892362167668831156960612640202170735835221294047782591091570411651472186029519906261646730733907419814952960000000000000000000000000000d,
        1506141741511140879795014161993280686076322918971939407100785852066825250652908790935063463115967385069171243567440461925041295354731044782551067660468376444194611004520057054167040000000000000000000000000000d,
        188267717688892609974376770249160085759540364871492425887598231508353156331613598866882932889495923133646405445930057740630161919341380597818883457558547055524326375565007131770880000000000000000000000000000000d,
        23721732428800468856771473051394170805702085973808045661837377170052497697783313457227249544076486314839447086187187275319400401837013955325179315652376928996065123321190898603130880000000000000000000000000000000d,
        3012660018457659544809977077527059692324164918673621799053346900596667207618480809067860692097713761984609779945772783965563851033300772326297773087851869982500270661791244122597621760000000000000000000000000000000d,
        385620482362580421735677065923463640617493109590223590278828403276373402575165543560686168588507361534030051833058916347592172932262498857766114955245039357760034644709279247692495585280000000000000000000000000000000d,
        49745042224772874403902341504126809639656611137138843145968864022652168932196355119328515747917449637889876686464600208839390308261862352651828829226610077151044469167497022952331930501120000000000000000000000000000000d,
        6466855489220473672507304395536485253155359447828049608975952322944781961185526165512707047229268452925683969240398027149120740074042105844737747799459310029635780991774612983803150965145600000000000000000000000000000000d,
        847158069087882051098456875815279568163352087665474498775849754305766436915303927682164623187034167333264599970492141556534816949699515865660644961729169613882287309922474300878212776434073600000000000000000000000000000000d,
        111824865119600430744996307607616902997562475571842633838412167568361169672820118454045730260688510087990927196104962685462595837360336094267205134948250389032461924909766607715924086489297715200000000000000000000000000000000d,
        14872707060906857289084508911813048098675809251055070300508818286592035566485075754388082124671571841702793317081960037166525246368924700537538282948117301741317436012998958826217903503076596121600000000000000000000000000000000d,
        1992942746161518876737324194182948445222558439641379420268181650403332765909000151088003004705990626788174304488982644980314383013435909872030129915047718433336536425741860482713199069412263880294400000000000000000000000000000000d,
        269047270731805048359538766214698040105045389351586221736204522804449923397715020396880405635308734616403531106012657072342441706813847832724067538531441988500432417475151165166281874370655623839744000000000000000000000000000000000d,
        36590428819525486576897272205198933454286172951815726156123815101405189582089242773975735166401987907830880230417721361838572072126683305250473185240276110436058808776620558462614334914409164842205184000000000000000000000000000000000d,
        5012888748274991661034926292112253883237205694398754483388962668892510972746226260034675717797072343372830591567227826571884373881355612819314826377917827129740056802397016509378163883274055583382110208000000000000000000000000000000000d,
        691778647261948849222819828311491035886734385827028118707676848307166514238979223884785249055995983385450621636277440066920043595627074569065446040152660143904127838730788278294186615891819670506731208704000000000000000000000000000000000d,
        96157231969410890041971956135297253988256079629956908500367081914696145479218112119985149618783441690577636407442564169301886059792163365100096999581219760002673769583579570682891939608962934200435638009856000000000000000000000000000000000d,
        13462012475717524605876073858941615558355851148193967190051391468057460367090535696797920946629681836680869097041958983702264048370902871114013579941370766400374327741701139895604871545254810788060989321379840000000000000000000000000000000000d,
        1898143759076170969428526414110767793728175011895349373797246196996101911759765533248506853474785138972002542682916216702019230820297304827075914771733278062452780211579860725280286887880928321116599494314557440000000000000000000000000000000000d,
        269536413788816277658850750803729026709400851689139611079208959973446471469886705721287973193419489734024361060974102771686730776482217285444779897586125484868294790044340222989800738079091821598557128192667156480000000000000000000000000000000000d
    };
}