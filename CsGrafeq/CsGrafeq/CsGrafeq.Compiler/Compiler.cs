using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CsGrafeq.Numeric;
using CsGrafeq.Variables;
using MathNet.Symbolics;
using Expression = System.Linq.Expressions.Expression;
using sysMath = System.Math;
using Expr = MathNet.Symbolics.SymbolicExpression;

namespace CsGrafeq.Compiler;

public static class Compiler
{
    private static readonly Regex LetterRegex = new("[a-zA-Z]");
    private static readonly Regex NumberRegex = new("[0-9]");
    private static readonly Regex OperatorRegex = new("([/<>+=^*%(),|&]|-)");
    private static readonly Regex LetterOrNumberOrUnderlineRegex = new("[a-zA-Z0-9_]");
    private static readonly Regex NumberOrPointRegex = new("[0-9.]");
    private static readonly Regex SpaceOrTabRegex = new(@"([ ]|\t)");

    public static Delegate Compile<T>(string expression, uint paraCount, bool useSimplification,
        out VariablesEnum usedVars)
        where T : IComputableNumber<T>, INeedClone<T>
    {
        ParameterExpression x, y, z;
        var exp = expression.ConstructExpTree<T>(paraCount, out x, out y, out z, out usedVars, useSimplification);
        var expResult = Expression.Lambda(exp, ((IEnumerable<ParameterExpression>)[x, y, z]).Take((int)paraCount));
        return expResult.Compile();
    }

    public static Result<(Delegate func, VariablesEnum usedVars)> TryCompile<T>(string expression, uint paraCount,
        bool useSimplification)
        where T : IComputableNumber<T>
    {
        try
        {
            return Result<(Delegate func, VariablesEnum)>.Success(
                (Compile<T>(expression, paraCount, useSimplification, out var usedVars), usedVars), expression);
        }
        catch (Exception e)
        {
            Exception ex;
            if (e is InvalidOperationException ioe && ioe.Message == "Stack empty.")
                ex = new Exception("Incomplete expression");
            else
                ex = e;
            return Result<(Delegate func, VariablesEnum)>.Failure(ex);
        }
    }

    public static Expression ConstructExpTree<T>(this string expression, [Range(0, 3)] uint argsLength,
        out ParameterExpression x, out ParameterExpression y, out ParameterExpression z,
        out VariablesEnum usedVars, bool enableSimplification) where T : IComputableNumber<T>, allows ref struct
    {
        return enableSimplification
            ? ContructSimplifiedExpTree<T>(expression, argsLength, out x, out y, out z, out usedVars)
            : expression.ConstructExpTree<T>(argsLength, out x, out y, out z, out usedVars);
    }

