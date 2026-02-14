using CsGrafeq.Compiler;
using CsGrafeq.Interval.Interface;
using CsGrafeq.Variables;

namespace CsGrafeq.Interval;

public class HasReferenceIntervalSetFunc<T>(IntervalHandler<T> function, VariablesEnum reference)
    : HasReferenceFunction<IntervalHandler<T>>(function, reference) where T : IInterval<T>, allows ref struct
{
}