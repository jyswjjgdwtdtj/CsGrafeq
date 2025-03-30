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
        public static Expression FromString(string s)
        {
            Expression exp= new Expression();
            exp.Elements.AddRange(ExpressionComplier.ParseTokens(ExpressionComplier.GetTokens(s)));
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
        public static ComparedExpression Equal(Expression a, Expression b)
        {
            ComparedExpression expressionCompared = new ComparedExpression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Equal", 2));
            return expressionCompared;
        }
        private static Expression OneVariable(Expression exp1, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, name, 1));
            return newexp;
        }
        private static Expression TwoVariable(Expression exp1, Expression exp2, string name)
        {
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, name, 2));
            return newexp;
        }
        public static Expression Add(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1,exp2,"Add");
        }
        public static Expression Subtract(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Subtract");
        }
        public static Expression Multiply(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Multiply");
        }
        public static Expression Divide(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Divide");
        }
        public static Expression Pow(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Pow");
        }
        public static Expression Sgn(Expression exp1)
        {
            return OneVariable(exp1, "Sgn");
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
            return OneVariable(exp1, "Exp");
        }
        public static Expression Ln(Expression exp1)
        {
            return OneVariable(exp1, "Ln");
        }
        public static Expression Lg(Expression exp1)
        {
            return OneVariable(exp1, "Lg");
        }
        public static Expression Log(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Log");
        }
        public static Expression Sqrt(Expression exp1)
        {
            return OneVariable(exp1, "Sqrt");

        }
        public static Expression Root(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Root");
        }
        public static Expression Min(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Min");
        }
        public static Expression Max(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "Max");
        }
        public static Expression Sin(Expression exp1)
        {
            return OneVariable(exp1, "Sin");
        }
        public static Expression Cos(Expression exp1)
        {
            return OneVariable(exp1, "Cos");
        }
        public static Expression Tan(Expression exp1)
        {
            return OneVariable(exp1, "Tan");
        }
        public static Expression Arcsin(Expression exp1)
        {
            return OneVariable(exp1, "Arcsin");
        }
        public static Expression Arccos(Expression exp1)
        {
            return OneVariable(exp1, "Arccos");
        }
        public static Expression Arctan(Expression exp1)
        {
            return OneVariable(exp1, "Arctan");
        }
        public static Expression Floor(Expression exp1)
        {
            return OneVariable(exp1, "Floor");
        }
        public static Expression Ceil(Expression exp1)
        {
            return OneVariable(exp1, "Ceil");
        }
        public static Expression GCD(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "GCD");
        }
        public static Expression LCM(Expression exp1, Expression exp2)
        {
            return TwoVariable(exp1, exp2, "LCM");
        }
        public static Expression Factorial(Expression exp1)
        {
            return OneVariable(exp1, "Factorial");
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
    public class ComparedExpression : ExpressionBase
    {
        internal ComparedExpression() { }
    }
}
