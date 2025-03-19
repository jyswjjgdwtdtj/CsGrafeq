using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct IntervalBase:IInterval
    {
        public double Min, Max;
        public IntervalBase(double num) : this(num, num)
        {
        }
        public IntervalBase(double min, double max)
        {
            if (min > max)
                (min, max) = (max, min);
            Min = min;
            Max = max;
        }
        public double Length
        {
            get => Max - Min;
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
    }
}
