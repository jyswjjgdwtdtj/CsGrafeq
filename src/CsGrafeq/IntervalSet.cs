using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct IntervalSet:IInterval
    {
        internal Range[] Intervals;
        internal (bool, bool) Def;
        internal bool Cont;
        internal bool IsNumber;
        //Range2为可空 空则标识为MinMax均为nan
        //Range1小于Range2
        public IntervalSet(double num)
        {
            Intervals = new Range[1] { new Range(num) };
            Def = (true, true);
            Cont = true;
            IsNumber= true;
        }
        public IntervalSet(double[] nums)
        {
            Array.Sort(nums);
            Intervals= new Range[nums.Length];
            for(int i=0;i<nums.Length;i++)
                Intervals[i]= new Range(nums[i]);
            Def = (true, true);
            Cont = true;
            IsNumber = false;
        }
        public IntervalSet(double num1,double num2)
        {
            Intervals = new Range[1] { new Range(num1,num2) };
            Def = (true, true);
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
            Def = (true,true);
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
            return $"{{Def:{Def},Cont:{Cont},Intervals:{result}}}";
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
    }
}
