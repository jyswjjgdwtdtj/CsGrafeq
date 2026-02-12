using CsGrafeq.Numeric;

namespace CsGrafeq.Interval.Extensions;

public static class NumberHelper
{
    public static bool IsAllLessThanZero(DoubleNumber[] value)
    {
        for(var i=0;i<value.Length;i++)
            if (value[i].Value >= 0)
                return false;
        return true;
    }
    public static bool IsAllGreaterThanZero(DoubleNumber[] value)
    {
        for(var i=0;i<value.Length;i++)
            if (value[i].Value <= 0)
                return false;
        return true;
    }

    public static bool IsSomeGreaterAndSomeLessThanZero(DoubleNumber[] value)
    {
        bool hasLess = false;
        for (int i = 0, j = 0; i < value.Length; i++)
        {
            if (value[i].Value < 0)
            {
                hasLess = true;
                break;
            }
        }
        if(!hasLess)
            return false;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i].Value > 0&&hasLess)
            {
                return true;
            }
        }
        return false;
    }
    
}