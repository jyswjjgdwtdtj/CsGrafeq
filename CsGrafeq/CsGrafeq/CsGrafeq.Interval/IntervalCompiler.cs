using System.Linq.Expressions;

namespace CsGrafeq.Interval;

public static class IntervalCompiler
{
    public static HasReferenceIntervalSetFunc<IntervalSet> Compile(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException("expression");
        var exptree =
            Compiler.Compiler.ConstructExpTree<IntervalSet>(expression, 2, out var xVar, out var yVar, out _,
                out var reference);
        return new HasReferenceIntervalSetFunc<IntervalSet>(
            Expression.Lambda<IntervalHandler<IntervalSet>>(exptree.Reduce(), xVar, yVar).Compile(), reference);
    }

    public static bool TryCompile(string expression)
    {
        try
        {
            Compile(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }
}