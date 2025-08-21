using System.Numerics;
using CsGrafeq.Numeric;
using static CsGrafeq.Interval.Def;

namespace CsGrafeq.Interval.Interface;

public interface IInterval
{
    public Def Def{get;}
}
public interface IInterval<T>:IRange,IComputableNumber<T>,IInterval where T : IInterval<T>
{
    static abstract T Create(double min, double max,Def def);
    static abstract T CreateWithNumber(double num);
    public static T InValid=>T.Create(double.NaN,double.NaN,FF);
    static Def Equal(T left,T right)=>left==right;
    static Def Less(T left,T right)=>left<right;
    static Def Greater(T left,T right)=>left>right;
    static Def LessEqual(T left,T right)=>left<=right;
    static Def GreaterEqual(T left,T right)=>left>=right;
    
    static abstract Def operator ==(T left, T right);
    [Obsolete("", true)]
    static virtual Def operator !=(T left, T right) => FF;
    static abstract Def operator <(T left, T right);
    static abstract Def operator >(T left, T right);
    static abstract Def operator <=(T left, T right);
    static abstract Def operator >=(T left, T right);
    static abstract T Clone(T source);
}