using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsGrafeq.Expression
{
    public static class ExpressionBuilder
    {
        private static readonly Expression _x, _y;
        static ExpressionBuilder()
        {
            _x = GetVariable('x');
            _y = GetVariable('y');
        }
        public static Expression x
        {
            get { return _x; }
        }
        public static Expression y
        {
            get { return _y; }
        }

        public static Expression GetVariable(char varname)
        {
            if (varname < 'a' || varname > 'z')
                throw new ArgumentException(nameof(varname));
            Expression exp = new Expression();
            exp.Elements.Add(new Element(ElementType.Variable, varname.ToString(), 0));
            return exp;
        }
        public static Expression GetNumber(double num)
        {
            Expression exp = new Expression();
            exp.Elements.Add(new Element(ElementType.Number, num.ToString(), 0));
            return exp;
        }
        public static Expression FromString(string s)
        {
            Expression exp= new Expression();
            exp.Elements.AddRange(ExpressionCompiler.ParseTokens(ExpressionCompiler.GetTokens(s)));
            return exp;
        }
        public static ComparedExpression Less(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Less", 2));
            return expressionCompared;
        }
        public static ComparedExpression Greater(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Greater", 2));
            return expressionCompared;
        }
        public static ComparedExpression LessEqual(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "LessEqual", 2));
            return expressionCompared;
        }
        public static ComparedExpression GreaterEqual(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "GreaterEqual", 2));
            return expressionCompared;
        }
        public static ComparedExpression Equal(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Equal", 2));
            return expressionCompared;
        }
        private static Expression Call(Expression exp1, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, name, 1));
            return newexp;
        }
        private static Expression Call(Expression exp1, Expression exp2, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, name, 2));
            return newexp;
        }
        public static Expression Add(Expression exp1, Expression exp2)
        {
            return Call(exp1,exp2,"Add");
        }
        public static Expression Subtract(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Subtract");
        }
        public static Expression Multiply(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Multiply");
        }
        public static Expression Divide(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Divide");
        }
        public static Expression Mod(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Mod");
        }
        public static Expression Pow(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Pow");
        }
        public static Expression Sgn(Expression exp1)
        {
            return Call(exp1, "Sgn");
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
            return Call(exp1, "Exp");
        }
        public static Expression Ln(Expression exp1)
        {
            return Call(exp1, "Ln");
        }
        public static Expression Lg(Expression exp1)
        {
            return Call(exp1, "Lg");
        }
        public static Expression Log(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Log");
        }
        public static Expression Sqrt(Expression exp1)
        {
            return Call(exp1, "Sqrt");

        }
        public static Expression Root(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Root");
        }
        public static Expression Min(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Min");
        }
        public static Expression Max(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Max");
        }
        public static Expression Sin(Expression exp1)
        {
            return Call(exp1, "Sin");
        }
        public static Expression Cos(Expression exp1)
        {
            return Call(exp1, "Cos");
        }
        public static Expression Tan(Expression exp1)
        {
            return Call(exp1, "Tan");
        }
        public static Expression Arcsin(Expression exp1)
        {
            return Call(exp1, "Arcsin");
        }
        public static Expression Arccos(Expression exp1)
        {
            return Call(exp1, "Arccos");
        }
        public static Expression Arctan(Expression exp1)
        {
            return Call(exp1, "Arctan");
        }
        public static Expression Floor(Expression exp1)
        {
            return Call(exp1, "Floor");
        }
        public static Expression Ceil(Expression exp1)
        {
            return Call(exp1, "Ceil");
        }
        public static Expression GCD(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "GCD");
        }
        public static Expression LCM(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "LCM");
        }
        public static Expression Factorial(Expression exp1)
        {
            return Call(exp1, "Factorial");
        }
        public static ComparedExpression Union(ComparedExpression exp1, ComparedExpression exp2)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(exp1.Elements);
            expressionCompared.Elements.AddRange(exp2.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Union", 2));
            return expressionCompared;
        }
        public static ComparedExpression Intersect(ComparedExpression exp1, ComparedExpression exp2)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(exp1.Elements);
            expressionCompared.Elements.AddRange(exp2.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Intersect", 2));
            return expressionCompared;
        }
    }
    public class ExpressionBase
    {
        internal ExpressionBase() { }
        internal List<Element> Elements = new List<Element>();
    }
    public class Expression:ExpressionBase
    {
        internal Expression() { }
        public static Expression operator +(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Add");
        }
        public static Expression operator -(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Subtract");
        }
        public static Expression operator -(Expression exp)
        {
            return Call(exp,"Neg");
        }
        public static Expression operator *(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Multiply");
        }
        public static Expression operator /(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Divide");
        }
        public static Expression operator ^(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Pow");
        }
        public static Expression operator %(Expression exp1, Expression exp2)
        {
            return Call(exp1, exp2, "Mod");
        }
        private static Expression Call(Expression exp1, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, name, 1));
            return newexp;
        }
        private static Expression Call(Expression exp1, Expression exp2, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, name, 2));
            return newexp;
        }
        public static ComparedExpression operator ==(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Equal(exp1, exp2);
        }
        public static ComparedExpression operator !=(Expression exp1, Expression exp2)
        {
            throw new NotImplementedException();
        }
        public static ComparedExpression operator <(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Less(exp1, exp2);
        }
        public static ComparedExpression operator >(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Greater(exp1, exp2);
        }
        public static ComparedExpression operator <=(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.LessEqual(exp1, exp2);
        }
        public static ComparedExpression operator >=(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.GreaterEqual(exp1, exp2);
        }
        public static implicit operator Expression(double num)
        {
            return ExpressionBuilder.GetNumber(num);
        }
        public static implicit operator Expression(char var)
        {
            return ExpressionBuilder.GetVariable(var);
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
    public class ComparedExpression : ExpressionBase
    {
        internal ComparedExpression() { }
        public static ComparedExpression operator &(ComparedExpression exp1, ComparedExpression exp2)
        {
            return ExpressionBuilder.Intersect(exp1, exp2);
        }
        public static ComparedExpression operator |(ComparedExpression exp1, ComparedExpression exp2)
        {
            return ExpressionBuilder.Union(exp1, exp2);
        }
    }
}
