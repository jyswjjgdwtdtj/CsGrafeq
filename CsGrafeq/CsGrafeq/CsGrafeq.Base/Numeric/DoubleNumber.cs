using System.Runtime.CompilerServices;
using sysMath = System.Math;

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
        return new DoubleNumber(sysMath.Sqrt(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cbrt(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Cbrt(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Pow(DoubleNumber num, DoubleNumber exp)
    {
        return new DoubleNumber(sysMath.Pow(num.Value, exp.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Exp(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Exp(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Log(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(sysMath.Log(num1.Value, num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Lg(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Log10(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Ln(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Log(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sin(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Sin(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cos(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Cos(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Tan(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Tan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cot(DoubleNumber num)
    {
        return new DoubleNumber(1.0 / sysMath.Tan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcSin(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Asin(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcCos(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Acos(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcTan(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Atan(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Tanh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Tanh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Cosh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Cosh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sinh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Sinh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcCosh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Acosh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcTanh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Atanh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber ArcSinh(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Asinh(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Floor(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Floor(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Ceil(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Ceiling(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber GCD(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(Math.GCD((long)num1.Value, (long)num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber LCM(DoubleNumber num1, DoubleNumber num2)
    {
        return new DoubleNumber(Math.LCM((long)num1.Value, (long)num2.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Sgn(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Sign(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Abs(DoubleNumber num)
    {
        return new DoubleNumber(sysMath.Abs(num.Value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DoubleNumber Median(DoubleNumber num1, DoubleNumber num2, DoubleNumber num3)
    {
        return new DoubleNumber(Math.DoubleMedian(num1.Value, num2.Value, num3.Value));
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
}