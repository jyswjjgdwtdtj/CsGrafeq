using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using CsGrafeq.Interval.Extensions;
using CsGrafeq.Interval.Interface;
using CsGrafeq.Numeric;
using FastExpressionCompiler;

namespace CsGrafeq.Interval;

public static class IntervalCompiler
{
    public static readonly object SyncObjForIntervalSetCalc = new();

    public static HasReferenceIntervalSetFunc<IntervalSet> Compile(string expression, bool enableSimplification = false)
    {
        if (string.IsNullOrEmpty(expression))
            throw new ArgumentNullException("expression");
        var exptree =
            Compiler.Compiler.ConstructExpTree<IntervalSet>(expression, 2, out var xVar, out var yVar, out _,
                out var reference, enableSimplification).Reduce();
        return new HasReferenceIntervalSetFunc<IntervalSet>(
            CompileByDynamicMethod(Expression.Lambda<IntervalHandler<IntervalSet>>(exptree, xVar, yVar)), reference);
    }

    public static IntervalHandler<IntervalSet> CompileByDynamicMethod(
        Expression<IntervalHandler<IntervalSet>> expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));
        var dynamicMethod = new DynamicMethod(nameof(DynamicMethod), typeof(Def),
            [typeof(IntervalSet), typeof(IntervalSet)], typeof(IntervalSet).Module, true);
        var ilg = dynamicMethod.GetILGenerator();
        var ilr = new ILRecorder(ilg);
        CompileToIlGenerator<IntervalSet>(ilr, expression.Body);
        ilr.Emit(OpCodes.Ret);
        var func = dynamicMethod.CreateDelegate<IntervalHandler<IntervalSet>>();
        try
        {
            var testInterval = IntervalSet.CreateFromDouble(-1.0);
            func(testInterval, testInterval);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to compile the expression to IL code.", ex);
        }

        return func;
    }

    public static void CompileToIlGenerator<T>(ILRecorder ilGenerator, Expression expression)
        where T : IInterval<T>, allows ref struct
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
                CompileToIlGenerator<T>(ilGenerator, exp.Left);
                CompileToIlGenerator<T>(ilGenerator, exp.Right);
                EmitExpressionMethod<T>(ilGenerator, exp.Method);
            }
                break;

            case ExpressionType.Negate:
            {
                var exp = (UnaryExpression)expression;
                CompileToIlGenerator<T>(ilGenerator, exp.Operand);
                EmitExpressionMethod<T>(ilGenerator, exp.Method);
            }
                break;
            case ExpressionType.Call:
            {
                var exp = (MethodCallExpression)expression;
                var method = exp.Method;
                foreach (var arg in exp.Arguments) CompileToIlGenerator<T>(ilGenerator, arg);
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

    public static void EmitExpressionMethod<T>(ILRecorder ilGenerator, MethodInfo? mi)
        where T : IInterval<T>, allows ref struct
    {
        if (mi is null)
            throw new ArgumentNullException(nameof(mi));
        var name = mi.Name.ToLower();
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

    public static Result<HasReferenceIntervalSetFunc<IntervalSet>> TryCompile(string expression,
        bool enableSimplification)
    {
        try
        {
            return Result<HasReferenceIntervalSetFunc<IntervalSet>>.Success(Compile(expression, enableSimplification));
        }
        catch (Exception e)
        {
            return Result<HasReferenceIntervalSetFunc<IntervalSet>>.Failure(e);
        }
    }

    public static Func<double, double, double, double, bool> GetMarchingSquaresFunc(string expression)
    {
        var xParams = Enumerable.Range(0, 4).Select(i => Expression.Parameter(typeof(DoubleNumber), "x" + i)).ToArray();
        var yParams = Enumerable.Range(0, 4).Select(i => Expression.Parameter(typeof(DoubleNumber), "y" + i)).ToArray();
        var exp = Compiler.Compiler.ConstructExpTree<DoubleNumber>(expression, 2, out var xVar, out var yVar, out _,
            out _,
            Setting.Instance.EnableExpressionSimplification);
        var regulatedExp = RegulateExpression(exp, xVar, yVar, xParams, yParams);
        Console.WriteLine(regulatedExp.ToCSharpString());
        var lambda = Expression
            .Lambda<Func<DoubleNumber, DoubleNumber, DoubleNumber, DoubleNumber, DoubleNumber, DoubleNumber,
                DoubleNumber, DoubleNumber, bool>>(regulatedExp, xParams.Concat(yParams)).Compile();
        return (left, top, right, bottom) =>
        {
            var xStep = (right - left) / 3;
            var yStep = (bottom - top) / 3;
            var midX1 = new DoubleNumber(left + xStep);
            var midY1 = new DoubleNumber(top + yStep);
            var midX2 = new DoubleNumber(right - xStep);
            var midY2 = new DoubleNumber(bottom - yStep);
            return lambda.Invoke(new DoubleNumber(left), midX1, midX2, new DoubleNumber(right), new DoubleNumber(top),
                midY1, midY2, new DoubleNumber(bottom));
        };
    }

    private static Expression RegulateExpression(Expression expression, ParameterExpression originalX,
        ParameterExpression originalY, ParameterExpression[] xParams, ParameterExpression[] yParams)
    {
        if (expression is BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Or)
                return Expression.Or(RegulateExpression(binaryExpression.Left, originalX, originalY, xParams, yParams),
                    RegulateExpression(binaryExpression.Right, originalX, originalY, xParams, yParams)); //均为bool
            if (binaryExpression.NodeType == ExpressionType.And)
                return Expression.And(RegulateExpression(binaryExpression.Left, originalX, originalY, xParams, yParams),
                    RegulateExpression(binaryExpression.Right, originalX, originalY, xParams, yParams)); //均为bool
            if (binaryExpression.NodeType == ExpressionType.Equal)
            {
                var res = RegulateExpression(Expression.Subtract(binaryExpression.Left, binaryExpression.Right),
                    originalX, originalY, xParams, yParams);
                return Expression.Call(
                    ((Func<DoubleNumber[], bool>)NumberHelper.IsSomeGreaterAndSomeLessThanZero).Method, res);
            }

            if (binaryExpression.NodeType == ExpressionType.LessThan ||
                binaryExpression.NodeType == ExpressionType.LessThanOrEqual)
            {
                var res = RegulateExpression(Expression.Subtract(binaryExpression.Left, binaryExpression.Right),
                    originalX, originalY, xParams, yParams);
                return Expression.Call(((Func<DoubleNumber[], bool>)NumberHelper.IsAllLessThanZero).Method, res);
            }

            if (binaryExpression.NodeType == ExpressionType.GreaterThan ||
                binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                var res = RegulateExpression(Expression.Subtract(binaryExpression.Left, binaryExpression.Right),
                    originalX, originalY, xParams, yParams);
                return Expression.Call(((Func<DoubleNumber[], bool>)NumberHelper.IsAllGreaterThanZero).Method, res);
            }
        }
        // Todo:使用Vector SIMD优化计算 
        // 这里太TM适合用SIMD了
        // 计算16个点的值，然后返回一个bool结果
        // 但怎么实现呢？

        var func = Expression.Lambda<Func<DoubleNumber, DoubleNumber, DoubleNumber>>(expression, originalX, originalY);
        var idx = 0;
        var exps = new Expression[16];
        foreach (var xParam in xParams)
        foreach (var yParam in yParams)
        {
            var callExp = Expression.Invoke(func, xParam, yParam);
            exps[idx++] = callExp;
        }

        return Expression.NewArrayInit(typeof(DoubleNumber), exps);
    }

    private static MethodInfo GetInfo(Delegate action)
    {
        return action.Method;
    }
}