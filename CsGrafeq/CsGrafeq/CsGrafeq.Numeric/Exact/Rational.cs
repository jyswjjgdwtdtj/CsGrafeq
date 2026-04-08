using CsGrafeq.Result;
using System.ComponentModel.DataAnnotations;

namespace CsGrafeq.Numeric.Exact;

public readonly struct Rational([Range(-1,1)]int sign,uint numerator, uint denominator) : IEquatable<Rational>
{
    private const int MaxValue = 10000;
    public uint Numerator { get;  } = numerator;
    public uint Denominator { get; } = denominator;
    public int Sign { get;} = sign;
    public double ToFloat()
    {
        return (double)Sign * Numerator / Denominator;
    }
    public override string ToString()
    {
        if(denominator == 0)
            throw new ArgumentOutOfRangeException(nameof(denominator));
        if(numerator == 0)
            return "0";
        if(denominator == 1)
            return $"{Sign * Numerator}";
        if(denominator == 10)
            return $"{Sign * Numerator/10d}";
        if(denominator == 5)
            return $"{Sign * Numerator/5d}";
        return $"{Sign * Numerator}/{Denominator}";
    }

    public Rational WithSign([Range(-1, 1)] int sign)
    {
        return  new Rational(sign, Numerator, Denominator);
    }

    public bool Equals(Rational other)
    {
        return Numerator == other.Numerator && Denominator == other.Denominator && Sign == other.Sign;
    }

    public override bool Equals(object? obj)
    {
        return obj is Rational other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator, Sign);
    }

    public static Result<Rational, double> operator +(Rational r1, Rational r2)
    {
        if (r1.Denominator == 0 || r2.Denominator == 0)
            return double.NaN;
    
        try
        {
            checked
            {
                long a = r1.Sign * r1.Numerator;
                long b = r2.Sign * r2.Numerator;
                long den = (long)r1.Denominator * r2.Denominator;
                long num = a * r2.Denominator + b * r1.Denominator;
    
                if (num == 0)
                    return new Rational(0, 0, 1);
    
                int sign = num < 0 ? -1 : 1;
                ulong absNum = (ulong)Math.Abs(num);
                ulong absDen = (ulong)den;
    
                ulong g = Gcd(absNum, absDen);
                absNum /= g;
                absDen /= g;
    
                if (absNum > uint.MaxValue || absDen > uint.MaxValue)
                    return r1.ToFloat() + r2.ToFloat();
                if(absNum<=MaxValue && absDen<=MaxValue)
                    return new Rational(sign, (uint)absNum, (uint)absDen);
                return r1.ToFloat() + r2.ToFloat();
            }
        }
        catch (OverflowException)
        {
            return r1.ToFloat() + r2.ToFloat();
        }
    }
    
    public static Result<Rational, double> operator -(Rational r1, Rational r2)
    {
        if (r2.Numerator == 0)
            return r1;
        return r1 + r2.WithSign(-r2.Sign);
    }
    
    public static Result<Rational, double> operator *(Rational r1, Rational r2)
    {
        if (r1.Denominator == 0 || r2.Denominator == 0)
            return double.NaN;
    
        if (r1.Numerator == 0 || r2.Numerator == 0)
            return new Rational(0, 0, 1);
    
        try
        {
            checked
            {
                int sign = r1.Sign * r2.Sign;
                ulong num = r1.Numerator * r2.Numerator;
                ulong den = r1.Denominator * r2.Denominator;
    
                ulong g = Gcd(num, den);
                num /= g;
                den /= g;
    
                if (num > MaxValue || den >MaxValue)
                    return r1.ToFloat() * r2.ToFloat();
    
                return new Rational(sign, (uint)num, (uint)den);
            }
        }
        catch (OverflowException)
        {
            return r1.ToFloat() * r2.ToFloat();
        }
    }

    public Rational GetReciprocal()
    {
        return new Rational(Sign, Denominator, Numerator);
    }
    public static Result<Rational, double> operator /(Rational r1, Rational r2)
    {
        if (r2.Numerator == 0)
            return double.NaN;
        return r1 * r2.GetReciprocal();
    }
    
    private static ulong Gcd(ulong a, ulong b)
    {
        while (b != 0)
        {
            ulong t = a % b;
            a = b;
            b = t;
        }
        return a == 0 ? 1UL : a;
    }
}