using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using CsGrafeq.Numeric;
using sysMath = System.Math;

namespace CsGrafeq.Compiler;

public static class Compiler
{
    private static readonly Regex letter = new("[a-zA-Z]");
    private static readonly Regex number = new("[0-9]");
    private static readonly Regex oper = new("([/<>+=^*%(),|&]|-)");
    private static readonly Regex letterOrnumberOr_ = new("[a-zA-Z0-9_]");
    private static readonly Regex numberOrpoint = new("[0-9.]");
    private static readonly Regex spaceOrtab = new(@"([ ]|\t)");

    public static Delegate Compile<T>(string expression, uint paraCount, out EnglishCharEnum usedVars)
        where T : IComputableNumber<T>, INeedClone<T>
    {
        var exp = ConstructExpTree<T>(expression, paraCount, out var x, out var y, out var z, out usedVars);
        var expResult = Expression.Lambda(exp, ((IEnumerable<ParameterExpression>)[x, y, z]).Take((int)paraCount));
        return expResult.Compile();
    }

    public static bool TryCompile<T>(string expression, uint paraCount, out Delegate expFunc,
        out EnglishCharEnum usedVars, out Exception? ex)
        where T : IComputableNumber<T>
    {
        //expFunc = Compile<T>(expression,paraCount,out usedVars);
        try
        {
            expFunc = Compile<T>(expression, paraCount, out usedVars);
            ex = null;
            return true;
        }
        catch (Exception e)
        {
            if (e is InvalidOperationException ioe && ioe.Message == "Stack empty.")
                ex = new Exception("Incomplete expression");
            else
                ex = e;
            expFunc = null;
            usedVars = EnglishCharEnum.None;
            return false;
        }
    }

