using CsGrafeq.Numeric;
using static CsGrafeq.Interval.Def;

namespace CsGrafeq.Interval.Interface;

public interface IInterval
{
    public Def Def { get; }
}

public interface IInterval<T> : IRange, IComputableNumber<T>, IInterval, INeedClone<T> where T : IInterval<T>
{
    public static T InValid => T.Create(double.NaN, double.NaN, FF);
    static abstract T Create(double min, double max, Def def);

    static Def Equal(T left, T right)
    {
        return left == right;
    }

    static Def Less(T left, T right)
    {
        return left < right;
    }

    static Def Greater(T left, T right)
    {
        return left > right;
    }

    static Def LessEqual(T left, T right)
    {
        return left <= right;
    }

    static Def GreaterEqual(T left, T right)
    {
        return left >= right;
    }

    static abstract Def operator ==(T left, T right);

    [Obsolete("", true)]
    static virtual Def operator !=(T left, T right)
    {
        return FF;
    }

    static abstract Def operator <(T left, T right);
    static abstract Def operator >(T left, T right);
    static abstract Def operator <=(T left, T right);
    static abstract Def operator >=(T left, T right);
}