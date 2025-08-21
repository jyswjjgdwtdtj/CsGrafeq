using DynamicData.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CsGrafeq.Interval
{
    public struct Def
    {
        public bool First { get; private set; }
        public bool Second {  get; private set; }
        public Def() {
            First = true;
            Second = true;
        }
        public static Def TT { get; } = new Def() { First = true, Second = true };
        public static Def FT { get; } = new Def() { First = false, Second = true };
        public static Def FF { get; } = new Def() { First = false, Second = false };
        public void Deconstruct(out bool first, out bool second)
        {
            first = First;
            second = Second;
        }
        public static bool operator==(Def lhs, Def rhs)
        {
            if(lhs.First==rhs.First&&lhs.Second==rhs.Second)
                return true;
            return false;
        }
        public static bool operator!=(Def lhs, Def rhs)
        {
            return !(lhs == rhs);
        }
        public static Def And(Def lhs, Def rhs)
        {
            return new Def() {First=lhs.First&&rhs.First,Second=lhs.Second&&rhs.Second};
        }
        public static Def Or(Def lhs, Def rhs)
        {
            return new Def() { First = lhs.First || rhs.First, Second = lhs.Second || rhs.Second };
        }
        public static Def operator &(Def lhs, Def rhs) => And(lhs, rhs);
        public static Def operator |(Def lhs, Def rhs) => Or(lhs, rhs);

    }
}
