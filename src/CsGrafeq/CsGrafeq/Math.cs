using System.Numerics;

namespace CsGrafeq;

public static class Math
{
    public static double PosMod(double a, double b)
    {
        return a - b * System.Math.Floor(a / b);
    }
    public static int Sgn<T>(T num) where T : INumber<T>
    {
        return T.Sign(num);
    }

    public static bool RangeIn<T>(T num1, T num2, T numtest) where T : INumber<T>
    {
        SwapIfNotLess(ref num1, ref num2);
        return num1 <= numtest && numtest <= num2;
    }

    public static void SwapIfNotLess<T>(ref T num1, ref T num2) where T : INumber<T>
    {
        if (num1 > num2) (num1, num2) = (num2, num1);
    }

    public static double RoundTen(double num, int fix)
    {
        num /= Pow(10d, -fix);
        num = System.Math.Round(num, 0);
        num *= Pow(10d, -fix);
        return num;
    }

    public static decimal RoundTen(decimal num, int fix)
    {
        var m = Pow(10M, -fix);
        num /= m;
        num = System.Math.Round(num, 0);
        num *= m;
        return num;
    }

    public static T Pow<T>(T num, int times) where T : INumber<T>
    {
        var result = T.One;
        if (times > 0)
            for (var i = 0; i < times; i++)
                result *= num;
        else if (times < 0)
            for (var i = 0; i > times; i--)
                result /= num;

        return result;
    }

    public static T RangeTo<T>(T min, T max, T target) where T : IComparable<T>
    {
        if (target.CompareTo(min) < 0)
            return min;
        if (target.CompareTo(max) > 0)
            return max;
        return target;
    }
}