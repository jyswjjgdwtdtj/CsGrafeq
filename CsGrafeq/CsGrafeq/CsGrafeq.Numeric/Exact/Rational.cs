
using System.ComponentModel.DataAnnotations;

namespace CsGrafeq.Numeric.Exact;

public readonly struct Rational([Range(-1,1)]int sign,uint numerator, uint denominator)
{
    public uint Numerator { get;  } = numerator;
    public uint Denominator { get; } = denominator;
    public int Sign { get;} = sign;
    public double ToFloat()
    {
        return (double)Sign * Numerator / Denominator;
    }
    public override string ToString()
    {
        return $"{Sign * Numerator}/{Denominator}";
    }
}