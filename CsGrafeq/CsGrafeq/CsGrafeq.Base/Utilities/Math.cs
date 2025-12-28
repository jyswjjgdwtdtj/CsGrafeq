using System.Numerics;
using System.Runtime.CompilerServices;
using sysMath = System.Math;

namespace CsGrafeq.Utilities;

public static class CsGrafeqMath
{
    /// <summary>
    ///     取模操作
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static double CustomMod(double a, double b)
    {
        return a - b * sysMath.Floor(a / b);
    }

    /// <summary>
    ///     Sgn函数
    /// </summary>
    /// <param name="num"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sgn<T>(T num) where T : INumber<T>
    {
        return T.Sign(num);
    }

    /// <summary>
    ///     是否在范围之中
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <param name="numtest"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool RangeIn<T>(T num1, T num2, T numtest) where T : INumber<T>
    {
        SwapIfNotLess(ref num1, ref num2);
        return num1 <= numtest && numtest <= num2;
    }

    /// <summary>
    ///     使num1&lt;=num2
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <typeparam name="T"></typeparam>
    public static void SwapIfNotLess<T>(ref T num1, ref T num2) where T : INumber<T>
    {
        if (num1 > num2) (num1, num2) = (num2, num1);
    }

    /// <summary>
    ///     四舍五入至小数点
    /// </summary>
    /// <param name="num"></param>
    /// <param name="fix"></param>
    /// <returns></returns>
    public static double RoundTen(double num, int fix)
    {
        var n = SpecialPow(10d, -fix);
        return Math.Round(num / n) * n;
    }

    /// <summary>
    ///     四舍五入至小数点
    /// </summary>
    /// <param name="num"></param>
    /// <param name="fix"></param>
    /// <returns></returns>
    public static decimal RoundTen(decimal num, int fix)
    {
        var n = SpecialPow(10m, -fix);
        return decimal.Round(num / n) * n;
    }

    /// <summary>
    ///     无优化的乘方运算
    /// </summary>
    /// <param name="num"></param>
    /// <param name="times"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T SpecialPow<T>(T num, int times) where T : INumber<T>
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

    /// <summary>
    ///     限定至范围
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RangeTo<T>(T min, T max, T target) where T : IComparable<T>
    {
        if (target.CompareTo(min) < 0)
            return min;
        if (target.CompareTo(max) > 0)
            return max;
        return target;
    }

    /// <summary>
    ///     获取四个数的最大值和最小值
    /// </summary>
    /// <param name="n1"></param>
    /// <param name="n2"></param>
    /// <param name="n3"></param>
    /// <param name="n4"></param>
    /// <returns></returns>
    public static (double min, double max) GetMinMax(double n1, double n2, double n3, double n4)
    {
        var minnum = n1;
        var maxnum = n1;
        minnum = minnum < n2 ? minnum : n2;
        maxnum = maxnum > n2 ? maxnum : n2;
        minnum = minnum < n3 ? minnum : n3;
        maxnum = maxnum > n3 ? maxnum : n3;
        minnum = minnum < n4 ? minnum : n4;
        maxnum = maxnum > n4 ? maxnum : n4;
        return (minnum, maxnum);
    }

    /// <summary>
    ///     获取三数中间的数
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <param name="t3"></param>
    /// <returns></returns>
    public static double Median(double t1, double t2, double t3)
    {
        if ((t1 - t2) * (t2 - t3) > 0) return t2;
        if ((t2 - t1) * (t1 - t3) > 0) return t1;
        return t3;
    }

    /// <summary>
    ///     最大公约数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static long GCD(long a, long b)
    {
        return a == 0 || b == 0 ? 0 : GCDForLong(a, b);
    }

    private static long GCDForLong(long a, long b)
    {
        return b == 0 ? sysMath.Abs(a) : GCDForLong(b, a % b);
    }

    /// <summary>
    ///     最小公倍数
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static long LCM(long a, long b)
    {
        return a == 0 || b == 0 ? 0 : a * b / GCDForLong(a, b);
    }

    /// <summary>
    ///     约至小数点后指定位数
    /// </summary>
    /// <param name="d"></param>
    /// <param name="dots"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    public static string CustomToString(this double d, int dots, double precision)
    {
        var tar = double.Round(d, dots);
        return double.Abs(tar - d) < precision ? tar.ToString() : d.ToString();
    }
}