using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ScriptCompilerEngine.ParseHelper;
using ScriptCompilerEngine.ScriptNative.ScriptNativeMethod;
using ScriptCompilerEngine.ScriptNative.ScriptNativeObject;
namespace ScriptCompilerEngine.CompileEngine
{
    public delegate object NoArgFunc();
    public static class CompileEngine
    {
        static CompileEngine()
        {
            AddMethodClass(typeof(ScriptNativeMethod));
        }
        public static void AddMethodClass(Type t)
        {
            ParseMethods.MethodFinder.Add(t);
        }
        public static string LastILRecord;
        public static NoArgFunc CompileNoArgFunc(string script)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("CplMethod", typeof(object), new Type[0]);
            ILGenerator il = dynamicMethod.GetILGenerator();
            ILHelper ILH=new ILHelper(il,new string[0]);
            ParseMethods.ParseScript(ILH,script);
            ILH.LoadVariable("ret");
            ILH.Emit(OpCodes.Ret);
            LastILRecord = ILH.GetRecord();
            return (NoArgFunc)dynamicMethod.CreateDelegate(typeof(NoArgFunc));
        }
        public static Delegate Compile(string script, string[] ArgName)
        {
            DynamicMethod dynamicMethod = new DynamicMethod("CplMethod", typeof(object), typeof(object).GetArray(ArgName.Length));
            ILGenerator il = dynamicMethod.GetILGenerator();
            ILHelper ILH = new ILHelper(il, ArgName);
            ParseMethods.ParseScript(ILH, script);
            ILH.LoadVariable("ret");
            ILH.Emit(OpCodes.Ret);
            LastILRecord = ILH.GetRecord();
            return dynamicMethod.CreateDelegate(GetFuncDelegate(ArgName.Length));
        }
        private static Type GetFuncDelegate(int len)
        {
            return Type.GetType($"System.Func`{len+1}[{string.Join(",", Enumerable.Repeat("[System.Object]", len+1))}]");
        }
    }
    public static class Test
    {
        public static object MsgBox(object prompt)
        {
            Interaction.MsgBox(prompt);
            return Empty.Instance;
        }
    }
}
