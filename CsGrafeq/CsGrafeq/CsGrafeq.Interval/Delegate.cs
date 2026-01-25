using CsGrafeq.Interval.Interface;

namespace CsGrafeq.Interval;

public delegate Def IntervalHandler<in T>(T x, T y) where T : IInterval<T>,allows ref struct;