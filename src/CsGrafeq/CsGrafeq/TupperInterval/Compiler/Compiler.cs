using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using sysMath = System.Math;

namespace CsGrafeq.TupperInterval.Compiler;

public static class Compiler
{
    private static readonly Regex letter = new("[a-zA-Z]");
    private static readonly Regex number = new("[0-9]");
    private static readonly Regex oper = new("([/<>+=^*%(),|&]|-)");
    private static readonly Regex letterOrnumberOr_ = new("[a-zA-Z0-9_]");
    private static readonly Regex numberOrpoint = new("[0-9.]");
    private static readonly Regex spaceOrtab = new(@"([ ]|\t)");
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(IInterval))]
    public static TupperIntervalHandler<T> Compile<T>(string expression, Type MathType) where T : IInterval
    {
        MethodInfoHelper helper = new MethodInfoHelper(MathType);
        Element[] elements = expression.GetTokens().ParseTokens();
        ParameterExpression x=Expression.Parameter(typeof(T),"x");
        ParameterExpression y=Expression.Parameter(typeof(T),"y");
        Stack<Expression> expStack = new Stack<Expression>();
        foreach (Element element in elements)
        {
            switch (element.Type)
            {
                case ElementType.Number:
                {
                    double d=double.Parse(element.NameOrValue);
                    expStack.Push(Expression.Call(helper.Get("New"), Expression.Constant(d)));
                }
                    break;
                case ElementType.Operator:
                {
                    var mf = helper.Get(element.NameOrValue);
                    if (mf == null)
                        throw new MissingMethodException(element.NameOrValue);
                    switch (mf.GetParameters().Length)
                    {
                        case 1:
                            expStack.Push(Expression.Call(mf, expStack.Pop()));
                            break;
                        case 2:
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Call(mf, expStack.Pop(),exp1));
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(element.NameOrValue+".ParamCount:"+element.ArgCount);
                    }
                }
                    break;
                case ElementType.Variable:
                {
                    string name=element.NameOrValue.ToLower();
                    if (name == "e")
                        expStack.Push(Expression.Call(helper.Get("New"), Expression.Constant(sysMath.E)));
                    else if (name == "pi")
                        expStack.Push(Expression.Call(helper.Get("New"), Expression.Constant(sysMath.PI)));
                    else if (name == "x")
                        expStack.Push(x);
                    else if (name == "y")
                        expStack.Push(y);
                    else
                        throw new MissingFieldException(element.NameOrValue);
                }
                    break;
                case ElementType.Function:
                {
                    var mf = helper.Get(element.NameOrValue,element.ArgCount);
                    if (mf == null)
                        throw new MissingMethodException(element.NameOrValue+".ArgCount:"+element.ArgCount);
                    switch (element.ArgCount)
                    {
                        case 1:
                            expStack.Push(Expression.Call(mf, expStack.Pop()));
                            break;
                        case 2:
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Call(mf, expStack.Pop(),exp1));
                        }
                            break;
                        case 3:
                        {
                            var exp1 = expStack.Pop();
                            var exp2 = expStack.Pop();
                            expStack.Push(Expression.Call(mf, expStack.Pop(),exp2,exp1));
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(element.NameOrValue+".ParamCount:"+element.ArgCount);
                    }
                }
                    break;
            }
        }
        return Expression.Lambda<TupperIntervalHandler<T>>(expStack.Single(),x,y).Compile();
    }
    public static TupperIntervalHandler<T> CompileAndTest<T>(string expression, Type MathType) where T : IInterval,new()
    {
        var res=Compile<T>(expression,MathType);
        T test = (T)T.Create(1);
        res.Invoke(test,test);
        return res;
    }

    public static void DynamicTest()
    {
        DynamicMethod met = new DynamicMethod("123", typeof(void), []);
        ILGenerator gen = met.GetILGenerator();
        gen.Emit(OpCodes.Ret);
        met.CreateDelegate<Action>().Invoke();
    }
    internal static Element[] ParseTokens(this Token[] Tokens)
    {
        var op = new Stack<OperatorType>();
        var exp = new Stack<Element>();
        var loc = 0;
        var len = Tokens.Length - 1;
        op.Push(OperatorType.Start);
        var Previous = ElementType.None;
        while (loc <= len)
            switch (GetTokenLevel(Tokens[loc]))
            {
                case -3: //var|func
                    if (Previous == ElementType.Number || Previous == ElementType.Variable ||
                        Previous == ElementType.Function)
                        throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                    var Tokenname = Tokens[loc].NameOrValue;
                    if (Tokens[sysMath.Min(loc + 1, len)].Type == TokenType.LeftBracket)
                    {
                        Previous = ElementType.Variable;
                        loc += 2;
                        if (Tokens[loc].Type == TokenType.RightBracket)
                        {
                            exp.Push(new Element(ElementType.Function, Tokenname, 0));
                            loc++;
                        }
                        else
                        {
                            var bc = 0;
                            var dimcount = 0;
                            var ts = new List<Token>();
                            while (true)
                            {
                                if (bc == 0 && Tokens[loc].Type == TokenType.Comma)
                                {
                                    dimcount++;
                                    foreach (var i in ParseTokens(ts.ToArray()))
                                        exp.Push(i);
                                    ts.Clear();
                                    loc++;
                                    continue;
                                }

                                if (bc == 0 && Tokens[loc].Type == TokenType.RightBracket)
                                {
                                    dimcount++;
                                    foreach (var i in ParseTokens(ts.ToArray()))
                                        exp.Push(i);
                                    loc++;
                                    break;
                                }

                                if (Tokens[loc].Type == TokenType.LeftBracket)
                                    bc++;
                                if (Tokens[loc].Type == TokenType.RightBracket)
                                    bc--;
                                ts.Add(Tokens[loc]);
                                loc++;
                            }

                            exp.Push(new Element(ElementType.Function, Tokenname, dimcount));
                        }
                    }
                    else if ((Tokenname.Length == 1 && 'a' <= Tokenname.ToLower()[0] &&
                              'z' >= Tokenname.ToLower()[0]) || Tokenname.ToLower() == "pi") //var
                    {
                        Previous = ElementType.Variable;
                        exp.Push(new Element(ElementType.Variable, Tokenname, 0));
                        loc++;
                    }
                    else
                    {
                        throw new Exception("未知变量:" + Tokens[loc].NameOrValue);
                    }

                    break;
                case -4: //num
                    if (Previous == ElementType.Number || Previous == ElementType.Variable ||
                        Previous == ElementType.Function)
                        throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                    Previous = ElementType.Number;
                    exp.Push(new Element(ElementType.Number, Tokens[loc].NameOrValue, 0));
                    loc++;
                    break;
                case -1: //'('|')'
                    if (Tokens[loc].Type == TokenType.LeftBracket)
                    {
                        if (Previous == ElementType.Number || Previous == ElementType.Variable)
                            throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                        op.Push(OperatorType.LeftBracket);
                        Previous = ElementType.Operator;
                    }
                    else if (Tokens[loc].Type == TokenType.RightBracket)
                    {
                        if (Previous == ElementType.Function || Previous == ElementType.Operator)
                            throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                        Previous = ElementType.Variable;
                        MoveOperatorTo(op, exp, OperatorType.LeftBracket);
                    }

                    loc++;
                    break;
                default:
                    if (Previous == ElementType.Operator && Tokens[loc].Type == TokenType.Subtract)
                    {
                        Tokens[loc].Type = TokenType.Neg;
                        goto jt;
                    }

                    if (Previous == ElementType.Function || Previous == ElementType.Operator)
                        throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间" + Previous);
                    jt:
                    Previous = ElementType.Operator;
                    JudgeOperator(op, exp, Tokens[loc].ToOper());
                    loc++;
                    break;
            }

        while (op.Count != 0 && op.Peek() != OperatorType.Start) exp.Push(op.Pop().ToElement());
        return exp.Reverse().ToArray();
    }

    internal static Token[] GetTokens(this string script) //词法分析器
    {
        script += '#';
        var loc = 0;
        var Tokens = new List<Token>();
        while (loc < script.Length)
        {
            Token t;
            t.NameOrValue = "";
            t.Type = TokenType.Err_UnDefined;
            if (script[loc] == '#') break;

            if (letter.IsMatch(script[loc] + ""))
            {
                t.NameOrValue += script[loc];
                loc++;
                while (letterOrnumberOr_.IsMatch(script[loc] + ""))
                {
                    t.NameOrValue += script[loc];
                    loc++;
                }

                t.Type = TokenType.VariableOrFunction;
            }
            else if (number.IsMatch(script[loc] + ""))
            {
                t.NameOrValue += script[loc];
                loc++;
                while (numberOrpoint.IsMatch(script[loc] + ""))
                {
                    t.NameOrValue += script[loc];
                    loc++;
                }

                t.Type = TokenType.Number;
            }
            else if (oper.IsMatch(script[loc] + ""))
            {
                switch (script[loc])
                {
                    case '+':
                        t.Type = TokenType.Add; break;
                    case '-':
                        t.Type = TokenType.Subtract; break;
                    case '*':
                        t.Type = TokenType.Multiply; break;
                    case '/':
                        t.Type = TokenType.Divide; break;
                    case '^':
                        t.Type = TokenType.Pow; break;
                    case '%':
                        t.Type = TokenType.Mod; break;
                    case '(':
                        t.Type = TokenType.LeftBracket; break;
                    case ')':
                        t.Type = TokenType.RightBracket; break;
                    case '>':
                        if (script[loc + 1] == '=')
                        {
                            t.Type = TokenType.GreaterEqual;
                            loc++;
                        }
                        else
                        {
                            t.Type = TokenType.Greater;
                        }

                        break;
                    case '<':
                        if (script[loc + 1] == '=')
                        {
                            t.Type = TokenType.LessEqual;
                            loc++;
                        }
                        else
                        {
                            t.Type = TokenType.Less;
                        }

                        break;
                    case '=':
                        if (script[loc + 1] == '=')
                            loc++;
                        t.Type = TokenType.Equal;
                        break;
                    case '|':
                        t.Type = TokenType.Union;
                        break;
                    case '&':
                        t.Type = TokenType.Intersect;
                        break;
                    case ',':
                        t.Type = TokenType.Comma; break;
                    default:
                        throw new Exception("未知符号:" + t.NameOrValue);
                }

                loc++;
            }
            else if (spaceOrtab.IsMatch(script[loc] + ""))
            {
                loc++;
                continue;
            }
            else
            {
                throw new Exception("未知字符" + script[loc]);
            }

            Tokens.Add(t);
        }

        return Tokens.ToArray();
    }

    private static int GetTokenLevel(Token c)
    {
        switch (c.Type)
        {
            case TokenType.LeftBracket:
            case TokenType.RightBracket:
                return -1;
            case TokenType.Comma: return -2;
            case TokenType.VariableOrFunction: return -3;
            case TokenType.Number: return -4;
            default: return 0;
        }
    }

    private static int GetOperLevel(OperatorType o)
    {
        switch (o)
        {
            case OperatorType.Union:
            case OperatorType.Intersect:
                return -1;
            case OperatorType.Equal:
            case OperatorType.Less:
            case OperatorType.Greater:
            case OperatorType.LessEqual:
            case OperatorType.GreaterEqual:
                return 0;
            case OperatorType.Add:
            case OperatorType.Subtract:
                return 1;
            case OperatorType.Mod:
                return 2;
            case OperatorType.Multiply:
            case OperatorType.Divide:
                return 3;
            case OperatorType.Neg:
                return 4;
            case OperatorType.Pow:
                return 5;
            default: return -1;
        }
    }

    private static int CheckArg(OperatorType op)
    {
        switch (op)
        {
            case OperatorType.Neg:
                return 1;
            default:
                return 2;
        }
    }

    private static void MoveOperatorTo(Stack<OperatorType> opStack, Stack<Element> expStack, OperatorType op)
    {
        var s = opStack.Pop();
        if (s != op)
        {
            expStack.Push(s.ToElement());
            MoveOperatorTo(opStack, expStack, op);
        }
    }

    private static void JudgeOperator(Stack<OperatorType> opStack, Stack<Element> expStack, OperatorType x)
    {
        var xNum = GetOperLevel(x);
        var opNum = GetOperLevel(opStack.Peek());
        if (xNum > opNum || opNum == -1 || (CheckArg(x) == 1 && CheckArg(opStack.Peek()) == 1))
        {
            opStack.Push(x);
        }
        else
        {
            var opStr = opStack.Pop().ToElement();
            expStack.Push(new Element(ElementType.Operator, opStr.NameOrValue, 2));
            JudgeOperator(opStack, expStack, x);
        }
    }
}