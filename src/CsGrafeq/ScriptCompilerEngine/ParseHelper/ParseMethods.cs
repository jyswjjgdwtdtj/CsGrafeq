using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ScriptCompilerEngine.ParseHelper;
using static ScriptCompilerEngine.ParseHelper.ParseHelperRegexp;
using ScriptCompilerEngine.ScriptNative.InternalMethod;
using static ScriptCompilerEngine.ScriptNative.InternalMethod.Method;
using System.Dynamic;
using ScriptCompilerEngine.ScriptNative.ScriptNativeMethod;
using System.Collections;
using Microsoft.VisualBasic;
using System.Configuration;
namespace ScriptCompilerEngine.ParseHelper
{
    internal static class ParseMethods
    {
        public static readonly MethodFinider MethodFinder=new MethodFinider();
        static ParseMethods()
        {
        }
        public static void ParseScript(ILHelper IL,string script)
        {
            ParseBlock(IL,GetToken(script),new Stack<Label>(),new Stack<Label>(),new Stack<Label>());
        }
        private static void ParseBlock(ILHelper IL, Token[][] Block, Stack<Label> ForExitLabels, Stack<Label> DoExitLabels, Stack<Label> IfExitLabels)
        {
            int lineloc = 0;
            while (lineloc < Block.Length)
            {
                Token[] Tokens = Block[lineloc];
                int loc = 0;
                int len = Tokens.Length;
                if (len == 0)
                {
                    lineloc++;
                    continue;
                }
                List<Token> ts = new List<Token>();
                List<Token[]> tss = new List<Token[]>();
                ts.Clear();
                switch (Tokens[loc].Type)
                {
                    case TokenType.Exit:
                    case TokenType.Dim:
                    case TokenType.ReDim:
                    case TokenType.VariableOrFunction:
                    case TokenType.Return:
                        ParseStatement(IL, Tokens,ForExitLabels,DoExitLabels);
                        lineloc++;
                        break;
                    case TokenType.If:
                        if (++loc == len)
                            throw new Exception("If后应为语句");
                        List<Token> iftothen_Tokens = new List<Token>();
                        //match if then (else)
                        while (true)
                        {
                            if (Tokens[loc].Type == TokenType.Then)
                                break;
                            iftothen_Tokens.Add(Tokens[loc]);
                            if (++loc == len)
                                throw new Exception("If后应有Then");
                        }
                        Label exitif = IL.DefineLabel();
                        IfExitLabels.Push(exitif);
                        EmitExpression(IL, iftothen_Tokens.ToArray());
                        IL.Emit(OpCodes.Call, typeof(Method).GetMethod("ObjectToBool"));
                        if (++loc != len)//单行if语句
                        {
                            List<Token> thentoelse = new List<Token>();
                            while (loc != len)
                            {
                                if (Tokens[loc].Type == TokenType.Else)
                                    break;
                                thentoelse.Add(Tokens[loc++]);
                            }
                            List<Token> afterelse = new List<Token>();
                            loc++;
                            while (loc < len)
                            {
                                afterelse.Add(Tokens[loc++]);
                            }
                            if (afterelse.Count == 0)
                            {
                                IL.Emit(OpCodes.Brfalse, exitif);
                                ParseBlock(IL, new Token[][] { thentoelse.ToArray() },ForExitLabels,DoExitLabels,IfExitLabels);
                                IL.MarkLabel(exitif);
                            }
                            else
                            {
                                Label elselabel = IL.DefineLabel();
                                IL.Emit(OpCodes.Brfalse, elselabel);
                                ParseBlock(IL, new Token[][] { thentoelse.ToArray() }, ForExitLabels, DoExitLabels, IfExitLabels);
                                IL.Emit(OpCodes.Br, exitif);
                                IL.MarkLabel(elselabel);
                                ParseBlock(IL, new Token[][] { afterelse.ToArray() }, ForExitLabels, DoExitLabels, IfExitLabels);
                                IL.MarkLabel(exitif);
                            }
                        }
                        else//多行if
                        {
                            int ifcount = 0;
                            List<Token[]> tl = new List<Token[]>();
                            Label nextlabel = IL.DefineLabel();
                            IL.Emit(OpCodes.Brfalse, nextlabel);
                            while (true)
                            {
                                lineloc++;
                                loc = 0;
                                if (lineloc == Block.Length)
                                    throw new Exception();
                                Token[] current = Block[lineloc];
                                if (current.Length == 0)
                                    continue;
                                if (ifcount == 0 && current[0].Type == TokenType.ElseIf)
                                {
                                    loc++;
                                    ParseBlock(IL, tl.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                                    tl.Clear();
                                    IL.Emit(OpCodes.Br, exitif);
                                    IL.MarkLabel(nextlabel);
                                    nextlabel = IL.DefineLabel();
                                    iftothen_Tokens.Clear();
                                    while (true)
                                    {
                                        if (current[loc].Type == TokenType.Then)
                                            break;
                                        iftothen_Tokens.Add(current[loc]);
                                        if (loc == len)
                                        {
                                            throw new Exception("error:no 'then' after 'if'");
                                        }
                                        loc++;
                                    }
                                    EmitExpression(IL, iftothen_Tokens.ToArray());
                                    IL.Emit(OpCodes.Ldc_I4_1);//true
                                    IL.Emit(OpCodes.Ceq);//check equality
                                    IL.Emit(OpCodes.Brfalse, nextlabel);
                                    continue;
                                }
                                if (ifcount == 0 && current[0].Type == TokenType.Else)
                                {
                                    ParseBlock(IL, tl.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                                    tl.Clear();
                                    IL.Emit(OpCodes.Br, exitif);
                                    IL.MarkLabel(nextlabel);
                                    nextlabel = IL.DefineLabel();
                                    continue;
                                }
                                if (ifcount == 0 && current[0].Type == TokenType.End && current[1].Type == TokenType.If)
                                {
                                    ParseBlock(IL, tl.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                                    IL.MarkLabel(nextlabel);
                                    IL.MarkLabel(exitif);
                                    break;
                                }
                                tl.Add(current);
                                if (current.Length < 2)
                                    continue;
                                if (current[0].Type == TokenType.If)
                                {
                                    if (new List<Token>(current).IndexOf(new Token() { NameOrValue = "Then",Type = TokenType.Then }) != current.Length - 1)
                                        ifcount++;
                                }
                                if (current[0].Type == TokenType.End && current[1].Type == TokenType.If)
                                    ifcount--;
                            }
                        }
                        IfExitLabels.Pop();
                        lineloc++;
                        break;
                    case TokenType.For:
                        loc++;
                        Label exitfor = IL.DefineLabel();
                        if (Tokens[loc].Type == TokenType.Each)
                        {
                            loc++;
                            if (Tokens[loc].Type == TokenType.VariableOrFunction)
                            {
                                string name = Tokens[loc].NameOrValue;
                                loc++;
                                if (Tokens[loc++].Type == TokenType.In)
                                {
                                    ts.Clear();
                                    while (loc < Tokens.Length)
                                        ts.Add(Tokens[loc++]);
                                    EmitExpression(IL, ts.ToArray());
                                    LocalBuilder enumerator = IL.DeclareLocal(typeof(IEnumerator));
                                    LocalBuilder var = IL.GetLocal(name);
                                    Label startenum = IL.DefineLabel();
                                    Label startst = IL.DefineLabel();
                                    ForExitLabels.Push(exitfor);
                                    IL.Emit(OpCodes.Castclass, typeof(IEnumerable));
                                    IL.Emit(OpCodes.Callvirt, typeof(IEnumerable).GetMethod("GetEnumerator"));
                                    IL.SetVariable(enumerator);
                                    IL.Emit(OpCodes.Br, startenum);
                                    IL.MarkLabel(startst);
                                    IL.LoadVariable(enumerator);
                                    IL.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("get_Current"));
                                    IL.SetVariable(var);
                                    tss.Clear();
                                    int forcount = 0;
                                    while (true)
                                    {
                                        lineloc++;
                                        if (lineloc == Block.Length)
                                            throw new Exception();
                                        if (Block[lineloc].Length == 0)
                                            continue;
                                        if (forcount == 0 && Block[lineloc][0].Type == TokenType.Next)
                                        {
                                            ParseBlock(IL, tss.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                                            break;
                                        }
                                        if (Block[lineloc][0].Type == TokenType.For)
                                            forcount++;
                                        if (Block[lineloc][0].Type == TokenType.Next)
                                            forcount--;
                                        tss.Add(Block[lineloc]);
                                    }
                                    IL.MarkLabel(startenum);
                                    IL.LoadVariable (enumerator);
                                    IL.Emit(OpCodes.Callvirt, typeof(IEnumerator).GetMethod("MoveNext"));
                                    IL.Emit(OpCodes.Brtrue, startst);
                                    IL.MarkLabel(exitfor);
                                    ForExitLabels.Pop();
                                }
                                else
                                    throw new Exception();
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else if (Tokens[loc].Type == TokenType.VariableOrFunction)
                        {
                            string varname = Tokens[loc].NameOrValue;
                            LocalBuilder local = IL.GetLocal(varname);
                            LocalBuilder min = IL.DeclareLocal(typeof(double));
                            LocalBuilder max = IL.DeclareLocal(typeof(double));
                            LocalBuilder step = IL.DeclareLocal(typeof(double));
                            Label addstep = IL.DefineLabel();
                            Label startst = IL.DefineLabel();
                            loc++;
                            if (Tokens[loc++].Type == TokenType.Equal)
                            {
                                ForExitLabels.Push(exitfor);
                                //获取首尾和步长
                                ts.Clear();
                                while (true)
                                {
                                    if (Tokens[loc].Type == TokenType.To)
                                        break;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                loc++;
                                EmitExpression(IL, ts.ToArray());//载入初始值
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("NumberToDouble"));
                                IL.SetVariable(min);
                                ts.Clear();
                                while (true)
                                {
                                    if (loc == Tokens.Length)
                                        break;
                                    if (Tokens[loc].Type == TokenType.Step)
                                        break;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                EmitExpression(IL, ts.ToArray());
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("NumberToDouble"));
                                IL.SetVariable(max);
                                ts.Clear();
                                if (loc == Tokens.Length)//无step
                                {
                                    IL.Emit(OpCodes.Ldc_R8, 1d);
                                    IL.SetVariable(step);
                                }
                                else
                                {
                                    loc++;
                                    while (true)
                                    {
                                        if (loc == Tokens.Length)
                                            break;
                                        ts.Add(Tokens[loc]);
                                        loc++;
                                    }
                                    EmitExpression(IL, ts.ToArray());
                                    IL.Emit(OpCodes.Call, typeof(Method).GetMethod("NumberToDouble"));
                                    IL.SetVariable(step);
                                }
                                //结束获取
                                //初始化
                                IL.LoadVariable(min);
                                IL.Emit(OpCodes.Box, typeof(double));
                                IL.SetVariable(local);
                                IL.Emit(OpCodes.Br, addstep);
                                IL.MarkLabel(startst);
                                //获取执行的语句
                                tss.Clear();
                                int forcount = 0;
                                while (true)
                                {
                                    lineloc++;
                                    if (lineloc == Block.Length)
                                        throw new Exception();
                                    if (Block[lineloc].Length == 0)
                                        continue;
                                    if (forcount == 0 && Block[lineloc][0].Type == TokenType.Next)
                                    {
                                        ParseBlock(IL, tss.ToArray(),ForExitLabels,DoExitLabels,IfExitLabels);
                                        break;
                                    }
                                    if (Block[lineloc][0].Type == TokenType.For)
                                        forcount++;
                                    if (Block[lineloc][0].Type == TokenType.Next)
                                        forcount--;
                                    tss.Add(Block[lineloc]);
                                }
                                //加步长
                                IL.LoadVariable(min);
                                IL.LoadVariable(step);
                                IL.Emit(OpCodes.Add);
                                IL.Emit(OpCodes.Dup);
                                IL.SetVariable(min);
                                IL.SetVariable(local);
                                IL.MarkLabel(addstep);
                                IL.LoadVariable(min);
                                IL.LoadVariable(max);
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("LessEqualNumber"));//cil不支持小于等于
                                IL.Emit(OpCodes.Brtrue, startst);
                                IL.MarkLabel(exitfor);
                                ForExitLabels.Pop();
                            }

                        }
                        lineloc++;
                        break;
                    case TokenType.Do:
                        loc++;
                        Label doststart = IL.DefineLabel();
                        Label doexit = IL.DefineLabel();
                        Label docondition = IL.DefineLabel();
                        DoExitLabels.Push(doexit);
                        int doflag = 0;//0:nothing 1:until 2:while
                        int loopflag = 0;
                        if (loc != len)//有while|until
                            if (Tokens[loc].Type == TokenType.Until)
                                doflag = 1;
                            else if (Tokens[loc].Type == TokenType.While)
                                doflag = 2;
                            else
                                throw new Exception();
                        else
                            doflag = 0;
                        loc++;
                        ts.Clear();
                        tss.Clear();
                        if (doflag != 0)
                        {
                            while (loc < len)
                                ts.Add(Tokens[loc++]);
                        }
                        int docount = 0;
                        while (true)
                        {
                            lineloc++;
                            if (lineloc == Block.Length)
                                throw new Exception();
                            if (Block[lineloc].Length == 0)
                                continue;
                            if (docount == 0 && Block[lineloc][0].Type == TokenType.Loop)
                            {
                                break;
                            }
                            if (Block[lineloc][0].Type == TokenType.Do)
                                docount++;
                            if (Block[lineloc][0].Type == TokenType.Loop)
                                docount--;
                            tss.Add(Block[lineloc]);
                        }
                        loc = 1;
                        Token[] loopline = Block[lineloc];
                        len = loopline.Length;
                        if (loc != len)
                            if (loopline[loc].Type == TokenType.Until)
                                loopflag = 1;
                            else if (loopline[loc].Type == TokenType.While)
                                loopflag = 2;
                            else
                                throw new Exception();
                        else
                            loopflag = 0;
                        loc++;
                        if (loopflag != 0)
                        {
                            ts.Clear();
                            while (loc < len)
                                ts.Add(loopline[loc++]);
                        }
                        if (doflag != 0 && loopflag != 0)
                            throw new Exception();
                        if (doflag != 0)
                        {
                            IL.Emit(OpCodes.Br, docondition);
                            IL.MarkLabel(doststart);
                            ParseBlock(IL, tss.ToArray(),ForExitLabels,DoExitLabels,IfExitLabels);
                            IL.MarkLabel(docondition);
                            EmitExpression(IL, ts.ToArray());
                            IL.Emit(OpCodes.Ldc_I4_1);
                            IL.Emit(OpCodes.Ceq);
                            if (doflag == 1)
                                IL.Emit(OpCodes.Brfalse, doststart);
                            else
                                IL.Emit(OpCodes.Brtrue, doststart);
                        }
                        else if (loopflag != 0)
                        {
                            IL.MarkLabel(doststart);
                            ParseBlock(IL, tss.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                            EmitExpression(IL, ts.ToArray());
                            IL.Emit(OpCodes.Ldc_I4_1);
                            IL.Emit(OpCodes.Ceq);
                            if (loopflag == 1)
                                IL.Emit(OpCodes.Brfalse, doststart);
                            else
                                IL.Emit(OpCodes.Brtrue, doststart);
                        }
                        else
                        {
                            IL.MarkLabel(doststart);
                            ParseBlock(IL, tss.ToArray(), ForExitLabels, DoExitLabels, IfExitLabels);
                            IL.Emit(OpCodes.Br, doststart);
                        }
                        IL.MarkLabel(doexit);
                        DoExitLabels.Pop();
                        lineloc++;
                        break;
                    default:
                        throw new Exception(Tokens[loc].ToString());
                }
            }
        }
        private static void ParseStatement(ILHelper IL, Token[] Tokens,Stack<Label> ForExitLabels,Stack<Label> DoExitLabels)//处理不包括结构的语句
        {
            int loc = 0;
            int len = Tokens.Length;
            List<Token> ts = new List<Token>();
            MethodInfo NumberToInt = ExMethods.GetMethodInfo<int, object>(Method.NumberToInt);
            switch (Tokens[loc].Type)
            {
                case TokenType.Dim:
                    if (++loc == len)
                    {
                        throw new Exception("Dim后应为变量");
                    }
                    while (true)
                    {
                        if (loc == len)
                            break;
                        if (Tokens[loc].Type == TokenType.VariableOrFunction)
                        {
                            string tn = Tokens[loc].NameOrValue;
                            if (IL.ContainsVariable(tn)||IsConstant(tn))
                                throw new Exception("变量重定义");
                            LocalBuilder lb = IL.DeclareLocal(typeof(object), tn);
                            if (++loc == len)
                            {
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("GetEmpty"));
                                IL.SetVariable(tn);
                                break;
                            }
                            if (Tokens[loc].Type == TokenType.LeftBracket)//数组
                            {
                                loc++;
                                if (Tokens[loc].Type == TokenType.RightBracket)
                                    throw new Exception("不可申明0维数组");
                                ts.Clear();
                                int bracketcount = 0;
                                int dimcount = 0;
                                while (true)
                                {
                                    if (Tokens[loc].Type == TokenType.Comma && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        dimcount++;
                                        loc++;
                                        continue;
                                    }
                                    if (Tokens[loc].Type == TokenType.RightBracket && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        dimcount++;
                                        loc++;
                                        break;
                                    }
                                    if (Tokens[loc].Type == TokenType.LeftBracket)
                                        bracketcount++;
                                    else if (Tokens[loc].Type == TokenType.RightBracket && bracketcount != 0)
                                        bracketcount--;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                Type[] tp = typeof(int).GetArray(dimcount);
                                ConstructorInfo ci = GetTypeOfObjectArray(dimcount).GetConstructor(tp);
                                IL.Emit(OpCodes.Newobj, ci);
                                IL.SetVariable(tn);
                                IL.LoadVariable(tn);
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("SetArrayEmpty"));
                            }
                            else
                            {
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("GetEmpty"));
                                IL.SetVariable (tn);
                            }
                        }
                        else if (Tokens[loc].Type == TokenType.Comma)
                        {
                            if (++loc == len)
                            {
                                throw new Exception();
                            }
                        }
                    }
                    break;
                case TokenType.ReDim:
                    loc++;
                    LocalBuilder lbb;
                    if (Tokens[loc].Type == TokenType.Preserve)
                    {
                        loc++;
                        if (Tokens[loc].Type == TokenType.VariableOrFunction)
                        {
                            LocalBuilder newarr = IL.DeclareLocal(typeof(object));
                            string tname = Tokens[loc].NameOrValue;
                            loc++;
                            if (Tokens[loc].Type == TokenType.LeftBracket)//数组
                            {
                                loc++;
                                if (Tokens[loc].Type == TokenType.RightBracket)
                                    throw new Exception("不可申明0维数组");
                                ts.Clear();
                                int bracketcount = 0;
                                int dimcount = 0;
                                while (true)
                                {
                                    if (Tokens[loc].Type == TokenType.Comma && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        dimcount++;
                                        loc++;
                                        continue;
                                    }
                                    if (Tokens[loc].Type == TokenType.RightBracket && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        dimcount++;
                                        loc++;
                                        break;
                                    }
                                    if (Tokens[loc].Type == TokenType.LeftBracket)
                                        bracketcount++;
                                    else if (Tokens[loc].Type == TokenType.RightBracket && bracketcount != 0)
                                        bracketcount--;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                Type[] tp = typeof(int).GetArray(dimcount);
                                ConstructorInfo ci = GetTypeOfObjectArray(dimcount).GetConstructor(tp);
                                IL.Emit(OpCodes.Newobj, ci);
                                IL.SetVariable(newarr);
                                IL.LoadVariable(newarr);
                                IL.Emit(OpCodes.Call, typeof(Method).GetMethod("SetArrayEmpty"));
                                IL.LoadVariable(tname);
                                IL.Emit(OpCodes.Castclass, typeof(Array));
                                IL.LoadVariable(newarr);
                                IL.Emit(OpCodes.Castclass, typeof(Array));
                                IL.Emit(OpCodes.Call, typeof(Microsoft.VisualBasic.CompilerServices.Utils).GetMethod("CopyArray"));
                                IL.SetVariable(tname);
                            }
                            else
                                throw new Exception();
                        }
                        else
                            throw new Exception();
                    }
                    else if (Tokens[loc].Type == TokenType.VariableOrFunction)
                    {
                        lbb = IL.GetLocal(Tokens[loc].NameOrValue);
                        loc++;
                        if (Tokens[loc].Type == TokenType.LeftBracket)//数组
                        {
                            loc++;
                            if (Tokens[loc].Type == TokenType.RightBracket)
                                throw new Exception("不可申明0维数组");
                            ts.Clear();
                            int bracketcount = 0;
                            int dimcount = 0;
                            while (true)
                            {
                                if (Tokens[loc].Type == TokenType.Comma && bracketcount == 0)
                                {
                                    EmitExpression(IL, ts.ToArray());
                                    IL.Emit(OpCodes.Call, NumberToInt);
                                    ts.Clear();
                                    dimcount++;
                                    loc++;
                                    continue;
                                }
                                if (Tokens[loc].Type == TokenType.RightBracket && bracketcount == 0)
                                {
                                    EmitExpression(IL, ts.ToArray());
                                    IL.Emit(OpCodes.Call, NumberToInt);
                                    ts.Clear();
                                    dimcount++;
                                    loc++;
                                    break;
                                }
                                if (Tokens[loc].Type == TokenType.LeftBracket)
                                    bracketcount++;
                                else if (Tokens[loc].Type == TokenType.RightBracket && bracketcount != 0)
                                    bracketcount--;
                                ts.Add(Tokens[loc]);
                                loc++;
                            }
                            Type[] tp = typeof(int).GetArray(dimcount);
                            ConstructorInfo ci = GetTypeOfObjectArray(dimcount).GetConstructor(tp);
                            IL.Emit(OpCodes.Newobj, ci);
                            IL.SetVariable(Tokens[loc].NameOrValue);
                            IL.LoadVariable(Tokens[loc].NameOrValue);
                            IL.Emit(OpCodes.Call, typeof(Method).GetMethod("SetArrayEmpty"));

                        }
                        else
                            throw new Exception();
                    }
                    else
                        throw new Exception();
                    break;
                case TokenType.VariableOrFunction:
                    string Tokenname = Tokens[loc].NameOrValue;
                    loc++;
                    switch (Tokens[loc].Type)
                    {
                        case TokenType.Equal:
                            if (IL.ContainsVariable(Tokenname))
                            {
                                List<Token> exp = new List<Token>();
                                while (++loc != len)
                                {
                                    exp.Add(Tokens[loc]);
                                }
                                EmitExpression(IL, exp.ToArray());
                                IL.SetVariable(Tokenname);
                            }
                            else
                            {
                                throw new Exception();
                            }
                            break;
                        case TokenType.LeftBracket:
                            int bc = 0;
                            bool expflag = false;
                            int dimcount = 0;
                            loc++;
                            while (true)
                            {
                                if (Tokens[loc].Type == TokenType.Comma && bc == 0)
                                    dimcount++;
                                if (bc == 0 && Tokens[loc].Type == TokenType.RightBracket)
                                {
                                    loc++;
                                    if (loc == len)
                                    {
                                        expflag = true;
                                        break;
                                    }
                                    expflag = Tokens[loc].Type != TokenType.Equal;
                                    dimcount++;
                                    break;
                                }
                                if (Tokens[loc].Type == TokenType.RightBracket)
                                    bc--;
                                else if (Tokens[loc].Type == TokenType.LeftBracket)
                                    bc++;
                                loc++;
                            }
                            if (expflag)
                            {
                                EmitExpression(IL, Tokens);
                                IL.Emit(OpCodes.Pop);
                                break;
                            }
                            //数组
                            if (IL.ContainsVariable(Tokenname))//数组
                            {
                                ts.Clear();
                                IL.LoadVariable(Tokenname);
                                IL.Emit(OpCodes.Castclass,GetTypeOfObjectArray(dimcount));
                                int bracketcount = 0;
                                loc = 2;
                                while (true)
                                {
                                    if (Tokens[loc].Type == TokenType.Comma && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        loc++;
                                        continue;
                                    }
                                    if (Tokens[loc].Type == TokenType.RightBracket && bracketcount == 0)
                                    {
                                        EmitExpression(IL, ts.ToArray());
                                        IL.Emit(OpCodes.Call, NumberToInt);
                                        ts.Clear();
                                        loc++;
                                        break;
                                    }
                                    if (Tokens[loc].Type == TokenType.LeftBracket)
                                        bracketcount++;
                                    else if (Tokens[loc].Type == TokenType.RightBracket && bracketcount != 0)
                                        bracketcount--;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                loc++;
                                while (loc < len)
                                {
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                EmitExpression(IL, ts.ToArray());
                                ts.Clear();
                                Type[] tps = new Type[len + 1];
                                for (int i = 0; i < len; i++)
                                    tps[i] = typeof(int);
                                tps[len] = typeof(object);
                                IL.Emit(OpCodes.Call, GetTypeOfObjectArray(dimcount).GetMethod("Set"));
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case TokenType.Exit:
                    loc++;
                    if (loc == Tokens.Length)
                        throw new Exception();
                    switch (Tokens[loc].Type)
                    {
                        case TokenType.For:
                            IL.Emit(OpCodes.Br, ForExitLabels.Peek());
                            break;
                        case TokenType.Do:
                            IL.Emit(OpCodes.Br, DoExitLabels.Peek());
                            break;
                        default:
                            throw new Exception();
                    }
                    break;
                case TokenType.Return:
                    loc++;
                    List<Token> tos= new List<Token>();
                    for(int i = 1; i < Tokens.Length; i++)
                    {
                        tos.Add(Tokens[i]);
                    }
                    EmitExpression(IL,tos.ToArray());
                    IL.Emit(OpCodes.Ret);
                    break;
                default:
                    throw new Exception();
            }
        }
        public static void EmitExpression(ILHelper IL, Token[] Tokens)
        {
            foreach(var i in GetElementOfExpression(IL,Tokens))
            {
                EmitElement(IL,i);
            }
        }
        private static void EmitElement(ILHelper IL,Element ele)
        {
            switch (ele.Type)
            {
                case ElementType.Number:
                    double d = Double.Parse(ele.NameOrValue);
                    if (d == (int)d)
                    {
                        IL.Emit(OpCodes.Ldc_I8, (long)d);
                        IL.Emit(OpCodes.Box, typeof(long));
                    }
                    else
                    {
                        IL.Emit(OpCodes.Ldc_R8, d);
                        IL.Emit(OpCodes.Box, typeof(double));
                    }
                    break;
                case ElementType.String:
                    IL.Emit(OpCodes.Ldstr, ele.NameOrValue);
                    break;
                case ElementType.Operator:
                    IL.Emit(OpCodes.Call, MethodFinder.GetMethod(ele.NameOrValue));
                    break;
                case ElementType.Variable:
                    if (IL.ContainsVariable(ele.NameOrValue))
                    {
                        IL.LoadVariable(ele.NameOrValue);
                    }
                    else
                    {
                        object constvalue = GetConstantValue(ele.NameOrValue);
                        if (constvalue is bool)
                        {
                            if ((bool)constvalue)
                                IL.Emit(OpCodes.Ldc_I4, 1);
                            else
                                IL.Emit(OpCodes.Ldc_I4, 0);
                            IL.Emit(OpCodes.Box, typeof(bool));
                        }
                        else
                            throw new Exception();
                    }
                    break;
                case ElementType.Function:
                    if (ele.ArgCount == -1)
                        IL.Emit(OpCodes.Ldstr, ele.NameOrValue);
                    else
                    {
                        if (ScriptNativeMethod.InputArgAsArrayFunc.Contains(ele.NameOrValue.ToLower()))//以数组传入
                        {
                            LocalBuilder args = IL.DeclareLocal(typeof(object[]));
                            LocalBuilder tmp = IL.DeclareLocal(typeof(object));
                            IL.Emit(OpCodes.Ldc_I4, ele.ArgCount);
                            IL.Emit(OpCodes.Newobj, typeof(object[]).GetConstructor(new Type[] { typeof(int) }));
                            IL.SetVariable(args);
                            for (int i = ele.ArgCount - 1; i >= 0; i--)
                            {
                                IL.SetVariable(tmp);
                                IL.LoadVariable(args);
                                IL.Emit(OpCodes.Ldc_I4, i);
                                IL.LoadVariable(tmp);
                                IL.Emit(OpCodes.Call, typeof(object[]).GetMethod("Set"));
                            }
                            IL.LoadVariable(args);
                            IL.Emit(OpCodes.Call, MethodFinder.GetMethod(
                            ele.NameOrValue,
                            new Type[] { typeof(object[]) }));
                            break;
                        }
                        var mt = MethodFinder.GetMethod(
                            ele.NameOrValue,
                            typeof(object).GetArray(ele.ArgCount));
                        if (mt != null)
                        {
                            IL.Emit(OpCodes.Call, mt);
                            break;
                        }
                        throw new Exception();
                    }
                    break;
                case ElementType.LateBingdingArgs:
                    LocalBuilder argarr = IL.DeclareLocal(typeof(object[]));
                    LocalBuilder obj = IL.DeclareLocal(typeof(object));
                    IL.Emit(OpCodes.Ldc_I4, ele.ArgCount);
                    IL.Emit(OpCodes.Newobj, typeof(object[]).GetConstructor(new Type[] { typeof(int) }));
                    IL.SetVariable(argarr);
                    for (int i = ele.ArgCount - 1; i >= 0; i--)
                    {
                        IL.SetVariable(obj);
                        IL.LoadVariable(argarr);
                        IL.Emit(OpCodes.Ldc_I4, i);
                        IL.LoadVariable(obj);
                        IL.Emit(OpCodes.Call, typeof(object[]).GetMethod("Set"));
                    }
                    IL.LoadVariable(argarr);
                    IL.Emit(OpCodes.Call, typeof(Method).GetMethod("LateGetWithArgs"));
                    break;
            }
        }
        private static Element[] GetElementOfExpression(ILHelper IL, Token[] Tokens)
        {
            Stack<OperatorType> op = new Stack<OperatorType>();
            Stack<Element> exp = new Stack<Element>();
            int loc = 0;
            int len = Tokens.Length - 1;
            op.Push(OperatorType.Start);
            bool subcanbeneg = true;
            bool previsobject = false;
            while (loc <= len)
            {
                switch (GetTokenLevel(Tokens[loc]))
                {
                    case -3://var|func
                        string Tokenname = Tokens[loc].NameOrValue;
                        if ((
                            MethodFinder.TryGetMethod(Tokenname,out _)
                            && Tokens[Math.Min(loc + 1, len)].Type ==TokenType.LeftBracket))
                        {//函数
                            loc += 2;
                            if (Tokens[loc].Type == TokenType.RightBracket)
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
                                    if (bc == 0 && Tokens[loc].Type == TokenType.Comma)
                                    {
                                        dimcount++;
                                        foreach (var i in GetElementOfExpression(IL, ts.ToArray()))
                                            exp.Push(i);
                                        ts.Clear();
                                        loc++;
                                        continue;
                                    }
                                    if (bc == 0 && Tokens[loc].Type == TokenType.RightBracket)
                                    {
                                        dimcount++;
                                        foreach (var i in GetElementOfExpression(IL, ts.ToArray()))
                                            exp.Push(i);
                                        loc++;
                                        subcanbeneg = false;
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
                        else if ((IL.ContainsVariable(Tokenname) || IsConstant(Tokenname)))//var
                        {
                            exp.Push(new Element(ElementType.Variable, Tokens[loc].NameOrValue, 0));
                            loc++;
                        }
                        else if (Tokens[Math.Min(loc + 1, len)].Type == TokenType.LeftBracket)
                        {
                            exp.Push(new Element(ElementType.LateBingdingFunc, Tokenname, -1));
                            loc++;
                        }
                        else
                        {
                            throw new Exception(Tokens[loc].NameOrValue);
                        }
                        subcanbeneg = false;
                        previsobject = true;
                        break;
                    case -4://num
                        exp.Push(new Element(ElementType.Number, Tokens[loc].NameOrValue, 0));
                        loc++;
                        subcanbeneg = false;
                        previsobject = false;
                        break;
                    case -5://string
                        exp.Push(new Element(ElementType.String, Tokens[loc].NameOrValue, 0));
                        loc++;
                        subcanbeneg = false;
                        previsobject = false;
                        break;
                    case -1://'('|')'
                        if (Tokens[loc].Type == TokenType.LeftBracket)
                        {
                            if (previsobject)//为数组或函数对象
                            {
                                loc++;
                                if (Tokens[loc].Type == TokenType.RightBracket)
                                {
                                    previsobject = true;
                                    exp.Push(new Element(ElementType.LateBingdingArgs, "", 0));
                                    loc++;
                                    subcanbeneg = false;
                                    break;
                                }
                                int bc = 0;
                                int dimcount = 0;
                                List<Token> ts = new List<Token>();
                                while (true)
                                {
                                    if (bc == 0 && Tokens[loc].Type == TokenType.Comma)
                                    {
                                        dimcount++;
                                        foreach (var i in GetElementOfExpression(IL, ts.ToArray()))
                                            exp.Push(i);
                                        ts.Clear();
                                        loc++;
                                        continue;
                                    }
                                    if (bc == 0 && Tokens[loc].Type == TokenType.RightBracket)
                                    {
                                        dimcount++;
                                        foreach (var i in GetElementOfExpression(IL, ts.ToArray()))
                                            exp.Push(i);
                                        loc++;
                                        subcanbeneg = false;
                                        break;
                                    }
                                    if (Tokens[loc].Type == TokenType.LeftBracket)
                                        bc++;
                                    if (Tokens[loc].Type == TokenType.RightBracket)
                                        bc--;
                                    ts.Add(Tokens[loc]);
                                    loc++;
                                }
                                previsobject = true;
                                exp.Push(new Element(ElementType.LateBingdingArgs, "", dimcount));
                                break;
                            }
                            else
                            {
                                op.Push(OperatorType.LeftBracket);
                                previsobject = false;
                                subcanbeneg = true;
                            }
                        }
                        else if (Tokens[loc].Type == TokenType.RightBracket)
                        {
                            MoveOperatorTo(op, exp, OperatorType.LeftBracket);
                            subcanbeneg = false;
                            previsobject = true;
                        }
                        loc++;
                        break;
                    default:
                        if (subcanbeneg && Tokens[loc].Type == TokenType.Subtract)
                            Tokens[loc].Type = TokenType.Neg;
                        JudgeOperator(op, exp, Tokens[loc].ToOper());
                        subcanbeneg = true;
                        loc++;
                        previsobject = false;
                        break;
                }
            }
            while (op.Count != 0 && op.Peek() != OperatorType.Start)
            {
                exp.Push(op.Pop().ToElement());
            }
            return exp.Reverse().ToArray();
        }
        private static Token[][] GetToken(string script)
        {
            List<Token[]> Block= new List<Token[]>();
            foreach(var i in script.Replace("\r\n", "\r").Replace("\n", "\r").Split('\r'))
            {
                Block.Add(GetTokenOfLine(i));
            }
            return Block.ToArray();
        }
        private static Token[] GetTokenOfLine(string script)//词法分析器
        {
            script = Regexp_Rem.Replace(script, "");
            script += '#';
            int loc = 0;
            List<Token> Tokens = new List<Token>();
            int len= script.Length;
            while (loc < script.Length)
            {
                Token t;
                t.NameOrValue = "";
                t.Type = TokenType.None;
                if (script[loc] == '#')
                {
                    break;
                }
                else if (Regexp_LetterChar.IsMatch(script[loc].CS()))
                {
                    t.NameOrValue = script[loc].CS();
                    loc++;
                    while (Regexp_Variable.IsMatch(t.NameOrValue + script[loc]))
                    {
                        t.NameOrValue += script[loc];
                        loc++;
                    }
                    if (ReservedWords.Contain(ref t.NameOrValue))
                        t.Type = (TokenType)Enum.Parse(typeof(TokenType), t.NameOrValue);
                    else
                        t.Type = TokenType.VariableOrFunction;
                }
                else if (Regexp_NumberChar.IsMatch(script[loc].CS()))
                {
                    t.NameOrValue=script[loc].CS();
                    loc++;
                    while (Regexp_Number.IsMatch(t.NameOrValue + script[loc]))
                    {
                        t.NameOrValue += script[loc];
                        loc++;
                    }
                    t.Type = TokenType.Number;
                }
                else if (Regexp_Operator.IsMatch(script[loc].CS()))
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
                            t.Type = TokenType.Pow; break;
                        case '(':
                            t.Type = TokenType.LeftBracket; break;
                        case ')':
                            t.Type = TokenType.RightBracket; break;
                        case '<':
                            if (script[loc + 1] == '=')
                            {
                                loc++;
                                t.Type = TokenType.LessEqual;
                            }
                            else if (script[loc + 1] == '>')
                            {
                                loc++;
                                t.Type = TokenType.NotEqual;
                            }
                            else
                            {
                                t.Type = TokenType.LessThan;
                            }
                            break;
                        case '>':
                            if (script[loc + 1] == '=')
                            {
                                loc++;
                                t.Type = TokenType.GreaterEqual;
                            }
                            else
                            {
                                t.Type = TokenType.GreaterThan;
                            }
                            break;
                        case '=':
                            t.Type = TokenType.Equal; break;
                        case '"':
                            StringBuilder r= new StringBuilder();
                            loc++;
                            t.Type = TokenType.String;
                            while (true)
                            {
                                if(script[loc] == '\\')
                                {
                                    switch(script[loc + 1])
                                    {
                                        case '"':
                                            r.Append('"');
                                            goto default;
                                        case 'n':
                                            r.Append('\n');
                                            goto default;
                                        case 'r':
                                            r.Append('\r');
                                            goto default;
                                        case 't':
                                            r.Append('\t');
                                            goto default;
                                        case '\\':
                                            r.Append('\\');
                                            goto default;
                                        case '\v':
                                            r.Append('\v');
                                            goto default;
                                        default:
                                            loc += 2;
                                            continue;
                                    }
                                }
                                if (script[loc] == '"')
                                {
                                    t.NameOrValue = r.ToString(); break;
                                }
                                r.Append( script[loc]);
                                loc++;
                                if (loc == len)
                                    throw new Exception("字符串无效");
                            }
                            break;
                        case ',':
                            t.Type = TokenType.Comma; break;
                        case '&':
                            t.Type = TokenType.Connect; break;
                        default:
                            throw new Exception();
                    }
                    loc++;
                }
                else if (Regexp_SpaceOrTab.IsMatch(script[loc].CS()))
                {
                    loc++;
                    continue;
                }
                else
                {
                    throw new Exception();
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
                case TokenType.String: return -5;
                default: return 0;
            }
        }
        private static int GetOperatorLevel(OperatorType o)
        {
            switch (o)
            {
                case OperatorType.Connect:
                    return 0;
                case OperatorType.Imp:
                case OperatorType.Eqv:
                case OperatorType.Xor:
                case OperatorType.Or:
                case OperatorType.And:
                    return 1;
                case OperatorType.Not:
                case OperatorType.GreaterEqual:
                case OperatorType.LessEqual:
                case OperatorType.GreaterThan:
                case OperatorType.LessThan:
                case OperatorType.NotEqual:
                case OperatorType.Equal:
                    return 2;
                case OperatorType.Add:
                case OperatorType.Subtract:
                    return 3;
                case OperatorType.Mod:
                case OperatorType.Multiply:
                case OperatorType.Divide:
                    return 4;
                case OperatorType.Neg:
                    return 5;
                case OperatorType.Pow:
                    return 6;
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
            int xNum = GetOperatorLevel(x);
            int opNum = GetOperatorLevel(opStack.Peek());
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
    }
}
namespace ScriptCompilerEngine
{
    internal static class ExMethods
    {
        public static string CS(this char character)
        {
            return new string(character, 1);
        }
        public static Element ToElement(this OperatorType op)
        {
            return new Element(ElementType.Operator, op.ToString(), 0);
        }
        public static OperatorType ToOper(this Token t)
        {
            return (OperatorType)t.Type;
        }
        public static Type[] GetArray(this Type type, int length)
        {
            Type[] types = new Type[length];
            for (int i = 0; i < length; i++)
            {
                types[i] = type;
            }
            return types;
        }
        public static MethodInfo GetMethodInfo<TOut,TIn>(this Func<TIn,TOut> @delegate)
        {
            return @delegate.Method;
        }
    }
}
