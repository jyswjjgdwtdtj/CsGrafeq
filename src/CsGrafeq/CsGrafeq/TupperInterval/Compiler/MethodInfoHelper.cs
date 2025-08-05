using System.Reflection;
using Splat;

namespace CsGrafeq.TupperInterval.Compiler;

public class MethodInfoHelper
{
    private Type Type;

    public MethodInfoHelper(Type type)
    {
        Type = type;
    }

    public MethodInfo? Get(string name)
    {
        return Type.GetMethod(name,BindingFlags.IgnoreCase|BindingFlags.Static|BindingFlags.Public);
    }

    public MethodInfo? Get(string name,int argcount)
    {
        foreach (var mi in Type.GetMethods(BindingFlags.Static|BindingFlags.Public))
        {
            if (mi.Name.ToLower() == name.ToLower())
            {
                if (mi.GetParameters().Length == argcount)
                {
                    return mi;
                }
            }
        }
        return null;
    }
}