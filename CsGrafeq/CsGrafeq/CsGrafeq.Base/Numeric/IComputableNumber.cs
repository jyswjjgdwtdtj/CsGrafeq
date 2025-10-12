namespace CsGrafeq.Numeric;

public interface IComputableNumber<T> : IHasOperatorNumber<T> where T : IComputableNumber<T>
{
    static IDictionary<string, Delegate> MethodDictionary { get; } = new Dictionary<string, Delegate>
    {
        { "sqrt", T.Sqrt },
        { "cbrt", T.Cbrt },
        { "pow", T.Pow },
        { "exp", T.Exp },
        { "log", T.Log },
        { "lg", T.Lg },
        { "ln", T.Ln },
        { "sin", T.Sin },
        { "cos", T.Cos },
        { "tan", T.Tan },
        { "cot", T.Cot },
        { "arcsin", T.ArcSin },
        { "arccos", T.ArcCos },
        { "arctan", T.ArcTan },
        { "tanh", T.Tanh },
        { "cosh", T.Cosh },
        { "sinh", T.Sinh },
        { "arccosh", T.ArcCosh },
        { "arctanh", T.ArcTanh },
        { "arcsinh", T.ArcSinh },
        { "floor", T.Floor },
        { "ceil", T.Ceil },
        { "gcd", T.GCD },
        { "lcm", T.LCM },
        { "sgn", T.Sgn },
        { "abs", T.Abs },
        { "median", T.Median },
        { "min", T.Min },
        { "max", T.Max }
    };

    static abstract T Sqrt(T num);
    static abstract T Cbrt(T num);
    static abstract T Pow(T num, T exp);

    static abstract T Exp(T num);
    static abstract T Log(T num1, T num2);
    static abstract T Lg(T num);
    static abstract T Ln(T num);

    static abstract T Sin(T num);
    static abstract T Cos(T num);
    static abstract T Tan(T num);
    static abstract T Cot(T num);

    static abstract T ArcSin(T num);
    static abstract T ArcCos(T num);
    static abstract T ArcTan(T num);

    static abstract T Tanh(T num);
    static abstract T Cosh(T num);
    static abstract T Sinh(T num);

    static abstract T ArcCosh(T num);
    static abstract T ArcTanh(T num);
    static abstract T ArcSinh(T num);

    static abstract T Floor(T num);
    static abstract T Ceil(T num);

    static abstract T GCD(T num1, T num2);
    static abstract T LCM(T num1, T num2);

    static abstract T Sgn(T num);
    static abstract T Abs(T num);

    static abstract T Median(T num1, T num2, T num3);
    static abstract T Min(T num1, T num2);
    static abstract T Max(T num1, T num2);
    static abstract T CreateFromDouble(double num);
}