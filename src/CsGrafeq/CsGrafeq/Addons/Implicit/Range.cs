using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq.Implicit
{
    public struct Range:IInterval
    {
        public double Min, Max;
        public Range(double num) : this(num, num)
        {
        }
        public Range(double min, double max)
        {
            if (min > max)
                (min, max) = (max, min);
            Min = min;
            Max = max;
        }
        public double GetLength()
        {
            return Max - Min;
        }
        public bool Contains(double num)
        {
            return Min < num && num < Max;
        }
        public bool ContainsEqual(double num)
        {
            return Min <= num && num <= Max;
        }
        public bool Exists()
        {
            return Double.IsNaN(Min);
        }
        public override string ToString()
        {
            return "[" + Min + "," + Max + "]";
        }
        public IInterval SetDef((bool, bool) def)
        {
            throw new NotImplementedException();
        }
        public IInterval SetCont(bool cont)
        {
            throw new NotImplementedException();
        }
        public Interval ToInterval()
        {
            return new Interval(Min, Max);
        }
        public Interval ToInterval((bool,bool) def,bool cont)
        {
            return new Interval(Min, Max) {Def=def,Cont=cont };
        }
        public bool Equals(Range obj)
        {
            return Min==obj.Min && Max==obj.Max;
        }
        public static implicit operator Range ((double,double) tuple)
        {
            return new Range(tuple.Item1,tuple.Item2);
        }
    }
}
