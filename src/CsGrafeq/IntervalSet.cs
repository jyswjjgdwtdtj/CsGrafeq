using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct IntervalSet
    {
        public Range[] Intervals;
        public (bool, bool) Def;
        public bool Cont;
        //Range2为可空 空则标识为MinMax均为nan
        //Range1小于Range2
        public IntervalSet(double num)
        {
            Intervals = new Range[1] { new Range(num) };
            Def = (true, true);
            Cont = true;
        }
        public IntervalSet(double[] nums)
        {
            Array.Sort(nums);
            Intervals= new Range[nums.Length];
            for(int i=0;i<nums.Length;i++)
                Intervals[i]= new Range(nums[i]);
            Def = (true, true);
            Cont = true;
        }
        public IntervalSet(double num1,double num2)
        {
            Intervals = new Range[1] { new Range(num1,num2) };
            Def = (true, true);
            Cont = true;
        }
        public IntervalSet(Range[] Ranges, (bool, bool) def, bool cont)
        {
            Intervals = Ranges;
            Def = def;
            Cont = cont;
        }
        public IntervalSet(Range[] Ranges)
        {
            Intervals = Ranges;
            Def = (true,true);
            Cont =true;
        }
        public double GetMax()
        {
            return Intervals[Intervals.Length-1].Max;
        }
        public double GetMin()
        {
            return Intervals[0].Max;
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
    }
}
