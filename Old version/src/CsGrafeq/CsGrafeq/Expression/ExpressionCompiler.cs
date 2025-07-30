using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections;
using System.Linq.Expressions;
using CsGrafeq.Addons.Implicit;

namespace CsGrafeq.Expression
{
    internal static class ExpressionCompiler
    {
        internal static bool[] usedconst;

        public static T Compile<T>(string expression, CompileInfo ci) where T : Delegate
        {
            return Compile<T>(ParseTokens(GetTokens(expression)),ci);
        }
        public static T Compile<T>(Element[] elements, CompileInfo ci) where T : Delegate
        {
            Type mathclass=ci.mathclass;
            MethodInfo mf = typeof(T).GetMethod("Invoke");
            Type returntype=mf.ReturnType;
            ParameterInfo[] pis=mf.GetParameters();
            int len = pis.Length;
            Type[] parametertypes = new Type[len];
            for(int i=0;i<len;i++)
                parametertypes[i] = pis[i].ParameterType;
            if (ci.constantMode == ConstantMode.Array && parametertypes[parametertypes.Length-1] != typeof(double[]))
            {
                throw new Exception("需要为双精度数组");
            }
            if(parametertypes.Length!=
                (ci.constantMode==ConstantMode.Array?ci.parameters.Length+1:ci.parameters.Length))
                throw new Exception("参数数量错误");
            DynamicMethod func = new DynamicMethod("MathFunction", returntype, parametertypes);
            ILGenerator ilr = func.GetILGenerator();
            ILRecorder il=new ILRecorder(ilr);
            usedconst = new bool['z' - 'a' + 1];
            EmitElements(il, elements, new MethodInfoHelper(mathclass),ci.parameters,ci.constantMode==ConstantMode.Array);
            il.Emit(OpCodes.Ret);
            T Tdelegate = (T)func.CreateDelegate(typeof(T));
            try
            {
                object obj = mathclass.GetMethod("New").Invoke(null, new object[] { 0 });
                Tdelegate.DynamicInvoke(obj, obj, new double[26]);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is System.Security.VerificationException)
                {
                    throw new CompileException("编译错误", il.GetRecord());
                }
                else
                    throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
            return (T)func.CreateDelegate(typeof(T));
        }
        private static readonly Regex letter = new Regex("[a-zA-Z]");
        private static readonly Regex number = new Regex("[0-9]");
        private static readonly Regex oper = new Regex("([/<>+=^*%(),|&]|-)");
        private static readonly Regex letterOrnumberOr_ = new Regex("[a-zA-Z0-9_]");
        private static readonly Regex numberOrpoint = new Regex("[0-9.]");
        private static readonly Regex spaceOrtab = new Regex(@"([ ]|\t)");
        private static readonly StringBuilder sb = new StringBuilder();
        private static int stacklength = 0;
        internal static string Record
        {
            get { return sb.ToString(); }
        }
        static ExpressionCompiler()
        {
        }
        internal static Element[] ParseTokens(this Token[] Tokens)
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
                        if (Tokens[Math.Min(loc + 1, len)].type == TokenType.LeftBracket)
                        {
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
        private static void EmitElement(ILRecorder IL, Element ele, MethodInfoHelper mih, char[] param,bool usearray)
        {
            //if (stacklength < 0)
            //throw new Exception("运算符或函数参数数量错误");
            switch (ele.type)
            {
                case ElementType.Number:
                    double d = Double.Parse(ele.NameOrValue);
                    IL.Emit(OpCodes.Ldc_R8, d);
                    IL.Emit(OpCodes.Call,mih.GetMethod("new",1));
                    sb.AppendLine("load number:" + d);
                    stacklength++;
                    break;
                case ElementType.Operator:
                    {
                        MethodInfo mf = mih.GetMethod(ele.NameOrValue);
                        if (mf == null)
                            MessageBox.Show(ele.NameOrValue);
                        IL.Emit(OpCodes.Call, mf);
                        stacklength -= mf.GetParameters().Length;
                        sb.AppendLine("call oper:" + ele.NameOrValue);
                        stacklength++;
                    }
                    break;
                case ElementType.Variable:
                    ele.NameOrValue=ele.NameOrValue.ToLower();
                    if (ele.NameOrValue == "e")
                    {
                        IL.Emit(OpCodes.Ldc_R8, Math.E);
                        IL.Emit(OpCodes.Call, mih.GetMethod("new", 1));
                        sb.AppendLine("load const:e");
                        goto end;
                    }
                    else if (ele.NameOrValue == "pi")
                    {
                        IL.Emit(OpCodes.Ldc_R8, Math.PI);
                        IL.Emit(OpCodes.Call, mih.GetMethod("new", 1));
                        sb.AppendLine("load const:pi");
                        goto end;
                    }
                    if (ele.NameOrValue.Length != 1)
                        throw new Exception("未知变量或常量："+ele.NameOrValue);
                    for(int i = 0; i < param.Length; i++)
                    {
                        if (ele.NameOrValue[0] == param[i])
                        {
                            IL.Emit(OpCodes.Ldarg,i); sb.AppendLine("load arg:" + param[i]);
                            goto end;
                        }
                    }
                    if (usearray)
                    {
                        if ('a' <= ele.NameOrValue[0] && ele.NameOrValue[0] <= 'z')
                        {
                            IL.Emit(OpCodes.Ldarg,param.Length);
                            IL.Emit(OpCodes.Ldc_I4, (ele.NameOrValue[0] - 'a'));
                            IL.Emit(OpCodes.Ldelem_R8);
                            IL.Emit(OpCodes.Call, mih.GetMethod("new", 1));
                            usedconst[ele.NameOrValue[0] - 'a'] = true;
                            sb.AppendLine("load const:" + ele.NameOrValue);
                        }
                        else
                            throw new Exception("常量不允许:" + ele.NameOrValue);
                    }
                    else
                        throw new Exception("常量不允许:" + ele.NameOrValue);
                    end:
                    stacklength++;
                    break;
                case ElementType.Function:
                    {
                        MethodInfo mf = mih.GetMethod(ele.NameOrValue, ele.arg);
                        if (mf == null)
                            if (mih.Contains(ele.NameOrValue))
                                throw new Exception("函数参数数量错误:" + ele.NameOrValue);
                            else
                            {
                                throw new Exception("函数不存在:" + ele.NameOrValue);
                            }
                        IL.Emit(OpCodes.Call, mf);
                        stacklength -= mf.GetParameters().Length;
                        sb.AppendLine("call func:" + ele.NameOrValue.ToLower());
                        stacklength++;
                    }
                    //当遇到可变参数数量函数时：
                    /*if (xxx.Contains(ele.NameOrValue))//以数组传入
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
        internal static void EmitElements(ILRecorder IL, Element[] eles, MethodInfoHelper mih, char[] param,bool usearray)
        {
            stacklength = 0;
            sb.Clear();
            foreach (var i in eles)
            {
                EmitElement(IL, i, mih,param,usearray);
            }
            //if (stacklength != 1)
            //    throw new Exception("缺少运算符或函数"+stacklength);
        }
        internal static Token[] GetTokens(this string script)//词法分析器
        {
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
                            if (script[loc + 1] == '=')
                            {
                                t.type = TokenType.GreaterEqual;
                                loc++;
                            }
                            else
                                t.type = TokenType.Greater;
                            break;
                        case '<':
                            if (script[loc + 1] == '=')
                            {
                                t.type = TokenType.LessEqual;
                                loc++;
                            }
                            else
                                t.type = TokenType.Less;
                            break;
                        case '=':
                            if (script[loc + 1] == '=')
                                loc++;
                            t.type = TokenType.Equal;
                            break;
                        case '|':
                            t.type = TokenType.Union;
                            break;
                        case '&':
                            t.type = TokenType.Intersect;
                            break;
                        case ',':
                            t.type = TokenType.Comma; break;
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
    }
    public class CompileInfo
    {
        internal char[] parameters=new char[] { 'x','y'};
        internal ConstantMode constantMode=ConstantMode.Array;
        internal Type mathclass;
        public CompileInfo(char[] parameters, ConstantMode constantMode, Type mathclass)
        {
            List<char> list = new List<char>();
            foreach (char c in parameters)
            {
                if (c < 'a' || c > 'z')
                    throw new Exception("需为参数名");
                if (list.Contains(c))
                    throw new Exception("参数重定义");
                list.Add(c);
            }
            this.parameters = list.ToArray();
            this.constantMode = constantMode;
            this.mathclass = mathclass;
            MethodInfo mi = mathclass.GetMethod("New");
            if (mi == null)
                throw new Exception("类中不包含New函数");
        }
        public CompileInfo(Type mathclass) { 
            this.mathclass=mathclass;
        }
    }
    public class CompileException : Exception
    {
        public readonly string il;
        public CompileException(string message,string il):base(message)
        {
            this.il = il;
        }
    }
    #region 枚举/类型
    public enum ConstantMode
    {
        None,
        Array
    }
    public enum FunctionType
    {
        Interval, IntervalSet, Number
    }
    public enum ElementType
    {
        Variable, Number, Function, BasicOperator, Operator,
    }
    public enum OperatorType
    {
        Add, Subtract, Multiply, Divide, Pow, Mod,
        LeftBracket, RightBracket, Start, Neg,
        Equal, Less, Greater, LessEqual, GreaterEqual,
        Union, Intersect
    }
    internal enum TokenType
    {
        Add, Subtract, Multiply, Divide, Pow, Mod,
        LeftBracket, RightBracket, Start, Neg,
        Equal, Less, Greater, LessEqual, GreaterEqual, Union, Intersect,
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
    internal struct Element
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
    #endregion
    internal class MethodInfoHelper
    {
        public readonly string TypeName;
        public MethodInfoHelper(Type type)
        {
            TypeName = type.FullName;
            mis = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            MethodInfo cons = GetMethod("new", 1);
            if (cons == null)
                throw new Exception("需要有初始化函数:" + type.Name);
            ParameterInfo[] parameters = cons.GetParameters();
            if (parameters.Length != 1)
                throw new Exception("初始化函数需要为单个参数:" + type.Name);
            if (parameters[0].ParameterType != typeof(double))
                throw new Exception("初始化函数参数需要为double类型:" + type.Name);
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
    internal class ILRecorder
    {
        private ILGenerator IL;
        private StringBuilder sb = new StringBuilder();
        public ILRecorder(ILGenerator il)
        {
            IL = il;
        }
        public void Emit(OpCode op)
        {
            sb.AppendLine(op.Name);
            IL.Emit(op);
        }
        public void Emit(OpCode op, byte b)
        {
            sb.AppendLine(op.Name + " " + b.ToString()); IL.Emit(op, b);
        }
        public void Emit(OpCode op, ConstructorInfo ci)
        {
            sb.AppendLine(op.Name + " " + ci.ToString()); IL.Emit(op, ci);
        }
        public void Emit(OpCode op, double d)
        {
            sb.AppendLine(op.Name + " " + d.ToString()); IL.Emit(op, d);
        }
        public void Emit(OpCode op, FieldInfo fi)
        {
            sb.AppendLine(op.Name + " " + fi.ToString()); IL.Emit(op, fi);
        }
        public void Emit(OpCode op, float f)
        {
            sb.AppendLine(op.Name + " " + f.ToString()); IL.Emit(op, f);
        }
        public void Emit(OpCode op, int i)
        {
            sb.AppendLine(op.Name + " " + i.ToString()); IL.Emit(op, i);
        }
        public void Emit(OpCode op, long l)
        {
            sb.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public void Emit(OpCode op, MethodInfo l)
        {
            if (l == null)
                throw new Exception(nameof(l) + "不能为null");
            sb.AppendLine(op.Name + " " + l.ToString().Split('\r')[0]); IL.Emit(op, l);
        }
        public void Emit(OpCode op, string l)
        {
            sb.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public void Emit(OpCode op, Type l)
        {
            sb.AppendLine(op.Name + " " + l.ToString()); IL.Emit(op, l);
        }
        public string GetRecord()
        {
            return sb.ToString();
        }
    }
}
