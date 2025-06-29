
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static System.Double;
using System.Windows.Forms;
using static CsGrafeq.ExMethods;

namespace CsGrafeq
{
    [Obsolete("Never use methods for interval", true)]
    public static class IntervalMath
    {
        static IntervalMath() { }
		private static Interval EmptyInterval=new Interval(NaN) { Def=FF,Cont=false};
        public static Interval New(double num)
        {
            return new Interval(num);
        }
        #region 四则运算
        public static Interval Add(Interval a, Interval b)
        {
            return (a.isEmpty()||b.isEmpty())?EmptyInterval:new Interval(a.Min + b.Min, a.Max + b.Max) { Def=And(a.Def,b.Def),Cont=a.Cont&&b.Cont};
        }
        public static Interval Subtract(Interval a,Interval b)
        {
            return Add(a, Neg(b));
        }
        public static Interval Multiply(Interval i1,Interval i2)
        {
            if(i1.isEmpty()||i2.isEmpty())
                return EmptyInterval;
            if (i1.Min > 0 && i2.Min > 0)
            {
                i1.Min *= i2.Min;
                i1.Max *= i2.Max;
                return i1;
            }
            if (i1.Max < 0 && i2.Max < 0)
            {
                return new Interval(i1.Max * i2.Max, i1.Min * i2.Min) { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
            }
            (double,double) ds =GetMinMax4(i1.Min * i2.Min, i1.Min * i2.Max, i1.Max * i2.Min, i1.Max * i2.Max);
            return new Interval(ds.Item1, ds.Item2) { Def = And(i1.Def, i2.Def), Cont = i1.Cont && i2.Cont };
        }
        public static Interval Divide(Interval m,Interval i)
        {
            //1/i1
            //a/b=a*(1/b)=a*(b=>1/x)
            if (i.isEmpty()||m.isEmpty())
            {
                return EmptyInterval;
            }
            if (i.ContainsEqual(0))
            {
                return new Interval(NegativeInfinity, PositiveInfinity) { Def = And(i.Def, m.Def), Cont = i.Cont && m.Cont };
            }
            else
            {
                Interval i1=new Interval(1/i.Min, 1/i.Max);
                return Multiply(m, i1);
            }
        }
        public static Interval Mod(Interval a, Interval b)
        {
            if (b.Min == b.Max)
            {
                double num=b.Min;
                double min =Math.Floor(a.Min / num);
                double max=Math.Floor(a.Max / num);
                if (min==max)
                    return new Interval(NumMod(b.Min, num), NumMod(b.Max, num)) { Def=And(a.Def,b.Def)};
                return new Interval(0, b.Max) { Def = FT };
            }
            return Subtract(a,Multiply(Floor(Divide(a, b)), b)).SetDef(FT);
        }
        private static double NumMod(double a,double b)
        {
            return a - Math.Floor(a / b) * b;
        }
        public static Interval Neg(Interval a)
        {
            return new Interval(-a.Max, -a.Min) { Def = a.Def, Cont = a.Cont };
        }
        #endregion
        #region 比较运算
        public static (bool, bool) Equal(Interval i1, Interval i2)
        {
            (bool,bool) def=And(i1.Def, i2.Def);
            if (!def.Item2)
                return def;
            if (i2.Max < i1.Min || i2.Min > i1.Max)
                return FF;
            //if(i2.Max==i1.Max&&i2.Min==i1.Min)
            //    return def;
            return (false,def.Item2);
        }
        public static (bool, bool) Greater(Interval i1, Interval i2)
        {
            (bool, bool) def = And(i1.Def, i2.Def);
            if (i1.isEmpty() || i2.isEmpty())
                return FF;
            if (i1.Max < i2.Min)
                return FF;
            if (i1.Min > i2.Max)
            {
                if ((!i1.Def.Item1) || (!i2.Def.Item1))
                {
                    return FT;
                }
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
            i.Cont &= (i.Min == i.Max);
            return i;
        }
        public static Interval Abs(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            if (i.ContainsEqual(0))
            {
                i.Max=Math.Max(-i.Min, i.Max);
                i.Min = 0;
                return i;
            }
            if (i.Max < 0)
            {
                (i.Min, i.Max) = (-i.Max,-i.Min);
                return i;
            }
            return i;
        }
        public static Interval Median(Interval i1, Interval i2, Interval i3)
        {
            if(i1.isEmpty()||i2.isEmpty()||i3.isEmpty())
                return EmptyInterval;
            Interval i = new Interval() {Def=And(And(i1.Def,i2.Def),i3.Def),Cont=i1.Cont&&i2.Cont&&i3.Cont };
            double[] arr = new double[3] { i1.Min, i2.Min, i3.Min };
            Array.Sort<double>(arr);
            i.Min = arr[1];
            arr = new double[3] { i1.Max, i2.Max, i3.Max };
            Array.Sort<double>(arr);
            i.Max = arr[1];
            return i;
        }
        public static Interval Exp(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            i.Min = Math.Pow(E, i.Min);
            i.Max= Math.Pow(E, i.Max);
            return i;
        }
        public static Interval Ln(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            if(i.Max<=0)
                return EmptyInterval;
            if (i.Min > 0)
            {
                i.Min=Math.Log(i.Min);
                i.Max=Math.Log(i.Max);
                return i;
            }
            i.Min = Double.NegativeInfinity;
            i.Max = Math.Log(i.Max);
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
            i.Min = Double.NegativeInfinity;
            i.Max = Log10(i.Max);
            i.Def = FT;
            return i;
        }
        public static Interval Log(Interval i1,Interval i2)
        {
            return Divide(Ln(i1) , Ln(i2));
        }
        public static Interval Pow(Interval i1,Interval i2)
        {
            return Exp(Multiply(Ln(i1),i2));
        }
        public static Interval Sqrt(Interval i)
        {
            if (i.isEmpty())
                return i;
            if (i.Max < 0)
                return EmptyInterval;
            if (i.Min >= 0)
            {
                i.Min=Math.Sqrt(i.Min);
                i.Max=Math.Sqrt(i.Max);
                return i;
            }
            i.Min = 0;
            i.Max = Math.Sqrt(i.Max);
            i.Def = FT;
            return i;
        }
        public static Interval Root(Interval i, Interval i2)
        {
            if (i2.Min != i2.Max)
            {
                return EmptyInterval;
            }
            if (i.isEmpty())
            {
                return i;
            }
            double num = i2.Max;
            if ((int)(num) == num && (int)(num / 2) != num / 2)
            {
                return new Interval(Math.Pow(i.Min, 1 / num), Math.Pow(i.Max, 1 / num));
            }
            else
            {
                if (i.Max < 0)
                {
                    return EmptyInterval;
                }
                else if (i.Contains(0))
                {
                    i = new Interval(0, Math.Pow(i.Max, 1 / num));
                    i.Def = FT;
                    return i;
                }
                else
                {
                    return new Interval(Math.Pow(i.Min, 1 / num), Math.Pow(i.Max, 1 / num));
                }
            }
        }
        public static Interval Min(Interval i1, Interval i2)
        {
            return new Interval(Math.Min(i1.Min, i2.Min), Math.Min(i1.Max, i2.Max)) { Def = And(i1.Def, i2.Def), Cont = i1.Cont & i2.Cont };
        }
        public static Interval Max(Interval i1, Interval i2)
        {
            return new Interval(Math.Max(i1.Min, i2.Min), Math.Max(i1.Max, i2.Max)) { Def = And(i1.Def, i2.Def), Cont = i1.Cont & i2.Cont };
        }
        public static Interval Floor(Interval i1)
        {
            i1.Min= Math.Floor(i1.Min);
            i1.Max= Math.Floor(i1.Max);
            i1.Cont &= i1.Min == i1.Max;
            return i1;
        }
        public static Interval Ceil(Interval i1)
        {
            i1.Min = Math.Ceiling(i1.Min);
            i1.Max = Math.Ceiling(i1.Max);
            i1.Cont &=i1.Min==i1.Max;
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
            return b == 0 ? Math.Abs(a) : GCDForInt(b, a % b);
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
            return new Interval(FactorialBase((int)i1.Min)) { Def=i1.Def, Cont = i1.Cont };
        }
        private static double FactorialBase(int num)
        {
            if (num >= FactorialValue.Values.Length)
                return double.MaxValue;
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
            if (i.isEmpty())
            {
                return i;
            }
            double a = i.Min;
            double b = i.Max;
            Interval minmax = new Interval();
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
        public static Interval Cos(Interval i)
        {
            i.Min += PI / 2;
            i.Max += PI / 2;
            return Sin(i);
        }
        public static Interval Tan(Interval i)
        {
            if (i.isEmpty())
            {
                return EmptyInterval;
            }
            double l = Math.Floor((i.Max + PI / 2) / PI);
            double r = Math.Floor((i.Min + PI / 2) / PI);
            if (l - r == 1)
            {
                return new Interval(NegativeInfinity, PositiveInfinity) { Def=i.Def,Cont=false};
            }
            else if (l - r == 0)
            {
                i.Min=Math.Tan(i.Min);
                i.Max=Math.Tan(i.Max);
                return i;
            }
            else
            {
                return new Interval(NegativeInfinity,PositiveInfinity) { Def = i.Def, Cont = false };
            }
        }
        public static Interval Cot(Interval i)
        {
            return Tan(Subtract(new Interval(PI/2),i));
        }
        public static Interval ArcTan(Interval i)
        {
            if(i.isEmpty())
                return EmptyInterval;
            i.Min = Math.Atan(i.Min);
            i.Max = Math.Atan(i.Max);
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
            i.Min = Math.Asin(Math.Max(i.Min, -1));
            i.Min = Math.Asin(Math.Min(i.Max, 1));
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
            double max = i.Max;
            i.Max = Math.Acos(Math.Max(i.Min, -1));
            i.Min = Math.Acos(Math.Min(max, 1));
            return i;
        }
        public static Interval Sinh(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            i.Min = Math.Sinh(i.Min);
            i.Min = Math.Sinh(i.Max);
            return i;
        }
        public static Interval Cosh(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            if (i.ContainsEqual(0))
            {
                double max = Math.Max(i.Max,-i.Min);
                i.Min = 1;
                i.Max=Math.Cosh(max);
                return i;
            }
            return new Interval(Math.Cosh(i.Min), Math.Cosh(i.Max)) { Def = i.Def, Cont = i.Cont };
        }
        public static Interval Tanh(Interval i)
        {
            if (i.isEmpty())
                return EmptyInterval;
            i.Min = Math.Tanh(i.Min);
            i.Min = Math.Tanh(i.Max);
            return i;
        }
        #endregion
        #region 其他计算
        private static (bool,bool) And((bool,bool) a,(bool,bool) b)
        {
            return (a.Item1 && b.Item1, a.Item2 && b.Item2);
        }
        private static (double, double) GetMinMax4(double n1, double n2, double n3, double n4)
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
        #endregion
    }
    internal static partial class ExMethods
    {
        public static bool IsInterger(this Interval Range)
        {
            if (double.IsInfinity(Range.Min) || double.IsInfinity(Range.Max))
                return false;
            if (double.IsNaN(Range.Min) || double.IsNaN(Range.Max))
                return false;
            return (Range.Min == Range.Max) && Range.Min == (int)Range.Min;
        }
    }
}
