using CsGrafeq.Collections;
using sysMath = System.Math;

namespace CsGrafeq.Interval.Extensions;

internal static class IntervalSetExtension
{
    /// <summary>
    ///     单调增
    /// </summary>
    public static unsafe void MonotoneTransformInplace(this Span<Range> ranges, delegate*<double, double> callback)
    {
        foreach (ref var range in ranges)
        {
            range._Inf = callback(range._Inf);
            range._Sup = callback(range._Sup);
        }
    }

    /// <summary>
    ///     单调增
    /// </summary>
    public static void MonotoneTransformInplace(this Span<Range> ranges, Func<double, double> callback)
    {
        foreach (ref var range in ranges)
        {
            range._Inf = callback(range._Inf);
            range._Sup = callback(range._Sup);
        }
    }

    /// <summary>
    ///     单调减
    /// </summary>
    public static unsafe void MonotoneTransformOpInplace(this Span<Range> ranges, delegate*<double, double> callback)
    {
        if (ranges.Length == 0)
            return;
        int i = 0, j = ranges.Length - 1;
        for (; i < j; i++, j--)
        {
            var ri = ranges[i];
            var rj = ranges[j];
            ranges[j] = new Range { _Inf = callback(ri._Sup), _Sup = callback(ri._Inf) };
            ranges[i] = new Range { _Inf = callback(rj._Sup), _Sup = callback(rj._Inf) };
        }

        if (i == j)
        {
            var r = ranges[i];
            ranges[i] = new Range { _Inf = callback(r._Sup), _Sup = callback(r._Inf) };
        }
    }

    /// <summary>
    ///     单调减
    /// </summary>
    public static void MonotoneTransformOpInplace(this Span<Range> ranges, Func<double, double> callback)
    {
        if (ranges.Length == 0)
            return;
        int i = 0, j = ranges.Length - 1; //注意！j为nuint类型 如果Length=0 j会变为最大值
        for (; i < j; i++, j--)
        {
            var ri = ranges[i];
            var rj = ranges[j];
            ranges[j] = new Range { _Inf = callback(ri._Sup), _Sup = callback(ri._Inf) };
            ranges[i] = new Range { _Inf = callback(rj._Sup), _Sup = callback(rj._Inf) };
        }

        if (i == j)
        {
            var r = ranges[i];
            ranges[i] = new Range { _Inf = callback(r._Sup), _Sup = callback(r._Inf) };
        }
    }

    public static Span<Range> FormatRanges(this Span<Range> ranges)
    {
        var validIndex = 0;
        for (var i = 0; i < ranges.Length; i++)
            if (!ranges[i].IsInValid)
                ranges[validIndex++] = ranges[i];
        if (validIndex == 0) return Span<Range>.Empty;

        //对区间排序
        for (var i = 0; i < validIndex - 1; i++)
        for (var j = 0; j < validIndex - i - 1; j++)
            if (ranges[j]._Inf > ranges[j + 1]._Inf)
                (ranges[j], ranges[j + 1]) = (ranges[j + 1], ranges[j]);
        var writeIndex = 0;
        for (var i = 1; i < validIndex; i++)
        {
            ref var writecurrent = ref ranges[writeIndex];
            ref var readcurrent = ref ranges[i];
            if (writecurrent._Sup >= readcurrent._Inf)
                writecurrent._Sup = sysMath.Max(writecurrent._Sup, readcurrent._Sup);
            else
                writeIndex++;
        }

        return ranges.Slice(0, writeIndex + 1);
    }

    public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, IntervalSet i2,
        delegate*<Range, Range, Range> handler)
    {
        var ranges = StaticUnsafeMemoryList<Range>.Rent(i1.Intervals.Length * i2.Intervals.Length);
        var loc = 0;
        foreach (var i in i1.Intervals)
        foreach (var j in i2.Intervals)
            ranges[loc++] = handler(i, j);
        var formatted = ranges.FormatRanges();
        return IntervalSet.Create(formatted, i1._Def & i2._Def);
    }

    public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, delegate*<Range, Range> handler)
    {
        var ivl = i1.Intervals;
        for (var i = 0; i < i1.Intervals.Length; i++)
            ivl[i] = handler(ivl[i]);

        var formatted = ivl.FormatRanges();
        return IntervalSet.Create(formatted, i1._Def);
    }

    /// <summary>
    ///     默认已经格式化
    /// </summary>
    /// <param name="ranges"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static Span<Range> SetBounds(this Span<Range> ranges, Range range, out bool ifSetBounds)
    {
        ifSetBounds = false;
        if (ranges.Length == 0)
            return Span<Range>.Empty;
        if (range._Inf <= ranges[0]._Inf && range._Sup >= ranges[^1]._Sup) return ranges;
        ifSetBounds = true;
        var min = range._Inf;
        var max = range._Sup;
        var left = 0;
        while (left < ranges.Length && ranges[left]._Sup < min)
            left++;
        if (left == ranges.Length)
            return Span<Range>.Empty;
        var right = ranges.Length - 1;
        while (right >= left && ranges[right]._Inf > max)
            right--;
        if (right < left)
            return Span<Range>.Empty;
        var result = ranges.Slice(left, right - left + 1);
        return result;
    }

    public static IntervalSet IntervalAggregate(this IEnumerable<IntervalSet> ranges,
        Func<IntervalSet, IntervalSet, IntervalSet> aggregator)
    {
        var enumerator = ranges.GetEnumerator();
        var value = enumerator.Current;
        while (enumerator.MoveNext()) value = aggregator(value, enumerator.Current);
        return value;
    }
}