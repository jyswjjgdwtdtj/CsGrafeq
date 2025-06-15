using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CsGrafeq.ExMethods;
using static CsGrafeq.Base.Values;

namespace CsGrafeq.Implicit
{
    public struct IntervalSet:IInterval
    {
        internal Range[] Intervals;
        internal (bool, bool) Def;
        internal bool Cont;
        internal bool IsNumber;
        public IntervalSet(double num)
        {
            Intervals = new Range[1] { new Range(num) };
            Def = TT;
            Cont = true;
            IsNumber= true;
        }
        public IntervalSet(double[] nums)
        {
            Array.Sort(nums);
            Intervals= new Range[nums.Length];
            for(int i=0;i<nums.Length;i++)
                Intervals[i]= new Range(nums[i]);
            Def = TT;
            Cont = true;
            IsNumber = false;
        }
        public IntervalSet(double num1,double num2)
        {
            Intervals = new Range[1] { new Range(num1,num2) };
            Def = TT;
            Cont = true;
            IsNumber = num1 == num2;
        }
        public IntervalSet(Range[] Ranges, (bool, bool) def, bool cont)
        {
            Intervals = Ranges;
            Def = def;
            Cont = cont;
            IsNumber=false;
        }
        public IntervalSet(Range[] Ranges)
        {
            Intervals = Ranges;
            Def = TT;
            Cont =true;
            IsNumber = false;
        }
        public double GetMax()
        {
            return Intervals[Intervals.Length-1].Max;
        }
        public double GetMin()
        {
            return Intervals[0].Min;
        }
        public double GetLength()
        {
            return GetMax() - GetMin();
        }
        public bool Contains(double num)
        {
            foreach (Range r in Intervals)
                if (r.Contains(num))
                    return true;
            return false;
        }
        public bool ContainsEqual(double num)
        {
            foreach (Range r in Intervals)
                if (r.ContainsEqual(num))
                    return true;
            return false;
        }
        public override string ToString()
        {
            string result = "";
            foreach(Range r in Intervals)
            {
                result += r.ToString()+",";
            }
            return $"{{Def:{Def},Cont:{Cont},Intervals:[{result}]}}";
        }
        public IInterval SetCont(bool cont)
        {
            Cont = cont;
            return this;
        }
        public IInterval SetDef((bool, bool) def)
        {
            Def = def;
            return this;
        }
        public static IntervalSet operator +(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Add(i1, i2);
        }
        public static IntervalSet operator -(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Subtract(i1, i2);
        }
        public static IntervalSet operator +(IntervalSet i1, double i2)
        {
            return IntervalSetMath.AddNumber(i1, i2);
        }
        public static IntervalSet operator -(IntervalSet i1, double i2)
        {
            return IntervalSetMath.AddNumber(i1, -i2);
        }
        public static IntervalSet operator -(double i2, IntervalSet i1)
        {
            return IntervalSetMath.AddNumber(i1, -i2);
        }
        public static IntervalSet operator +(double i2, IntervalSet i1)
        {
            return IntervalSetMath.AddNumber(i1, i2);
        }
        public static IntervalSet operator *(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Multiply(i1, i2);
        }
        public static IntervalSet operator /(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Divide(i1, i2);
        }
        public static IntervalSet operator %(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Mod(i1, i2);
        }
        public static IntervalSet operator ++(IntervalSet i1)
        {
            return IntervalSetMath.AddNumber(i1, 1);
        }
        public static IntervalSet operator --(IntervalSet i1)
        {
            return IntervalSetMath.AddNumber(i1, -1);
        }
        public static (bool, bool) operator ==(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Equal(i1, i2);
        }
        public static (bool, bool) operator !=(IntervalSet i1, IntervalSet i2)
        {
            throw new NotImplementedException();
        }
        public static (bool, bool) operator <(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Less(i1, i2);
        }
        public static (bool, bool) operator >(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.Greater(i1, i2);
        }
        public static (bool, bool) operator <=(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.LessEqual(i1, i2);
        }
        public static (bool, bool) operator >=(IntervalSet i1, IntervalSet i2)
        {
            return IntervalSetMath.GreaterEqual(i1, i2);
        }
        public static IntervalSet operator -(IntervalSet i1)
        {
            return IntervalSetMath.Neg(i1);
        }
        public static implicit operator IntervalSet(double num)
        {
            return new IntervalSet(num);
        }
        public static implicit operator IntervalSet(Range range)
        {
            return new IntervalSet(range.Min, range.Max);
        }
        public static implicit operator IntervalSet(Interval range)
        {
            return new IntervalSet(range.Min, range.Max) {Def=range.Def,Cont=range.Cont };
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if(obj is IntervalSet iset)
                if (iset.Cont == Cont && (iset.Def == Def) && iset.Intervals.Length == Intervals.Length)
                    for (int i = 0; i < Intervals.Length; i++)
                        if (Intervals[i].Equals(iset.Intervals[i]))
                            return true;
            return false;
        }
    }
}
