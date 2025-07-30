using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal enum ElementType
    {
        Variable, Number, String, Function, BasicOperator, Operator, LateBingdingArgs, LateBingdingFunc
    }
}
