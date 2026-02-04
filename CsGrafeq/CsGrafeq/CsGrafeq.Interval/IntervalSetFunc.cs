using CsGrafeq.Compiler;
using CsGrafeq.Interval.Interface;

namespace CsGrafeq.Interval;

public class HasReferenceIntervalSetFunc<T>(IntervalHandler<T> function, EnglishCharEnum reference)
    : HasReferenceFunction<IntervalHandler<T>>(function, reference) where T : IInterval<T>, allows ref struct
{
}