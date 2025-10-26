using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using CsGrafeq;
using CsGrafeq.Compiler;

namespace CsGrafeq.Interval
{
    public static class IntervalCompiler
    {
        public static HasReferenceIntervalSetFunc<IntervalSet> Compile(string expression)
        {
            var exptree = Compiler.Compiler.ConstructExpTree<IntervalSet>(expression, 2, out var xVar, out var yVar, out _, out var reference);
            
            //return new(FastExpressionCompiler.ExpressionCompiler.CompileFast(Expression.Lambda<IntervalHandler<IntervalSet>>(exptree, xVar, yVar)),reference);
            return new(Expression.Lambda<IntervalHandler<IntervalSet>>(exptree.Reduce(), xVar, yVar).Compile(), reference);
        }
    }
}
