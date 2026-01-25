using System.Runtime.CompilerServices;
using MathNet.Numerics;
using CGMath = CsGrafeq.Numeric.CsGrafeqMath;

namespace CsGrafeq.Numeric;

public struct DoubleNumber : IComputableNumber<DoubleNumber>
{
    public readonly double Value;

    public DoubleNumber(double value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sqrt(DoubleNumber num)
    {
        return new DoubleNumber(Math.Sqrt(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cbrt(DoubleNumber num)
    {
        return new DoubleNumber(Math.Cbrt(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Pow(DoubleNumber num, DoubleNumber exp)
    {
        return new DoubleNumber(Math.Pow(num.Value, exp.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Exp(DoubleNumber num)
    {
        return new DoubleNumber(Math.Exp(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Log(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(Math.Log(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Lg(DoubleNumber num)
    {
        return new DoubleNumber(Math.Log10(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Ln(DoubleNumber num)
    {
        return new DoubleNumber(Math.Log(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sin(DoubleNumber num)
    {
        return new DoubleNumber(Math.Sin(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cos(DoubleNumber num)
    {
        return new DoubleNumber(Math.Cos(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Tan(DoubleNumber num)
    {
        return new DoubleNumber(Math.Tan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cot(DoubleNumber num)
    {
        return new DoubleNumber(1.0 / Math.Tan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcSin(DoubleNumber num)
    {
        return new DoubleNumber(Math.Asin(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcCos(DoubleNumber num)
    {
        return new DoubleNumber(Math.Acos(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcTan(DoubleNumber num)
    {
        return new DoubleNumber(Math.Atan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Tanh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Tanh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cosh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Cosh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sinh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Sinh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcCosh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Acosh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcTanh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Atanh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcSinh(DoubleNumber num)
    {
        return new DoubleNumber(Math.Asinh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Floor(DoubleNumber num)
    {
        return new DoubleNumber(Math.Floor(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Ceil(DoubleNumber num)
    {
        return new DoubleNumber(Math.Ceiling(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber GCD(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(CGMath.GCD((long)num1.Value, (long)num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber LCM(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(CGMath.LCM((long)num1.Value, (long)num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sgn(DoubleNumber num)
    {
        return new DoubleNumber(Math.Sign(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Abs(DoubleNumber num)
    {
        return new DoubleNumber(Math.Abs(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Median(DoubleNumber num1, DoubleNumber num2, DoubleNumber num3)
    {
        return new DoubleNumber(CGMath.Median(num1.Value, num2.Value, num3.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Min(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(num1.Value < num2.Value ? num1.Value : num2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Max(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(num1.Value > num2.Value ? num1.Value : num2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber MaxOf(IEnumerable<DoubleNumber> nums)
    {
        return new DoubleNumber(nums.Select(static o => o.Value).Max());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber MinOf(IEnumerable<DoubleNumber> nums)
    {
        return new DoubleNumber(nums.Select(static o => o.Value).Min());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcTan2(DoubleNumber y, DoubleNumber x)
    {
        return new DoubleNumber(Math.Atan2(y.Value, x.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Mod(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(num1.Value % num2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Gamma(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.Gamma(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber LnGamma(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.GammaLn(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Psi(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.DiGamma(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Erf(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.Erf(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Erfc(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.Erfc(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Erfinv(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.ErfInv(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Erfcinv(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.ErfcInv(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Digamma(DoubleNumber num)
    {
        return new DoubleNumber(SpecialFunctions.DiGamma(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber BesselJ(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(SpecialFunctions.BesselJ(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber BesselY(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(SpecialFunctions.BesselY(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber BesselI(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(SpecialFunctions.BesselI(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber BesselK(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(SpecialFunctions.BesselK(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator +(DoubleNumber a, DoubleNumber b)
    {
        return new DoubleNumber(a.Value + b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator -(DoubleNumber a, DoubleNumber b)
    {
        return new DoubleNumber(a.Value - b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator *(DoubleNumber a, DoubleNumber b)
    {
        return new DoubleNumber(a.Value * b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator /(DoubleNumber a, DoubleNumber b)
    {
        return new DoubleNumber(a.Value / b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator -(DoubleNumber a)
    {
        return new DoubleNumber(-a.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber operator %(DoubleNumber a, DoubleNumber b)
    {
        return new DoubleNumber(a.Value % b.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber CreateFromDouble(double num)
    {
        return new DoubleNumber(num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Clone(DoubleNumber source)
    {
        return source;
    }

    public static bool NeedClone => false;
}