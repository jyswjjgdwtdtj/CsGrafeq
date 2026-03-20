
namespace CsGrafeq.Numeric.Exact;

public struct Surd
{
    public Surd(Rational rational,uint radicand)
    {
        RationalPart = rational;
        Radicand = radicand;
        Reduce();
    }

    public uint Radicand;
    public Rational RationalPart;
    public double ToFloat()
    {
        return Math.Sqrt(Radicand)*RationalPart.ToFloat();
    }
    public override string ToString()
    {
        if(Radicand == 0u|| RationalPart.Numerator == 0)
            return "0";
        if(Radicand==1u)
            return RationalPart.ToString();
        return $"{RationalPart}*√({Radicand})";
    }
    public void Reduce()
    {
        // 0 * √(n) => 0（用 √(1) 兜底）
        if (Radicand == 0u || RationalPart.Numerator == 0)
        {
            RationalPart = new Rational(0, 0, 1);
            Radicand = 0;
        }

        uint n = Radicand;

        // 把 n 的平方因子尽可能提出：n = outside^2 * inside
        uint outside = 1u;
        uint inside = n;

        // divide 4
        while ((inside & 0x11) == 0u) // divisible by 4
        {
            inside >>= 2;
            outside <<= 1;
        }

        foreach (var prime in PrimeTable.UIntType0To1000Except2)
        {
            uint p2 = prime * prime;
            while (inside % p2 == 0u)
            {
                inside /= p2;
                outside *= prime;
            }   
        }
        RationalPart = new Rational(RationalPart.Sign, RationalPart.Numerator * outside, RationalPart.Denominator);
        Radicand = inside;
    }
    public bool IsTooComplicated()
    {
        return Radicand > 200||RationalPart.IsTooComplicated();
    }
    
}