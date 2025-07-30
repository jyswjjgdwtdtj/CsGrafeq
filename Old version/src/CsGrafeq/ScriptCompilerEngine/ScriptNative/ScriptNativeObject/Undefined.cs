using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ScriptNative.ScriptNativeObject
{
    internal class Undefined:ScriptNativeObject
    {
        public static Undefined Instance = new Undefined();
    }
}
