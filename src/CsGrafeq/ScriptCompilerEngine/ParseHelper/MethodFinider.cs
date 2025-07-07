using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal class MethodFinider
    {
        private List<Type> types=new List<Type>();
        public void Add(Type t)
        {
            types.Add(t);   
        }
        public void Remove(Type t)
        {
            types.Remove(t);
        }
        public bool TryGetMethod(string name, out MethodInfo mi)
        {
            name = name.ToLower();
            mi = null;
            foreach (var i in types)
            {
                
                var mti = i.GetMethod(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase);
                if (mti != null)
                {
                    mi = mti;
                    return true;
                }
            }
            return false;
        }
        public MethodInfo GetMethod(string name)
        {
            if (TryGetMethod(name, out MethodInfo mi))
            {
                return mi;
            }
            throw new Exception();
        }
        public bool TryGetMethod(string name, out MethodInfo mi, Type[] ts)
        {
            name = name.ToLower();
            mi = null;
            foreach (var i in types)
            {
                var mti = i.GetMethod(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.IgnoreCase,null,ts,null);
                if (mti != null)
                {
                    mi = mti;
                    return true;
                }
            }
            return false;
        }
        public MethodInfo GetMethod(string name, Type[] types)
        {
            if (TryGetMethod(name, out MethodInfo mi,types))
            {
                return mi;
            }
            throw new Exception();
        }
    }
}
