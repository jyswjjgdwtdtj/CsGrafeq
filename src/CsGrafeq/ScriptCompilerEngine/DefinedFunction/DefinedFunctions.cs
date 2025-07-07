using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.DefinedFunction
{
    public static class DefinedFunctions
    {
        internal static List<DefinedFunction> Funcs=new List<DefinedFunction>();
        public static IEnumerable<DefinedFunction> GetEnumerable()
        {
            foreach(var func in Funcs)
            {
                yield return func;
            }
            yield break;
        }
        public static object CallFunction(string Name, object[] args)
        {
            MethodInfo method=GetFunction(Name);
            if (method == null)
                throw new Exception();
            return method.Invoke(null, args);
        }
        public static MethodInfo GetFunction(string Name)
        {
            Name = Name.ToLower();
            int hash=Name.GetHashCode();
            return Funcs.Find(
                new Predicate<DefinedFunction>((f) =>
                {
                    return f.Hash == hash;
                })
            ).CompiledFunction;
        }
        public static void AddFunction(string Name,string Body, string[] ArgumentName)
        {
            Name=Name.ToLower();
            if (GetFunction(Name) != null)
                Funcs.Add(new DefinedFunction(Name, Body, ArgumentName));
            else
                throw new Exception();
        }
        public static void RemoveFunction(string Name)
        {
            int hash=Name.ToLower().GetHashCode();
            Funcs.Remove(Funcs.Find(
                new Predicate<DefinedFunction>((f) =>
                {
                    return f.Hash == hash;
                })
            ));
        }
    }
}
