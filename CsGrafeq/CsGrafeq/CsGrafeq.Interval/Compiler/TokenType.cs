namespace CsGrafeq.Interval.Compiler;

internal enum TokenType
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Pow,
    Mod,
    LeftBracket,
    RightBracket,
    Start,
    Neg,
    Equal,
    Less,
    Greater,
    LessEqual,
    GreaterEqual,
    Union,
    Intersect,
    VariableOrFunction,
    Number,
    Comma,
    Err_UnDefined
}