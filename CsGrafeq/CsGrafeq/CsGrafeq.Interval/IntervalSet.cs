using System.Diagnostics.CodeAnalysis;
using CsGrafeq.Interval.Interface;
using CsGrafeq.Utilities;
using static CsGrafeq.Interval.Def;
using static CsGrafeq.Interval.Extensions.IntervalSetExtension;
using CGMath=CsGrafeq.Utilities.CsGrafeqMath;

namespace CsGrafeq.Interval;

public readonly struct IntervalSet : IInterval<IntervalSet>
{
    #region 定义

    public static readonly IntervalSet Empty = new(new Range[0], FF, false, double.NaN, double.NaN, double.NaN);
    public static readonly Range EmptyRange = new(double.NaN);
    internal readonly Range[] Intervals;
    public readonly Def Def => _Def;
    public readonly double Sup => _Sup;
    public readonly double Inf => _Inf;

    public readonly Def _Def;

    public readonly double _Sup;

    public readonly double _Inf;

    internal readonly bool IsNumber;

    internal readonly double Number;

    public IntervalSet() : this([],TT,false,double.NaN,double.NaN,double.NaN) { 
    }
    public IntervalSet(Range[] ranges, Def def, bool isNumber, double inf, double sup, double number)
    {
        Intervals = ranges;
        _Def = TT;
        IsNumber = isNumber;
        _Inf = inf;
        _Sup = sup;
        Number = number;
    }

    public static IntervalSet CreateFromDouble(double num)
    {
        return Create(num);
    }

    public static IntervalSet Create(double num)
    {
        return new IntervalSet(new Range[1] { new(num, num) }, TT, true, num, num, num);
    }

    public static IntervalSet Create(double min, double max, Def def)
    {
        CsGrafeqMath.SwapIfNotLess(ref min, ref max);
        return new IntervalSet(new Range[1] { new(min, max) }, TT, false, min, max, double.NaN);
    }

    public static IntervalSet Create(double[] nums, Def def, bool needSort = false)
    {
        Array.Sort(nums);
        var intervals = new Range[nums.Length];
        for (var idx = 0; idx < intervals.Length; idx++) intervals[idx] = new Range(nums[idx]);
        return new IntervalSet(intervals, def, false, nums.Length == 0 ? double.NaN : nums[0],
            nums.Length == 0 ? double.NaN : nums[nums.Length - 1], double.NaN);
    }

    public static IntervalSet Create(Range[] ranges, Def def)
    {
        return new IntervalSet(ranges, def, false, ranges.Length == 0 ? double.NaN : ranges[0]._Inf,
            ranges.Length == 0 ? double.NaN : ranges[ranges.Length - 1]._Sup, double.NaN);
    }

    public static IntervalSet Clone(IntervalSet source)
    {
        return Create((Range[])source.Intervals.Clone(), source._Def);
    }

    public double Width => _Sup - _Inf;
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
        if (i1.IsEmpty || i2.IsEmpty) return Empty;

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
        for (var idx = 0; idx < i1.Intervals.Length; idx++)
        {
            var i = i1.Intervals[idx];
            i1.Intervals[idx] = new Range(i._Inf + num, i._Sup + num);
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
        if (i1.IsEmpty || i2.IsEmpty) return Empty;

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
        if (double.IsNaN(i1._Inf) || double.IsNaN(i2._Inf))
            return EmptyRange;
        if (i1._Inf > 0 && i2._Inf > 0) return new Range { _Inf = i1._Inf * i2._Inf, _Sup = i1._Sup * i2._Sup };
        if (i1._Sup < 0 && i2._Sup < 0) return new Range { _Inf = i1._Sup * i2._Sup, _Sup = i1._Inf * i2._Inf };
        var res = CGMath.GetMinMax4(i1._Inf * i2._Inf, i1._Inf * i2._Sup, i1._Sup * i2._Inf, i1._Sup * i2._Sup);
        return new Range(res.Item1, res.Item2);
    }

    public static IntervalSet operator *(double num, IntervalSet i1)
    {
        return i1 * num;
    }

    public static IntervalSet operator *(IntervalSet i1, double num)
    {
        if (num == 0) return Create(0);

        if (double.IsInfinity(num)) return Create(double.NegativeInfinity, double.PositiveInfinity, TT);

        if (num > 0)
        {
            for (var idx = 0; idx < i1.Intervals.Length; idx++)
            {
                var i = i1.Intervals[idx];
                i1.Intervals[idx] = new Range(i._Inf * num, i._Sup * num);
            }

            return i1;
        }

        return -(i1 * -num);
    }

    public static IntervalSet operator /(IntervalSet i1, IntervalSet i2)
    {
        var i1i = i1.Intervals;
        var i2i = i2.Intervals;
        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number / i2.Number);

        if (i2.IsNumber) return i1 * (1 / i2.Number);
        if (i1.IsNumber) i1 = Create(new[] { new Range(i1.Number, i1.Number) }, i1._Def);

        if (i1.IsEmpty || i2.IsEmpty) return Empty;

        var ranges = new Range[i2.Intervals.Length + 1];
        for (var idx = 0; idx < i1.Intervals.Length; idx++)
        {
            var ii = i1.Intervals[idx];
            i1.Intervals[idx] = new Range(ii._Inf == 0 ? double.Epsilon : ii._Inf,
                ii._Sup == 0 ? -double.Epsilon : ii._Sup);
        }

        var loc = 0;
        for (var idx = 0; idx < i2.Intervals.Length; idx++)
        {
            var i = i2.Intervals[idx];
            if (i.ContainsEqual(0))
            {
                ranges[loc++] = new Range { _Inf = double.NegativeInfinity, _Sup = 1 / i._Inf };
                ranges[loc++] = new Range { _Inf = 1 / i._Sup, _Sup = double.PositiveInfinity };
                continue;
            }

            ranges[loc++] = new Range(1 / i._Inf, 1 / i._Sup);
        }

        var res = ranges.Slice(0, loc);
        return i1 * Create(res, i2._Def);
    }

    public static IntervalSet operator %(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(i1.Number % i2.Number);

        if (i1.IsEmpty || i2.IsEmpty) return Empty;

        if (i2.IsNumber)
        {
            var num = i2.Number;
            var ranges = new Range[i1.Intervals.Length * 2];
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

            if (loc == 0) return Empty;

            var res = ranges.Slice(0, loc);
            return Create(res, FT);
        }

        {
            var q = Floor(i1 / i2);
            var min = q._Inf;
            var max = q._Sup;
            var i = i1 - i2 * q;
            double sup = 0, inf = 0;
            if (i2._Sup > 0)
                sup = i2._Sup;
            if (i2._Inf < 0)
                inf = i2._Inf;
            return Create(i.Intervals.SetBounds(new Range(inf, sup)), FT);
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
        return ThrowWithMessage<NotImplementedException, Def>(new NotImplementedException());
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
        if (i.IsEmpty) return Empty;

        if (i.IsNumber) return Create(Math.Sign(i.Number));

        var tmp = new Range[3];
        var loc = 0;
        if (i._Inf < 0)
            tmp[loc++] = new Range(-1);
        foreach (var j in i.Intervals)
            if (j.ContainsEqual(0))
                tmp[loc++] = new Range(0);

        if (i._Sup < 0)
            tmp[loc++] = new Range(1);
        var res = new Range[loc];
        for (var j = 0; j < loc; j++) res[j] = tmp[j];
        return Create(res, FT);
    }

    public static unsafe IntervalSet Abs(IntervalSet i1)
    {
        if (i1.IsEmpty)
            return Empty;
        if (i1.IsNumber) return Create(Math.Abs(i1._Inf));
        return IntervalSetMethod(i1, &RangeAbs);
    }

    private static Range RangeAbs(Range i)
    {
        double inf = 0, sup = 0;
        if (i.ContainsEqual(0))
        {
            sup = Math.Max(-i._Inf, i._Sup);
            inf = 0;
            return new Range(inf, sup);
        }

        if (i._Sup < 0) (inf, sup) = (-i._Sup, -i._Inf);
        return new Range(inf, sup);
    }

    public static unsafe IntervalSet Min(IntervalSet i1, IntervalSet i2)
    {
        if (i1.IsNumber && i2.IsNumber) return Create(Math.Min(i1.Number, i2.Number));
        if (i1.IsEmpty || i2.IsEmpty)
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
        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        return IntervalSetMethod(i1, i2, &RangeMax);
    }

    private static Range RangeMax(Range i1, Range i2)
    {
        return new Range { _Inf = Math.Max(i1._Inf, i2._Inf), _Sup = Math.Max(i1._Sup, i2._Sup) };
    }

    public static IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
    {
        if (c1.IsNumber && c2.IsNumber && c3.IsNumber) return Create(DoubleMedian(c1.Number, c2.Number, c3.Number));

        if (c1.IsEmpty || c2.IsEmpty || c3.IsEmpty) return Empty;

        var loc = 0;
        var res = new Range[c1.Intervals.Length * c2.Intervals.Length * c3.Intervals.Length];
        foreach (var i1 in c1.Intervals)
        foreach (var i2 in c2.Intervals)
        foreach (var i3 in c3.Intervals)
            res[loc++] = RangeMedian(i1, i2, i3);

        return Create(res.FormatRanges(), c1._Def & c2._Def & c3._Def);
    }

    public static unsafe IntervalSet Exp(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Exp(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&Math.Exp);
        return i1;
    }

    public static IntervalSet Ln(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Log(i1.Number));
        //e为底
        if (i1.IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1]._Sup <= 0) return Empty;

        var current = 0;
        for (var i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0)
                i1.Intervals[current++] = new Range(double.NegativeInfinity, Math.Log(range._Sup));
            else
                i1.Intervals[current++] = new Range(Math.Log(range._Inf), Math.Log(range._Sup));
        }

        var res = i1.Intervals.Slice(0, current);
        return Create(res, FT);
    }

    public static IntervalSet Lg(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Log(i1.Number));

        //e为底
        if (i1.IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1]._Sup <= 0) return Empty;

        var current = 0;
        for (var i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0)
                i1.Intervals[current++] = new Range(double.NegativeInfinity, Math.Log10(range._Sup));
            else
                i1.Intervals[current++] = new Range(Math.Log10(range._Inf), Math.Log10(range._Sup));
        }

        var res = i1.Intervals.Slice(0, current);
        return Create(res, FT);
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
                    var ranges = new Range[i1.Intervals.Length + 1];
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
                        ranges[loc] = EmptyRange;
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
        //e为底
        if (i1.IsEmpty) return Empty;

        var len = i1.Intervals.Length;
        if (i1.Intervals[len - 1]._Sup <= 0) return Empty;

        var current = 0;
        for (var i = 0; i < len; i++)
        {
            var range = i1.Intervals[i];
            if (range._Sup < 0)
                continue;
            if (range._Inf < 0)
                i1.Intervals[current++] = new Range(0, Math.Sqrt(range._Sup));
            else
                i1.Intervals[current++] = new Range(Math.Sqrt(range._Inf), Math.Sqrt(range._Sup));
        }

        var res = i1.Intervals.Slice(0, current);
        return Create(res, FT);
    }

    public static unsafe IntervalSet Cbrt(IntervalSet num)
    {
        if (num.IsNumber)
            return Create(Math.Cbrt(num.Number));
        if (num.IsEmpty)
            return Empty;
        num.Intervals.MonotoneTransform(&Math.Cbrt);
        return num;
    }

    private static Range RangeCbrt(Range num)
    {
        return new Range(Math.Cbrt(num._Inf), Math.Cbrt(num._Sup));
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
        if (i1.IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        var loc = 0;
        var ranges = new Range[len * 2 + 1];
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
        if (i1.IsEmpty)
            return Empty;
        var len = i1.Intervals.Length;
        var loc = 0;
        var ranges = new Range[len * 2 + 1];
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

        var res =ranges.Slice(0, loc);
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
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
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
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
        }

        if (i1.IsEmpty || i2.IsEmpty)
            return Empty;
        if (!(i1.Intervals[0].TryGetInteger(out _) && i2.Intervals[0].TryGetInteger(out _)))
            return ThrowWithMessage<ArgumentException, IntervalSet>(new ArgumentException("参数需经过Floor,Ceil函数处理"));
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
        if (i1.IsEmpty)
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

        if (i1.IsEmpty)
            return Empty;
        var ranges = new Range[i1.Intervals.Length * 2];
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
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&Math.Atan);
        return i1;
    }

    public static unsafe IntervalSet ArcCos(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Acos(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        if (i1._Sup < -1 || i1.Number > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { _Inf = -1, _Sup = 1 });
        res.MonotoneTransformOp(&Math.Acos);
        return Create(res, i1._Sup > 1 || i1.Number < -1 ? FT : i1._Def);
    }

    public static unsafe IntervalSet ArcSin(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Asin(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        if (i1._Sup < -1 || i1._Inf > 1)
            return Empty;
        var res = i1.Intervals.SetBounds(new Range { _Inf = -1, _Sup = 1 });
        res.MonotoneTransform(&Math.Asin);
        return Create(res, i1._Sup > 1 || i1._Inf < -1 ? FT : i1._Def);
    }

    public static unsafe IntervalSet Sinh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Sinh(i1.Number));
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&Math.Sinh);
        return i1;
    }

    public static IntervalSet Cosh(IntervalSet i1)
    {
        if (i1.IsNumber) return Create(Math.Cosh(i1.Number));
        if (i1.IsEmpty)
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
        if (i1.IsEmpty)
            return Empty;
        i1.Intervals.MonotoneTransform(&Math.Tanh);
        return i1;
    }

    public static unsafe IntervalSet ArcCosh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Acosh(num.Number));
        var min = num._Inf;
        var res = num.Intervals.SetBounds(new Range(1, double.PositiveInfinity));
        var min1 = double.NaN;
        if (res.Length > 0)
            min1 = res[0]._Inf;
        res.MonotoneTransformOp(&Math.Acosh);
        return Create(res, min == min1 ? num._Def : FT);
    }

    public static unsafe IntervalSet ArcSinh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Asinh(num.Number));
        num.Intervals.MonotoneTransform(&Math.Asinh);
        return num;
    }

    public static IntervalSet ArcTanh(IntervalSet num)
    {
        if (num.IsNumber) return Create(Math.Atanh(num.Number));
        //e为底
        if (num.IsEmpty) return Empty;

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

    #endregion

    [DoesNotReturn]
    private static TResult ThrowWithMessage<TException, TResult>(TException exception) where TException : Exception
    {
        throw exception;
    }
}