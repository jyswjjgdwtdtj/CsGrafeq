using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ScriptNative.ScriptNativeObject
{
    public class Empty:ScriptNativeObject
    {
        public static Empty Instance = new Empty();
    }
}
