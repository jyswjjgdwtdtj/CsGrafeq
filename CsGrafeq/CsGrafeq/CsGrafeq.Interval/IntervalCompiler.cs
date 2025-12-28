using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using CsGrafeq.Interval.Interface;
using FastExpressionCompiler;

namespace CsGrafeq.Interval;

public static class IntervalCompiler
{
    public static HasReferenceIntervalSetFunc<IntervalSet> Compile(string expression)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException("expression");
        var exptree =
            Compiler.Compiler.ConstructExpTree<IntervalSet>(expression, 2, out var xVar, out var yVar, out _,
                out var reference).Reduce();
        return new HasReferenceIntervalSetFunc<IntervalSet>(
            CompileByDynamicMethod(Expression.Lambda<IntervalHandler<IntervalSet>>(exptree, xVar, yVar)), reference);
    }

    public static IntervalHandler<IntervalSet> CompileByDynamicMethod(
        Expression<IntervalHandler<IntervalSet>> expression)
    {
        expression.CompileFast();
        if (expression == null)
            throw new ArgumentNullException("expression");
        var dynamicMethod = new DynamicMethod("dynamicmethod", typeof(Def),
            new[] { typeof(IntervalSet), typeof(IntervalSet) }, typeof(IntervalSet).Module, true);
        var ilg = dynamicMethod.GetILGenerator();
        var ilr = new ILRecorder(ilg);
        CompileToILGenerator<IntervalSet>(ilr, expression.Body);
        ilr.Emit(OpCodes.Ret);
        var func = dynamicMethod.CreateDelegate<IntervalHandler<IntervalSet>>();
        try
        {
            var testInterval = IntervalSet.CreateFromDouble(-1.0);
            var result = func(testInterval, testInterval);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to compile the expression to IL code.", ex);
        }

        return func;
    }

    public static void CompileToILGenerator<T>(ILRecorder ilGenerator, Expression expression) where T : IInterval<T>
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.Equal:
            case ExpressionType.LessThan:
            case ExpressionType.GreaterThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.GreaterThanOrEqual:
            {
                var exp = (BinaryExpression)expression;
                CompileToILGenerator<T>(ilGenerator, exp.Left);
                CompileToILGenerator<T>(ilGenerator, exp.Right);
                EmitExpressionMethod<T>(ilGenerator, exp.Method);
            }
                break;

            case ExpressionType.Negate:
            {
                var exp = (UnaryExpression)expression;
                CompileToILGenerator<T>(ilGenerator, exp.Operand);
                EmitExpressionMethod<T>(ilGenerator, exp.Method);
            }
                break;
            case ExpressionType.Call:
            {
                var exp = (MethodCallExpression)expression;
                var method = exp.Method!;
                foreach (var arg in exp.Arguments) CompileToILGenerator<T>(ilGenerator, arg);
                EmitExpressionMethod<T>(ilGenerator, method);
            }
                break;
            case ExpressionType.Constant:
            {
                var exp = (ConstantExpression)expression;
                var value = exp.Value;
                if (value is double d)
                    ilGenerator.Emit(OpCodes.Ldc_R8, d);
                else if (value is int c)
                    ilGenerator.Emit(OpCodes.Ldc_I4, c);
                else if (value is char ch)
                    ilGenerator.Emit(OpCodes.Ldc_I4, ch);
                else if (value is float f)
                    ilGenerator.Emit(OpCodes.Ldc_R4, f);
                else
                    throw new Exception();
            }
                break;
            case ExpressionType.Parameter:
            {
                var exp = (ParameterExpression)expression;
                if (exp.Name == "x")
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                else if (exp.Name == "y")
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                else
                    throw new Exception();
            }
                break;
            default:
            {
                throw new NotSupportedException();
            }
        }
    }

    public static void EmitExpressionMethod<T>(ILRecorder ilGenerator, MethodInfo? mi) where T : IInterval<T>
    {
        if (mi is null)
            throw new ArgumentNullException(nameof(mi));
        var name = mi.Name.ToLower();
        var ts = new Type[mi.GetParameters().Length];
        for (var i = 0; i < ts.Length; i++)
            ts[i] = typeof(T);
        if (name.StartsWith("op_")) //like op_Addition...... the operator overload method
            ilGenerator.Emit(OpCodes.Call, mi);
        /*if (IHasOperatorNumber<T>.HasOperatorNumberPtrMethodDictionary.TryGetValue(name, out var ptr))
        {
            ilGenerator.Emit(OpCodes.Ldc_I8, ptr.ToInt64()); // 常量 64 位
            ilGenerator.Emit(OpCodes.Conv_I);
            ilGenerator.EmitCalli(
                OpCodes.Calli,
                System.Runtime.InteropServices.CallingConvention.Cdecl,
                typeof(T),
                ts);
            return;
        }
        if (IComputableNumber<T>.MethodPtrDictionary.TryGetValue(name, out var cptr))
        {
            ilGenerator.Emit(OpCodes.Ldc_I8, ptr.ToInt64()); // 常量 64 位
            ilGenerator.Emit(OpCodes.Conv_I);
            ilGenerator.EmitCalli(
                OpCodes.Calli,
                System.Runtime.InteropServices.CallingConvention.Cdecl,
                typeof(T),
                ts);
            return;
        }*/
        else
            ilGenerator.Emit(OpCodes.Call, mi);
    }

    public static Result<HasReferenceIntervalSetFunc<IntervalSet>> TryCompile(string expression)
    {
        try
        {
            return Result<HasReferenceIntervalSetFunc<IntervalSet>>.Success(Compile(expression));
        }
        catch (Exception e)
        {
            return Result<HasReferenceIntervalSetFunc<IntervalSet>>.Error(e);
        }
    }
}