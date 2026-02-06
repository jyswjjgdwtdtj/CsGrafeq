using CsGrafeq.Collections;
using CsGrafeq.Interval.Interface;
using MathNet.Numerics;
using static CsGrafeq.Interval.Def;
using static CsGrafeq.Interval.Extensions.IntervalSetExtension;
using CGMath = CsGrafeq.Numeric.CsGrafeqMath;
using static CsGrafeq.Utilities.ThrowHelper;
using Ranges = System.Span<CsGrafeq.Interval.Range>;

namespace CsGrafeq.Interval;

/// <summary>
///     禁止多线程允许 只允许允许再单个线程！！！
///     Pool只使用不Return释放 释放统一到最后结束运行 Pool容量足够大不会有问题 除非确保无问题
/// </summary>
public readonly ref struct IntervalSet : IInterval<IntervalSet>
{
    #region 静态定义

    public static bool NeedClone => true;

    public static IntervalSet Empty =>
        new(Span<Range>.Empty, IntervalSetType.Empty, FF, double.NaN, double.NaN, double.NaN);

    #endregion

    #region 定义

    public readonly IntervalSetType Type; // 1 byte
    public readonly bool IsNumber;

    // Number
    /// <summary>
    ///     数
    /// </summary>
    internal readonly double Number; // 8 bytes
    // End Number

    // IntervalSet
    public readonly Def _Def; // 8 bytes
    public Def Def => _Def;

    /// <summary>
    ///     最大值
    /// </summary>
    public readonly double _Sup;

    public double Sup => _Sup;

    /// <summary>
    ///     最小值
    /// </summary>
    public readonly double _Inf;

    public double Inf => _Inf;

    internal readonly Ranges Intervals;
    // End IntervalSet

    // Empty

    // End Empty

    /// <summary>
    ///     大而全
    /// </summary>
    private IntervalSet(Ranges ranges, IntervalSetType type, Def def, double inf, double sup, double number)
    {
        Intervals = ranges;
        if (ranges.IsEmpty)
            Type = IntervalSetType.Empty;
        else
            Type = type;
        _Def = def;
        _Inf = inf;
        _Sup = sup;
        Number = number;
        IsNumber = type == IntervalSetType.Number;
        _IsEmpty = type == IntervalSetType.Empty;
        _Width = sup - inf;
    }

    public static IntervalSet CreateFromDouble(double num)
    {
        return Create(num);
    }

    public static IntervalSet Create(double num)
    {
        return new IntervalSet((new Ranges([new Range(num)])), IntervalSetType.Number, TT, num, num, num);
    }

    public static IntervalSet Create(double min, double max, Def def)
    {
        CGMath.SwapIfNotLess(ref min, ref max);
        return new IntervalSet((new Ranges([new Range(min,max)])), IntervalSetType.IntervalSet, TT, min, max,
            double.NaN);
    }

    public static IntervalSet Create(Span<double> nums, Def def, bool needSort = false)
    {
        var len = nums.Length;
        if (len == 0)
            return new IntervalSet([], IntervalSetType.Empty, def, double.NaN, double.NaN, double.NaN);
        if (needSort)
            nums.Sort();
        var intervals = new Ranges(new Range[len]);
        for (var idx = 0; idx < len; idx++) intervals[idx] = new Range(nums[idx]);
        return new IntervalSet(intervals, IntervalSetType.IntervalSet, def, nums[0], nums[^1], double.NaN);
    }

    public static IntervalSet Create(Ranges ranges, Def def)
    {
        var len = ranges.Length;
        if (len == 0)
            return new IntervalSet([], IntervalSetType.Empty, def, double.NaN, double.NaN, double.NaN);
        return new IntervalSet(ranges, IntervalSetType.IntervalSet, def, ranges[0]._Inf, ranges[^1]._Sup, double.NaN);
    }

    public static IntervalSet Clone(IntervalSet source)
    {
        var ranges = new Ranges(new Range[source.Intervals.Length]);
        source.Intervals.CopyTo(ranges);
        return Create(ranges, source._Def);
    }

    public double Width => _Width;
    public readonly double _Width;
    public bool IsEmpty => _IsEmpty;
    public readonly bool _IsEmpty;

    public bool TryGetNumber(out double number)
    {
        number = Number;
        return Type == IntervalSetType.Number;
    }

    public bool Contains(double num)
    {
        foreach (ref var r in Intervals)
            if (r.Contains(num))
                return true;
        return false;
    }

    public bool ContainsEqual(double num)
    {
        foreach (ref var r in Intervals)
            if (r.ContainsEqual(num))
                return true;
        return false;
    }

    public override string ToString()
    {
        return $"{{Def:{Def},Intervals:[{string.Join(',', Intervals.Select(o => o.ToString()))}]}}";
    }

    #endregion

    #region 四则运算

    public static IntervalSet operator +(IntervalSet i1, IntervalSet i2)
    {
        if (i1._IsEmpty || i2._IsEmpty) return Empty;

        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number + i2.Number);

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
        return new Range { _Inf = i1._Inf + i2._Inf, _Sup = i1._Sup + i2._Sup };
    }

    public static IntervalSet operator +(IntervalSet i1, double num)
    {
        foreach (ref var ranges in i1.Intervals)
        {
            ranges._Inf += num;
            ranges._Sup += num;
        }

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
        if (i1._IsEmpty || i2._IsEmpty) return Empty;

        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number * i2.Number);

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
        if (i1._Inf > 0 && i2._Inf > 0) return new Range { _Inf = i1._Inf * i2._Inf, _Sup = i1._Sup * i2._Sup };
        if (i1._Sup < 0 && i2._Sup < 0) return new Range { _Inf = i1._Sup * i2._Sup, _Sup = i1._Inf * i2._Inf };
        var (min, max) = CGMath.GetMinMax(i1._Inf * i2._Inf, i1._Inf * i2._Sup, i1._Sup * i2._Inf, i1._Sup * i2._Sup);
        return new Range(min, max);
    }

    public static IntervalSet operator *(double num, IntervalSet i1)
    {
        return i1 * num;
    }

    public static IntervalSet operator *(IntervalSet i1, double num)
    {
        if (num == 0) return Create(0);

        if (double.IsInfinity(num)) return Empty;

        if (num > 0)
        {
            foreach (ref var range in i1.Intervals)
            {
                range._Inf *= num;
                range._Sup *= num;
            }

            return i1;
        }

        return -(i1 * -num);
    }

    public static IntervalSet operator /(IntervalSet i1, IntervalSet i2)
    {
        if (i1._IsEmpty || i2._IsEmpty) return Empty;
        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number / i2.Number);
        if (i2.IsNumber) return i1 * (1 / i2.Number);
        if (i1.IsNumber) i1 = Create(new[] { new Range(i1.Number, i1.Number) }, TT);
        var ranges = new Ranges(new Range[i2.Intervals.Length + 1]);
        foreach (ref var range in i1.Intervals)
        {
            range._Inf = range._Inf == 0 ? double.Epsilon : range._Inf;
            range._Sup = range._Sup == 0 ? -double.Epsilon : range._Sup;
        }

        var loc = 0;
        foreach (ref var range in i2.Intervals)
        {
            if (range.ContainsEqual(0))
            {
                ranges[loc++] = new Range { _Inf = double.NegativeInfinity, _Sup = 1 / range._Inf };
                ranges[loc++] = new Range { _Inf = 1 / range._Sup, _Sup = double.PositiveInfinity };
                continue;
            }

            ranges[loc++] = new Range(1 / range._Inf, 1 / range._Sup);
        }

        var res = ranges.Slice(0, loc);
        return i1 * Create(res, i2._Def);
    }

    public static IntervalSet operator %(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number % i2.Number);

        if (i1._IsEmpty || i2._IsEmpty) return Empty;

        if (i2.IsNumber)
        {
            var num = i2.Number;
            var ranges = new Ranges(new Range[i1.Intervals.Length * 2]);
            var loc = 0;
            foreach (var i in i1.Intervals)
            {
                var min = Math.Floor(i._Inf / num);
                var max = Math.Floor(i._Sup / num);
                if (min == max)
                {
                    ranges[loc++] = new Range(i._Inf - min * num, i._Sup - max * num);
                }
                else if (min + 1 == max)
                {
                    ranges[loc++] = new Range(i._Inf - min * num, num);
                    ranges[loc++] = new Range(0, i._Sup - max * num);
                }
                else
                {
                    return Create(0, num, i1._Def & i2._Def);
                }
            }

            if (loc == 0)
            {
                return Empty;
            }

            var res = ranges.Slice(0, loc);
            return Create(res, FT);
        }

        {
            var q = Floor(i1 / i2);
            var i = i1 - i2 * q;
            double sup = 0, inf = 0;
            if (i2._Sup > 0)
                sup = i2._Sup;
            if (i2._Inf < 0)
                inf = i2._Inf;
            return Create(i.Intervals.SetBounds(new Range(inf, sup), out var ifSetBounds),
                ifSetBounds ? FT : i1._Def & i2.Def);
        }
    }

    public static Def operator ==(IntervalSet i1, IntervalSet i2)
    {
        if (i1._Def == FF || i2._Def == FF) return FF;
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
        return FF;
    }

    public static Def operator <(IntervalSet i1, IntervalSet i2)
    {
        return i2 > i1;
    }

    public static Def operator >(IntervalSet i1, IntervalSet i2)
    {
        return Interval.Create(i1._Inf, i1._Sup, i1._Def) > Interval.Create(i2._Inf, i2._Sup, i2._Def);
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
        i1.Intervals.MonotoneTransformOpInplace(&NumNeg);
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
        if (i._IsEmpty) return Empty;

        if (i.IsNumber) return Create(Math.Sign(i.Number));
        var tmp = new Ranges(new Range[3]);
        var loc = 0;
        if (i._Inf < 0)
            tmp[loc++] = new Range(-1);
        if (i.Contains(0))
            tmp[loc++] = new Range(0);
        if (i._Sup > 0)
            tmp[loc++] = new Range(1);
        return Create(tmp.Slice(0, loc), FT);
    }

    public static unsafe IntervalSet Abs(IntervalSet i1)
    {
        if (i1._IsEmpty)
            return Empty;
        if (i1.IsNumber) return Create(Math.Abs(i1._Inf));
        return IntervalSetMethod(i1, &RangeAbs);
    }

    private static Range RangeAbs(Range i)
    {
        if (i.ContainsEqual(0)) return new Range(0, Math.Max(-i._Inf, i._Sup));

        if (i._Sup < 0) return new Range(-i._Sup, -i.Inf);
        return new Range(i._Inf, i._Sup);
    }

    public static unsafe IntervalSet Min(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(Math.Min(i1.Number, i2.Number));
        if (i1._IsEmpty || i2._IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, i2, &RangeMin);
    }

    private static Range RangeMin(Range i1, Range i2)
    {
        return new Range { _Inf = Math.Min(i1._Inf, i2._Inf), _Sup = Math.Min(i1._Sup, i2._Sup) };
    }

    public static unsafe IntervalSet Max(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(Math.Max(i1.Number, i2.Number));
        if (i1._IsEmpty || i2._IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, i2, &RangeMax);
    }

    public static IntervalSet MaxOf(IEnumerable<IntervalSet> nums)
    {
        return nums.IntervalAggregate(Max);
    }

    public static IntervalSet MinOf(IEnumerable<IntervalSet> nums)
    {
        return nums.IntervalAggregate(Min);
    }

    private static Range RangeMax(Range i1, Range i2)
    {
        return new Range { _Inf = Math.Max(i1._Inf, i2._Inf), _Sup = Math.Max(i1._Sup, i2._Sup) };
    }

    public static IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
    {
        if (c1.IsNumber && c2.IsNumber && c3.IsNumber) return Create(DoubleMedian(c1.Number, c2.Number, c3.Number));

        if (c1._IsEmpty || c2._IsEmpty || c3._IsEmpty) return Empty;

        var loc = 0;
        var res = new Ranges(new Range[c1.Intervals.Length * c2.Intervals.Length * c3.Intervals.Length]);
        foreach (var i1 in c1.Intervals)
        foreach (var i2 in c2.Intervals)
        foreach (var i3 in c3.Intervals)
            res[loc++] = RangeMedian(i1, i2, i3);
        return Create(res.FormatRanges(), c1._Def & c2._Def & c3._Def);
    }

    public static unsafe IntervalSet Exp(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Exp(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransformInplace(&Math.Exp);
        return i1;
    }

    public static IntervalSet Ln(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Log(i1.Number));
        //e为底
        if (i1._IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[^ 1]._Sup <= 0) return Empty;

        var loc = -1;
        for (var i = 0; i < len; i++)
        {
            ref var range = ref i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0) range._Inf = 0;
            range = new Range(Math.Log(range._Inf), Math.Log(range._Sup));
            if (loc == -1) loc = i;
        }

        var res = i1.Intervals.Slice(loc, len - loc);
        return Create(res, loc == 0 ? FT : i1._Def);
    }

    public static IntervalSet Lg(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Log10(i1.Number));
        //10为底
        if (i1._IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[^ 1]._Sup <= 0) return Empty;

        var loc = -1;
        for (var i = 0; i < len; i++)
        {
            ref var range = ref i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0) range._Inf = 0;
            range = new Range(Math.Log10(range._Inf), Math.Log10(range._Sup));
            if (loc == -1) loc = i;
        }

        var res = i1.Intervals.Slice(loc, len - loc);
        return Create(res, loc == 0 ? FT : i1._Def);
    }

    public static IntervalSet Log(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(Math.Log(i1.Number, i2.Number));
        return Ln(i1) / Ln(i2);
    }

    public static IntervalSet Pow(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(Math.Pow(i1.Number, i2.Number));
        if (i2.IsNumber)
        {
            var num = i2.Number;
            if (num == (int)num)
            {
                var inum = (int)num;
                if (inum % 2 == 0) //偶数
                {
                    var ranges = new Ranges(new Range[i1.Intervals.Length + 1]);
                    var loc = 0;
                    foreach (var r in i1.Intervals)
                        if (r.Contains(0))
                        {
                            if (inum < 0)
                            {
                                ranges[loc++] = new Range(Math.Pow(r._Inf, num), double.NegativeInfinity);
                                ranges[loc++] = new Range(Math.Pow(r._Sup, num), double.PositiveInfinity);
                            }
                            else
                            {
                                ranges[loc++] = new Range(0,
                                    Math.Max(Math.Pow(r._Sup, num), Math.Pow(r._Inf, num)));
                            }
                        }
                        else
                        {
                            ranges[loc++] = new Range(Math.Pow(r._Inf, num), Math.Pow(r._Sup, num));
                        }

                    if (loc == i1.Intervals.Length)
                        ranges[loc] = new Range(double.NaN);
                    return Create(ranges.FormatRanges(), i1._Def);
                }

                for (var i = 0; i < i1.Intervals.Length; i++)
                {
                    var r = i1.Intervals[i];
                    i1.Intervals[i] = new Range(Math.Pow(r._Inf, num), Math.Pow(r._Sup, num));
                }

                return i1;
            }
        }

        return Exp(Ln(i1) * i2);
    }

    public static IntervalSet Sqrt(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Sqrt(i1.Number));
        if (i1._IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[^ 1]._Sup <= 0) return Empty;

        var loc = -1;
        for (var i = 0; i < len; i++)
        {
            ref var range = ref i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0) range._Inf = 0;
            range = new Range(Math.Sqrt(range._Inf), Math.Sqrt(range._Sup));
            if (loc == -1) loc = i;
        }

        var res = i1.Intervals.Slice(loc, len - loc);
        return Create(res, loc == 0 ? FT : i1._Def);
    }

    public static unsafe IntervalSet Cbrt(IntervalSet num)
    {
        if (num.IsNumber)
            return Create(Math.Cbrt(num.Number));
        if (num._IsEmpty)
            return Empty;
        num.Intervals.MonotoneTransformInplace(&Math.Cbrt);
        return num;
    }

    private static Range RangeMedian(Range i1, Range i2, Range i3)
    {
        return new Range
            { _Inf = DoubleMedian(i1._Inf, i2._Inf, i3._Inf), _Sup = DoubleMedian(i1._Sup, i2._Sup, i3._Sup) };
    }

    private static double DoubleMedian(double t1, double t2, double t3)
    {
        if ((t1 - t2) * (t2 - t3) > 0) return t2;
        if ((t2 - t1) * (t1 - t3) > 0) return t1;
        return t3;
    }

    public static IntervalSet Floor(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Floor(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        var loc = 0;
        var ranges = new Ranges(new Range[len * 2 + 1]);
        for (var i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (loc < 2)
            {
                var min = Math.Floor(range._Inf);
                var max = Math.Floor(range._Sup);
                if (min == max)
                {
                    ranges[loc++] = new Range(min);
                    continue;
                }

                ranges[loc++] = new Range(min);
                ranges[loc++] = new Range(min + 1);
                if (max > min + 1)
                    ranges[loc++] = new Range(min + 2, max);
                continue;
            }

            for (var j = i; j < len; j++)
                ranges[loc++] = rfloor(i1.Intervals[j]);
            break;
        }

        var res = ranges.Slice(0, loc);
        return Create(res, FT);
    }

    private static Range rfloor(Range r)
    {
        return new Range(Math.Floor(r._Inf), Math.Floor(r._Sup));
    }

    public static IntervalSet Ceil(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Ceiling(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        var loc = 0;
        var ranges = new Ranges(new Range[len * 2 + 1]);
        for (var i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (loc < 2)
            {
                var min = Math.Ceiling(range._Inf);
                var max = Math.Ceiling(range._Sup);
                if (min == max)
                {
                    ranges[loc++] = new Range(min);
                    continue;
                }

                ranges[loc++] = new Range(min);
                ranges[loc++] = new Range(min + 1);
                if (max > min + 1)
                    ranges[loc++] = new Range(min + 2, max);
                continue;
            }

            for (var j = i; j < len; j++)
                ranges[loc++] = rceil(i1.Intervals[j]);
            break;
        }

        var res = ranges.Slice(0, loc);
        return Create(res, FT);
    }

    private static Range rceil(Range r)
    {
        return new Range(Math.Ceiling(r._Inf), Math.Ceiling(r._Sup));
    }

    public static IntervalSet GCD(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Number == (int)i1.Number && i2._Inf == (int)i2._Inf)
                return Create(CGMath.GCD((int)i1.Number, (int)i2._Inf));
            return Throw<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1._IsEmpty || i2._IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return Throw<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return Create(CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return Create(new double[]
            {
                CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.GCD((int)i1.Intervals[1]._Inf, (int)i2.Intervals[0]._Inf)
            }, FT);
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[1]._Inf)
            }, FT);
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.GCD((int)i1.Intervals[0]._Inf, (int)i2.Intervals[1]._Inf),
                CGMath.GCD((int)i1.Intervals[1]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.GCD((int)i1.Intervals[1]._Inf, (int)i2.Intervals[1]._Inf)
            }, FT);
        return Create(1, double.PositiveInfinity, FT);
    }

    public static IntervalSet LCM(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber)
        {
            if (i1.Number == (int)i1.Number && i2._Inf == (int)i2._Inf)
                return Create(CGMath.LCM((int)i1.Number, (int)i2._Inf));
            return Throw<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1._IsEmpty || i2._IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return Throw<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            return Create(CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf));
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            return Create(new double[]
            {
                CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.LCM((int)i1.Intervals[1]._Inf, (int)i2.Intervals[0]._Inf)
            }, FT);
        if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[1]._Inf)
            }, FT);
        if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            return Create(new double[]
            {
                CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.LCM((int)i1.Intervals[0]._Inf, (int)i2.Intervals[1]._Inf),
                CGMath.LCM((int)i1.Intervals[1]._Inf, (int)i2.Intervals[0]._Inf),
                CGMath.LCM((int)i1.Intervals[1]._Inf, (int)i2.Intervals[1]._Inf)
            }, FT);
        return Create(1, double.PositiveInfinity, FT);
    }

    #endregion

    #region 三角

    public static unsafe IntervalSet Sin(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Sin(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, &RangeSin);
    }

    public static IntervalSet Cos(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Cos(i1.Number));
        return Sin(i1 + Math.PI / 2);
    }

    private static Range RangeSin(Range i)
    {
        var a = i._Inf;
        var b = i._Sup;
        (double _Inf, double _Sup) minmax = (0, 0);
        (double _Inf, double _Sup) j = (0, 0);
        if (Math.Floor((a / Math.PI - 0.5) / 2) < Math.Floor((b / Math.PI - 0.5) / 2))
        {
            minmax._Sup = 1;
            j._Sup = 1;
        }

        if (Math.Floor((a / Math.PI + 0.5) / 2) < Math.Floor((b / Math.PI + 0.5) / 2))
        {
            minmax._Inf = 1;
            j._Inf = -1;
        }

        if (minmax._Inf == 0) j._Inf = Math.Min(Math.Sin(a), Math.Sin(b));
        if (minmax._Sup == 0) j._Sup = Math.Max(Math.Sin(a), Math.Sin(b));
        return new Range(j._Inf, j._Sup);
    }

    public static IntervalSet Tan(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Tan(i1.Number));

        if (i1._IsEmpty)
            return Empty;
        var ranges = new Ranges(new Range[i1.Intervals.Length * 2]);
        var loc = 0;
        foreach (var i in i1.Intervals)
        {
            var r = (int)Math.Floor((i._Sup + Math.PI / 2) / Math.PI);
            var l = (int)Math.Floor((i._Inf + Math.PI / 2) / Math.PI);
            if (r - l == 1)
            {
                ranges[loc++] = new Range { _Sup = Math.Tan(i._Sup), _Inf = double.NegativeInfinity };
                ranges[loc++] = new Range { _Inf = Math.Tan(i._Inf), _Sup = double.PositiveInfinity };
            }
            else if (l == r)
            {
                ranges[loc++] = new Range { _Inf = Math.Tan(i._Inf), _Sup = Math.Tan(i._Sup) };
            }
            else
            {
                return Create(double.NegativeInfinity, double.PositiveInfinity, i1._Def);
            }
        }

        var res = ranges.Slice(0, loc).FormatRanges();
        return Create(res, FT);
    }

    public static IntervalSet Cot(IntervalSet i1)
    {
        return -Tan(i1 + Math.PI / 2);
    }

    public static unsafe IntervalSet ArcTan(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Atan(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransformInplace(&Math.Atan);
        return i1;
    }

    public static unsafe IntervalSet ArcCos(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Acos(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        if (i1._Sup < -1 || i1.Number > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { _Inf = -1, _Sup = 1 }, out _);
        res.MonotoneTransformOpInplace(&Math.Acos);
        return Create(res, i1._Sup > 1 || i1.Number < -1 ? FT : i1._Def);
    }

    public static unsafe IntervalSet ArcSin(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Asin(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        if (i1._Sup < -1 || i1._Inf > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { _Inf = -1, _Sup = 1 }, out _);
        res.MonotoneTransformInplace(&Math.Asin);
        return Create(res, i1._Sup > 1 || i1._Inf < -1 ? FT : i1._Def);
    }

    public static unsafe IntervalSet Sinh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Sinh(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransformInplace(&Math.Sinh);
        return i1;
    }

    public static IntervalSet Cosh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Cosh(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        for (var i = 0; i < i1.Intervals.Length; i++)
        {
            ref var r = ref i1.Intervals[i];
            r = r.Contains(0)
                ? new Range { _Sup = Math.Cosh(Math.Max(r._Sup, -r._Inf)), _Inf = 1 }
                : new Range(Math.Cosh(r._Inf), Math.Cosh(r._Sup));
        }

        return Create(i1.Intervals.FormatRanges(), i1._Def);
    }

    public static unsafe IntervalSet Tanh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Tanh(i1.Number));
        if (i1._IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransformInplace(&Math.Tanh);
        return i1;
    }

    public static unsafe IntervalSet ArcCosh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Acosh(num.Number));
        var min = num._Inf;
        var res = num.Intervals.SetBounds(new Range(1, double.PositiveInfinity), out _);
        var min1 = double.NaN;
        if (res.Length > 0)
            min1 = res[0]._Inf;
        res.MonotoneTransformOpInplace(&Math.Acosh);
        return Create(res, min == min1 ? num._Def : FT);
    }

    public static unsafe IntervalSet ArcSinh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Asinh(num.Number));
        num.Intervals.MonotoneTransformInplace(&Math.Asinh);
        return num;
    }

    public static IntervalSet ArcTanh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Atanh(num.Number));
        //e为底
        if (num._IsEmpty) return Empty;

        var len = num.Intervals.Length;
        if (num._Sup <= -1 || num._Inf >= 1) return Empty;

        var current = 0;
        for (var i = 0; i < len; i++)
        {
            var range = num.Intervals[i];
            if (range._Sup < -1 || range._Inf > 1)
                continue;
            var inf = Math.Max(-1, range._Inf);
            var sup = Math.Min(1, range._Sup);
            num.Intervals[current++] = new Range(Math.Atanh(inf), Math.Atanh(sup));
        }

        var res = num.Intervals.Slice(0, current);
        return Create(res, FT);
    }

    /// <summary>
    ///     图像见 <a href="https://www.desmos.com/3d/ssq30zgep2" />
    /// </summary>
    public static IntervalSet ArcTan2(IntervalSet y, IntervalSet x)
    {
        if (y._IsEmpty || x._IsEmpty)
            return Empty;

        if (y.IsNumber && x.IsNumber)
            return Create(Math.Atan2(y.Number, x.Number));

        // 笛卡尔积枚举边界点：atan2 在矩形上极值只可能出现在边界，
        // 此处用四个角点做保守包络；若穿过原点或 x==0，会返回全角以保证正确性。
        var tmp = new Ranges(new Range[y.Intervals.Length * x.Intervals.Length]);
        var loc = 0;

        foreach (var ry in y.Intervals)
        foreach (var rx in x.Intervals)
        {
            // if rectangle contains (0,0), atan2 取值会覆盖整个 (-π, π]
            if (ry.ContainsEqual(0) && rx.ContainsEqual(0))
            {
                tmp[loc++] = new Range(-Math.PI, Math.PI);
                continue;
            }

            // if x spans 0, atan2 会跨越不连续点，直接给全角最保守
            if (rx.ContainsEqual(0))
            {
                tmp[loc++] = new Range(-Math.PI, Math.PI);
                continue;
            }

            var a1 = Math.Atan2(ry._Inf, rx._Inf);
            var a2 = Math.Atan2(ry._Inf, rx._Sup);
            var a3 = Math.Atan2(ry._Sup, rx._Inf);
            var a4 = Math.Atan2(ry._Sup, rx._Sup);

            var min = Math.Min(Math.Min(a1, a2), Math.Min(a3, a4));
            var max = Math.Max(Math.Max(a1, a2), Math.Max(a3, a4));

            // 角度在 -π/π 处可能“折返”，如果跨越该切点，保守返回全角
            if (max - min > Math.PI)
            {
                tmp[loc++] = new Range(-Math.PI, Math.PI);
                continue;
            }

            tmp[loc++] = new Range(min, max);
        }

        var res = tmp.Slice(0, loc).FormatRanges();
        return Create(res, y._Def & x._Def);
    }

    #endregion

    #region 特殊函数

    public static IntervalSet Mod(IntervalSet num1, IntervalSet num2)
    {
        return num1 % num2;
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Gamma(IntervalSet num)
    {
        // Gamma(x) 在 (0, +∞) 上：在 (0, ~1.4616) 单调递减，之后单调递增；存在极点于 0,-1,-2,...
        // 先处理包含非正整数/0 的区间：直接给全范围保守
        if (num._IsEmpty) return Empty;
        if (!num.IsNumber && ContainsNonPositiveInteger(num))
            return Create(double.NegativeInfinity, double.PositiveInfinity, FT);

        // 若整个输入位于 (0, +∞)，可按性质在“最小值点”处分段以收紧
        if (num._Inf > 0)
        {
            const double gammaMinX = 1.4616321449683623; // Gamma 在正实数上的全局最小点（近似常数）
            return ApplyUnarySplitByPoint(num, gammaMinX, static x => SpecialFunctions.Gamma(x));
        }

        // 其它情况（包括跨过 0 或负数区域）强行端点包络（保守）
        return ApplyUnaryCornerEnvelope(num, static x => SpecialFunctions.Gamma(x));
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet LnGamma(IntervalSet num)
    {
        // ln(Gamma(x)) 在 (0, +∞) 上也具有单峰（在 ~1.4616 附近达到最小）
        if (num._IsEmpty) return Empty;
        if (!num.IsNumber && ContainsNonPositiveInteger(num))
            return Create(double.NegativeInfinity, double.PositiveInfinity, FT);

        if (num._Inf > 0)
        {
            const double gammaMinX = 1.4616321449683623;
            return ApplyUnarySplitByPoint(num, gammaMinX, static x => SpecialFunctions.GammaLn(x));
        }

        return ApplyUnaryCornerEnvelope(num, static x => SpecialFunctions.GammaLn(x));
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Psi(IntervalSet num)
    {
        // Psi == Digamma。Digamma 在每个 (−n, −n+1) 上严格递增，在正实数上严格递增；在 0,-1,-2... 有极点
        return Digamma(num);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Digamma(IntervalSet num)
    {
        if (num._IsEmpty) return Empty;
        if (!num.IsNumber && ContainsNonPositiveInteger(num))
            return Create(double.NegativeInfinity, double.PositiveInfinity, FT);

        // 在 (0,+∞) 上严格单调递增
        if (num._Inf > 0)
            return ApplyUnaryMonotoneInc(num, SpecialFunctions.DiGamma);

        // 负区间分段/角点包络（保守）
        return ApplyUnaryCornerEnvelope(num, static x => SpecialFunctions.DiGamma(x));
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Erf(IntervalSet num)
    {
        // erf(x) 在 R 上严格递增，值域 (-1,1)
        return ApplyUnaryMonotoneInc(num, SpecialFunctions.Erf);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Erfc(IntervalSet num)
    {
        // erfc(x)=1-erf(x) 在 R 上严格递减，值域 (0,2)
        return ApplyUnaryMonotoneDec(num, SpecialFunctions.Erfc);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Erfinv(IntervalSet num)
    {
        // erfinv 在 [-1,1] 上严格递增
        return ApplyUnaryMonotoneIncWithDomain(num, -1.0, 1.0, SpecialFunctions.ErfInv);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet Erfcinv(IntervalSet num)
    {
        // erfcinv 在 [0,2] 上严格递减
        return ApplyUnaryMonotoneDecWithDomain(num, 0.0, 2.0, SpecialFunctions.ErfcInv);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet BesselJ(IntervalSet num1, IntervalSet num2)
    {
        return ApplyBinaryCornerEnvelope(num1, num2, SpecialFunctions.BesselJ);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet BesselY(IntervalSet num1, IntervalSet num2)
    {
        return ApplyBinaryCornerEnvelope(num1, num2, SpecialFunctions.BesselY);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet BesselI(IntervalSet num1, IntervalSet num2)
    {
        return ApplyBinaryCornerEnvelope(num1, num2, SpecialFunctions.BesselI);
    }

    /// <summary>
    ///     由AI生成 不保证准确性 请谨慎使用
    /// </summary>
    public static IntervalSet BesselK(IntervalSet num1, IntervalSet num2)
    {
        return ApplyBinaryCornerEnvelope(num1, num2, SpecialFunctions.BesselK);
    }

    private static bool ContainsNonPositiveInteger(IntervalSet num)
    {
        // 检测是否包含 0,-1,-2,...（Gamma/LnGamma/Digamma/Trigamma 极点）
        // 由于 IntervalSet 可能是多个 Range：逐段判断是否跨越某个非正整数
        foreach (var r in num.Intervals)
            if (r._Sup < 0)
            {
                // 负区间：[a,b]，若包含某个 -k（k>=0）
                var ceil = (int)Math.Ceiling(r._Inf); // 注意 r._Inf <= r._Sup
                var floor = (int)Math.Floor(r._Sup);
                for (var n = ceil; n <= floor; n++)
                    if (n <= 0 && r.ContainsEqual(n))
                        return true;
            }
            else if (r.ContainsEqual(0))
            {
                return true;
            }

        return false;
    }

    private static IntervalSet ApplyUnaryMonotoneInc(IntervalSet input, Func<double, double> f)
    {
        if (input._IsEmpty) return Empty;
        if (input.IsNumber) return Create(f(input.Number));

        var ranges = input.Intervals;
        ranges.MonotoneTransformInplace(f);

        var def = input._Def;
        if (HasNaNOrInf(ranges))
            return Create(double.NegativeInfinity, double.PositiveInfinity, FT);

        return Create(ranges.FormatRanges(), def);
    }

    private static IntervalSet ApplyUnaryMonotoneDec(IntervalSet input, Func<double, double> f)
    {
        if (input._IsEmpty) return Empty;
        if (input.IsNumber) return Create(f(input.Number));

        var ranges = input.Intervals;
        ranges.MonotoneTransformOpInplace(f);

        var def = input._Def;
        if (HasNaNOrInf(ranges))
            return Create(double.NegativeInfinity, double.PositiveInfinity, FT);

        return Create(ranges.FormatRanges(), def);
    }

    private static IntervalSet ApplyUnaryMonotoneIncWithDomain(IntervalSet input, double domainMin, double domainMax,
        Func<double, double> f)
    {
        if (input._IsEmpty) return Empty;

        if (input.IsNumber)
        {
            if (input.Number < domainMin || input.Number > domainMax) return Empty;
            return Create(f(input.Number));
        }

        var clipped = input.Intervals.SetBounds(new Range(domainMin, domainMax), out _);
        if (clipped.Length == 0) return Empty;

        var def = clipped.Length == input.Intervals.Length && input._Inf >= domainMin && input._Sup <= domainMax
            ? input._Def
            : FT;

        var tmp = Create(clipped, def);
        return ApplyUnaryMonotoneInc(tmp, f);
    }

    private static IntervalSet ApplyUnaryMonotoneDecWithDomain(IntervalSet input, double domainMin, double domainMax,
        Func<double, double> f)
    {
        if (input._IsEmpty) return Empty;

        if (input.IsNumber)
        {
            if (input.Number < domainMin || input.Number > domainMax) return Empty;
            return Create(f(input.Number));
        }

        var clipped = input.Intervals.SetBounds(new Range(domainMin, domainMax), out _);
        if (clipped.Length == 0) return Empty;

        var def = clipped.Length == input.Intervals.Length && input._Inf >= domainMin && input._Sup <= domainMax
            ? input._Def
            : FT;

        var tmp = Create(clipped, def);
        return ApplyUnaryMonotoneDec(tmp, f);
    }

    private static IntervalSet ApplyUnarySplitByPoint(IntervalSet input, double splitX, Func<double, double> f)
    {
        // 对每个 Range，如果跨越 splitX，则拆成两段分别按单调处理后再合并（用于“单峰/单谷”）
        if (input._IsEmpty) return Empty;
        if (input.IsNumber) return Create(f(input.Number));

        var tmp = new Ranges(new Range[input.Intervals.Length * 2]);
        var loc = 0;

        foreach (var r in input.Intervals)
        {
            if (r._Sup <= splitX || r._Inf >= splitX)
            {
                tmp[loc++] = r;
                continue;
            }

            tmp[loc++] = new Range(r._Inf, splitX);
            tmp[loc++] = new Range(splitX, r._Sup);
        }

        var splitSet = Create(tmp.Slice(0, loc), input._Def);

        // (0, split] 上按递减处理，[split, +∞) 上按递增处理（这里用于 Gamma/LnGamma 的“先降后升”）
        var resRanges = new Ranges(new Range[splitSet.Intervals.Length]);
        for (var i = 0; i < splitSet.Intervals.Length; i++)
        {
            var rr = splitSet.Intervals[i];
            if (rr._Sup <= splitX)
            {
                var a = f(rr._Inf);
                var b = f(rr._Sup);
                resRanges[i] = new Range(Math.Min(a, b), Math.Max(a, b));
            }
            else
            {
                var a = f(rr._Inf);
                var b = f(rr._Sup);
                resRanges[i] = new Range(Math.Min(a, b), Math.Max(a, b));
            }
        }

        var def = input._Def;
        if (HasNaNOrInf(resRanges))
            def = FT;

        return Create(resRanges.FormatRanges(), def);
    }

    private static IntervalSet ApplyUnaryCornerEnvelope(IntervalSet input, Func<double, double> f)
    {
        if (input._IsEmpty) return Empty;
        if (input.IsNumber) return Create(f(input.Number));

        var res = new Ranges(new Range[input.Intervals.Length]);
        var def = input._Def;

        for (var i = 0; i < input.Intervals.Length; i++)
        {
            var r = input.Intervals[i];
            var fa = f(r._Inf);
            var fb = f(r._Sup);

            if (double.IsNaN(fa) || double.IsNaN(fb) || double.IsInfinity(fa) || double.IsInfinity(fb))
            {
                res[i] = new Range(double.NegativeInfinity, double.PositiveInfinity);
                def = FT;
                continue;
            }

            res[i] = new Range(Math.Min(fa, fb), Math.Max(fa, fb));
        }

        return Create(res.FormatRanges(), def);
    }

    private static IntervalSet ApplyBinaryCornerEnvelope(IntervalSet a, IntervalSet b, Func<double, double, double> f)
    {
        if (a._IsEmpty || b._IsEmpty) return Empty;

        if (a.IsNumber && b.IsNumber)
            return Create(f(a.Number, b.Number));

        var tmp = new Ranges(new Range[a.Intervals.Length * b.Intervals.Length]);
        var loc = 0;
        var def = a._Def & b._Def;

        foreach (var ra in a.Intervals)
        foreach (var rb in b.Intervals)
        {
            var v1 = f(ra._Inf, rb._Inf);
            var v2 = f(ra._Inf, rb._Sup);
            var v3 = f(ra._Sup, rb._Inf);
            var v4 = f(ra._Sup, rb._Sup);

            if (double.IsNaN(v1) || double.IsNaN(v2) || double.IsNaN(v3) || double.IsNaN(v4) ||
                double.IsInfinity(v1) || double.IsInfinity(v2) || double.IsInfinity(v3) || double.IsInfinity(v4))
            {
                tmp[loc++] = new Range(double.NegativeInfinity, double.PositiveInfinity);
                def = FT;
                continue;
            }

            var min = Math.Min(Math.Min(v1, v2), Math.Min(v3, v4));
            var max = Math.Max(Math.Max(v1, v2), Math.Max(v3, v4));
            tmp[loc++] = new Range(min, max);
        }

        return Create(tmp.Slice(0, loc).FormatRanges(), def);
    }

    private static bool HasNaNOrInf(Ranges ranges)
    {
        foreach (var r in ranges)
            if (double.IsNaN(r._Inf) || double.IsNaN(r._Sup) || double.IsInfinity(r._Inf) || double.IsInfinity(r._Sup))
                return true;

        return false;
    }

    #endregion
}