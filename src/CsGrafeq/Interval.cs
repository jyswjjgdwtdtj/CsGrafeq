using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public struct Interval:IInterval
    {
        public double Min, Max;
        public Interval(double num) : this(num, num)
        {
        }
        public Interval(double min, double max)
        {
            if (min > max)
                (min, max) = (max, min);
            Min = min;
            Max = max;
            Def = (true, true);
            Cont=true;
        }
        public double GetLength()
        {
           return Max - Min;
        }
        public (bool, bool) Def;
        //指定义域不完整 即定义域的所有值不能一一对应到函数值
        public bool Cont;
        public bool Contains(double num)
        {
            return Min < num && num < Max;
        }
        public bool ContainsEqual(double num)
        {
            return Min <= num && num <= Max;
        }
        public override string ToString()
        {
            return "[" + Min + "," + Max + "]";
        }
        public bool isEmpty()
        {
            return Def.Item2 == false;
        }
        public bool isPartial()
        {
            return Def == (false, true);
        }
        public bool isNumber()
        {
            return Min == Max;
        }
        IInterval IInterval.SetDef((bool, bool) def)
        {
            Def = def;
            return this;
        }
        IInterval IInterval.SetCont(bool cont)
        {
            Cont = cont;
            return this;
        }
        public Interval SetCont(bool cont)
        {
            Cont = cont;
            return this;
        }
        public Interval SetDef((bool, bool) def)
        {
            Def = def;
            return this;
        }
        public Range ToRange()
        {
            return new Range(Min, Max);
        }

    }
}
