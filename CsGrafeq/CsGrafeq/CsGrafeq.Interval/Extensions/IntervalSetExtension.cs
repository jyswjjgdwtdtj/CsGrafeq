using sysMath = System.Math;

namespace CsGrafeq.Interval.Extensions;

internal static class IntervalSetExtension
{
    /// <summary>
    ///     单调增
    /// </summary>
    public static unsafe void MonotoneTransform(this Range[] ranges, delegate*<double, double> callback)
    {
        for (var i = 0; i < ranges.Length; i++)
            ranges[i] = new Range
            {
                _Inf = callback(ranges[i]._Inf),
                _Sup = callback(ranges[i]._Sup)
            };
    }

    /// <summary>
    ///     单调减
    /// </summary>
    public static unsafe void MonotoneTransformOp(this Range[] ranges, delegate*<double, double> callback)
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

    public static Range[] FormatRanges(this Range[] Ranges)
    {
        var validIndex = 0;
        for (var i = 0; i < Ranges.Length; i++)
            if (!Ranges[i].IsInValid)
                Ranges[validIndex++] = Ranges[i];

        if (validIndex == 0) return new Range[0];

        //对区间排序
        for (var i = 0; i < validIndex - 1; i++)
        for (var j = 0; j < validIndex - i - 1; j++)
            if (Ranges[j]._Inf > Ranges[j + 1]._Inf)
                (Ranges[j], Ranges[j + 1]) = (Ranges[j + 1], Ranges[j]);
        var writeIndex = 0;
        for (var i = 1; i < validIndex; i++)
        {
            ref var writecurrent = ref Ranges[writeIndex];
            ref var readcurrent = ref Ranges[i];
            if (writecurrent._Sup >= readcurrent._Inf)
                writecurrent._Sup = sysMath.Max(writecurrent._Sup, readcurrent._Sup);
            else
                writeIndex++;
        }

        return Ranges.Slice(0, writeIndex + 1);
    }

    public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, IntervalSet i2,
        delegate*<Range, Range, Range> handler)
    {
        var Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
        var loc = 0;
        foreach (var i in i1.Intervals)
        foreach (var j in i2.Intervals)
            Ranges[loc++] = handler(i, j);
        return IntervalSet.Create(FormatRanges(Ranges), i1._Def & i2._Def);
    }

    public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, delegate*<Range, Range> handler)
    {
        var ivl = i1.Intervals;
        for (var i = 0; i < i1.Intervals.Length; i++)
            ivl[i] = handler(ivl[i]);
        return IntervalSet.Create(FormatRanges(i1.Intervals), i1._Def);
    }

    public static Range[] SetBounds(this Range[] ranges, Range range)
    {
        var writeIndex = 0;
        for (var readIndex = 0; readIndex < ranges.Length; readIndex++)
        {
            var readCurrent = ranges[readIndex];
            if (readCurrent._Sup < range._Inf)
                continue;
            if (readCurrent._Inf > range._Sup)
                break;
            readCurrent._Inf = sysMath.Max(readCurrent._Inf, range._Inf);
            readCurrent._Sup = sysMath.Min(readCurrent._Sup, range._Sup);
            ranges[writeIndex++] = readCurrent;
        }

        return ranges.Slice(0, writeIndex);
    }

    public static Range[] Slice(this Range[] ranges, int start, int end)
    {
        return ranges[start..end];
    }
}