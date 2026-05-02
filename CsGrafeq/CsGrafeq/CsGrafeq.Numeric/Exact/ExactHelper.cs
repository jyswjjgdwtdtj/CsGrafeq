namespace CsGrafeq.Numeric.Exact;

public static class ExactHelper
{
    public static string ExactToString(this double value)
    {
        if (value == (int)value)
        {
            return value.ToString();
        }
        var sgn= Math.Sign(value);
        value = Math.Abs(value);
        uint integerPart = (uint)Math.Floor(value);
        var decimalPart= value - integerPart;
        if (DecimalPart.TryFindNear(decimalPart, out var index))
        {
            var res = (DecimalPart.Backward[index] + new Rational(1, integerPart, 1));
            if (res.IsSuccessful)
                return res.Success().ToString();
        }

        return value.CustomToString(8);
    }
}