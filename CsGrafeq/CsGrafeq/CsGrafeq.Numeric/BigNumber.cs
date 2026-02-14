global using BigNumber = CsGrafeq.Numeric.BigNumber<long, decimal>;
global using BigPoint = CsGrafeq.PointBase<CsGrafeq.Numeric.BigNumber<long, decimal>>;
using System.Globalization;
using System.Numerics;

namespace CsGrafeq.Numeric;

public readonly struct BigNumber<TInteger, TDecimal> :
    INumber<BigNumber<TInteger, TDecimal>>
    where TInteger : struct, IBinaryInteger<TInteger>
    where TDecimal : struct, IFloatingPoint<TDecimal>
{
    public readonly TInteger IntegerPart { get; }
    public readonly TDecimal DecimalPart { get; } // always in [0, 1)

    public BigNumber(TInteger integerPart, TDecimal decimalPart)
    {
        (IntegerPart, DecimalPart) = Normalize(integerPart, decimalPart);
    }

    private static BigNumber<TInteger, TDecimal> CreateNormalized(TInteger integerPart, TDecimal decimalPart)
    {
        return new BigNumber<TInteger, TDecimal>(integerPart, decimalPart);
    }

    private static (TInteger IntegerPart, TDecimal DecimalPart) Normalize(TInteger integerPart, TDecimal decimalPart)
    {
        // Move integral carry from decimalPart into integerPart.
        var carryDec = TDecimal.Truncate(decimalPart); // integral value in decimal's domain
        if (carryDec != TDecimal.Zero)
        {
            integerPart += TInteger.CreateChecked(carryDec);
            decimalPart -= carryDec;
        }

        // Ensure decimalPart in [0, 1)
        if (decimalPart < TDecimal.Zero)
        {
            integerPart -= TInteger.One;
            decimalPart += TDecimal.One;
        }
        else if (decimalPart >= TDecimal.One)
        {
            integerPart += TInteger.One;
            decimalPart -= TDecimal.One;
        }

        // Safety loop for large carry due to precision quirks (rare, but cheap).
        while (decimalPart < TDecimal.Zero)
        {
            integerPart -= TInteger.One;
            decimalPart += TDecimal.One;
        }

        while (decimalPart >= TDecimal.One)
        {
            integerPart += TInteger.One;
            decimalPart -= TDecimal.One;
        }

        return (integerPart, decimalPart);
    }

    public static TDecimal ToDecimal(BigNumber<TInteger, TDecimal> value)
    {
        return TDecimal.CreateChecked(value.IntegerPart) + value.DecimalPart;
    }

    public static BigNumber<TInteger, TDecimal> FromDecimal(TDecimal value)
    {
        var i = TDecimal.Truncate(value);
        return CreateNormalized(TInteger.CreateChecked(i), value - i);
    }

    // -------- convenience --------
    public override string ToString()
    {
        return $"{IntegerPart}+{DecimalPart}";
    }

    // -------- operators (always normalized) --------
    public static BigNumber<TInteger, TDecimal> operator +(BigNumber<TInteger, TDecimal> a,
        BigNumber<TInteger, TDecimal> b)
    {
        return CreateNormalized(a.IntegerPart + b.IntegerPart, a.DecimalPart + b.DecimalPart);
    }

    public static BigNumber<TInteger, TDecimal> operator -(BigNumber<TInteger, TDecimal> a,
        BigNumber<TInteger, TDecimal> b)
    {
        return CreateNormalized(a.IntegerPart - b.IntegerPart, a.DecimalPart - b.DecimalPart);
    }

    public static BigNumber<TInteger, TDecimal> operator -(BigNumber<TInteger, TDecimal> value)
    {
        return CreateNormalized(-value.IntegerPart, -value.DecimalPart);
    }

    public static BigNumber<TInteger, TDecimal> operator *(BigNumber<TInteger, TDecimal> a,
        BigNumber<TInteger, TDecimal> b)
    {
        var integerPart = a.IntegerPart * b.IntegerPart;
        var decimalPart = TDecimal.CreateChecked(a.IntegerPart) * b.DecimalPart +
                          TDecimal.CreateChecked(b.IntegerPart) * a.DecimalPart +
                          a.DecimalPart * b.DecimalPart;
        return CreateNormalized(integerPart, decimalPart);
    }

    public static BigNumber<TInteger, TDecimal> operator *(BigNumber<TInteger, TDecimal> a, TDecimal b)
    {
        return a * new BigNumber<TInteger, TDecimal>(TInteger.Zero, b);
    }

    public static BigNumber<TInteger, TDecimal> operator /(BigNumber<TInteger, TDecimal> a,
        BigNumber<TInteger, TDecimal> b)
    {
        var q = ToDecimal(a) / ToDecimal(b);
        return FromDecimal(q);
    }

    public static BigNumber<TInteger, TDecimal> operator %(BigNumber<TInteger, TDecimal> a,
        BigNumber<TInteger, TDecimal> b)
    {
        var r = ToDecimal(a) % ToDecimal(b);
        return FromDecimal(r);
    }

    public static BigNumber<TInteger, TDecimal> operator ++(BigNumber<TInteger, TDecimal> value)
    {
        return value + One;
    }

    public static BigNumber<TInteger, TDecimal> operator --(BigNumber<TInteger, TDecimal> value)
    {
        return value - One;
    }

    // -------- identities / radix --------
    public static BigNumber<TInteger, TDecimal> Zero => new(TInteger.Zero, TDecimal.Zero);
    public static BigNumber<TInteger, TDecimal> One => new(TInteger.One, TDecimal.Zero);
    public static int Radix => TInteger.Radix;

    public static BigNumber<TInteger, TDecimal> AdditiveIdentity => Zero;
    public static BigNumber<TInteger, TDecimal> MultiplicativeIdentity => One;

    // -------- sign / magnitude --------
    public static BigNumber<TInteger, TDecimal> Abs(BigNumber<TInteger, TDecimal> value)
    {
        return value < Zero ? -value : value;
    }

    public static BigNumber<TInteger, TDecimal> Clamp(BigNumber<TInteger, TDecimal> value,
        BigNumber<TInteger, TDecimal> min, BigNumber<TInteger, TDecimal> max)
    {
        return value < min ? min : (value > max ? max : value);
    }

    public static BigNumber<TInteger, TDecimal> Max(BigNumber<TInteger, TDecimal> x, BigNumber<TInteger, TDecimal> y)
    {
        return x >= y ? x : y;
    }

    public static BigNumber<TInteger, TDecimal> Min(BigNumber<TInteger, TDecimal> x, BigNumber<TInteger, TDecimal> y)
    {
        return x <= y ? x : y;
    }

    public static BigNumber<TInteger, TDecimal> MaxMagnitude(BigNumber<TInteger, TDecimal> x,
        BigNumber<TInteger, TDecimal> y)
    {
        return Abs(x) >= Abs(y) ? x : y;
    }

    public static BigNumber<TInteger, TDecimal> MaxMagnitudeNumber(BigNumber<TInteger, TDecimal> x,
        BigNumber<TInteger, TDecimal> y)
    {
        return MaxMagnitude(x, y);
    }

    public static BigNumber<TInteger, TDecimal> MinMagnitude(BigNumber<TInteger, TDecimal> x,
        BigNumber<TInteger, TDecimal> y)
    {
        return Abs(x) <= Abs(y) ? x : y;
    }

    public static BigNumber<TInteger, TDecimal> MinMagnitudeNumber(BigNumber<TInteger, TDecimal> x,
        BigNumber<TInteger, TDecimal> y)
    {
        return MinMagnitude(x, y);
    }

    public static BigNumber<TInteger, TDecimal> Sign(BigNumber<TInteger, TDecimal> value)
    {
        return CreateChecked(TDecimal.Sign(ToDecimal(value)));
    }

    // -------- classification --------
    public static bool IsCanonical(BigNumber<TInteger, TDecimal> value)
    {
        return true;
    }

    public static bool IsComplexNumber(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsFinite(BigNumber<TInteger, TDecimal> value)
    {
        return true;
    }

    public static bool IsImaginaryNumber(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsInfinity(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsInteger(BigNumber<TInteger, TDecimal> value)
    {
        return value.DecimalPart == TDecimal.Zero;
    }

    public static bool IsNaN(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsNegative(BigNumber<TInteger, TDecimal> value)
    {
        return value.IntegerPart < TInteger.Zero;
    }

    public static bool IsNegativeInfinity(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsNormal(BigNumber<TInteger, TDecimal> value)
    {
        return value != Zero;
    }

    public static bool IsPositive(BigNumber<TInteger, TDecimal> value)
    {
        return value.IntegerPart > TInteger.Zero ||
               (value.IntegerPart == TInteger.Zero && value.DecimalPart > TDecimal.Zero);
    }

    public static bool IsPositiveInfinity(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsRealNumber(BigNumber<TInteger, TDecimal> value)
    {
        return true;
    }

    public static bool IsSubnormal(BigNumber<TInteger, TDecimal> value)
    {
        return false;
    }

    public static bool IsZero(BigNumber<TInteger, TDecimal> value)
    {
        return value.IntegerPart == TInteger.Zero && value.DecimalPart == TDecimal.Zero;
    }

    public static bool IsEvenInteger(BigNumber<TInteger, TDecimal> value)
    {
        return value.DecimalPart == TDecimal.Zero && TInteger.IsEvenInteger(value.IntegerPart);
    }

    public static bool IsOddInteger(BigNumber<TInteger, TDecimal> value)
    {
        return value.DecimalPart == TDecimal.Zero && TInteger.IsOddInteger(value.IntegerPart);
    }

    public static bool IsPositiveInteger(BigNumber<TInteger, TDecimal> value)
    {
        return value.DecimalPart == TDecimal.Zero && value.IntegerPart > TInteger.Zero;
    }

    public static bool IsNegativeInteger(BigNumber<TInteger, TDecimal> value)
    {
        return value.DecimalPart == TDecimal.Zero && value.IntegerPart < TInteger.Zero;
    }

    // -------- rounding helpers --------
    public static BigNumber<TInteger, TDecimal> Floor(BigNumber<TInteger, TDecimal> value)
    {
        // normalized: decimal in [0,1), so floor is IntegerPart for non-negative.
        if (value.IntegerPart >= TInteger.Zero)
            return new BigNumber<TInteger, TDecimal>(value.IntegerPart, TDecimal.Zero);
        // negative: since decimal is positive fraction, e.g. -3 + 0.2 means -2.8; floor => -3
        // but representation of -2.8 normalized would be -3 + 0.2, so IntegerPart already floor.
        return new BigNumber<TInteger, TDecimal>(value.IntegerPart, TDecimal.Zero);
    }

    public static BigNumber<TInteger, TDecimal> Ceiling(BigNumber<TInteger, TDecimal> value)
    {
        if (value.DecimalPart == TDecimal.Zero) return value;
        return value.IntegerPart >= TInteger.Zero
            ? new BigNumber<TInteger, TDecimal>(value.IntegerPart + TInteger.One, TDecimal.Zero)
            : new BigNumber<TInteger, TDecimal>(value.IntegerPart, TDecimal.Zero);
    }

    public static BigNumber<TInteger, TDecimal> Truncate(BigNumber<TInteger, TDecimal> value)
    {
        // toward 0
        return value.IntegerPart >= TInteger.Zero
            ? new BigNumber<TInteger, TDecimal>(value.IntegerPart, TDecimal.Zero)
            : value.DecimalPart == TDecimal.Zero
                ? value
                : new BigNumber<TInteger, TDecimal>(value.IntegerPart + TInteger.One, TDecimal.Zero);
    }

    public static BigNumber<TInteger, TDecimal> Round(BigNumber<TInteger, TDecimal> value, int digits,
        MidpointRounding mode)
    {
        return FromDecimal(TDecimal.Round(ToDecimal(value), digits, mode));
    }

    public static BigNumber<TInteger, TDecimal> Round(BigNumber<TInteger, TDecimal> value)
    {
        return FromDecimal(TDecimal.Round(ToDecimal(value)));
    }

    public static BigNumber<TInteger, TDecimal> Round(BigNumber<TInteger, TDecimal> value, MidpointRounding mode)
    {
        return FromDecimal(TDecimal.Round(ToDecimal(value), mode));
    }

    public static BigNumber<TInteger, TDecimal> Round(BigNumber<TInteger, TDecimal> value, int digits)
    {
        return FromDecimal(TDecimal.Round(ToDecimal(value), digits));
    }

    // -------- parsing / formatting --------
    public static BigNumber<TInteger, TDecimal> Parse(string s, IFormatProvider? provider)
    {
        return FromDecimal(TDecimal.Parse(s, provider));
    }

    public static BigNumber<TInteger, TDecimal> Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return FromDecimal(TDecimal.Parse(s, provider));
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out BigNumber<TInteger, TDecimal> result)
    {
        if (TDecimal.TryParse(s, provider, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider,
        out BigNumber<TInteger, TDecimal> result)
    {
        if (TDecimal.TryParse(s, provider, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    // -------- create / convert --------
    public static BigNumber<TInteger, TDecimal> CreateChecked<TOther>(TOther value) where TOther : INumberBase<TOther>
    {
        return FromDecimal(TDecimal.CreateChecked(value));
    }

    public static BigNumber<TInteger, TDecimal> CreateSaturating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        return FromDecimal(TDecimal.CreateSaturating(value));
    }

    public static BigNumber<TInteger, TDecimal> CreateTruncating<TOther>(TOther value)
        where TOther : INumberBase<TOther>
    {
        return FromDecimal(TDecimal.CreateTruncating(value));
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out BigNumber<TInteger, TDecimal> result)
        where TOther : INumberBase<TOther>
    {
        if (TDecimal.TryConvertFromChecked(value, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out BigNumber<TInteger, TDecimal> result)
        where TOther : INumberBase<TOther>
    {
        if (TDecimal.TryConvertFromSaturating(value, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out BigNumber<TInteger, TDecimal> result)
        where TOther : INumberBase<TOther>
    {
        if (TDecimal.TryConvertFromTruncating(value, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryConvertToChecked<TOther>(BigNumber<TInteger, TDecimal> value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        return TDecimal.TryConvertToChecked(ToDecimal(value), out result);
    }

    public static bool TryConvertToSaturating<TOther>(BigNumber<TInteger, TDecimal> value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        return TDecimal.TryConvertToSaturating(ToDecimal(value), out result);
    }

    public static bool TryConvertToTruncating<TOther>(BigNumber<TInteger, TDecimal> value, out TOther result)
        where TOther : INumberBase<TOther>
    {
        return TDecimal.TryConvertToTruncating(ToDecimal(value), out result);
    }

    // -------- INumberBase / ISpanFormattable --------
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToDecimal(this).ToString(format, formatProvider);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        return ToDecimal(this).TryFormat(destination, out charsWritten, format, provider);
    }

    // -------- comparisons --------
    public int CompareTo(BigNumber<TInteger, TDecimal> other)
    {
        var c = IntegerPart.CompareTo(other.IntegerPart);
        return c != 0 ? c : DecimalPart.CompareTo(other.DecimalPart);
    }

    public bool Equals(BigNumber<TInteger, TDecimal> other)
    {
        return IntegerPart == other.IntegerPart && DecimalPart == other.DecimalPart;
    }

    public override bool Equals(object? obj)
    {
        return obj is BigNumber<TInteger, TDecimal> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IntegerPart, DecimalPart);
    }

    public static implicit operator TDecimal(BigNumber<TInteger, TDecimal> value)
    {
        return ToDecimal(value);
    }

    public TDecimal ToDecimal()
    {
        return ToDecimal(this);
    }

    public static bool operator ==(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(BigNumber<TInteger, TDecimal> left, BigNumber<TInteger, TDecimal> right)
    {
        return left.CompareTo(right) >= 0;
    }

    public int CompareTo(object? obj)
    {
        if (obj is BigNumber<TInteger, TDecimal> other)
            return CompareTo(other);
        throw new ArgumentException("Object must be of type BigNumber<TInteger, TDecimal>");
    }

    public static implicit operator BigNumber<TInteger, TDecimal>(TInteger value)
    {
        return new BigNumber<TInteger, TDecimal>(value, TDecimal.Zero);
    }

    public static BigNumber<TInteger, TDecimal> operator +(BigNumber<TInteger, TDecimal> value)
    {
        return value;
    }

    public static BigNumber<TInteger, TDecimal> Parse(ReadOnlySpan<char> s, NumberStyles style,
        IFormatProvider? provider)
    {
        var dec = TDecimal.Parse(s, style, provider);
        return FromDecimal(dec);
    }

    public static BigNumber<TInteger, TDecimal> Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        return Parse(s.AsSpan(), style, provider);
    }

    public static BigNumber<TInteger, TDecimal> Parse(ReadOnlySpan<byte> utf8Text, NumberStyles style,
        IFormatProvider? provider)
    {
        var dec = TDecimal.Parse(utf8Text, style, provider);
        return FromDecimal(dec);
    }

    public static BigNumber<TInteger, TDecimal> Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        var dec = TDecimal.Parse(utf8Text, provider);
        return FromDecimal(dec);
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider,
        out BigNumber<TInteger, TDecimal> result)
    {
        if (TDecimal.TryParse(s, style, provider, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider,
        out BigNumber<TInteger, TDecimal> result)
    {
        return TryParse(s.AsSpan(), style, provider, out result);
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, NumberStyles style, IFormatProvider? provider,
        out BigNumber<TInteger, TDecimal> result)
    {
        if (TDecimal.TryParse(utf8Text, style, provider, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider,
        out BigNumber<TInteger, TDecimal> result)
    {
        if (TDecimal.TryParse(utf8Text, provider, out var dec))
        {
            result = FromDecimal(dec);
            return true;
        }

        result = Zero;
        return false;
    }
}