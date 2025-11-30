using System.Drawing;

namespace CsGrafeq.Utilities;

public class ColorHelper
{
    private static readonly Array Colors = Enum.GetValues(typeof(KnownColor));
    private static readonly Random Rnd = new();
    private static readonly double k = Math.Pow(1 / (1 + Math.Pow(1.5, 2.2) + Math.Pow(0.6, 2.2)), 1 / 2.2);

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
        return k * Math.Pow(
            Math.Pow((double)c.R / 255, 2.2) + Math.Pow((double)c.G / 255 * 1.5, 2.2) +
            Math.Pow((double)c.B / 255 * 0.6, 2.2), 1 / 2.2);
    }
}