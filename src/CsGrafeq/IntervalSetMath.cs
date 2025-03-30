using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;

namespace CsGrafeq
{
    /// <summary>
    /// 1.对于所能达到的最小区间，计算结果必须绝对精确
    /// 2.对于任何区间计算，计算的结果必须包含实际结果
    /// </summary>
    public static class IntervalSetMath
    {
        private static IntervalSet EmptyIntervalSet=new IntervalSet(double.NaN) { Def = (false, false) };
        private static Range EmptyRange = new Range(double.NaN);
        private static readonly double neginf = double.NegativeInfinity;
        private static readonly double posinf = double.PositiveInfinity;
        private const double PI=Math.PI;
        private const double E=Math.E;
        #region 四则运算
        public unsafe static IntervalSet Add(IntervalSet i1,IntervalSet i2)
        {
            if(!(i1.Def.Item2&&i2.Def.Item2))
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (Range* ptr=Ranges,i1start=i1.Intervals,i2start=i2.Intervals)
            {
                for (Range* i = i1start; i < i1start+i1.Intervals.Length; i++)
                    for (Range* j = i2start; j < i2start + i2.Intervals.Length; j++)
                        *(ptr+loc++) = RangeAdd(i, j);
            }
            return GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private unsafe static Range RangeAdd(Range* i1,Range* i2)
        {
            return new Range() { Min=i1->Min+i2->Min,Max=i1->Max+i2->Max};
        }
        public unsafe static IntervalSet Neg(IntervalSet i)
        {
            int len = i.Intervals.Length;
            Range[] Ranges = new Range[i.Intervals.Length];
            for(int j = 0; j < i.Intervals.Length; j++)
            {
                Ranges[j]=RangeNeg(i.Intervals[len-1-j]);
            }
            i.Intervals=Ranges;
            return i;
        }
        private static Range RangeNeg(Range i)
        {
            return new Range() { Max = -i.Min, Min = -i.Max };
        }
        public static IntervalSet Subtract(IntervalSet i1,IntervalSet i2)
        {
            return Add(i1,Neg(i2));
        }
        public unsafe static IntervalSet Multiply(IntervalSet i1,IntervalSet i2)
        {
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (Range* ptr = Ranges, i1start = i1.Intervals, i2start = i2.Intervals)
            {
                for (Range* i = i1start; i < i1start + i1.Intervals.Length; i++)
                    for (Range* j = i2start; j < i2start + i2.Intervals.Length; j++)
                        *(ptr + loc++) = RangeMultiply(*i, *j);
            }
            return GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private static Range RangeMultiply(Range i1,Range i2)
        {
            if (double.IsNaN(i1.Min) || double.IsNaN(i2.Min))
                return EmptyRange;
            if (i1.Min > 0 && i2.Min > 0)
            {
                i1.Min*=i2.Min;
                i1.Max*=i2.Max;
                return i1;
            }
            if (i1.Max < 0 && i2.Max < 0)
            {
                return new Range(i1.Max*i2.Max,i1.Min*i2.Min);
            }
            (i1.Min,i1.Max) = GetMinMax4(i1.Min*i2.Min,i1.Min*i2.Max,i1.Max*i2.Min,i1.Max*i2.Max);
            return i1;
        }
        public static IntervalSet Divide(IntervalSet i1, IntervalSet i2)
        {
            //a/b=>a*(1/b)
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            if(i2.Intervals.Length==1)
                if (i2.Intervals[0].Min==0&& i2.Intervals[0].Max == 0)
                    return EmptyIntervalSet;
            //1/i2
            Range[] Ranges = new Range[5];
            int loc = 0;
            int len = 5;
            Range Range1, Range2;
            //for(int ii = 0; ii < i1.Intervals.Length; ii++)
            //{
                //i1.Intervals[ii].Max = i1.Intervals[ii].Max == 0 ? -double.Epsilon : i1.Intervals[ii].Max;
                //i1.Intervals[ii].Min = i1.Intervals[ii].Min == 0 ? double.Epsilon : i1.Intervals[ii].Min;
            //}
            foreach (Range i in i2.Intervals)
            {
                if (i.ContainsEqual(0))
                {
                    Range1 = new Range() { Min = neginf, Max = 1 / i.Min };
                    Range2 = new Range() { Min = 1 / i.Max, Max = posinf };
                    foreach (Range j in i1.Intervals)
                    {
                        Ranges[loc++] = RangeMultiply(Range1, j);
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref Ranges, len);
                        }
                        Ranges[loc++] = RangeMultiply(Range2, j);
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref Ranges, len);
                        }
                    }
                    continue;
                }
                Range1 = new Range(1 / i.Min, 1 / i.Max);
                foreach (Range j in i1.Intervals)
                {
                    Ranges[loc++] = RangeMultiply(Range1, j);
                    if (loc == len)
                    {
                        len *= 2;
                        Array.Resize(ref Ranges, len);
                    }
                }
            }
            Array.Resize(ref Ranges, loc);
            IntervalSet iss = GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
            return iss;
        }
        #endregion
        #region 数学函数    
        public unsafe static IntervalSet Sgn(IntervalSet i)
        {
            if (!i.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[3];
            int loc = 0;
            fixed(Range* first = i.Intervals)
            {
                if (first->Min < 0)
                    Ranges[loc++] = new Range(-1);
                for (Range* f = first; f < first + i.Intervals.Length; f++)
                {
                    if (f->Min <= 0 && f->Max >= 0)
                    {
                        Ranges[loc++] = new Range(0);
                        break;
                    }
                }
                if ((first + i.Intervals.Length - 1)->Max > 0)
                    Ranges[loc++] = new Range(1);
                while (loc < 3)
                    Ranges[loc++] = EmptyRange;
            }
            i.Intervals = Ranges;
            return i;
        }
        public unsafe static IntervalSet Abs(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = RangeAbs(*i);
            return GetIntervalSetFromRangeArray(Ranges,i1.Def,i1.Cont);
        }
        private static Range RangeAbs(Range i)
        {
            if (i.ContainsEqual(0))
            {
                i.Max = Math.Max(-i.Min, i.Max);
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
        public unsafe static IntervalSet Min(IntervalSet i1, IntervalSet i2)
        {
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (Range* ptr = Ranges, i1start = i1.Intervals, i2start = i2.Intervals)
            {
                for (Range* i = i1start; i < i1start + i1.Intervals.Length; i++)
                    for (Range* j = i2start; j < i2start + i2.Intervals.Length; j++)
                        *(ptr + loc++) = RangeMin(i, j);
            }
            return GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private unsafe static Range RangeMin(Range* i1, Range* i2)
        {
            return new Range() { Min = Math.Min(i1->Min, i2->Min), Max = Math.Min(i1->Max, i2->Max) };
        }
        public unsafe static IntervalSet Max(IntervalSet i1, IntervalSet i2)
        {
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (Range* ptr = Ranges, i1start = i1.Intervals, i2start = i2.Intervals)
            {
                for (Range* i = i1start; i < i1start + i1.Intervals.Length; i++)
                    for (Range* j = i2start; j < i2start + i2.Intervals.Length; j++)
                        *(ptr + loc++) = RangeMax(i, j);
            }
            return GetIntervalSetFromRangeArray(Ranges, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private unsafe static Range RangeMax(Range* i1, Range* i2)
        {
            return new Range() { Min = Math.Max(i1->Min, i2->Min), Max = Math.Max(i1->Max, i2->Max) };
        }
        public unsafe static IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
        {
            if (!(c1.Def.Item2&&c2.Def.Item2&&c3.Def.Item2))
                return EmptyIntervalSet;
            Range[] Ranges=new Range[5];
            int len = 5;
            int loc = 0;
            int c1len=c1.Intervals.Length;
            int c2len=c2.Intervals.Length;
            int c3len=c3.Intervals.Length;
            fixed(Range* c1first = c1.Intervals, c2first = c2.Intervals, c3first = c3.Intervals)
            {
                for(Range* j1 = c1first; j1 < c1first + c1len; j1++)
                {
                    for (Range* j2 = c2first; j2 < c2first + c2len; j2++)
                    {
                        for (Range* j3 = c3first; j3 < c3first + c3len; j3++)
                        {
                            Ranges[loc++] = RangeMedian(j1,j2,j3);
                            if (loc == len)
                            {
                                len *= 2;
                                Array.Resize(ref Ranges, len);
                            }
                        }
                    }
                }
            }
            Array.Resize(ref Ranges, loc);
            return GetIntervalSetFromRangeArray(Ranges,And(And(c1.Def,c2.Def),c3.Def),c1.Cont&&c2.Cont&&c3.Cont);
        }
        public unsafe static IntervalSet Exp(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals,Rangesfirst=Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = new Range() { Min = Math.Exp(i->Min), Max = Math.Exp(i->Max) };
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet Ln(IntervalSet i1)
        {
            //e为底
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            int len = i1.Intervals.Length;
            if (i1.Intervals[len - 1].Max <= 0)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[len];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
            {
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(Rangesfirst + loc++) = new Range(neginf, Math.Log(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(Rangesfirst + loc++) = new Range(Math.Log(i->Min), Math.Log(i->Max));
                }
            }
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet Lg(IntervalSet i1)
        {
            //e为底
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            int len = i1.Intervals.Length;
            if (i1.Intervals[len - 1].Max <= 0)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[len];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
            {
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(Rangesfirst + loc++) = new Range(neginf, Math.Log10(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(Rangesfirst + loc++) = new Range(Math.Log10(i->Min), Math.Log10(i->Max));
                }
            }
            i1.Intervals = Ranges;
            return i1;
        }
        public static IntervalSet Log(IntervalSet i1,IntervalSet i2)
        {
            return Divide(Ln(i1),Ln(i2));
        }
        public static IntervalSet Pow(IntervalSet i1, IntervalSet i2)
        {
            return Exp(Multiply(Ln(i1), i2));
        }
        public unsafe static IntervalSet Sqrt(IntervalSet i1)
        {
            //e为底
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            int len = i1.Intervals.Length;
            if (i1.Intervals[len - 1].Max <= 0)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[len];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
            {
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(Rangesfirst + loc++) = new Range(0, Math.Sqrt(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(Rangesfirst + loc++) = new Range(Math.Sqrt(i->Min), Math.Sqrt(i->Max));
                }
            }
            i1.Intervals = Ranges;
            return i1;
        }
        private unsafe static Range RangeMedian(Range* i1, Range* i2, Range* i3)
        {
            Range i = new Range();
            double[] arr = new double[3] { i1->Min, i2->Min, i3->Min };
            Array.Sort<double>(arr);
            i.Min = arr[1];
            arr = new double[3] { i1->Max, i2->Max, i3->Max };
            Array.Sort<double>(arr);
            i.Max = arr[1];
            return i;
        }
        public unsafe static IntervalSet Floor(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            int len = i1.Intervals.Length;
            int loc=0;
            Range[] Ranges=new Range[len*2+1];
            fixed(Range* first = i1.Intervals)
            {
                for (int i = 0; i < len; i++)
                {
                    if (loc <2)
                    {
                        double min = Math.Floor((first + i)->Min);
                        double max = Math.Floor((first + i)->Max);
                        if (min == max)
                        {
                            Ranges[loc++]=new Range(min);
                            continue;
                        }
                        Ranges[loc++] = new Range(min);
                        Ranges[loc++] = new Range(min+1);
                        if (max > min + 1)
                            Ranges[loc++] = new Range(min + 2,max);
                        continue;
                    }
                    for (int j = i; j < len; j++)
                        Ranges[loc++] = *(first + j);
                    break;
                }
            }
            Array.Resize(ref Ranges, loc);
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet Ceil(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            int len = i1.Intervals.Length;
            int loc = 0;
            Range[] Ranges = new Range[len * 2+1];
            fixed (Range* first = i1.Intervals)
            {
                for (int i = 0; i < len; i++)
                {
                    if (loc < 2)
                    {
                        double min = Math.Ceiling((first + i)->Min);
                        double max = Math.Ceiling((first + i)->Max);
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
                    for (int j = i; j < len; j++)
                        Ranges[loc++] = *(first + j);
                    break;
                }
            }
            Array.Resize(ref Ranges, loc);
            i1.Intervals = Ranges;
            return i1;
        }
        public static IntervalSet GCD(IntervalSet i1, IntervalSet i2)
        {
            if ((!i1.Def.Item2) || (!i2.Def.Item2))
                return EmptyIntervalSet;
            if (!(i1.Intervals[0].IsInterger() && i2.Intervals[0].IsInterger()))
            {
                throw new ArgumentException("参数需经过Floor,Ceil函数处理");
            }
            if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            {
                return new IntervalSet(GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min));
            }
            if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            {
                return new IntervalSet(new double[] {
                    GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min)});
            }
            if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            {
                return new IntervalSet(new double[] {
                    GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min)});
            }
            if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            {
                return new IntervalSet(new double[] {
                    GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    GCDBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min),
                    GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min),
                    GCDBase((int)i1.Intervals[1].Min, (int)i2.Intervals[1].Min)});
            }
            return new IntervalSet(1, posinf);
        }
        public static IntervalSet LCM(IntervalSet i1, IntervalSet i2)
        {
            if ((!i1.Def.Item2) || (!i2.Def.Item2))
                return EmptyIntervalSet;
            if (!(i1.Intervals[0].IsInterger() && i2.Intervals[0].IsInterger()))
            {
                throw new ArgumentException("参数需经过Floor,Ceil函数处理");
            }
            if (i1.Intervals.Length == 1 && i2.Intervals.Length == 1)
            {
                return new IntervalSet(LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min));
            }
            if (i1.Intervals.Length == 2 && i2.Intervals.Length == 1)
            {
                return new IntervalSet(new double[] {
                    LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min)});
            }
            if (i1.Intervals.Length == 1 && i2.Intervals.Length == 2)
            {
                return new IntervalSet(new double[] {
                    LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min)});
            }
            if (i1.Intervals.Length == 2 && i2.Intervals.Length == 2)
            {
                return new IntervalSet(new double[] {
                    LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[0].Min),
                    LCMBase((int)i1.Intervals[0].Min, (int)i2.Intervals[1].Min),
                    LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[0].Min),
                    LCMBase((int)i1.Intervals[1].Min, (int)i2.Intervals[1].Min)});
            }
            return new IntervalSet(1, posinf);
        }
        private static int GCDBase(int a, int b)
        {
            return a == 0 || b == 0 ? 0 : GCDForInt(a, b);
        }
        private static int GCDForInt(int a, int b)
        {
            return b == 0 ? Math.Abs(a) : GCDForInt(b, a % b);
        }
        private static int LCMBase(int a, int b)
        {
            return a == 0 || b == 0 ? 0 : a*b/GCDForInt(a, b);
        }
        private unsafe static IntervalSet AddNumber(IntervalSet i1,double i2)
        {
            int len = i1.Intervals.Length;
            Range[] Ranges = new Range[len];
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
            {
                for (int i=0;i<len;i++)
                {
                    *(Rangesfirst + i) = new Range((first + i)->Min+i2, (first + i)->Max + i2);
                }
            }
            i1.Intervals=Ranges;
            return i1;
        }
        #endregion
        #region 三角函数
        public unsafe static IntervalSet Sin(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = RangeSin(*i);
            return GetIntervalSetFromRangeArray(Ranges,i1.Def,i1.Cont);
        }
        public unsafe static IntervalSet Cos(IntervalSet i1)
        {
            Range[] Ranges=new Range[i1.Intervals.Length];
            fixed(Range* first = i1.Intervals,Rangesfirst=Ranges)
            {
                for (Range* i = first,j=Rangesfirst;i< first + i1.Intervals.Length; i++,j++)
                {
                    *j = new Range() {Min=i->Min+PI/2,Max=i->Max+PI/2 };
                }
            }
            i1.Intervals = Ranges;
            return Sin(i1);
        }
        private static Range RangeSin(Range i)
        {
            double a = i.Min;
            double b = i.Max;
            Range minmax = new Range();
            if (Math.Floor((a / PI - 0.5) / 2) < Math.Floor((b / PI - 0.5) / 2))
            {
                minmax.Max = 1;
                i.Max = 1;
            }
            if (Math.Floor((a / PI + 0.5) / 2) < Math.Floor((b / PI + 0.5) / 2))
            {
                minmax.Min = 1;
                i.Min = -1;
            }
            if (minmax.Min == 0)
            {
                i.Min = Math.Min(Math.Sin(a), Math.Sin(b));
            }
            if (minmax.Max == 0)
            {
                i.Max = Math.Max(Math.Sin(a), Math.Sin(b));
            }
            return i;
        }
        public unsafe static IntervalSet Tan(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[5];
            int loc = 0;
            int len = 5;
            fixed (Range* first = i1.Intervals)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    double l = Math.Floor((i->Max + PI / 2) / PI);
                    double r = Math.Floor((i->Min + PI / 2) / PI);
                    if (l - r == 1)
                    {
                        Ranges[loc++] = new Range() {Min= Math.Tan(i->Min), Max=posinf };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref Ranges, len);
                        }
                        Ranges[loc++] = new Range() { Max=Math.Tan(i->Max), Min=neginf };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref Ranges, len);
                        }
                        continue;
                    }
                    if (l == r)
                    {
                        Ranges[loc++] = new Range() { Min = Math.Tan(i->Min), Max = Math.Tan(i->Max) };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref Ranges, len);
                        }
                        continue;
                    }
                    return new IntervalSet(neginf, posinf) { Def=i1.Def,Cont=false};
                }
            Array.Resize(ref Ranges, loc);
            return GetIntervalSetFromRangeArray(Ranges, i1.Def, false);
        }
        public unsafe static IntervalSet Cot(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            double p = PI / 2;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = new Range() { Max = p - i->Min, Min = p - i->Max };
            i1.Intervals = Ranges;
            return Tan(i1);
        }
        public unsafe static IntervalSet ArcTan(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = new Range() { Min = Math.Atan(i->Min), Max = Math.Atan(i->Max) };
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet ArcCos(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            if (i1.GetMax() < -1 || i1.GetMin() > 1)
                return EmptyIntervalSet;
            if (i1.GetMax() > 1 || i1.GetMin() < -1)
                i1.Def = (false, true);
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Min > 1 || i->Max < -1)
                        continue;
                    *(Rangesfirst + loc++) = new Range() { Min = Math.Acos(Math.Min(i->Max, 1)), Max = Math.Acos(Math.Max(i->Min, -1)) };
                }
            Array.Resize(ref Ranges, loc);
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet ArcSin(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            if (i1.GetMax() < -1 || i1.GetMin() > 1)
                return EmptyIntervalSet;
            if (i1.GetMax() > 1 || i1.GetMin() < -1)
                i1.Def = (false, true);
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Min > 1 || i->Max < -1)
                        continue;
                    *(Rangesfirst + loc++) = new Range() { Min = Math.Asin(Math.Max(i->Min, -1)), Max = Math.Asin(Math.Min(i->Max, 1)) };
                }
            Array.Resize(ref Ranges, loc);
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet Sinh(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                    *(Rangesfirst + loc++) = new Range() { Max = Math.Sinh(i->Max), Min = Math.Sinh(i->Min) };
            i1.Intervals = Ranges;
            return i1;
        }
        public unsafe static IntervalSet Cosh(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            Range[] Ranges = new Range[i1.Intervals.Length];
            int loc = 0;
            fixed (Range* first = i1.Intervals, Rangesfirst = Ranges)
                for (Range* i = first; i < first + i1.Intervals.Length; i++)
                {
                    *(Rangesfirst + loc++) = i->ContainsEqual(0) ? new Range() { Max = Math.Cosh(Math.Max(i->Max, -i->Min)), Min = 1 } : new Range(Math.Cosh(i->Min), Math.Cosh(i->Max));
                }
            return GetIntervalSetFromRangeArray(Ranges,i1.Def,i1.Cont) ;
        }
        #endregion
        #region 比较运算    

        public static (bool,bool) Equal(IntervalSet i1,IntervalSet i2)
        {
            if (i1.Def == (false, false) || i2.Def == (false, false))
            {
                return (false, false);
            }
            if(i1.Intervals.Length*i2.Intervals.Length==0)
                return (false, false);
            foreach (var j1 in Subtract(i1,i2).Intervals)
            {
                if(j1.ContainsEqual(0))
                    return (false, true);
            }
            return (false,false);
        }
        public static (bool,bool) Greater(IntervalSet i1,IntervalSet i2)
        {
            return IntervalMath.Greater(new Interval(i1.GetMin(), i1.GetMax()) { Def = i1.Def, Cont = i1.Cont }, new Interval(i2.GetMin(), i2.GetMax()) { Def = i2.Def, Cont = i2.Cont });
        }
        public static (bool,bool) Less(IntervalSet i1,IntervalSet i2)
        {
            return Greater(i2,i1);
        }
        private static bool RangeEqual(Range i1,Range i2)
        {
            if (double.IsNaN(i1.Min) || double.IsNaN(i2.Min))
                return false;
            return !(i2.Max < i1.Min || i2.Min > i1.Max);
        }
        #endregion
        private static (bool, bool) And((bool, bool) a, (bool, bool) b)
        {
            return (a.Item1 && b.Item1, a.Item2 && b.Item2);
        }
        private static (double,double) GetMinMax4(double n1,double n2,double n3,double n4)
        {
            double minnum = n1;
            double maxnum = n1;
            minnum = minnum < n2 ? minnum : n2;
            maxnum = maxnum > n2 ? maxnum : n2;
            minnum = minnum < n3 ? minnum : n3;
            maxnum = maxnum > n3 ? maxnum : n3;
            minnum = minnum < n4 ? minnum : n4;
            maxnum = maxnum > n4 ? maxnum : n4;
            /*minnum = Math.Min(minnum, n2);
            maxnum = Math.Max(maxnum, n2);
            minnum = Math.Min(minnum, n3);
            maxnum = Math.Max(maxnum, n3);
            minnum = Math.Min(minnum, n4);
            maxnum = Math.Max(maxnum, n4);*/
            return (minnum, maxnum);
        }
        private static double GetMin3(double n1, double n2, double n3)
        {
            double min = (n1 < n2) ? n1 : n2;
            return min < n3 ? min : n3;
        }
        private static double GetMax3(double n1, double n2, double n3)
        {
            double max = (n1 > n2) ? n1 : n2;
            return max > n3 ? max : n3;
        }
        public unsafe static IntervalSet GetIntervalSetFromRangeArray(Range[] Ranges)
        {
            return GetIntervalSetFromRangeArray(Ranges, (true,true),true);
        }
        public unsafe static IntervalSet GetIntervalSetFromRangeArray(Range[] Ranges,(bool,bool) def,bool cont)
        {
            //删除空区间
            Range[] nRanges=new Range[Ranges.Length];
            Array.Copy(Ranges, nRanges, Ranges.Length);
            int count = 0;
            fixed (Range* first = nRanges,Rangesfirst=Ranges)
            {
                for (Range* f=first;f<first+Ranges.Length;f++)
                    if (!(double.IsNaN(f->Min) || double.IsNaN(f->Max)))
                        *(Rangesfirst + count++) = *f;
            }
            if (count == 0)
                return EmptyIntervalSet;
            //对区间排序
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - i - 1; j++)
                {
                    if (Ranges[j].Min > Ranges[j + 1].Min)
                        (Ranges[j], Ranges[j + 1]) = (Ranges[j + 1], Ranges[j]);
                }
            }
            Range latest = Ranges[0];
            Range[] intervals = new Range[count];
            int loc = 0;
            for(int i = 1; i < count; i++)
            {
                Range interval = Ranges[i];
                if (latest.ContainsEqual(interval.Min))
                {
                    if (!latest.ContainsEqual(interval.Max))
                    {
                        latest.Max= interval.Max;
                    }
                }
                else
                {
                    intervals[loc++] = latest;
                    latest = Ranges[i];
                }
            }
            intervals[loc++] = latest;
            Array.Resize(ref intervals, loc);
            return new IntervalSet(intervals,def,cont);
        }
    }
    internal static partial class ExMethods
    {
        public static bool IsInterger(this Range Range)
        {
            if (double.IsInfinity(Range.Min) || double.IsInfinity(Range.Max))
                return false;
            if (double.IsNaN(Range.Min) || double.IsNaN(Range.Max)) 
                return false;
            return (Range.Min == Range.Max)&&Range.Min==(int)Range.Min;
        }
    }
}
