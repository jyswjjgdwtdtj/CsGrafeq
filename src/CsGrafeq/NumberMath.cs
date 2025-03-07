using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CsGrafeq
{
    public static class NumberMath
    {
        internal static Dictionary<string, OpCode> ToOpcodesDic = new Dictionary<string, OpCode>()
        {
            { "add",OpCodes.Add },
            { "subtract",OpCodes.Sub },
            { "multiply",OpCodes.Mul },
            { "divide",OpCodes.Div },
            {"mod",OpCodes.Rem },
        };
        internal static Dictionary<string, string> ToMathDic = new Dictionary<string, string>()
        {
            { "sin","Sin" },
            { "tan","Tan" },
            { "cos","Cos" },
            { "pow","Pow" },
            { "exp","Exp" },
            { "ln","log"  },
            { "lg","log10"},
            { "log","log" },
            {"sqrt","Sqrt"},
            {"arctan","Atan"},
            {"arcsin","Asin"},
            {"arccos","Acos"},
            {"sinh","Sinh"},
            {"cosh","Cosh"},
            {"tanh","Tanh"},
            {"max","Max"},
            {"min","Min"},
        };
        public static double Neg(double n)
        {
            return -n;
        }
        public static double Median(double n1,double n2,double n3)
        {
            double[] arr = new double[3] { n1,n2,n3 };
            Array.Sort(arr);
            return arr[1];
        }
        public static double Sgn(double n1)
        {
            return Math.Sign(n1);
        }
        public static double Root(double n1,double n2)
        {
            return Math.Pow(n1, 1 / n2);
        }
        public static int Equal(double x, double y)
        {
            if (x == y)
                return 0;
            return (x > y) ? 10 : 1;
        }
        public static int Less(double x, double y)
        {
            if (x == y)
                return 0;
            return (x > y) ? 10 : 1;
        }
        public static int Greater(double x, double y)
        {
            if (x == y)
                return 0;
            return (x > y) ? 10 : 1;
        }
    }
}
