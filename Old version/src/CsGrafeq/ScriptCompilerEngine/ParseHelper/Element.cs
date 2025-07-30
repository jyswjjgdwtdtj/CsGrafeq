using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal struct Element
    {
        public ElementType Type;
        public string NameOrValue;
        public int ArgCount;
        public Element(ElementType type, string nameOrValue, int arg)
        {
            Type= type;
            NameOrValue= nameOrValue;
            ArgCount= arg;
        }
        public override string ToString()
        {
            return $"Type:{Type.ToString()},NameOrValue:{NameOrValue},ArgCount:{ArgCount}";
        }
    }
}
