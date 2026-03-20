using CsGrafeq.Utilities;
using MathNet.Numerics;
using MathNet.Symbolics;

namespace CsGrafeq.Numeric.Exact;

public struct ExactNumber : IComputableNumber<ExactNumber>
{
    public const double Eps = 1e-8;
    public static readonly ExactNumber NaN =new(){_floatPart = Double.NaN,NumberType = ExactNumberType.Float};
    public ExactNumberType NumberType { get; private set; } = ExactNumberType.None;

    public ExactNumber()
    {
    }

    private Rational _rationalPart;
    private Surd _surdPart1;
    private Surd _surdPart2;
    private double _floatPart;

    public static ExactNumber CreateRational(Rational rational)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.Rational,
            _rationalPart = rational
        }.ReduceAndReturn();
    }

    public static ExactNumber CreateUniSurd(Surd surd)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.UniSurd,
            _surdPart1 = surd
        }.ReduceAndReturn();
    }

    public static ExactNumber CreateRationalAndSurd(Rational rational, Surd surd)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.RationalAndSurd,
            _rationalPart = rational,
            _surdPart1 = surd
        }.ReduceAndReturn();
    }

    public static ExactNumber CreateFloat(double value)
    {
        if (Double.IsNaN(value))
            return NaN;
        if(Math.Abs(value-Double.Round(value,4)) < Eps)
            return CreateRational(new Rational(Math.Sign(value), (uint)Double.Round(Math.Abs(value) * 10000), 10000)).ReduceAndReturn();
        return new ExactNumber()
        {
            _floatPart = value, NumberType = ExactNumberType.Float
        };
    }

    public static ExactNumber CreateContainsPi(Rational rational)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.ContainsPi,
            _rationalPart = rational
        }.ReduceAndReturn();
    }

    public static ExactNumber CreateBiSurd(Surd surd1, Surd surd2)
    {
        return new ExactNumber
        {
            NumberType = ExactNumberType.BiSurd,
            _surdPart1 = surd1,
            _surdPart2 = surd2
        }.ReduceAndReturn();
    }

    public double ToFloat()
    {
        switch (NumberType)
        {
            case ExactNumberType.UniSurd:
                return _surdPart1.ToFloat();
            case ExactNumberType.Rational:
                return _rationalPart.ToFloat();
            case ExactNumberType.Float:
                return _floatPart;
            case ExactNumberType.ContainsPi:
                return _rationalPart.ToFloat() * Math.PI;
            case ExactNumberType.BiSurd:
                return _surdPart1.ToFloat() + _surdPart2.ToFloat();
            case ExactNumberType.RationalAndSurd:
                return _rationalPart.ToFloat() + _surdPart1.ToFloat();
            default:
            {
                return Double.NaN;
                throw new NotImplementedException(){Source = $"{_surdPart1} {_surdPart2} {_floatPart} {_rationalPart} {NumberType}"};
            }
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
        if (left.NumberType == right.NumberType)
        {
            switch (left.NumberType)
            {
                case ExactNumberType.None:
                    return NaN;
                case ExactNumberType.Float:
                    return CreateFloat(left.ToFloat() + right.ToFloat());
                case ExactNumberType.Rational:
                {
                    var res = left._rationalPart + right._rationalPart;
                    if(res.numerator>5000||res.denominator>5000)
                        return CreateFloat(left.ToFloat() + right.ToFloat());
                    return CreateRational(new Rational(res.sign,(uint)res.numerator,(uint)res.denominator));
                }
                case ExactNumberType.ContainsPi:
                {
                    var res = left._rationalPart + right._rationalPart;
                    if(res.numerator>5000||res.denominator>5000||!AcceptedPiDenominator.Contains((uint)res.denominator))
                        return CreateFloat(left.ToFloat() + right.ToFloat());
                    return CreateContainsPi(new Rational(res.sign,(uint)res.numerator,(uint)res.denominator));
                }
                case ExactNumberType.UniSurd:
                {
                    return CreateBiSurd(left._surdPart1, right._surdPart2);
                }
            }
        }
        
    }

    public static ExactNumber operator -(ExactNumber left, ExactNumber right)
        => left + -right;

    public static ExactNumber operator *(ExactNumber left, ExactNumber right)
        => CreateFloat(left.ToFloat() * right.ToFloat());

    public static ExactNumber operator /(ExactNumber left, ExactNumber right)
        => CreateFloat(left.ToFloat() / right.ToFloat());

    public static ExactNumber operator %(ExactNumber left, ExactNumber right)
        => CreateFloat(left.ToFloat() % right.ToFloat());

    public static ExactNumber operator -(ExactNumber num)
    {
        switch (num.NumberType)
        {
            case ExactNumberType.None:
                return NaN;
            case ExactNumberType.Float:
                num._floatPart = -num._floatPart;
                break;
            case ExactNumberType.Rational:
            case ExactNumberType.ContainsPi:
                num._rationalPart.Sign=-num._rationalPart.Sign;
                break;
            case ExactNumberType.UniSurd:
                num._surdPart1.RationalPart.Sign=-num._surdPart1.RationalPart.Sign;
                break;
            case ExactNumberType.BiSurd:
                num._surdPart1.RationalPart.Sign=-num._surdPart1.RationalPart.Sign;
                num._surdPart2.RationalPart.Sign=-num._surdPart2.RationalPart.Sign;
                break;
            case ExactNumberType.RationalAndSurd:
                num._surdPart1.RationalPart.Sign=-num._surdPart1.RationalPart.Sign;
                num._rationalPart.Sign=-num._rationalPart.Sign;
                break;
            default:
                throw new NotImplementedException();
        }

        return num;
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
            case ExactNumberType.UniSurd:
                return _surdPart1.ToString();
            case ExactNumberType.Rational:
                return _rationalPart.ToString();
            case ExactNumberType.Float:
                return fix==int.MaxValue?_floatPart.ToString():((decimal)_floatPart).Round(fix).ToString();
            case ExactNumberType.ContainsPi:
                return _rationalPart.ToString()+"*π";
            case ExactNumberType.BiSurd:
                return CombineExpr(_surdPart1.ToString(),_surdPart2.ToString());
            case ExactNumberType.RationalAndSurd:
                return CombineExpr(_rationalPart.ToString(),_surdPart1.ToString());
            default: throw new NotImplementedException();
        }
    }

    public override string ToString()
    {
        return ToString(int.MaxValue);
    }

    private static string CombineExpr(string expr1, string expr2)
    {
        return expr1 + (expr2.StartsWith('-')?(expr2):("+"+expr2));
    }
    public override bool Equals(object? obj)
    {
        if (obj is not ExactNumber other)
            return false;
        return ToFloat().Equals(other.ToFloat());
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
    public ExactNumber ReduceAndReturn()
    {
        Reduce();
        return this;
    }
    public void Reduce()
    {
        switch (NumberType)
        {
            case ExactNumberType.Float:
                break;
            case ExactNumberType.Rational:
                if (_rationalPart.IsTooComplicated())
                {
                    _floatPart= _rationalPart.ToFloat();
                    NumberType=ExactNumberType.Float;
                }
                break;
            case ExactNumberType.BiSurd:
                if (_surdPart1.IsTooComplicated() || _surdPart2.IsTooComplicated())
                {
                    _floatPart = _surdPart1.ToFloat() + _surdPart2.ToFloat();
                    NumberType = ExactNumberType.Float;
                }

                if (_surdPart1.Radicand == _surdPart2.Radicand)
                {
                    //error;
                }
                break;
            case ExactNumberType.ContainsPi:
                if (_rationalPart.IsTooComplicated()||!AcceptedPiDenominator.Contains(_rationalPart.Denominator))
                {
                    _floatPart = _rationalPart.ToFloat() * Math.PI;
                    NumberType = ExactNumberType.Float;
                }
                break;
            case ExactNumberType.UniSurd:
                if (_surdPart1.IsTooComplicated())
                {
                    _floatPart = _surdPart1.ToFloat();
                    NumberType = ExactNumberType.Float;
                }
                break;
            case ExactNumberType.RationalAndSurd:
                if (_rationalPart.IsTooComplicated()||_surdPart1.IsTooComplicated())
                {
                    _floatPart = _rationalPart.ToFloat() + _surdPart1.ToFloat();
                    NumberType = ExactNumberType.Float;
                }
                break;
             default: throw new NotImplementedException();
        }
    }

    private static readonly uint[] AcceptedPiDenominator = [2,3,4,6,12];
}