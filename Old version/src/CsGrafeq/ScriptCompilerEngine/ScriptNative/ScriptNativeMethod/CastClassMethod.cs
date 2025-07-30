using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCompilerEngine.ScriptNative.InternalMethod;
using ScriptCompilerEngine.ScriptNative.ScriptNativeObject;
namespace ScriptCompilerEngine.ScriptNative.ScriptNativeMethod
{
    public static partial class ScriptNativeMethod
    {
        
        public static object CStr(object obj)
        {
            return Method.ObjectToString(obj);
        }
        public static object CInt(object obj)
        {
            return Method.NumberToLong(obj);
        }
        public static object CDbl(object obj)
        {
            return Method.NumberToDouble(obj);
        }
        public static object CBool(object obj)
        {
            return Method.ObjectToBool(obj);
        }
        public static object TypeName(object obj)
        {
            switch (obj)
            {
                case long _:
                    return "Integer";
                case double _:
                    return "Double";
                case Empty _:
                    return "Empty";
                case Undefined _:
                    throw new Exception();
                case string _:
                    return "String";
                case bool _:
                    return "Bool";
                case null:
                    return "Nothing";
                default:
                    throw new Exception();
            }
        }
    }
}
