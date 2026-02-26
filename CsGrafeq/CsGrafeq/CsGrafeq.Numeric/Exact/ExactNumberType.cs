
namespace CsGrafeq.Numeric.Exact;

public enum ExactNumberType
{
    /// <summary>
    /// e.g. sqrt(3)+cbrt(4)=>3.3194...
    ///      ln(2)=>0.6931...
    /// </summary>
    Float,
    /// <summary>
    /// e.g. 3/4*pi=>3/4*pi
    ///      2*pi=>2*pi
    /// </summary>
    ContainsPi,
    /// <summary>
    /// e.g. sqrt(3)=>√3
    /// </summary>
    UniSurd,
    /// <summary>
    /// e.g. sqrt(3)+sqrt(5)=>√3+√5
    /// </summary>
    BiSurd,
    /// <summary>
    /// e.g. 1+5/3+16/7=>104/21
    /// </summary>
    Rational,
    /// <summary>
    /// e.g. 1+5/3+sqrt(2)=>(8+3√2)/3
    /// Important: when the number is too big, it will be converted to Float type,
    ///            e.g. 1+5/3+16/7+sqrt(2)=>6.366...
    /// </summary>
    RationalAndSurd
}