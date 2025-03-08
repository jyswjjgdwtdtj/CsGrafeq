using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CsGrafeq.ExpressionComplier;

namespace CsGrafeq
{
    public static class ExpressionBuilder
    {
        public static readonly Expression x, y;
        static ExpressionBuilder()
        {
            x = GetVariable("x");
            y = GetVariable("y");
        }
        public static Expression GetVariable(string varname)
        {
            if (varname.Length != 1)
                throw new ArgumentException(nameof(varname));
            if (varname[0] < 'a' || varname[0] > 'z')
                throw new ArgumentException(nameof(varname));
            Expression exp = new Expression();
            exp.Elements.Add(new Element(ElementType.Variable, varname, 0));
            return exp;
        }
        public static Expression GetNumber(double num)
        {
            Expression exp = new Expression();
            exp.Elements.Add(new Element(ElementType.Number, num.ToString(), 0));
            return exp;
        }
        public static ExpressionCompared Less(Expression a, Expression b)
        {
            ExpressionCompared expressionCompared = new ExpressionCompared();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Less", 2));
            return expressionCompared;
        }
        public static ExpressionCompared Greater(Expression a, Expression b)
        {
            ExpressionCompared expressionCompared = new ExpressionCompared();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Greater", 2));
            return expressionCompared;
        }
        public static ExpressionCompared Equal(Expression a, Expression b)
        {
            ExpressionCompared expressionCompared = new ExpressionCompared();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Equal", 2));
            return expressionCompared;
        }
        public static Expression Add(Expression exp1, Expression exp2)
        {
            Expression newexp= new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Add", 2));
            return newexp;
        }
        public static Expression Subtract(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Subtract", 2));
            return newexp;
        }
        public static Expression Multiply(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Multiply", 2));
            return newexp;
        }
        public static Expression Divide(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Divide", 2));
            return newexp;
        }
        public static Expression Pow(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Pow", 2));
            return newexp;
        }
        public static Expression Sgn(Expression exp1)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Sgn", 1));
            return newexp;
        }
        public static Expression Median(Expression exp1, Expression exp2, Expression exp3)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.AddRange(exp3.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Median", 3));
            return newexp;
        }
        public static Expression Exp(Expression exp1)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Exp", 1));
            return newexp;
        }
        public static Expression Ln(Expression exp1)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Ln", 1));
            return newexp;
        }
        public static Expression Lg(Expression exp1)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Lg", 1));
            return newexp;
        }
        public static Expression Log(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Log", 2));
            return newexp;
        }
        public static Expression Sqrt(Expression exp1)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Sqrt", 1));
            return newexp;
        }
        public static Expression Root(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Root", 2));
            return newexp;
        }
        public static Expression Min(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Min", 2));
            return newexp;
        }
        public static Expression Max(Expression exp1, Expression exp2)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Max", 2));
            return newexp;
        }
    }
    public class Expression
    {
        internal Expression() { }
        internal List<Element> Elements=new List<Element>();
        public static Expression operator +(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Add(exp1, exp2);
        }
        public static Expression operator -(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Subtract(exp1, exp2);
        }
        public static Expression operator *(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Multiply(exp1, exp2);
        }
        public static Expression operator /(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Divide(exp1, exp2);
        }
        public static Expression operator ^(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Pow(exp1, exp2);
        }
        public static ExpressionCompared operator ==(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Equal(exp1, exp2);
        }
        public static ExpressionCompared operator !=(Expression exp1, Expression exp2)
        {
            throw new NotImplementedException();
        }
        public static ExpressionCompared operator <(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Less(exp1, exp2);
        }
        public static ExpressionCompared operator >(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Greater(exp1, exp2);
        }
        public static implicit operator Expression(double num)
        {
            return ExpressionBuilder.GetNumber(num);
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class ExpressionCompared
    {
        internal List<Element> Elements = new List<Element>();
    }

}
