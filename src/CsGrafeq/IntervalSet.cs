using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct IntervalSet
    {
        public IntervalBase[] Intervals;
        public (bool, bool) Def;
        public bool Cont;
        //IB2为可空 空则标识为MinMax均为nan
        //IB1小于IB2
        public IntervalSet(double num)
        {
            Intervals=new IntervalBase[1]{new IntervalBase(num) };
            Def = (true, true);
            Cont = true;
        }
        public IntervalSet(double num1,double num2)
        {
            Intervals = new IntervalBase[1] { new IntervalBase(num1,num2) };
            Def = (true, true);
            Cont = true;
        }
        public IntervalSet(IntervalBase[] ibs,(bool,bool) def,bool cont)
        {
            Intervals = ibs;
            Def = def;
            Cont = cont;
        }
        public double GetMax()
        {
            return Intervals[Intervals.Length-1].Max;
        }
        public double GetMin()
        {
            return Intervals[0].Max;
        }
    }
}
