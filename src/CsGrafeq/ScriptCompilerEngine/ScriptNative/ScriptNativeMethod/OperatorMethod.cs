using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ScriptCompilerEngine.ScriptNative.InternalMethod.Method;

namespace ScriptCompilerEngine.ScriptNative.ScriptNativeMethod
{
    public static partial class ScriptNativeMethod
    {
        public static object Add(object a, object b)
        {
            if(a is string || b is string)
            {
                return ObjectToString(a)+ObjectToString(b);
            }
            if(a is double||b is double)
            {
                return NumberToDouble(a)+NumberToDouble(b);
            }
            return NumberToLong(a)+NumberToLong(b);
        }
        public static object Subtract(object a, object b)
        {
            if (a is double || b is double)
            {
                return NumberToDouble(a) - NumberToDouble(b);
            }
            return NumberToLong(a) - NumberToLong(b);
        }
        public static object Multiply(object a, object b)
        {
            if(a is string sa&&b is long lb)
            {
                if (lb < 0)
                    throw new Exception();
                return new StringBuilder(sa.Length * 3).Insert(0, sa, (int)lb).ToString();
            }
            if (a is double || b is double)
            {
                return NumberToDouble(a) * NumberToDouble(b);
            }
            return NumberToLong(a) * NumberToLong(b);
        }
        public static object Divide(object a, object b)
        {
            if (a is double || b is double)
            {
                return NumberToDouble(a) / NumberToDouble(b);
            }
            return NumberToLong(a) / NumberToLong(b);
        }
        public static object Mod(object a, object b)
        {
            return NumberToDouble(a)%NumberToDouble(b);
        }
        public static object Pow(object a, object b)
        {
            return Math.Pow(NumberToDouble(a) , NumberToDouble(b));
        }
        public static object Connect(object a, object b)
        {
            return ObjectToString(a) + ObjectToString(b);
        }
        public static object Not(object a)
        {
            if (a is bool)
            {
                return !(bool)a;
            }
            return ~NumberToLong(a);
        }
        public static object Neg(object a)
        {
            if(a is double)
                return -NumberToDouble(a);
            return -NumberToLong(a);
        }
        public static object And(object a, object b)
        {
            if (a is bool || b is bool)
                return ObjectToBool(a) && ObjectToBool(b);
            return NumberToLong(a) &NumberToLong(b);
        }
        public static object Or(object a, object b)
        {
            if (a is bool || b is bool)
                return ObjectToBool(a) || ObjectToBool(b);
            return NumberToLong(a) | NumberToLong(b);
        }
        public static object Xor(object a, object b)
        {
            if (a is bool || b is bool)
                return ObjectToBool(a) ^ ObjectToBool(b);
            return NumberToLong(a) ^ NumberToLong(b);
        }
        public static object Equal(object a, object b)
        {
            if ((a is long||a is double)&&(b is long||b is double))
            {
                return NumberToDouble(a) == NumberToLong(b);
            }
            if (a == null && b == null)
                return true;
            if(TypeName(a)!=TypeName(b))
                return false;
            return ((ValueType)a).Equals(b);
        }
        public static object NotEqual(object a, object b)
        {
            return !(bool)Equal(a, b);
        }
        public static object LessThan(object a, object b)
        {
            if ((a is long || a is double) && (b is long || b is double))
                return NumberToDouble(a) < NumberToDouble(b);
            if (a is string || b is string)
                return String.Compare(ObjectToString(a), ObjectToString(b)) == -1;
            throw new Exception();
        }
        public static object GreaterThan(object a, object b)
        {
            if ((a is long || a is double) && (b is long || b is double))
                return NumberToDouble(a) > NumberToDouble(b);
            if (a is string || b is string)
                return String.Compare(ObjectToString(a), ObjectToString(b)) == 1;
            throw new Exception();
        }
        public static object LessEqual(object a, object b)
        {
            if ((a is long || a is double) && (b is long || b is double))
                return NumberToDouble(a) <= NumberToDouble(b);
            if (a is string || b is string)
                return String.Compare(ObjectToString(a), ObjectToString(b)) !=1;
            throw new Exception();
        }
        public static object GreaterEqual(object a, object b)
        {
            if ((a is long || a is double) && (b is long || b is double))
                return NumberToDouble(a) >= NumberToDouble(b);
            if (a is string || b is string)
                return String.Compare(ObjectToString(a), ObjectToString(b)) != -1;
            throw new Exception();
        }
    }
}