    public static Expression ConstructExpTree<T>(string expression, [Range(0, 3)] uint argsLength,
        out ParameterExpression xVar, out ParameterExpression yVar, out ParameterExpression zVar,
        out EnglishCharEnum usedVars) where T : IComputableNumber<T>
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new Exception("Expression cannot be empty");
        usedVars = EnglishCharEnum.None;
        var elements = expression.GetTokens().ParseTokens();
        var expStack = new Stack<Expression>();
        var cloneMethod = GetInfo(T.Clone);
        var needClone = true;
        xVar = Expression.Parameter(typeof(T), "x");
        yVar = Expression.Parameter(typeof(T), "y");
        zVar = Expression.Parameter(typeof(T), "z");
        var variables = Expression.Constant(EnglishChar.Instance);
        foreach (var element in elements)
            switch (element.Type)
            {
                case ElementType.Number:
                {
                    if (!double.TryParse(element.NameOrValue, out var d))
                        throw new Exception("Number can not be parsed: " + element.NameOrValue);
                    expStack.Push(Expression.Call(GetInfo(T.CreateFromDouble), Expression.Constant(d)));
                }
                    break;
                case ElementType.Operator:
                {
                    switch (element.NameOrValue)
                    {
                        case "Pow":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Call(GetInfo(T.Pow), expStack.Pop(), exp1));
                        }
                            break;
                        case "Add":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Add(expStack.Pop(), exp1));
                        }
                            break;
                        case "Subtract":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Subtract(expStack.Pop(), exp1));
                        }
                            break;
                        case "Multiply":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Multiply(expStack.Pop(), exp1));
                        }
                            break;

                        case "Divide":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Divide(expStack.Pop(), exp1));
                        }
                            break;
                        case "Modulo":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Modulo(expStack.Pop(), exp1));
                        }
                            break;
                        case "Less":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.LessThan(expStack.Pop(), exp1));
                        }
                            break;

                        case "Greater":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.GreaterThan(expStack.Pop(), exp1));
                        }
                            break;
                        case "Equal":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Equal(expStack.Pop(), exp1));
                        }
                            break;
                        case "LessEqual":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.LessThanOrEqual(expStack.Pop(), exp1));
                        }
                            break;
                        case "GreaterEqual":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.GreaterThanOrEqual(expStack.Pop(), exp1));
                        }
                            break;
                        case "Union":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.Or(expStack.Pop(), exp1));
                        }
                            break;
                        case "Intersect":
                        {
                            var exp1 = expStack.Pop();
                            expStack.Push(Expression.And(expStack.Pop(), exp1));
                        }
                            break;
                        case "Neg":
                        {
                            expStack.Push(Expression.Negate(expStack.Pop()));
                        }
                            break;
                        default:
                            throw new Exception("Unknown operator");
                    }
                }
                    break;
                case ElementType.Variable:
                {
                    var name = element.NameOrValue.ToLower();
                    if (name == "e")
                    {
                        expStack.Push(Expression.Call(GetInfo(T.CreateFromDouble), Expression.Constant(sysMath.E)));
                    }
                    else if (name == "pi")
                    {
                        expStack.Push(Expression.Call(GetInfo(T.CreateFromDouble), Expression.Constant(sysMath.PI)));
                    }
                    else if (name == "x")
                    {
                        if (argsLength < 1) throw new Exception("Variable 'X' is unavailable");
                        expStack.Push(needClone ? Expression.Call(cloneMethod, xVar) : xVar);
                    }
                    else if (name == "y")
                    {
                        if (argsLength < 2) throw new Exception("Variable 'Y' is unavailable");
                        expStack.Push(needClone ? Expression.Call(cloneMethod, yVar) : yVar);
                    }
                    else if (name == "z")
                    {
                        if (argsLength < 3) throw new Exception("Variable 'Y' is unavailable");
                        expStack.Push(needClone ? Expression.Call(cloneMethod, zVar) : zVar);
                    }
                    else if (name.Length == 1 && 'a' <= name[0] && name[0] <= 'z')
                    {
                        expStack.Push(Expression.Call(GetInfo(T.CreateFromDouble),
                            Expression.Call(GetInfo(EnglishChar.StaticGetValue),
                                Expression.Constant(name[0]))));
                        usedVars |= (EnglishCharEnum)sysMath.Pow(2, name[0] - 'a');
                    }
                    else
                    {
                        throw new Exception("Unknown variable:" + element.NameOrValue);
                    }
                }
                    break;
                case ElementType.Function:
                {
                    if (IComputableNumber<T>.ComputableNumberMethodDictionary.TryGetValue(element.NameOrValue.ToLower(),
                            out var method) && method.Method.GetParameters().Length == element.ArgCount)
                        switch (element.ArgCount)
                        {
                            case 1:
                                expStack.Push(Expression.Call(method.Method, expStack.Pop()));
                                break;
                            case 2:
                            {
                                var exp1 = expStack.Pop();
                                expStack.Push(Expression.Call(method.Method, expStack.Pop(), exp1));
                            }
                                break;
                            case 3:
                            {
                                var exp1 = expStack.Pop();
                                var exp2 = expStack.Pop();
                                expStack.Push(Expression.Call(method.Method, expStack.Pop(), exp2, exp1));
                            }
                                break;
                            default:
                                throw new Exception(element.NameOrValue + " " + element.ArgCount);
                        }
                    else
                        throw new Exception("Method not found: " + element.NameOrValue);
                }
                    break;
                default:
                    throw new Exception("Unknown element:" + element.NameOrValue + " " + element.Type);
            }

        return expStack.Single();
        //var expres = Expression.Lambda<Function0<T>>(expStack.Single());
        //return expres.Compile();
    }

    public static Element[] ParseTokens(this Token[] Tokens)
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
                        throw new Exception("Missing operator between " + Tokens[loc - 1] + " and " + Tokens[loc]);
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
                        throw new Exception("Unknown variable:" + Tokens[loc].NameOrValue);
                    }

                    break;
                case -4: //num
                    if (Previous == ElementType.Number || Previous == ElementType.Variable ||
                        Previous == ElementType.Function)
                        throw new Exception("Missing operator between " + Tokens[loc - 1] + " and " + Tokens[loc]);
                    Previous = ElementType.Number;
                    exp.Push(new Element(ElementType.Number, Tokens[loc].NameOrValue, 0));
                    loc++;
                    break;
                case -1: //'('|')'
                    if (Tokens[loc].Type == TokenType.LeftBracket)
                    {
                        if (Previous == ElementType.Number || Previous == ElementType.Variable)
                            throw new Exception("Missing operator between " + Tokens[loc - 1] + " and " + Tokens[loc]);
                        op.Push(OperatorType.LeftBracket);
                        Previous = ElementType.Operator;
                    }
                    else if (Tokens[loc].Type == TokenType.RightBracket)
                    {
                        if (Previous == ElementType.Function || Previous == ElementType.Operator)
                            throw new Exception("Missing operator between " + Tokens[loc - 1] + " and " + Tokens[loc]);
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
                        throw new Exception("Missing operator between " + Tokens[loc - 1] + " and " + Tokens[loc]);
                    jt:
                    Previous = ElementType.Operator;
                    JudgeOperator(op, exp, Tokens[loc].ToOper());
                    loc++;
                    break;
            }

        while (op.Count != 0 && op.Peek() != OperatorType.Start) exp.Push(op.Pop().ToElement());
        return exp.Reverse().ToArray();
    }

    public static Token[] GetTokens(this string script) //词法分析器
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
                        throw new Exception("Unknown character:" + t.NameOrValue);
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
                throw new Exception("Unknown character:" + script[loc]);
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
            case OperatorType.Modulo:
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

    private static MethodInfo GetInfo(Delegate action)
    {
        return action.Method;
    }
}