    public static Expression ConstructExpTree<T>(this string expression, [Range(0, 3)] uint argsLength,
        out ParameterExpression xVar, out ParameterExpression yVar, out ParameterExpression zVar,
        out VariablesEnum usedVars) where T : IComputableNumber<T>, allows ref struct
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new Exception("Expression cannot be empty");
        if (expression.StartsWith('-'))
            expression = '0' + expression;
        usedVars = VariablesEnum.None;
        var elements = expression.GetTokens().GetElements();
        var expStack = new Stack<Expression>();
        var cloneMethod = GetInfo(T.Clone);
        var needClone = T.NeedClone;
        xVar = Expression.Parameter(typeof(T), "x");
        yVar = Expression.Parameter(typeof(T), "y");
        zVar = Expression.Parameter(typeof(T), "z");
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
                            Expression.Call(GetInfo(Variables.VarRecorder.StaticGetValue),
                                Expression.Constant(name[0]))));
                        usedVars |= (VariablesEnum)sysMath.Pow(2, name[0] - 'a');
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

    public static Expression ContructSimplifiedExpTree<T>(string expression, [Range(0, 3)] uint argsLength,
        out ParameterExpression xVar, out ParameterExpression yVar, out ParameterExpression zVar,
        out VariablesEnum usedVars) where T : IComputableNumber<T>, allows ref struct
    {
        var strspan = expression.AsSpan();
        var sb = new StringBuilder();
        var totalexp = new StringBuilder();
        for (var i = 0; i < strspan.Length; i++)
        {
            var c = strspan[i];
            if (c == '<' || c == '>' || c == '=' || c == '|' || c == '&')
            {
                totalexp.Append(SimplifyExpression(sb.ToString()));
                sb.Clear();
                totalexp.Append(c);
                if ((c == '<' || c == '>') && strspan[i + 1] == '=')
                {
                    totalexp.Append('=');
                    i++;
                }

                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0)
            totalexp.Append(SimplifyExpression(sb.ToString()));
        return totalexp.ToString().ConstructExpTree<T>(argsLength, out xVar, out yVar, out zVar, out usedVars);

        static string SimplifyExpression(string expression)
        {
            //虽然我知道很蠢 但是也没办法 
            //这个库没有给出输出Expression的接口
            //而且是F#写的 我也不会改它的源码。。。
            var exp = Expr.Parse(expression);
            var simplified = exp.ExponentialSimplify().TrigonometricContract();
            var str = Infix.Format(simplified.Expression);
            if (str.StartsWith('-'))
                str = '0' + str;
            return str;
        }
    }

    public static Element[] GetElements(this Token[] tokens)
    {
        var op = new Stack<OperatorType>();
        var exp = new Stack<Element>();
        var loc = 0;
        var len = tokens.Length - 1;
        op.Push(OperatorType.Start);
        var previous = ElementType.None;
        while (loc <= len)
            switch (GetTokenLevel(tokens[loc]))
            {
                case -3: //var|func
                    if (previous == ElementType.Number || previous == ElementType.Variable)// rider说这永远不会发生 => previous == ElementType.Function
                        throw new Exception("Missing operator between " + tokens[loc - 1] + " and " + tokens[loc]);
                    var tokenName = tokens[loc].NameOrValue;
                    if (tokens[sysMath.Min(loc + 1, len)].Type == TokenType.LeftBracket)
                    {
                        previous = ElementType.Variable;
                        loc += 2;
                        if (tokens[loc].Type == TokenType.RightBracket)
                        {
                            exp.Push(new Element(ElementType.Function, tokenName, 0));
                            loc++;
                        }
                        else
                        {
                            var bc = 0;
                            var dimCounter = 0;
                            var ts = new List<Token>();
                            while (true)
                            {
                                if (bc == 0 && tokens[loc].Type == TokenType.Comma)
                                {
                                    dimCounter++;
                                    foreach (var i in ts.ToArray().GetElements())
                                        exp.Push(i);
                                    ts.Clear();
                                    loc++;
                                    continue;
                                }

                                if (bc == 0 && tokens[loc].Type == TokenType.RightBracket)
                                {
                                    dimCounter++;
                                    foreach (var i in ts.ToArray().GetElements())
                                        exp.Push(i);
                                    loc++;
                                    break;
                                }

                                if (tokens[loc].Type == TokenType.LeftBracket)
                                    bc++;
                                if (tokens[loc].Type == TokenType.RightBracket)
                                    bc--;
                                ts.Add(tokens[loc]);
                                loc++;
                            }

                            exp.Push(new Element(ElementType.Function, tokenName, dimCounter));
                        }
                    }
                    else if ((tokenName.Length == 1 && 'a' <= tokenName.ToLower()[0] &&
                              'z' >= tokenName.ToLower()[0]) || tokenName.ToLower() == "pi") //var
                    {
                        previous = ElementType.Variable;
                        exp.Push(new Element(ElementType.Variable, tokenName, 0));
                        loc++;
                    }
                    else
                    {
                        throw new Exception("Unknown variable:" + tokens[loc].NameOrValue);
                    }

                    break;
                case -4: //num
                    if (previous == ElementType.Number || previous == ElementType.Variable)
                        throw new Exception("Missing operator between " + tokens[loc - 1] + " and " + tokens[loc]);
                    previous = ElementType.Number;
                    exp.Push(new Element(ElementType.Number, tokens[loc].NameOrValue, 0));
                    loc++;
                    break;
                case -1: //'('|')'
                    if (tokens[loc].Type == TokenType.LeftBracket)
                    {
                        if (previous == ElementType.Number || previous == ElementType.Variable)
                            throw new Exception("Missing operator between " + tokens[loc - 1] + " and " + tokens[loc]);
                        op.Push(OperatorType.LeftBracket);
                        previous = ElementType.Operator;
                    }
                    else if (tokens[loc].Type == TokenType.RightBracket)
                    {
                        if (previous == ElementType.Operator)
                            throw new Exception("Missing operator between " + tokens[loc - 1] + " and " + tokens[loc]);
                        previous = ElementType.Variable;
                        MoveOperatorTo(op, exp, OperatorType.LeftBracket);
                    }

                    loc++;
                    break;
                default:
                    if (previous == ElementType.Operator && tokens[loc].Type == TokenType.Subtract)
                    {
                        tokens[loc].Type = TokenType.Neg;
                        goto jt;
                    }

                    if (previous == ElementType.Operator)
                        throw new Exception("Missing operator between " + tokens[loc - 1] + " and " + tokens[loc]);
                    jt:
                    previous = ElementType.Operator;
                    JudgeOperator(op, exp, tokens[loc].ToOper());
                    loc++;
                    break;
            }

        while (op.Count != 0 && op.Peek() != OperatorType.Start) exp.Push(op.Pop().ToElement());
        return exp.Reverse().ToArray();
    }
    /// <summary>
    /// 词法分析器 陈年老代码 本用于某个脚本解释器
    /// </summary>
    public static Token[] GetTokens(this string script)
    {
        script += '#';
        var loc = 0;
        var tokens = new List<Token>();
        while (loc < script.Length)
        {
            Token t;
            t.NameOrValue = "";
            t.Type = TokenType.Err_UnDefined;
            if (script[loc] == '#') break;
            if (LetterRegex.IsMatch(script[loc] + ""))
            {
                t.NameOrValue += script[loc];
                loc++;
                while (LetterOrNumberOrUnderlineRegex.IsMatch(script[loc] + ""))
                {
                    t.NameOrValue += script[loc];
                    loc++;
                }

                t.Type = TokenType.VariableOrFunction;
            }
            else if (NumberRegex.IsMatch(script[loc] + ""))
            {
                t.NameOrValue += script[loc];
                loc++;
                while (NumberOrPointRegex.IsMatch(script[loc] + ""))
                {
                    t.NameOrValue += script[loc];
                    loc++;
                }

                t.Type = TokenType.Number;
            }
            else if (OperatorRegex.IsMatch(script[loc] + ""))
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
            else if (SpaceOrTabRegex.IsMatch(script[loc] + ""))
            {
                loc++;
                continue;
            }
            else
            {
                throw new Exception("Unknown character:" + script[loc]);
            }

            tokens.Add(t);
        }

        return tokens.ToArray();
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