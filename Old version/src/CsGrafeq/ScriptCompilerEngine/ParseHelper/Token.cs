using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal struct Token
    {
        public TokenType Type;
        public string NameOrValue;
        public override string ToString()
        {
            return $"Type:{Type},NameOrValue:{NameOrValue}";
        }
    }
}
