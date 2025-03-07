using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct IntervalSet
    {
        public IntervalBase IB1;
        public IntervalBase IB2;
        public (bool, bool) Def;
        public bool Cont;
        //IB2为可空 空则标识为MinMax均为nan
        //IB1较小
        public IntervalSet(double num){
            IB1 = new IntervalBase(num);
            IB2 = new IntervalBase(double.NaN);
            Def = (true, true);
            Cont = true;
        }
        public double Min()
        {
            return Def.Item2 ? IB1.Min : double.NaN;
        }
    }
}
