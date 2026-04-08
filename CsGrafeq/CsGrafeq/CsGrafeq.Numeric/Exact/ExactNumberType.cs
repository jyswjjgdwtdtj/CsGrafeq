
namespace CsGrafeq.Numeric.Exact;

public enum ExactNumberType
{
    /// <summary>
    /// e.g. sqrt(3)+cbrt(4)=>3.3194...
    ///      ln(2)=>0.6931...
    /// </summary>
    Float,
    /// <summary>
    /// e.g. 1+5/3+16/7=>104/21
    /// </summary>
    Rational,
}