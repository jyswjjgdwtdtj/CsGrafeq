using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq
{
    public static class IntervalSetMath
    {
        private static IntervalSet EmptyIntervalSet=new IntervalSet(double.NaN) { Def = (false, false) };
        private static IntervalBase EmptyIntervalBase = new IntervalBase(double.NaN);
        private static readonly double neginf = double.NegativeInfinity;
        private static readonly double posinf = double.PositiveInfinity;
        private const double PI=Math.PI;
        private const double E=Math.E;

        public unsafe static IntervalSet Add(IntervalSet i1,IntervalSet i2)
        {
            if(!(i1.Def.Item2&&i2.Def.Item2))
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* ptr=ibs,i1start=i1.Intervals,i2start=i2.Intervals)
            {
                for (IntervalBase* i = i1start; i < i1start+i1.Intervals.Length; i++)
                    for (IntervalBase* j = i2start; j < i2start + i2.Intervals.Length; j++,loc++)
                        *(ptr+loc) = IBAdd(i, j);
            }
            return GetIntervalSetFromIntervalBaseArray(ibs, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private unsafe static IntervalBase IBAdd(IntervalBase* i1,IntervalBase* i2)
        {
            return new IntervalBase() { Min=i1->Min+i2->Min,Max=i1->Max+i2->Max};
        }
        public unsafe static IntervalSet Neg(IntervalSet i)
        {
            int len = i.Intervals.Length;
            IntervalBase[] ibs = new IntervalBase[i.Intervals.Length];
            for(int j = 0; j < i.Intervals.Length; j++)
            {
                ibs[j]=IBNeg(i.Intervals[len-1-j]);
            }
            i.Intervals=ibs;
            return i;
        }
        private static IntervalBase IBNeg(IntervalBase i)
        {
            return new IntervalBase() { Max = -i.Min, Min = -i.Max };
        }
        private unsafe static void IBSwapNeg(IntervalBase* i1,IntervalBase* i2)
        {
            IntervalBase tmp = *i1;
            if (i1 == i2)
            {
                i1->Max = -tmp.Min;
                i1->Min = -tmp.Max;
                return;
            }
            i1->Min = -i2->Max;
            i1->Max = -i2->Min;
            i2->Min=-tmp.Max;
            i2->Max=-tmp.Min;
        }
        public static IntervalSet Subtract(IntervalSet i1,IntervalSet i2)
        {
            return Add(i1,Neg(i2));
        }
        public unsafe static IntervalSet Multiply(IntervalSet i1,IntervalSet i2)
        {
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length * i2.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* ptr = ibs, i1start = i1.Intervals, i2start = i2.Intervals)
            {
                for (IntervalBase* i = i1start; i < i1start + i1.Intervals.Length; i++)
                    for (IntervalBase* j = i2start; j < i2start + i2.Intervals.Length; j++)
                        *(ptr + loc++) = IBMultiply(*i, *j);
            }
            return GetIntervalSetFromIntervalBaseArray(ibs, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
        }
        private static IntervalBase IBMultiply(IntervalBase i1,IntervalBase i2)
        {
            if (double.IsNaN(i1.Min) || double.IsNaN(i2.Min))
                return EmptyIntervalBase;
            if (i1.Min > 0 && i2.Min > 0)
            {
                i1.Min*=i2.Min;
                i1.Max*=i2.Max;
                return i1;
            }
            if (i1.Max < 0 && i2.Max < 0)
            {
                return new IntervalBase(i1.Max*i2.Max,i1.Min*i2.Min);
            }
            (i1.Min,i1.Max) = GetMinMax4(i1.Min*i2.Min,i1.Min*i2.Max,i1.Max*i2.Min,i1.Max*i2.Max);
            return i1;
        }
        public static IntervalSet Divide(IntervalSet i1, IntervalSet i2)
        {
            //a/b=>a*(1/b)
            if (!(i1.Def.Item2 && i2.Def.Item2))
                return EmptyIntervalSet;
            //1/i2
            IntervalBase[] ibs = new IntervalBase[5];
            int loc = 0;
            int len = 5;
            IntervalBase ib1, ib2;
            //for(int ii = 0; ii < i1.Intervals.Length; ii++)
            //{
                //i1.Intervals[ii].Max = i1.Intervals[ii].Max == 0 ? -double.Epsilon : i1.Intervals[ii].Max;
                //i1.Intervals[ii].Min = i1.Intervals[ii].Min == 0 ? double.Epsilon : i1.Intervals[ii].Min;
            //}
            foreach (IntervalBase i in i2.Intervals)
            {
                if (i.ContainsEqual(0))
                {
                    ib1 = new IntervalBase() { Min = neginf, Max = 1 / i.Min };
                    ib2 = new IntervalBase() { Min = 1 / i.Max, Max = posinf };
                    foreach (IntervalBase j in i1.Intervals)
                    {
                        ibs[loc++] = IBMultiply(ib1, j);
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref ibs, len);
                        }
                        ibs[loc++] = IBMultiply(ib2, j);
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref ibs, len);
                        }
                    }
                    continue;
                }
                ib1 = new IntervalBase(1 / i.Min, 1 / i.Max);
                foreach (IntervalBase j in i1.Intervals)
                {
                    ibs[loc++] = IBMultiply(ib1, j);
                    if (loc == len)
                    {
                        len *= 2;
                        Array.Resize(ref ibs, len);
                    }
                }
            }
            Array.Resize(ref ibs, loc);
            IntervalSet iss = GetIntervalSetFromIntervalBaseArray(ibs, And(i1.Def, i2.Def), i1.Cont && i2.Cont);
            return iss;
        }
        public unsafe static IntervalSet Sgn(IntervalSet i)
        {
            if (!i.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[3];
            int loc = 0;
            fixed(IntervalBase* first = i.Intervals)
            {
                if (first->Min < 0)
                    ibs[loc++] = new IntervalBase(-1);
                for (IntervalBase* f = first; f < first + i.Intervals.Length; f++)
                {
                    if (f->Min <= 0 && f->Max >= 0)
                    {
                        ibs[loc++] = new IntervalBase(0);
                        break;
                    }
                }
                if ((first + i.Intervals.Length - 1)->Max > 0)
                    ibs[loc++] = new IntervalBase(1);
                while (loc < 3)
                    ibs[loc++] = EmptyIntervalBase;
            }
            i.Intervals = ibs;
            return i;
        }
        public unsafe static IntervalSet Median(IntervalSet c1, IntervalSet c2, IntervalSet c3)
        {
            if (!(c1.Def.Item2&&c2.Def.Item2&&c3.Def.Item2))
                return EmptyIntervalSet;
            IntervalBase[] ibs=new IntervalBase[5];
            int len = 5;
            int loc = 0;
            int c1len=c1.Intervals.Length;
            int c2len=c2.Intervals.Length;
            int c3len=c3.Intervals.Length;
            fixed(IntervalBase* c1first = c1.Intervals, c2first = c2.Intervals, c3first = c3.Intervals)
            {
                for(IntervalBase* j1 = c1first; j1 < c1first + c1len; j1++)
                {
                    for (IntervalBase* j2 = c2first; j2 < c2first + c2len; j2++)
                    {
                        for (IntervalBase* j3 = c3first; j3 < c3first + c3len; j3++)
                        {
                            ibs[loc++] = IBMedian(j1,j2,j3);
                            if (loc == len)
                            {
                                len *= 2;
                                Array.Resize(ref ibs, len);
                            }
                        }
                    }
                }
            }
            Array.Resize(ref ibs, loc);
            return GetIntervalSetFromIntervalBaseArray(ibs,And(And(c1.Def,c2.Def),c3.Def),c1.Cont&&c2.Cont&&c3.Cont);
        }
        public unsafe static IntervalSet Exp(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals,ibsfirst=ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                    *(ibsfirst + loc) = new IntervalBase() { Min = Math.Exp(i->Min), Max = Math.Exp(i->Max) };
            i1.Intervals = ibs;
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
            IntervalBase[] ibs = new IntervalBase[len];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
            {
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(ibsfirst + loc++) = new IntervalBase(neginf, Math.Log(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(ibsfirst + loc++) = new IntervalBase(Math.Log(i->Min), Math.Log(i->Max));
                }
            }
            i1.Intervals = ibs;
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
            IntervalBase[] ibs = new IntervalBase[len];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
            {
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(ibsfirst + loc++) = new IntervalBase(neginf, Math.Log10(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(ibsfirst + loc++) = new IntervalBase(Math.Log10(i->Min), Math.Log10(i->Max));
                }
            }
            i1.Intervals = ibs;
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
            IntervalBase[] ibs = new IntervalBase[len];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
            {
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Max <= 0)
                        continue;
                    if (i->Min <= 0)
                    {
                        *(ibsfirst + loc++) = new IntervalBase(0, Math.Sqrt(i->Max));
                        i1.Def.Item1 = false;
                    }
                    else
                        *(ibsfirst + loc++) = new IntervalBase(Math.Sqrt(i->Min), Math.Sqrt(i->Max));
                }
            }
            i1.Intervals = ibs;
            return i1;
        }
        private unsafe static IntervalBase IBMedian(IntervalBase* i1, IntervalBase* i2, IntervalBase* i3)
        {
            IntervalBase i = new IntervalBase();
            double[] arr = new double[3] { i1->Min, i2->Min, i3->Min };
            Array.Sort<double>(arr);
            i.Min = arr[1];
            arr = new double[3] { i1->Max, i2->Max, i3->Max };
            Array.Sort<double>(arr);
            i.Max = arr[1];
            return i;
        }
        public unsafe static IntervalSet Sin(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                    *(ibsfirst + loc++) = IBSin(*i);
            return GetIntervalSetFromIntervalBaseArray(ibs,i1.Def,i1.Cont);
        }
        public unsafe static IntervalSet Cos(IntervalSet i1)
        {
            IntervalBase[] ibs=new IntervalBase[i1.Intervals.Length];
            fixed(IntervalBase* first = i1.Intervals,ibsfirst=ibs)
            {
                for (IntervalBase* i = first,j=ibsfirst;i< first + i1.Intervals.Length; i++,j++)
                {
                    *j = new IntervalBase() {Min=i->Min+PI/2,Max=i->Max+PI/2 };
                }
            }
            i1.Intervals = ibs;
            return Sin(i1);
        }
        private static IntervalBase IBSin(IntervalBase i)
        {
            double a = i.Min;
            double b = i.Max;
            IntervalBase minmax = new IntervalBase();
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
            IntervalBase[] ibs = new IntervalBase[5];
            int loc = 0;
            int len = 5;
            fixed (IntervalBase* first = i1.Intervals)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    double l = Math.Floor((i->Max + PI / 2) / PI);
                    double r = Math.Floor((i->Min + PI / 2) / PI);
                    if (l - r == 1)
                    {
                        ibs[loc++] = new IntervalBase() {Min= Math.Tan(i->Min), Max=posinf };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref ibs, len);
                        }
                        ibs[loc++] = new IntervalBase() { Max=Math.Tan(i->Max), Min=neginf };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref ibs, len);
                        }
                        continue;
                    }
                    if (l == r)
                    {
                        ibs[loc++] = new IntervalBase() { Min = Math.Tan(i->Min), Max = Math.Tan(i->Max) };
                        if (loc == len)
                        {
                            len *= 2;
                            Array.Resize(ref ibs, len);
                        }
                        continue;
                    }
                    return new IntervalSet(neginf, posinf) { Def=i1.Def,Cont=false};
                }
            Array.Resize(ref ibs, loc);
            return GetIntervalSetFromIntervalBaseArray(ibs, i1.Def, false);
        }
        public unsafe static IntervalSet Cot(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            double p = PI / 2;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                    *(ibsfirst + loc++) = new IntervalBase() { Max = p - i->Min, Min = p - i->Max };
            i1.Intervals = ibs;
            return Tan(i1);
        }
        public unsafe static IntervalSet ArcTan(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                    *(ibsfirst + loc++) = new IntervalBase() { Min = Math.Atan(i->Min), Max = Math.Atan(i->Max) };
            i1.Intervals = ibs;
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
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Min > 1 || i->Max < -1)
                        continue;
                    *(ibsfirst + loc++) = new IntervalBase() { Min = Math.Acos(Math.Min(i->Max, 1)), Max = Math.Acos(Math.Max(i->Min, -1)) };
                }
            Array.Resize(ref ibs, loc);
            i1.Intervals = ibs;
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
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    if (i->Min > 1 || i->Max < -1)
                        continue;
                    *(ibsfirst + loc++) = new IntervalBase() { Min = Math.Asin(Math.Max(i->Min, -1)), Max = Math.Asin(Math.Min(i->Max, 1)) };
                }
            Array.Resize(ref ibs, loc);
            i1.Intervals = ibs;
            return i1;
        }
        public unsafe static IntervalSet Sinh(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                    *(ibsfirst + loc++) = new IntervalBase() { Max = Math.Sinh(i->Max), Min = Math.Sinh(i->Min) };
            i1.Intervals = ibs;
            return i1;
        }
        public unsafe static IntervalSet Cosh(IntervalSet i1)
        {
            if (!i1.Def.Item2)
                return EmptyIntervalSet;
            IntervalBase[] ibs = new IntervalBase[i1.Intervals.Length];
            int loc = 0;
            fixed (IntervalBase* first = i1.Intervals, ibsfirst = ibs)
                for (IntervalBase* i = first; i < first + i1.Intervals.Length; i++)
                {
                    *(ibsfirst + loc++) = i->ContainsEqual(0) ? new IntervalBase() { Max = Math.Cosh(Math.Max(i->Max, -i->Min)), Min = 1 } : new IntervalBase(Math.Cosh(i->Min), Math.Cosh(i->Max));
                }
            return GetIntervalSetFromIntervalBaseArray(ibs,i1.Def,i1.Cont) ;
        }

        public static (bool,bool) Equal(IntervalSet i1,IntervalSet i2)
        {
            if (i1.Def == (false, false) || i2.Def == (false, false))
            {
                return (false, false);
            }
            if(i1.Intervals.Length*i2.Intervals.Length==0)
                return (false, false);
            foreach (var j1 in i1.Intervals)
            {
                foreach (var j2 in i2.Intervals)
                {
                    if(IBEqual(j1,j2))
                        return (false, true);
                }
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
        private static bool IBEqual(IntervalBase i1,IntervalBase i2)
        {
            if (double.IsNaN(i1.Min) || double.IsNaN(i2.Min))
                return false;
            return !(i2.Max < i1.Min || i2.Min > i1.Max);
        }
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
            //若出现NaN 则应当使用以下代码
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
        public unsafe static IntervalSet GetIntervalSetFromIntervalBaseArray(IntervalBase[] ibs,(bool,bool) def,bool cont)
        {
            //删除空区间
            IntervalBase[] nibs=new IntervalBase[ibs.Length];
            Array.Copy(ibs, nibs, ibs.Length);
            int count = 0;
            fixed (IntervalBase* first = nibs,ibsfirst=ibs)
            {
                for (IntervalBase* f=first;f<first+ibs.Length;f++)
                    if (!(double.IsNaN(f->Min) || double.IsNaN(f->Max)))
                        *(ibsfirst + count++) = *f;
            }
            if (count == 0)
                return EmptyIntervalSet;
            //对区间排序
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = 0; j < count - i - 1; j++)
                {
                    if (ibs[j].Min > ibs[j + 1].Min)
                        (ibs[j], ibs[j + 1]) = (ibs[j + 1], ibs[j]);
                }
            }
            IntervalBase latest = ibs[0];
            IntervalBase[] intervals = new IntervalBase[count];
            int loc = 0;
            for(int i = 1; i < count; i++)
            {
                IntervalBase interval = ibs[i];
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
                    latest = ibs[i];
                }
            }
            intervals[loc++] = latest;
            Array.Resize(ref intervals, loc);
            return new IntervalSet(intervals,def,cont);
        }
    }
}
