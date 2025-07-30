using Microsoft.VisualBasic;
using ScriptCompilerEngine.ScriptNative.ScriptNativeObject;
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
        public static List<string> InputArgAsArrayFunc = new List<string>() {
            "array"
        };
        public static object Val(object obj)
        {
            if (!(obj is string str))
                return double.NaN;
            double result = double.NaN;
            for(int i = 1; i < str.Length; i++)
            {
                if(double.TryParse(str.Substring(0,i),out double num))
                {
                    result = num;
                }
                else
                {
                    return result;
                }
            }
            return result;
        }
        public static object Hex(object num)
        {
            return Convert.ToString(NumberToLong(num), 16);
        }
        public static object Oct(object num)
        {
            return Convert.ToString(NumberToLong(num), 8);
        }
        public static object Bin(object num)
        {
            return Convert.ToString(NumberToLong(num), 2);
        }
        public static object ConvertBase(object num,object basenum)
        {
            if(!(basenum is long bn))
                throw new Exception();
            if (bn<2)
                throw new Exception();
            return Convert.ToString(NumberToLong(num), (int)bn);
        }
        public static object Abs(object var)
        {
            return Math.Abs(NumberToDouble(var));
        }
        public static object Atn(object var)
        {
            return Math.Atan(NumberToDouble(var));
        }
        public static object Atn(object a, object b)
        {
            return Math.Atan2(NumberToDouble(a), NumberToDouble(b));
        }
        public static object Cos(object a)
        {
            return Math.Cos(NumberToDouble(a));
        }
        public static object Sin(object a)
        {
            return Math.Sin(NumberToDouble(a));
        }
        public static object Sgn(object a)
        {
            return Math.Sign(NumberToDouble(a));
        }
        public static object Tan(object a)
        {
            return Math.Tan(NumberToDouble(a));
        }
        public static object Sqr(object a)
        {
            return Math.Sqrt(NumberToDouble(a));
        }
        public static object Exp(object a)
        {
            return Math.Pow(Math.E, NumberToDouble(a));
        }
        public static object Int(object a)
        {
            return Math.Floor(NumberToDouble(a));
        }
        public static object Fix(object a)
        {
            double n = NumberToDouble(a);
            return (n > 0) ? Math.Floor(n) : Math.Ceiling(n);
        }
        public static object Log(object a)
        {
            return Math.Log(NumberToDouble(a));
        }
        public static object Rnd(object a)
        {
            return (double)VBMath.Rnd((float)NumberToDouble(a));
        }
        public static object Rnd()
        {
            return (double)VBMath.Rnd();
        }
        public static object Randomize()
        {
            VBMath.Randomize();
            return Empty.Instance;
        }
        public static object Randomize(object a)
        {
            VBMath.Randomize(NumberToDouble(a));
            return Empty.Instance;
        }
        public static object Array(object[] a)
        {
            return a;
        }
        public static object Join(object obj)
        {
            if(obj is Array a)
            {
                StringBuilder sb = new StringBuilder();
                foreach (object o in a)
                {
                    sb.Append(o.ToString());
                }
                return sb.ToString();
            }
            throw new Exception();
        }
        public static object Split(object text, object spectator)
        {
            string s = ObjectToString(text);
            string[] ss = s.Replace(ObjectToString(spectator), "\0").Split('\0');
            object[] os = new object[ss.Length];
            for (int i = 0; i < ss.Length; i++)
                os[i] = ss[i];
            return os;
        }
        public static object UBound(object arr)
        {
            if (!(arr is Array))
                throw new ArgumentException();
            return (arr as Array).GetLength(0);
        }
        public static object UBound(object arr, object dim)
        {
            if (!(arr is Array))
                throw new ArgumentException();
            return (arr as Array).GetLength(NumberToInt(dim));
        }
    }
}
