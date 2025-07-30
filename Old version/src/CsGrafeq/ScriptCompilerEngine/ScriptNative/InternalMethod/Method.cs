using ScriptCompilerEngine.ScriptNative.ScriptNativeObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ScriptCompilerEngine.DefinedFunction;

namespace ScriptCompilerEngine.ScriptNative.InternalMethod
{
    public class Method
    {
        //只允许long,double,bool,string,array,empty,undefined,null
        public static Empty GetEmpty() { return new Empty(); }
        public static bool ObjectToBool(object obj)
        {
            bool result;
            if (obj is bool)
                result = (bool)obj;
            else if (obj is Empty)
                result = false;
            else if (obj is Undefined)
                throw new Exception("变量未声明");
            else if (obj == null)
                result = false;
            else if (obj is string)
                result = ((string)obj).Length != 0;
            else if (obj is long l)
                result = l != 0;
            else if (obj is double d)
                result = d != 0;
            else if (obj is Array a)
                result = a.Length != 0;
            else
                throw new Exception();
            return result;
        }
        public static void SetArrayEmpty(object arr)
        {
            object value = Empty.Instance;
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            if (!(arr is Array))
                throw new ArgumentNullException(nameof(arr));
            Array array = arr as Array;
            int rank = array.Rank;
            if (rank == 0)
                return;

            int[] lengths = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                int len = array.GetLength(i);
                if (len == 0)
                    return; // 存在空维度，无需处理
                lengths[i] = len;
            }

            int[] indices = new int[rank];

            while (true)
            {
                array.SetValue(value, indices);

                int currentDimension = rank - 1;
                while (currentDimension >= 0)
                {
                    indices[currentDimension]++;
                    if (indices[currentDimension] < lengths[currentDimension])
                        break;

                    indices[currentDimension] = 0;
                    currentDimension--;
                }

                if (currentDimension < 0)
                    break;
            }
        }
        public static bool IsConstant(string constname)
        {
            constname = constname.ToLower();
            if (constname == "true")
                return true;
            if (constname == "false")
                return true;
            return false;
        }
        public static object GetConstantValue(string constname)
        {
            constname = constname.ToLower();
            if (constname == "true")
                return true;
            if (constname == "false")
                return false;
            throw new Exception();
        }
        public static long NumberToLong(object a)
        {
            if (a is bool b)
                return b ? 1 : 0;
            if (a is long)
                return (int)(long)a;
            if (a is double)
                if ((double)a == (long)(double)a)
                    return (long)(double)a;
            throw new Exception();
        }
        public static int NumberToInt(object a)
        {
            return (int)NumberToLong(a);
        }
        public static double NumberToDouble(object a)
        {
            if (a is bool bl)
                return bl ? 1 : 0;
            else if (a is double)
                return (double)a;
            if (a is long)
                return (long)a;
            else if (a is Empty)
                return 0;
            throw new Exception();
        }
        public static string ObjectToString(object a)
        {
            if (a == null)
                return "";
            if (a is bool b)
                return b ? "True" : "False";
            if (a is Array)
                return "Array";
            if (a is string s)
                return s;
            if (a is double d)
                return d.ToString();
            if (a is long l)
                return l.ToString();
            if (a is Empty)
                return "Empty";
            if (a is Undefined)
                throw new Exception();
            throw new Exception(a.GetType().FullName);
        }
        public static bool LessEqualNumber(double a, double b)
        {
            return a <= b;
        }
        public static bool GreaterEqualNumber(double a, double b)
        {
            return a >= b;
        }
        public static int BoolToInt(bool a)
        {
            return a ? 1 : 0;
        }
        public static object LateGetWithArgs(object target, object[] indexs)
        {
            switch (target)
            {
                case Array array:
                    {
                        int argnum = indexs.Length;
                        Type arrtype =GetTypeOfObjectArray(argnum);
                        if (arrtype!= target.GetType())
                            throw new Exception();
                        for (int i = 0; i < argnum; i++)
                        {
                            indexs[i] = (int)(long)(indexs[i]);
                        }
                        return arrtype.GetMethod("Get").Invoke(target, indexs);
                    }
                case string s://自定义函数
                    {
                        return DefinedFunctions.CallFunction(s, indexs);
                    }
                default:
                    throw new Exception();
            }
            
        }
        public static Type GetTypeOfObjectArray(int rank)
        {
            return ObjArrayType[rank];
        }
        private static List<Type> ObjArrayType = new List<Type>()
            {
typeof(object),
typeof(object[]),
typeof(object[,]),
typeof(object[,,]),
typeof(object[,,,]),
typeof(object[,,,,]),
typeof(object[,,,,,]),
typeof(object[,,,,,,]),
typeof(object[,,,,,,,]),
typeof(object[,,,,,,,,]),
typeof(object[,,,,,,,,,]),
typeof(object[,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,]),
typeof(object[,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,])
            };
    }
}
