using CsGrafeq.Utilities;
using MathNet.Numerics;
using MathNet.Symbolics;
using CsGrafeq.Result;

namespace CsGrafeq.Numeric.Exact;

public readonly struct ExactNumber : IComputableNumber<ExactNumber>, IEquatable<ExactNumber>
{
    public const double Epsilon = 1e-10;
    public static readonly ExactNumber NaN = CreateFloat(double.NaN);
    public ExactNumberType NumberType { get; init; }

    public ExactNumber()
    {
    }
    private Rational RationalPart { get; init; }
    private double FloatPart { get; init; }

    public static ExactNumber CreateRational(Rational rational)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.Rational,
            RationalPart = rational
        };
    }
    

    public static ExactNumber CreateFloat(double value,bool tryOptimize=false)
    {
        var d = value;
        if (tryOptimize)
        {
            if (value == (int)value)
            {
                // value 是一个整数，可以直接创建一个 Rational 来存储。
                return CreateRational(new Rational(Math.Sign(value), (uint)Math.Abs((long)value),1));
            }
            var sgn= Math.Sign(value);
            value = Math.Abs(value);
            uint integerPart = (uint)Math.Floor(value);
            var decimalPart= value - integerPart;
            if (DecimalPart.TryFindNear(decimalPart, out var index))
            {
                var res = (DecimalPart.Backward[index] + new Rational(1, integerPart, 1));
                if(res.IsSuccessful)
                    return CreateRational(res.Success().WithSign(sgn));
            } 
        }
        return new ExactNumber
        {
            NumberType = ExactNumberType.Float,
            FloatPart = d
        };
    }
    public double ToFloat()
    {
        switch (NumberType)
        {
            case ExactNumberType.Rational:
                return RationalPart.ToFloat();
            case ExactNumberType.Float:
                return FloatPart;
            default: throw new NotImplementedException();
        }
    }

    // INeedClone<T>
    public static bool NeedClone => false;

    public static ExactNumber Clone(ExactNumber source) => source;

    // IHasOperatorNumber<T>
    public static implicit operator ExactNumber(double num)
        => CreateFloat(num);
    public static implicit operator ExactNumber(int num)
        => CreateFloat(num);

    public static ExactNumber operator +(ExactNumber left, ExactNumber right)
    {
        if (left.NumberType == ExactNumberType.Rational && right.NumberType == ExactNumberType.Rational)
            return ToExactNumber(left.RationalPart + right.RationalPart);
        return  CreateFloat(left.ToFloat() + right.ToFloat());
    }

    public static ExactNumber operator -(ExactNumber left, ExactNumber right)
    {
        if (left.NumberType == ExactNumberType.Rational && right.NumberType == ExactNumberType.Rational)
            return ToExactNumber(left.RationalPart - right.RationalPart);
        return  CreateFloat(left.ToFloat() - right.ToFloat());
    }

    public static ExactNumber operator *(ExactNumber left, ExactNumber right)
    {
        if (left.NumberType == ExactNumberType.Rational && right.NumberType == ExactNumberType.Rational)
            return ToExactNumber(left.RationalPart * right.RationalPart);
        return  CreateFloat(left.ToFloat() * right.ToFloat());
    }

    public static ExactNumber operator /(ExactNumber left, ExactNumber right)
    {
        if (left.NumberType == ExactNumberType.Rational && right.NumberType == ExactNumberType.Rational)
            return ToExactNumber(left.RationalPart / right.RationalPart);
        return  CreateFloat(left.ToFloat() / right.ToFloat());
    }

    public static ExactNumber operator %(ExactNumber left, ExactNumber right)
        => CreateFloat(left.ToFloat() % right.ToFloat());

    public static ExactNumber operator -(ExactNumber num)
    {
        switch (num.NumberType)
        {
            case ExactNumberType.Float:
                return -num.FloatPart;
            case ExactNumberType.Rational:
                return CreateRational(num.RationalPart.WithSign(-num.RationalPart.Sign));
            default: throw new NotImplementedException();
        }
    }

    // IComputableNumber<T>
    public static ExactNumber Sqrt(ExactNumber num)
        => CreateFloat(Math.Sqrt(num.ToFloat()));

    public static ExactNumber Cbrt(ExactNumber num)
        => CreateFloat(Math.Cbrt(num.ToFloat()));

    public static ExactNumber Pow(ExactNumber num, ExactNumber exp)
        => CreateFloat(Math.Pow(num.ToFloat(), exp.ToFloat()));

    public static ExactNumber Exp(ExactNumber num)
        => CreateFloat(Math.Exp(num.ToFloat()));

    public static ExactNumber Log(ExactNumber num1, ExactNumber num2)
        => CreateFloat(Math.Log(num1.ToFloat(), num2.ToFloat()));

    public static ExactNumber Lg(ExactNumber num)
        => CreateFloat(Math.Log10(num.ToFloat()));

    public static ExactNumber Ln(ExactNumber num)
        => CreateFloat(Math.Log(num.ToFloat()));

    public static ExactNumber Sin(ExactNumber num)
        => CreateFloat(Math.Sin(num.ToFloat()));

    public static ExactNumber Cos(ExactNumber num)
        => CreateFloat(Math.Cos(num.ToFloat()));

    public static ExactNumber Tan(ExactNumber num)
        => CreateFloat(Math.Tan(num.ToFloat()));

    public static ExactNumber Cot(ExactNumber num)
        => CreateFloat(1.0 / Math.Tan(num.ToFloat()));

    public static ExactNumber ArcSin(ExactNumber num)
        => CreateFloat(Math.Asin(num.ToFloat()));

    public static ExactNumber ArcCos(ExactNumber num)
        => CreateFloat(Math.Acos(num.ToFloat()));

    public static ExactNumber ArcTan(ExactNumber num)
        => CreateFloat(Math.Atan(num.ToFloat()));

    public static ExactNumber Tanh(ExactNumber num)
        => CreateFloat(Math.Tanh(num.ToFloat()));

    public static ExactNumber Cosh(ExactNumber num)
        => CreateFloat(Math.Cosh(num.ToFloat()));

    public static ExactNumber Sinh(ExactNumber num)
        => CreateFloat(Math.Sinh(num.ToFloat()));

    public static ExactNumber ArcCosh(ExactNumber num)
        => CreateFloat(Math.Acosh(num.ToFloat()));

    public static ExactNumber ArcTanh(ExactNumber num)
        => CreateFloat(Math.Atanh(num.ToFloat()));

    public static ExactNumber ArcSinh(ExactNumber num)
        => CreateFloat(Math.Asinh(num.ToFloat()));

    public static ExactNumber Floor(ExactNumber num)
        => CreateFloat(Math.Floor(num.ToFloat()));

    public static ExactNumber Ceil(ExactNumber num)
        => CreateFloat(Math.Ceiling(num.ToFloat()));

    public static ExactNumber GCD(ExactNumber num1, ExactNumber num2)
        => CreateFloat(CsGrafeq.Numeric.CsGrafeqMath.GCD((long)num1.ToFloat(), (long)num2.ToFloat()));

    public static ExactNumber LCM(ExactNumber num1, ExactNumber num2)
        => CreateFloat(CsGrafeq.Numeric.CsGrafeqMath.LCM((long)num1.ToFloat(), (long)num2.ToFloat()));

    public static ExactNumber Sgn(ExactNumber num)
        => CreateFloat(Math.Sign(num.ToFloat()));

    public static ExactNumber Abs(ExactNumber num)
        => CreateFloat(Math.Abs(num.ToFloat()));

    public static ExactNumber Median(ExactNumber num1, ExactNumber num2, ExactNumber num3)
        => CreateFloat(CsGrafeq.Numeric.CsGrafeqMath.Median(num1.ToFloat(), num2.ToFloat(), num3.ToFloat()));

    public static ExactNumber Min(ExactNumber num1, ExactNumber num2)
    {
        var a = num1.ToFloat();
        var b = num2.ToFloat();
        return CreateFloat(a < b ? a : b);
    }

    public static ExactNumber Max(ExactNumber num1, ExactNumber num2)
    {
        var a = num1.ToFloat();
        var b = num2.ToFloat();
        return CreateFloat(a > b ? a : b);
    }

    public static ExactNumber ArcTan2(ExactNumber y, ExactNumber x)
        => CreateFloat(Math.Atan2(y.ToFloat(), x.ToFloat()));

    public static ExactNumber MaxOf(IEnumerable<ExactNumber> nums)
        => CreateFloat(nums.Select(static o => o.ToFloat()).Max());

    public static ExactNumber MinOf(IEnumerable<ExactNumber> nums)
        => CreateFloat(nums.Select(static o => o.ToFloat()).Min());

    public static ExactNumber Mod(ExactNumber num1, ExactNumber num2)
        => CreateFloat(num1.ToFloat() % num2.ToFloat());

    public static ExactNumber Gamma(ExactNumber num)
        => CreateFloat(SpecialFunctions.Gamma(num.ToFloat()));

    public static ExactNumber LnGamma(ExactNumber num)
        => CreateFloat(SpecialFunctions.GammaLn(num.ToFloat()));

    public static ExactNumber Psi(ExactNumber num)
        => CreateFloat(SpecialFunctions.DiGamma(num.ToFloat()));

    public static ExactNumber Erf(ExactNumber num)
        => CreateFloat(SpecialFunctions.Erf(num.ToFloat()));

    public static ExactNumber Erfc(ExactNumber num)
        => CreateFloat(SpecialFunctions.Erfc(num.ToFloat()));

    public static ExactNumber Erfinv(ExactNumber num)
        => CreateFloat(SpecialFunctions.ErfInv(num.ToFloat()));

    public static ExactNumber Erfcinv(ExactNumber num)
        => CreateFloat(SpecialFunctions.ErfcInv(num.ToFloat()));

    public static ExactNumber Digamma(ExactNumber num)
        => CreateFloat(SpecialFunctions.DiGamma(num.ToFloat()));

    public static ExactNumber BesselJ(ExactNumber num1, ExactNumber num2)
        => CreateFloat(SpecialFunctions.BesselJ(num1.ToFloat(), num2.ToFloat()));

    public static ExactNumber BesselY(ExactNumber num1, ExactNumber num2)
        => CreateFloat(SpecialFunctions.BesselY(num1.ToFloat(), num2.ToFloat()));

    public static ExactNumber BesselI(ExactNumber num1, ExactNumber num2)
        => CreateFloat(SpecialFunctions.BesselI(num1.ToFloat(), num2.ToFloat()));

    public static ExactNumber BesselK(ExactNumber num1, ExactNumber num2)
        => CreateFloat(SpecialFunctions.BesselK(num1.ToFloat(), num2.ToFloat()));

    public static ExactNumber CreateFromDouble(double num)
        => CreateFloat(num);
    
    public string ToString(int fix)
    {
        switch (NumberType)
        {
            case ExactNumberType.Rational:
                return RationalPart.ToString();
            case ExactNumberType.Float:
                return fix==int.MaxValue?FloatPart.ToString():((decimal)FloatPart).Round(fix).ToString();
            default: throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        return ToString(int.MaxValue);
    }

    private static string CombineExpr(string expr1, string expr2) => expr1 + (expr2.StartsWith('-')?(expr2):("+"+expr2));

    public override bool Equals(object? obj)
    {
        if (obj is not ExactNumber other)
            return false;
        return ToFloat().Equals(other.ToFloat());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)NumberType, RationalPart, FloatPart);
    }

    public bool Equals(ExactNumber other)
    {
        var thisValue = ToFloat();
        var otherValue = other.ToFloat();
        return DoubleCompareHelper.CompareDoubleIfBothNaNThenEqual(thisValue, otherValue);
    }
    public double Value=>ToFloat();
    public static bool operator ==(ExactNumber left, ExactNumber right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(ExactNumber left, ExactNumber right)
    {
        return left.Value != right.Value;
    }

    public static bool operator <(ExactNumber left, ExactNumber right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(ExactNumber left, ExactNumber right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <=(ExactNumber left, ExactNumber right)
    {
        return left.Value <= right.Value;
    }

    public static bool operator >=(ExactNumber left, ExactNumber right)
    {
        return left.Value >= right.Value;
    }

    public int CompareTo(ExactNumber other)
    {
        if(other<this)
            return -1;
        if(other==this)
            return 0;
        return 1;
    }

    public static ExactNumber ToExactNumber(Result<Rational, double> input)
    {
        input.Success(out var okValue, out _);
        if (input.IsSuccessful)            
            return CreateRational(okValue);
        input.Error(out var errorValue);
        return CreateFloat(errorValue);
    }
}