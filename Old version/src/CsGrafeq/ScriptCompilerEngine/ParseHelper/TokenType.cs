using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompilerEngine.ParseHelper
{
    internal enum TokenType
    {
        Add, Subtract, Multiply, Divide, Pow, Mod, Connect,
        Equal, LessThan, GreaterThan, LessEqual, GreaterEqual, NotEqual,
        Eqv, And, Not, Or, Xor, Imp,
        LeftBracket, RightBracket, Start, Neg,
        Dim, ReDim, Preserve,
        If, Else, ElseIf, Then, End,
        For, Next, Each, To, In, Step,
        Do, While, Loop, Until,Return,
        Exit,
        Comma, Empty,
        Number, String, VariableOrFunction,
        None
    }
}
