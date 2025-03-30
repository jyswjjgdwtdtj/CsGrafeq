using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CsGrafeq
{
    internal static class ExpressionComplier
    {
        private static bool[] usedconst;
        public static CompileResult Complie(string Expression)
        {
            DynamicMethod imp = new DynamicMethod("ImpFunction", typeof((bool, bool)), new Type[] { typeof(Interval), typeof(Interval), typeof(double[]) });
            ILGenerator il = imp.GetILGenerator();
            usedconst = new bool['z' - 'a' + 1];
            EmitTokens(il, GetTokens(Expression), FunctionType.Interval);
            il.Emit(OpCodes.Ret);
            IntervalImpFunctionDelegate ic = (IntervalImpFunctionDelegate)imp.CreateDelegate(typeof(IntervalImpFunctionDelegate));

            DynamicMethod isfunc = new DynamicMethod("ImpFunction", typeof((bool, bool)), new Type[] { typeof(IntervalSet), typeof(IntervalSet), typeof(double[]) });
            ILGenerator ilis = isfunc.GetILGenerator();
            EmitTokens(ilis, GetTokens(Expression), FunctionType.IntervalSet);
            ilis.Emit(OpCodes.Ret);
            IntervalSetImpFunctionDelegate isc = (IntervalSetImpFunctionDelegate)isfunc.CreateDelegate(typeof(IntervalSetImpFunctionDelegate));
            DynamicMethod num = new DynamicMethod("NumberFunction", typeof(int), new Type[] { typeof(double), typeof(double), typeof(double[]) });
            ILGenerator ilnum = num.GetILGenerator();
            EmitTokens(ilnum, GetTokens(Expression), FunctionType.Number);
            ilnum.Emit(OpCodes.Ret);
            NumberImpFunctionDelegate nc = (NumberImpFunctionDelegate)num.CreateDelegate(typeof(NumberImpFunctionDelegate));
            ic.Invoke(new Interval(0),new Interval(1),new double['z'-'a'+1]);
            isc.Invoke(new IntervalSet(0), new IntervalSet(1), new double['z' - 'a' + 1]);
            nc.Invoke(1, 2, new double['z' - 'a' + 1]);
            return new CompileResult { IntervalImpFunctionDelegate = ic, NumberImpFunctionDelegate = nc ,IntervalSetImpFunctionDelegate=isc,UsedConstant=usedconst};
        }
        public static CompileResult Complie(ComparedExpression ec)
        {
            DynamicMethod imp = new DynamicMethod("ImpFunction", typeof((bool, bool)), new Type[] { typeof(IntervalSet), typeof(IntervalSet), typeof(double[]) });
            ILGenerator il = imp.GetILGenerator();
            usedconst = new bool['z' - 'a' + 1];
            EmitElements(il,ec.Elements.ToArray(), FunctionType.IntervalSet);
            il.Emit(OpCodes.Ret);
            IntervalSetImpFunctionDelegate ic = (IntervalSetImpFunctionDelegate)imp.CreateDelegate(typeof(IntervalSetImpFunctionDelegate));

            imp = new DynamicMethod("ImpFunction", typeof((bool, bool)), new Type[] { typeof(Interval), typeof(Interval), typeof(double[]) });
            il = imp.GetILGenerator();
            EmitElements(il, ec.Elements.ToArray(), FunctionType.Interval);
            il.Emit(OpCodes.Ret);
            IntervalImpFunctionDelegate icc = (IntervalImpFunctionDelegate)imp.CreateDelegate(typeof(IntervalImpFunctionDelegate));
            DynamicMethod num = new DynamicMethod("NumberFunction", typeof(int), new Type[] { typeof(double), typeof(double), typeof(double[]) });
            ILGenerator ilnum = num.GetILGenerator();
            EmitElements(ilnum, ec.Elements.ToArray(), FunctionType.Number);
            ilnum.Emit(OpCodes.Ret);
            NumberImpFunctionDelegate nc = (NumberImpFunctionDelegate)num.CreateDelegate(typeof(NumberImpFunctionDelegate));
            return new CompileResult { IntervalImpFunctionDelegate = icc, NumberImpFunctionDelegate = nc,IntervalSetImpFunctionDelegate=ic,UsedConstant=usedconst };
        }
        private static readonly Regex letter = new Regex("[a-zA-Z]");
        private static readonly Regex number = new Regex("[0-9]");
        private static readonly Regex oper = new Regex("([/<>+=^*%(),]|-)");
        private static readonly Regex letterOrnumberOr_ = new Regex("[a-zA-Z0-9_]");
        private static readonly Regex numberOrpoint = new Regex("[0-9.]");
        private static readonly Regex spaceOrtab = new Regex(@"([ ]|\t)");
        private static readonly MethodInfoHelper mih = new MethodInfoHelper(typeof(IntervalMath));
        private static readonly MethodInfoHelper mihis = new MethodInfoHelper(typeof(IntervalSetMath));
        private static readonly MethodInfoHelper mihn = new MethodInfoHelper(typeof(NumberMath));
        private static readonly StringBuilder sb = new StringBuilder();
        private static int stacklength = 0;
        internal static string Record
        {
            get { return sb.ToString(); }
        }
        static ExpressionComplier()
        {
        }
        internal static Element[] ParseTokens(Token[] Tokens)
        {
            Stack<OperatorType> op = new Stack<OperatorType>();
            Stack<Element> exp = new Stack<Element>();
            int loc = 0;
            int len = Tokens.Length - 1;
            op.Push(OperatorType.Start);
            ElementType Previous = ElementType.BasicOperator;
            while (loc <= len)
            {
                switch (GetTokenLevel(Tokens[loc]))
                {
                    case -3://var|func
                        if (Previous == ElementType.Number || Previous == ElementType.Variable || Previous == ElementType.Function)
                            throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                        string Tokenname = Tokens[loc].NameOrValue;
                        if (mih.Contains(Tokenname.ToLower()))
                        {//函数
                            Previous = ElementType.Variable;
                            loc += 2;
                            if (Tokens[loc].type == TokenType.RightBracket)
                            {
                                exp.Push(new Element(ElementType.Function, Tokenname, 0));
                                loc++;
                            }
                            else
                            {
                                int bc = 0;
                                int dimcount = 0;
                                List<Token> ts = new List<Token>();
                                while (true)
                                {
                                    if (bc == 0 && Tokens[loc].type == TokenType.Comma)
                                    {
                                        dimcount++;
                                        foreach (var i in ParseTokens(ts.ToArray()))
                                            exp.Push(i);
                                        ts.Clear();
                                        loc++;
                                        continue;
                                    }
                                    if (bc == 0 && Tokens[loc].type == TokenType.RightBracket)
                                    {
                                        dimcount++;
                                        foreach (var i in ParseTokens(ts.ToArray()))
                                            exp.Push(i);
                                        loc++;
                                        break;
                                    }
                                    if (Tokens[loc].type == TokenType.LeftBracket)
                                        bc++;
                                    if (Tokens[loc].type == TokenType.RightBracket)
                                        bc--;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                exp.Push(new Element(ElementType.Function, Tokenname, dimcount));
                            }
                        }
                        else if (Tokenname.Length == 1 && 'a' <= Tokenname.ToLower()[0] && 'z' >= Tokenname.ToLower()[0] || Tokenname.ToLower()=="pi")//var
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
                    case -4://num
                        if (Previous == ElementType.Number || Previous == ElementType.Variable || Previous == ElementType.Function)
                            throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                        Previous = ElementType.Number;
                        exp.Push(new Element(ElementType.Number, Tokens[loc].NameOrValue, 0));
                        loc++;
                        break;
                    case -1://'('|')'
                        if (Tokens[loc].type == TokenType.LeftBracket)
                        {
                            if (Previous == ElementType.Number || Previous == ElementType.Variable)
                                throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                            op.Push(OperatorType.LeftBracket);
                            Previous = ElementType.Operator;
                        }
                        else if (Tokens[loc].type == TokenType.RightBracket)
                        {
                            if (Previous == ElementType.Function || Previous == ElementType.Operator)
                                throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间");
                            Previous = ElementType.Variable;
                            MoveOperatorTo(op, exp, OperatorType.LeftBracket);
                        }
                        loc++;
                        break;
                    default:
                        if (Previous == ElementType.Operator && Tokens[loc].type == TokenType.Subtract)
                        {
                            Tokens[loc].type = TokenType.Neg; goto jt;
                        }
                        if (Previous == ElementType.Function || Previous == ElementType.Operator)
                            throw new Exception("缺少运算符:" + Tokens[loc - 1] + "与" + Tokens[loc] + "之间" + Previous.ToString());
                        jt:
                        Previous = ElementType.Operator;
                        JudgeOperator(op, exp, Tokens[loc].ToOper());
                        loc++;
                        break;
                }
            }
            while (op.Count != 0 && op.Peek() != OperatorType.Start)
            {
                exp.Push(op.Pop().ToElement());
            }
            return exp.Reverse().ToArray();
        }
        private static void EmitElement(ILGenerator IL, Element ele, FunctionType functionType)
        {
            //if (stacklength < 0)
            //throw new Exception("运算符或函数参数数量错误");
            switch (ele.type)
            {
                case ElementType.Number:
                    double d = Double.Parse(ele.NameOrValue);
                    IL.Emit(OpCodes.Ldc_R8, d);
                    if (functionType == FunctionType.Interval)
                        IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double) }));
                    else if (functionType == FunctionType.IntervalSet)
                        IL.Emit(OpCodes.Newobj, typeof(IntervalSet).GetConstructor(new Type[] { typeof(double) }));
                    sb.AppendLine("load number:" + d);
                    stacklength++;
                    break;
                case ElementType.Operator:
                    {
                        if (functionType == FunctionType.Interval)
                        {
                            MethodInfo mf = mih.GetMethod(ele.NameOrValue);
                            //MessageBox.Show("Test");
                            IL.Emit(OpCodes.Call, mf);
                            stacklength -= mf.GetParameters().Length;
                        }
                        else if (functionType == FunctionType.IntervalSet)
                        {
                            MethodInfo mf = mihis.GetMethod(ele.NameOrValue);
                            IL.Emit(OpCodes.Call, mf);
                            stacklength -= mf.GetParameters().Length;
                        }
                        else
                        {
                            if (NumberMath.ToOpcodesDic.ContainsKey(ele.NameOrValue.ToLower()))
                            {
                                IL.Emit(NumberMath.ToOpcodesDic[ele.NameOrValue.ToLower()]);
                                stacklength -= 2;
                            }
                            else if (ele.NameOrValue.ToLower() == "pow")
                            {
                                IL.Emit(OpCodes.Call, typeof(Math).GetMethod("Pow"));
                                stacklength -= 2;
                            }
                            else if (mihn.Contains(ele.NameOrValue))
                            {
                                MethodInfo mf = mihn.GetMethod(ele.NameOrValue);
                                IL.Emit(OpCodes.Call, mf);
                                stacklength -= mf.GetParameters().Length; ;
                            }
                            else
                                throw new Exception("未知函数:"+ele.NameOrValue);
                        }
                        sb.AppendLine("call oper:" + ele.NameOrValue);
                        stacklength++;
                    }
                    break;
                case ElementType.Variable:
                    ele.NameOrValue=ele.NameOrValue.ToLower();
                    if (ele.NameOrValue == "x")
                    { IL.Emit(OpCodes.Ldarg_0); sb.AppendLine("load arg:x"); }
                    else if (ele.NameOrValue == "y")
                    { IL.Emit(OpCodes.Ldarg_1); sb.AppendLine("load arg:y"); }
                    else if (ele.NameOrValue == "e")
                    {
                        IL.Emit(OpCodes.Ldc_R8, Math.E);
                        if (functionType == FunctionType.Interval)
                            IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double) }));
                        if (functionType == FunctionType.IntervalSet)
                            IL.Emit(OpCodes.Newobj, typeof(IntervalSet).GetConstructor(new Type[] { typeof(double) }));
                        sb.AppendLine("load const:e");
                    }else if (ele.NameOrValue == "pi")
                    {
                        IL.Emit(OpCodes.Ldc_R8, Math.PI);
                        if (functionType == FunctionType.Interval)
                            IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double) }));
                        if (functionType == FunctionType.IntervalSet)
                            IL.Emit(OpCodes.Newobj, typeof(IntervalSet).GetConstructor(new Type[] { typeof(double) }));
                        sb.AppendLine("load const:pi");
                    }else if (ele.NameOrValue.Length == 1)
                    {
                        if ('a' <= ele.NameOrValue[0] && ele.NameOrValue[0] <= 'z')
                        {
                            IL.Emit(OpCodes.Ldarg_2);
                            IL.Emit(OpCodes.Ldc_I4, (ele.NameOrValue[0] - 'a'));
                            IL.Emit(OpCodes.Ldelem_R8);
                            if (functionType == FunctionType.Interval)
                                IL.Emit(OpCodes.Newobj, typeof(Interval).GetConstructor(new Type[] { typeof(double) }));
                            if (functionType == FunctionType.IntervalSet)
                            {
                                IL.Emit(OpCodes.Newobj, typeof(IntervalSet).GetConstructor(new Type[] { typeof(double) }));
                                usedconst[ele.NameOrValue[0] - 'a'] = true;
                            }
                            sb.AppendLine("load const:"+ele.NameOrValue);
                        }
                        else
                            throw new Exception("变量不允许:" + ele.NameOrValue);
                    }
                    else
                        throw new Exception("变量不允许:" + ele.NameOrValue);
                    stacklength++;
                    break;
                case ElementType.Function:
                    {
                        if (functionType == FunctionType.Interval)
                        {
                            MethodInfo mf = mih.GetMethod(ele.NameOrValue, ele.arg);
                            if (mf == null)
                                if (mih.Contains(ele.NameOrValue))
                                    throw new Exception("函数参数数量错误:"+ele.NameOrValue);
                                else
                                    throw new Exception("函数不存在:"+ele.NameOrValue);
                            IL.Emit(OpCodes.Call, mf);
                            stacklength -= mf.GetParameters().Length;
                        }
                        else if (functionType == FunctionType.IntervalSet)
                        {
                            MethodInfo mf = mihis.GetMethod(ele.NameOrValue, ele.arg);
                            if (mf == null)
                                if (mihis.Contains(ele.NameOrValue))
                                    throw new Exception("函数参数数量错误:" + ele.NameOrValue);
                                else
                                    throw new Exception("函数不存在:" + ele.NameOrValue);
                            IL.Emit(OpCodes.Call, mf);
                            stacklength -= mf.GetParameters().Length;
                        }
                        else
                        {
                            if (NumberMath.ToMathDic.ContainsKey(ele.NameOrValue.ToLower()))
                            {
                                IL.Emit(OpCodes.Call, typeof(Math).GetMethod(NumberMath.ToMathDic[ele.NameOrValue.ToLower()], GetTypeArray(typeof(double), ele.arg)));
                                stacklength -= 2;
                            }
                            else if (mihn.Contains(ele.NameOrValue))
                            {
                                MethodInfo mf = mihn.GetMethod(ele.NameOrValue, ele.arg);
                                if (mf == null)
                                    if (mihn.Contains(ele.NameOrValue))
                                        throw new Exception("函数参数数量错误:" + ele.NameOrValue);
                                    else
                                        throw new Exception("函数不存在:" + ele.NameOrValue);
                                IL.Emit(OpCodes.Call, mf);
                                stacklength -= mf.GetParameters().Length; ;
                            }
                            else
                                throw new Exception("未知函数:" + ele.NameOrValue);
                        }
                        sb.AppendLine("call func:" + ele.NameOrValue.ToLower());
                        stacklength++;
                    }
                    /*if (ScriptNativeMethodAttr.InputArgAsArrayFunc.Contains(ele.NameOrValue))//以数组传入
                    {
                        LocalBuilder args = IL.DeclareLocal(typeof(object[]));
                        LocalBuilder tmp = IL.DeclareLocal(typeof(object));
                        IL.Emit(OpCodes.Ldc_I4, ele.arg);
                        IL.Emit(OpCodes.Newobj, typeof(object[]).GetConstructor(new Type[] { typeof(int) }));
                        IL.Emit(OpCodes.Stloc, args);
                        for (int i = ele.arg - 1; i >= 0; i--)
                        {
                            IL.Emit(OpCodes.Stloc, tmp);
                            IL.Emit(OpCodes.Ldloc, args);
                            IL.Emit(OpCodes.Ldc_I4, i);
                            IL.Emit(OpCodes.Ldloc, tmp);
                            IL.Emit(OpCodes.Call, typeof(object[]).GetMethod("Set"));
                        }
                        IL.Emit(OpCodes.Ldloc, args);
                        IL.Emit(OpCodes.Call, typeof(ScriptNativeMethod).GetMethod(
                        ele.NameOrValue,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase,
                        null,
                        new Type[] { typeof(object[]) },
                        null));
                        break;
                    }*/
                    break;
                default:
                    throw new Exception("未知Element:" + ele.ToString());
            }
        }
        private static void EmitElements(ILGenerator IL, Element[] eles, FunctionType functionType)
        {
            stacklength = 0;
            sb.Clear();
            foreach (var i in eles)
            {
                EmitElement(IL, i, functionType);
            }
            //if (stacklength != 1)
            //throw new Exception("缺少运算符或函数"+stacklength);
        }
        private static void EmitTokens(ILGenerator IL, Token[] Tokens, FunctionType functionType)
        {
            EmitElements(IL, ParseTokens(Tokens), functionType);
        }
        internal static Token[] GetTokens(string script)//词法分析器
        {
            bool compareoperatorexists = false;
            script += '#';
            int loc = 0;
            List<Token> Tokens = new List<Token>();
            while (loc < script.Length)
            {
                Token t;
                t.NameOrValue = "";
                t.type = TokenType.Err_UnDefined;
                if (script[loc] == '#')
                {
                    break;
                }
                else if (letter.IsMatch(script[loc] + ""))
                {
                    t.NameOrValue += script[loc];
                    loc++;
                    while (letterOrnumberOr_.IsMatch(script[loc] + ""))
                    {
                        t.NameOrValue += script[loc];
                        loc++;
                    }
                    t.type = TokenType.VariableOrFunction;
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
                    t.type = TokenType.Number;
                }
                else if (oper.IsMatch(script[loc] + ""))
                {
                    switch (script[loc])
                    {
                        case '+':
                            t.type = TokenType.Add; break;
                        case '-':
                            t.type = TokenType.Subtract; break;
                        case '*':
                            t.type = TokenType.Multiply; break;
                        case '/':
                            t.type = TokenType.Divide; break;
                        case '^':
                            t.type = TokenType.Pow; break;
                        case '%':
                            t.type = TokenType.Mod; break;
                        case '(':
                            t.type = TokenType.LeftBracket; break;
                        case ')':
                            t.type = TokenType.RightBracket; break;
                        case '>':
                            if (compareoperatorexists)
                                throw new Exception("比较运算符超过一个");
                            t.type = TokenType.Greater;
                            compareoperatorexists = true;
                            break;
                        case '<':
                            if (compareoperatorexists)
                                throw new Exception("比较运算符超过一个");
                            t.type = TokenType.Less;
                            compareoperatorexists = true;
                            break;
                        case '=':
                            if (compareoperatorexists)
                                throw new Exception("比较运算符超过一个");
                            t.type = TokenType.Equal;
                            compareoperatorexists = true;
                            break;
                        case ',':
                            t.type = TokenType.Comma; break;
                        default:
                            throw new Exception("未知符号:"+t.NameOrValue);
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
            if (!compareoperatorexists)
                throw new Exception("需要比较运算符");
            return Tokens.ToArray();
        }
        private static int GetTokenLevel(Token c)
        {
            switch (c.type)
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
                case OperatorType.Equal:
                case OperatorType.Less:
                case OperatorType.Greater:
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
            OperatorType s = opStack.Pop();
            if (s != op)
            {
                expStack.Push(s.ToElement());
                MoveOperatorTo(opStack, expStack, op);
            }
        }
        private static void JudgeOperator(Stack<OperatorType> opStack, Stack<Element> expStack, OperatorType x)
        {
            int xNum = GetOperLevel(x);
            int opNum = GetOperLevel(opStack.Peek());
            if (xNum > opNum || opNum == -1 || (CheckArg(x) == 1 && CheckArg(opStack.Peek()) == 1))
            {
                opStack.Push(x);
                return;
            }
            else
            {
                Element opStr = opStack.Pop().ToElement();
                expStack.Push(new Element(ElementType.Operator, opStr.NameOrValue, 2));
                JudgeOperator(opStack, expStack, x);
                return;
            }
        }
        public enum FunctionType
        {
            Interval,IntervalSet, Number
        }
        #region 枚举/类型
        public enum ElementType
        {
            Variable, Number, Function, BasicOperator, Operator,
        }
        public enum OperatorType
        {
            Add, Subtract, Multiply, Divide, Pow, Mod,
            LeftBracket, RightBracket, Start, Neg,
            Equal, Less, Greater
        }
        public struct Element
        {
            public ElementType type;
            public string NameOrValue;
            public int arg;
            public Element(ElementType type, string nameOrValue, int arg)
            {
                this.type = type;
                this.NameOrValue = nameOrValue;
                this.arg = arg;
            }
            public override string ToString()
            {
                return type.ToString() + " " + NameOrValue + " " + arg;
            }
        }
        internal enum TokenType
        {
            Add, Subtract, Multiply, Divide, Pow, Mod,
            LeftBracket, RightBracket, Start, Neg,
            Equal, Less, Greater,
            VariableOrFunction, Number, Comma,
            Err_UnDefined
        }
        internal struct Token
        {
            public TokenType type;
            public string NameOrValue;
            public override string ToString()
            {
                return type.ToString() + " " + NameOrValue;
            }
        }
        #endregion
        #region 其他函数
        private static Element ToElement(this OperatorType t)
        {
            return new Element(ElementType.Operator, t.ToString(), 0);
        }
        private static OperatorType ToOper(this Token t)
        {
            return (OperatorType)t.type;
        }
        private static Type[] GetTypeArray(Type type, int arg)
        {
            Type[] types = new Type[arg];
            for (int i = 0; i < types.Length; i++)
                types[i] = type;
            return types;
        }
        #endregion
        #region 其他类型
        private class MethodInfoHelper
        {
            public MethodInfoHelper(Type type)
            {
                mis = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            }
            private readonly MethodInfo[] mis;
            public bool Contains(string name)
            {
                name = name.ToLower();
                foreach (MethodInfo mi in mis)
                {
                    if (mi.Name.ToLower() == name) 
                        return true;
                }
                return false;
            }
            public MethodInfo GetMethod(string name)
            {
                name = name.ToLower();
                foreach (MethodInfo mi in mis)
                {
                    if (mi.Name.ToLower() == name) 
                        return mi;
                }
                return null;
            }
            public MethodInfo GetMethod(string name, int argcount)
            {
                name = name.ToLower();
                foreach (MethodInfo mi in mis)
                {
                    if (mi.Name.ToLower() == name && mi.GetParameters().Length == argcount) return mi;
                }
                return null;
            }
        }
        internal struct CompileResult
        {
            public IntervalImpFunctionDelegate IntervalImpFunctionDelegate;
            public IntervalSetImpFunctionDelegate IntervalSetImpFunctionDelegate;
            public NumberImpFunctionDelegate NumberImpFunctionDelegate;
            public bool[] UsedConstant;
            public void Deconstruct(out IntervalImpFunctionDelegate impfunc, out IntervalSetImpFunctionDelegate isfunc, out NumberImpFunctionDelegate numfunc,out bool[] usedconstant)
            {
                impfunc = IntervalImpFunctionDelegate;
                numfunc = NumberImpFunctionDelegate;
                isfunc = IntervalSetImpFunctionDelegate;
                usedconstant = UsedConstant;
            }
        }
        #endregion
    }
}
