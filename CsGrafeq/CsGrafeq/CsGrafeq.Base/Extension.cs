using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using sysMath = System.Math;

namespace CsGrafeq;

public static class ColorExtension
{
    private static readonly Array Colors = Enum.GetValues(typeof(KnownColor));
    private static readonly Random Rnd = new();
    private static readonly double k = sysMath.Pow(1 / (1 + sysMath.Pow(1.5, 2.2) + sysMath.Pow(0.6, 2.2)), 1 / 2.2);

    public static uint GetRandomColor()
    {
        Color color;
        do
        {
            color = Color.FromKnownColor((KnownColor)Colors.GetValue(Rnd.Next(Colors.Length - 27) + 27));
        } while (GetRealBrightness(color) > 0.8 || GetRealBrightness(color) < 0.2);

        return (uint)color.ToArgb();
    }

    public static double GetRealBrightness(Color c)
    {
        return k * sysMath.Pow(
            sysMath.Pow((double)c.R / 255, 2.2) + sysMath.Pow((double)c.G / 255 * 1.5, 2.2) +
            sysMath.Pow((double)c.B / 255 * 0.6, 2.2), 1 / 2.2);
    }
}
public static class Extension
{
    [DoesNotReturn]
    public static TResult Throw<TException, TResult>(TException exception) where TException : Exception
    {
        throw exception;
    }
    public static void Throw<TException>(TException exception) where TException : Exception
    {
        throw exception;
    }

    [DoesNotReturn]
    public static TResult Throw<TException, TResult>() where TException : Exception, new()
    {
        throw new TException();
    }

    [DoesNotReturn]
    public static TResult Throw<TResult>(string message)
    {
        throw new Exception(message);
    }
    public static void Throw(string message)
    {
        throw new Exception(message);
    }
}