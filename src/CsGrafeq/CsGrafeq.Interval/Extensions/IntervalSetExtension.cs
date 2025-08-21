using CsGrafeq.Interval.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ranges=CsGrafeq.Collections.NativeBuffer<CsGrafeq.Interval.Range>;
using sysMath=System.Math;

namespace CsGrafeq.Interval.Extensions
{
    internal static class IntervalSetExtension
    {
        public static void ForceToIntervals(ref IntervalSet set)
        {
            if (set.IsNumber)
            {
                set=IntervalSet.Create(new Ranges(new Range(set.Number)), set.Def);
            }
        }
        /// <summary>
        /// 单调增
        /// </summary>
        public unsafe static void MonotoneTransform(this Ranges ranges, delegate*<double,double> callback)
        {
            foreach (ref var i in ranges)
            {
                i = new Range
                {
                    Inf = callback(i.Inf),
                    Sup = callback(i.Sup)
                };
            }
        }
        /// <summary>
        /// 单调减
        /// </summary>
        public unsafe static void MonotoneTransformOp(this Ranges ranges, delegate*<double, double> callback)
        {
            if (ranges.Length == 0)
                return;
            nuint i = 0, j = ranges.Length - 1;//注意！j为nuint类型 如果Length=0 j会变为最大值
            for (; i < j; i++, j--)
            {
                Range ri = ranges[i];
                Range rj = ranges[j];
                ranges[j] = new Range { Inf = callback(ri.Sup), Sup = callback(ri.Inf) };
                ranges[i] = new Range { Inf = callback(rj.Sup), Sup = callback(rj.Inf) };
            }
            if (i == j)
            {
                Range r = ranges[i];
                ranges[i] = new Range { Inf = callback(r.Sup), Sup = callback(r.Inf) };
            }
        }
        public static Ranges FormatRanges(this Ranges Ranges)
        {
            nuint validIndex = 0;
            for (nuint i = 0; i < Ranges.Length; i++)
            {
                if (!Ranges[i].IsInValid)
                {
                    Ranges[validIndex++]=Ranges[i];
                }
            }
            if (validIndex == 0)
            {
                Ranges.Dispose();
                return new Ranges(0);
            }
            //对区间排序
            for (nuint i = 0; i < validIndex - 1; i++)
                for (nuint j = 0; j < validIndex - i - 1; j++)
                    if (Ranges[j].Inf > Ranges[j + 1].Inf)
                        (Ranges[j], Ranges[j + 1]) = (Ranges[j + 1], Ranges[j]);
            nuint writeIndex = 0;
            for (nuint i = 1; i < validIndex; i++)
            {
                ref var writecurrent=ref Ranges[writeIndex];
                ref var readcurrent=ref Ranges[i];
                if (writecurrent.Sup >= readcurrent.Inf)
                {
                    writecurrent.Sup = sysMath.Max(writecurrent.Sup, readcurrent.Sup);
                }
                else
                {
                    writeIndex++;
                }
            }
            return Ranges.SliceAndDispose(0,writeIndex+1);
        }
        public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, IntervalSet i2, delegate*<Range, Range, Range> handler)
        {
            var Ranges = new Ranges(i1.Intervals.Length * i2.Intervals.Length);
            nuint loc = 0;
            foreach(ref var i in i1.Intervals)
            {
                foreach(ref var j in i2.Intervals)
                {
                    Ranges[loc++] = handler(i, j);
                }
            }
            i1.Intervals.Dispose();
            i2.Intervals.Dispose();
            return IntervalSet.Create(FormatRanges(Ranges), i1.Def & i2.Def);
        }

        public static unsafe IntervalSet IntervalSetMethod(IntervalSet i1, delegate*<Range, Range> handler)
        {
            foreach(ref var i in i1.Intervals)
            {
                i = handler(i);
            }
            return IntervalSet.Create(FormatRanges(i1.Intervals), i1.Def);
        }
        public static Ranges SetBounds(this Ranges ranges,Range range)
        {
            nuint writeIndex = 0;
            for(nuint readIndex=0;readIndex<ranges.Length; readIndex++)
            {
                var readCurrent=ranges[readIndex];
                if (readCurrent.Sup < range.Inf)
                    continue;
                if (readCurrent.Inf > range.Sup)
                    break;
                readCurrent.Inf = sysMath.Max(readCurrent.Inf, range.Inf);
                readCurrent.Sup = sysMath.Min(readCurrent.Sup, range.Sup);
                ranges[writeIndex++] = readCurrent;
            }
            return ranges.SliceAndDispose(0,writeIndex); 
        }
    }
}
