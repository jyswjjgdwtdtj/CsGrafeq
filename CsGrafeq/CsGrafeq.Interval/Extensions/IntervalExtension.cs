using CsGrafeq.Interval.Interface;
using static CsGrafeq.Interval.Def;

namespace CsGrafeq.Interval.Extensions;

public static class IntervalExtension
{
    public static T Create<T>(double num) where T : IInterval<T>
    {
        return T.Create(num, num, TT);
    }

    public static T Create<T>(double min, double max) where T : IInterval<T>
    {
        return T.Create(min, max, TT);
    }

    public static T Create<T>(double num, Def def) where T : IInterval<T>
    {
        return T.Create(num, num, def);
    }
}