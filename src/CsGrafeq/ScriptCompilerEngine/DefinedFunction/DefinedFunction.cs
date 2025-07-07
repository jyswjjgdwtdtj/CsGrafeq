using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.DefinedFunction
{
    public class DefinedFunction
    {
        internal string _FunctionBody = "";
        internal string _Name = "";
        internal string[] ArgumentName;
        internal int Hash;
        internal DynamicMethod CompiledFunction;
        internal DefinedFunction(string name,string functionBody, string[] arguementName)
        {
            _Name = name.ToLower();
            ArgumentName = arguementName;
            Body = functionBody;
            Hash=_Name.GetHashCode();
        }
        public string Name
        {
            get
            {
                return _Name;
            }
        }
        public string Body
        {
            get
            {
                return _FunctionBody;
            }
            set
            {
                //
                _FunctionBody = value;
            }
        }
    }
}
