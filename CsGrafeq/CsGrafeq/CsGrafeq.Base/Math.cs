using System.Numerics;
using sysMath = System.Math;

namespace CsGrafeq;

public static class Math
{
    public static double PosMod(double a, double b)
    {
        return a - b * sysMath.Floor(a / b);
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
        num = sysMath.Round(num, 0);
        num *= Pow(10d, -fix);
        return num;
    }

    public static decimal RoundTen(decimal num, int fix)
    {
        var m = Pow(10M, -fix);
        num /= m;
        num = sysMath.Round(num, 0);
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

    public static double NumMod(double a, double b)
    {
        return a - sysMath.Floor(a / b) * b;
    }

    public static (double, double) GetMinMax4(double n1, double n2, double n3, double n4)
    {
        var minnum = n1;
        var maxnum = n1;
        minnum = minnum < n2 ? minnum : n2;
        maxnum = maxnum > n2 ? maxnum : n2;
        minnum = minnum < n3 ? minnum : n3;
        maxnum = maxnum > n3 ? maxnum : n3;
        minnum = minnum < n4 ? minnum : n4;
        maxnum = maxnum > n4 ? maxnum : n4;
        //������NaN ��Ӧ��ʹ�����´���
        /*minnum = Math.Min(minnum, n2);
        maxnum = Math.Max(maxnum, n2);
        minnum = Math.Min(minnum, n3);
        maxnum = Math.Max(maxnum, n3);
        minnum = Math.Min(minnum, n4);
        maxnum = Math.Max(maxnum, n4);*/
        return (minnum, maxnum);
    }

    public static double DoubleMedian(double t1, double t2, double t3)
    {
        if ((t1 - t2) * (t2 - t3) > 0) return t2;
        if ((t2 - t1) * (t1 - t3) > 0) return t1;
        return t3;
    }

    public static long GCD(long a, long b)
    {
        return a == 0 || b == 0 ? 0 : GCDForLong(a, b);
    }

    private static long GCDForLong(long a, long b)
    {
        return b == 0 ? sysMath.Abs(a) : GCDForLong(b, a % b);
    }

    public static long LCM(long a, long b)
    {
        return a == 0 || b == 0 ? 0 : a * b / GCDForLong(a, b);
    }
}