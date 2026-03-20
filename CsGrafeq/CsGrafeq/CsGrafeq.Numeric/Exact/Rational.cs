
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace CsGrafeq.Numeric.Exact;

public record struct Rational
{
    public Rational([Range(-1,1)]int sign, uint numerator, uint denominator)
    {
        Sign = sign;  
        Numerator = numerator;
        Denominator = denominator;
        Reduce();
    }
    /// <summary>
    /// 分子
    /// </summary>
    public uint Numerator { get; set; }
    /// <summary>
    /// 分母
    /// </summary>
    public uint Denominator { get; set; }
    public int Sign { get; set; }

    public double ToFloat()
    {
        return (double)Sign * Numerator / Denominator;
    }
    public override string ToString()
    {
        if (Numerator == 0)
            return "0";
        if(Denominator == 1)
            return $"{Sign * Numerator}";
        return $"{Sign * Numerator}/{Denominator}";
    }

    public void Reduce()
    {
        uint g = Gcd(Numerator, Denominator);
        Numerator /= g;
        Denominator /= g;
    }
    private static T Gcd<T>(T a, T b) where T:struct, IBinaryInteger<T>
    {
        while (b != T.Zero)
        {
            T t = a % b;
            a = b;
            b = t;
        }
        return a == T.Zero ? T.One : a;
    }
    public bool IsTooComplicated()
    {
        return Numerator > 5000||Denominator>5000;
    }
    public static (int sign,ulong numerator,ulong denominator) operator +(Rational a, Rational b)
    {
        long an = a.Sign * a.Numerator;
        long ad = a.Denominator;
        long bn = b.Sign * b.Numerator;
        long bd = b.Denominator;
        long numerator = an * bd + bn * ad;
        ulong denominator = (ulong)(ad * bd);
        int sign = long.Sign(numerator);
        ulong absn=(ulong)long.Abs(numerator);
        ulong g = Gcd(absn, denominator);
        return (sign, absn / g, denominator / g);
    }
}