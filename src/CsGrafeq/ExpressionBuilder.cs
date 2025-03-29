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
        public static Expression Less(Expression a, Expression b)
        {
            if(a.Compared||b.Compared)
                throw new ArgumentException("不能比较已经比较过的算式");
            Expression expressionCompared = new Expression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Less", 2));
            expressionCompared.Compared = true;
            return expressionCompared;
        }
        public static Expression Greater(Expression a, Expression b)
        {
            if (a.Compared || b.Compared)
                throw new ArgumentException("不能比较已经比较过的算式");
            Expression expressionCompared = new Expression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Greater", 2));
            expressionCompared.Compared = true;
            return expressionCompared;
        }
        public static Expression Equal(Expression a, Expression b)
        {
            if (a.Compared || b.Compared)
                throw new ArgumentException("不能比较已经比较过的算式");
            Expression expressionCompared = new Expression();
            expressionCompared.Elements.AddRange(a.Elements);
            expressionCompared.Elements.AddRange(b.Elements);
            expressionCompared.Elements.Add(new Element(ElementType.Function, "Equal", 2));
            expressionCompared.Compared = true;
            return expressionCompared;
        }
        public static Expression Add(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp= new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, "Add", 2));
            return newexp;
        }
        public static Expression Subtract(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, "Subtract", 2));
            return newexp;
        }
        public static Expression Multiply(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, "Multiply", 2));
            return newexp;
        }
        public static Expression Divide(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, "Divide", 2));
            return newexp;
        }
        public static Expression Pow(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Operator, "Pow", 2));
            return newexp;
        }
        public static Expression Sgn(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Sgn", 1));
            return newexp;
        }
        public static Expression Median(Expression exp1, Expression exp2, Expression exp3)
        {
            if (exp1.Compared || exp2.Compared || exp3.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.AddRange(exp3.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Median", 3));
            return newexp;
        }
        public static Expression Exp(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Exp", 1));
            return newexp;
        }
        public static Expression Ln(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Ln", 1));
            return newexp;
        }
        public static Expression Lg(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Lg", 1));
            return newexp;
        }
        public static Expression Log(Expression exp1, Expression exp2)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Log", 2));
            return newexp;
        }
        public static Expression Sqrt(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Sqrt", 1));
            return newexp;
        }
        public static Expression Root(Expression exp1, Expression exp2)
        {
            if (exp1.Compared||exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Root", 2));
            return newexp;
        }
        public static Expression Min(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Min", 2));
            return newexp;
        }
        public static Expression Max(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Max", 2));
            return newexp;
        }
        public static Expression Sin(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Sin", 1));
            return newexp;
        }
        public static Expression Cos(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Cos", 1));
            return newexp;
        }
        public static Expression Tan(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Tan", 1));
            return newexp;
        }
        public static Expression Arcsin(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Arcsin", 1));
            return newexp;
        }
        public static Expression Arccos(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Arccos", 1));
            return newexp;
        }
        public static Expression Arctan(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Arctan", 1));
            return newexp;
        }
        public static Expression Floor(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Floor", 1));
            return newexp;
        }
        public static Expression Ceil(Expression exp1)
        {
            if (exp1.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "Ceil", 1));
            return newexp;
        }
        public static Expression GCD(Expression exp1, Expression exp2)
        {
            if (exp1.Compared || exp2.Compared)
                throw new ArgumentException("不能对已经比较过的算式执行计算");
            Expression newexp = new Expression();
            newexp.Elements.AddRange(exp1.Elements);
            newexp.Elements.AddRange(exp2.Elements);
            newexp.Elements.Add(new Element(ElementType.Function, "GCD", 1));
            return newexp;
        }
    }
    public class Expression
    {
        internal Expression() { }
        internal List<Element> Elements=new List<Element>();
        internal bool Compared = false;
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
        public static Expression operator ==(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Equal(exp1, exp2);
        }
        public static Expression operator !=(Expression exp1, Expression exp2)
        {
            throw new NotImplementedException();
        }
        public static Expression operator <(Expression exp1, Expression exp2)
        {
            return ExpressionBuilder.Less(exp1, exp2);
        }
        public static Expression operator >(Expression exp1, Expression exp2)
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
}
