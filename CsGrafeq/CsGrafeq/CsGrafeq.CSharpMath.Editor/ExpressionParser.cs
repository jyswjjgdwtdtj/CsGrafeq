using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using CsGrafeq;
using CSharpMath;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using CSharpMath.Rendering.Text;
using CSharpMath.Structures;
using DynamicData;
using StrResult=CsGrafeq.Result<string>;
using Space = CSharpMath.Atom.Atoms.Space;

namespace CsGrafeq.CSharpMath.Editor;

public static class ExpressionParser
{
    private static Dictionary<string, string> SymbolToReplace = new Dictionary<string, string>()
    {
        {"≤","<="},
        {"≥",">="},
        {"∧","|"},
        {"∨","&"},
        {"×","*"},
        
    };

    public static StrResult Parse(this MathList mathList)
    {
        var builder = new CursorStringBuilder();
        try
        {
            MathListToExpression(mathList, builder);
        }
        catch (Exception e)
        {
            return StrResult.Error(e);
        }
        return StrResult.Success(builder.ToString().ForEachReplace(SymbolToReplace));
    }
/*
    internal static MathAtom PreProcessMathAtom(MathAtom source)
    {
        MathAtom res;
        switch (source)
        {
            case Inner inner:
                res= new Inner(new("("), PreProcessMathList(inner.InnerList), new(")"));
                break;
            default:
                
                break;
        }
        res.Superscript.Append(source.Superscript);
    }
    internal static MathList PreProcessMathList(MathList mathList)
    {
        var myml = mathList.Clone(false);
        int brackets = 0;
        for (var i = 0; i < myml.Count; i++)
        {
            var cur = myml[i];
            switch (cur)
            {
                case Open:
                    if (brackets == 0)
                    {
                        myml[i] = new TempMathAtom(0);
                    }
                    if (cur.Superscript.IsNonEmpty())
                        throw new Exception();
                    brackets++;
                    break;
                case Close:
                    if (brackets == 1)
                    {
                        myml[i] = new TempMathAtom(1);
                        myml[i].Superscript.Append(cur.Superscript);
                    }
                    brackets--;
                    break;
                default:
                    break;
            }
        }
        var newml=new MathList();
        for (var i = 0; i < myml.Count; i++)
        {
            var cur = myml[i];
            switch (cur)
            {
                case TempMathAtom:
                    var inner = new MathList();
                    while (true)
                    {
                        cur = myml[++i];
                        if (cur is TempMathAtom)
                        {
                            newml.Add(new Inner(new Boundary("("),inner,new Boundary(")")));
                            break;
                        }
                        inner.Add(cur);
                    }
                    break;
                default:
                    newml.Add(cur);
                    break;
            }
        }
        return newml;
    }
    */
    private static void MathListToExpression
        (MathList mathList, CursorStringBuilder builder)
    {
        if (mathList is null)
        {
            throw(new ArgumentNullException(nameof(mathList)));
        }

        if (mathList.IsEmpty())
            throw new Exception("Mathlist is empty");
        for(var i=0;i<mathList.Count;i++)
        {
            var atom = mathList[i];
            MathAtom? ScanNext()
            {
                var j = i + 1;
                if (j < mathList.Count) return mathList[j];
                return null;
            }
            switch (atom)
            {
                case Fraction fraction:
                    builder.Insert(@"(");
                    MathListToExpression(fraction.Numerator, builder);
                    builder.Insert(")/(");
                    MathListToExpression(fraction.Denominator, builder);
                    builder.Insert(")");
                    goto NextShouldBeMultiply;
                case Radical radical:
                    if (radical.Degree.IsNonEmpty())//非二次开方
                    {
                        var degree = radical.Degree;
                        var first = degree[0];
                        if (degree.Count == 1 && first.EqualsAtom(new Number("3")))//开三次方
                        {
                            builder.Insert("cbrt(");
                            MathListToExpression(radical.Radicand, builder);
                            builder.Insert(")");
                        }
                        else//其他开方
                        {
                            builder.Insert("(");
                            MathListToExpression(radical.Radicand, builder);
                            builder.Insert(")^(1/(");
                            MathListToExpression(radical.Degree, builder);
                            builder.Insert("))");
                        }
                    }
                    else
                    {
                        builder.Insert("sqrt(");
                        MathListToExpression(radical.Radicand, builder);
                        builder.Insert(")");
                    }
                    goto NextShouldBeMultiply;
                case Inner { LeftBoundary: { Nucleus: null }, InnerList: var list, RightBoundary: { Nucleus: null } }:
                    MathListToExpression(list, builder);
                    goto NextShouldBeMultiply;
                case Inner { LeftBoundary: var left, InnerList: var list, RightBoundary: var right }:
                {
                    if (left.Nucleus == "(" && right.Nucleus == ")")
                    {
                        builder.Insert(@"(");
                        MathListToExpression(list, builder);
                        builder.Insert(@")");
                    }else if (left.Nucleus == "|" && right.Nucleus == "|")
                    {
                        builder.Insert(@"abs(");
                        MathListToExpression(list, builder);
                        builder.Insert(@")");
                    }else if (left.Nucleus == Symbols.LeftCeiling&& right.Nucleus == Symbols.RightCeiling)
                    {
                        builder.Insert(@"ceiling(");
                        MathListToExpression(list, builder);
                        builder.Insert(@")");
                    }else if (left.Nucleus == Symbols.LeftFloor&& right.Nucleus == Symbols.RightFloor)
                    {
                        builder.Insert(@"floor(");
                        MathListToExpression(list, builder);
                        builder.Insert(@")");
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                    goto NextShouldBeMultiply;
                case LargeOperator op:
                    var cmd = LaTeXSettings.CommandForAtom(op);
                    if (cmd is string command)
                    {
                        if (!(LaTeXSettings.AtomForCommand(command) is LargeOperator originalOperator))
                            throw new InvalidCodePathException("original operator not found!");
                        var next = ScanNext();
                        if (next == null)
                        {
                            throw new Exception("next not found!");
                        }

                        var oppower = op.Superscript;
                        if (next is Inner inner && inner.LeftBoundary.Nucleus == "(" &&
                            inner.RightBoundary.Nucleus == ")")
                        {
                            var inpower = inner.Superscript;
                            if(oppower.IsNonEmpty()&&inpower.IsNonEmpty())
                                throw new Exception("Power can not be placed on both function and brackets");
                            inpower=inpower.IsNonEmpty()?inpower:oppower;
                            builder.Insert("(");
                            builder.Insert(originalOperator.Nucleus);
                            builder.Insert(@"(");
                            MathListToExpression(inner.InnerList, builder);
                            builder.Insert(@")");
                            builder.Insert(")");
                            AppendPowScript(builder,inpower);
                            i++;
                            goto NextShouldBeMultiply;
                        }
                        if (next is Open)
                        {
                            List<MathAtom> ms=new();
                            i+=2;
                            var bracket = 0;
                            while (true)
                            {
                                if (i == mathList.Count)
                                    throw new Exception("Out of range");
                                var current = mathList[i];
                                if (current is Open)
                                {
                                    bracket++;
                                }else if (current is Close)
                                {
                                    bracket--;
                                    if (bracket == 0)
                                    {
                                        //此时游标在Close上
                                        break;   
                                    }
                                }
                                ms.Add(current);
                                i++;
                            }

                            var inpower = mathList[i].Superscript;
                            if(oppower.IsNonEmpty()&&inpower.IsNonEmpty())
                                throw new Exception("Power can not be placed on both function and brackets");
                            inpower=inpower.IsNonEmpty()?inpower:oppower;
                            builder.Insert("(");
                            builder.Insert(originalOperator.Nucleus);
                            builder.Insert(@"(");
                            MathListToExpression(new MathList(ms), builder);
                            builder.Insert(@")");
                            builder.Insert(")");
                            AppendPowScript(builder,inpower);
                            goto NextShouldBeMultiply;
                        }
                        throw new InvalidCodePathException("inner not found!");
                    }
                    else
                    {
                        throw new Exception("");
                    }
                    break;
                case Variable:
                case Close:
                {
                    builder.Insert(atom.Nucleus);
                }
                    goto NextShouldBeMultiply;
                case BinaryOperator:
                case UnaryOperator:
                case Relation:
                case Open: 
                {
                    builder.Insert(atom.Nucleus);
                }
                 break;
                case Number:
                {
                    StringBuilder numstr=new StringBuilder(atom.Nucleus);
                    while (true)
                    {
                        var next = ScanNext();
                        if (next is Number)
                        {
                            numstr.Append(next.Nucleus);   
                        }
                        else
                        {
                            break;
                        }
                        i++;
                    }
                    builder.Insert(numstr.ToString());
                }
                    goto NextShouldBeMultiply;
                case Placeholder:
                    builder.ForceToBeEmpty=true;
                    break;
                case Punctuation punc when punc.Nucleus==",":
                    builder.Insert(",");
                    break;
                default:
                    throw new Exception(atom.ToString() + "|" + atom.Nucleus + "|" + atom.GetType()); 
                NextShouldBeMultiply: //在两个多项式见添加乘号
                {
                    var next = ScanNext();
                    if (next != null)
                    {
                        switch (next)
                        {
                            case Inner:
                            case Open:
                            case Variable:
                            case Number:
                            case LargeOperator:
                            case Fraction:
                            case Radical:
                                builder.Insert("*");
                                break;
                        }
                    }
                }
                    break;
            }

            AppendPowScript(builder, atom.Superscript);
        }
    }
    

    static void AppendPowScript
        (CursorStringBuilder builder, MathList script)
    {
        if (script.IsNonEmpty())
        {
            builder.Insert(@"^(");
            MathListToExpression(script,builder);
            builder.Insert(")");
        }
    }

    private static string ForEachReplace(this string input, IDictionary<string, string> toReplace)
    {
        foreach (var kvpair in toReplace)
        {
            input=input.Replace(kvpair.Key, kvpair.Value);
        }

        return input;
    }

    private class TempMathAtom: MathAtom
    {
        public int Identifier;
        public override bool ScriptsAllowed { get; } = true;
        protected override MathAtom CloneInside(bool finalize)
        {
            throw new NotSupportedException();
        }

        public TempMathAtom(int identifier)
        {
            Identifier = identifier;
        }
    }
}