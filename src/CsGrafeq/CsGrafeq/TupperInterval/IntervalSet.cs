using static CsGrafeq.TupperInterval.IInterval;

namespace CsGrafeq.TupperInterval;

public struct IntervalSet : IInterval
{
    internal Range[] Intervals;
    internal (bool, bool) Def;
    internal bool Cont;
    internal bool IsNumber;

    public IntervalSet(double num)
    {
        Intervals = new Range[1] { new(num) };
        Def = TT;
        Cont = true;
        IsNumber = true;
    }

    public IntervalSet(double[] nums)
    {
        Array.Sort(nums);
        Intervals = new Range[nums.Length];
        for (var i = 0; i < nums.Length; i++)
            Intervals[i] = new Range(nums[i]);
        Def = TT;
        Cont = true;
        IsNumber = false;
    }

    public IntervalSet(double num1, double num2)
    {
        Intervals = new Range[1] { new(num1, num2) };
        Def = TT;
        Cont = true;
        IsNumber = num1 == num2;
    }

    public IntervalSet(Range[] Ranges, (bool, bool) def, bool cont)
    {
        Intervals = Ranges;
        Def = def;
        Cont = cont;
        IsNumber = false;
    }

    public IntervalSet(Range[] Ranges)
    {
        Intervals = Ranges;
        Def = TT;
        Cont = true;
        IsNumber = false;
    }

    public double Max => Intervals[Intervals.Length - 1].Max;
    public double Min => Intervals[0].Min;
    public double Length => Max - Min;

    public bool Contains(double num)
    {
        foreach (var r in Intervals)
            if (r.Contains(num))
                return true;
        return false;
    }

    public bool ContainsEqual(double num)
    {
        foreach (var r in Intervals)
            if (r.ContainsEqual(num))
                return true;
        return false;
    }

    public override string ToString()
    {
        var result = "";
        foreach (var r in Intervals) result += r + ",";
        return $"{{Def:{Def},Cont:{Cont},Intervals:[{result}]}}";
    }

    public IInterval SetCont(bool cont)
    {
        Cont = cont;
        return this;
    }

    public IInterval SetDef((bool, bool) def)
    {
        Def = def;
        return this;
    }

    public static IntervalSet operator +(IntervalSet i1, IntervalSet i2)
    {
        return i1.Add(i2);
    }

    public static IntervalSet operator -(IntervalSet i1, IntervalSet i2)
    {
        return i1.Subtract(i2);
    }

    public static IntervalSet operator +(IntervalSet i1, double i2)
    {
        return IntervalSetMath.AddNumber(i1, i2);
    }

    public static IntervalSet operator -(IntervalSet i1, double i2)
    {
        return IntervalSetMath.AddNumber(i1, -i2);
    }

    public static IntervalSet operator -(double i2, IntervalSet i1)
    {
        return IntervalSetMath.AddNumber(i1, -i2);
    }

    public static IntervalSet operator +(double i2, IntervalSet i1)
    {
        return IntervalSetMath.AddNumber(i1, i2);
    }

    public static IntervalSet operator *(IntervalSet i1, IntervalSet i2)
    {
        return i1.Multiply(i2);
    }

    public static IntervalSet operator /(IntervalSet i1, IntervalSet i2)
    {
        return i1.Divide(i2);
    }

    public static IntervalSet operator %(IntervalSet i1, IntervalSet i2)
    {
        return i1.Mod(i2);
    }

    public static IntervalSet operator ++(IntervalSet i1)
    {
        return IntervalSetMath.AddNumber(i1, 1);
    }

    public static IntervalSet operator --(IntervalSet i1)
    {
        return IntervalSetMath.AddNumber(i1, -1);
    }

    public static (bool, bool) operator ==(IntervalSet i1, IntervalSet i2)
    {
        return i1.Equal(i2);
    }

    public static (bool, bool) operator !=(IntervalSet i1, IntervalSet i2)
    {
        throw new NotImplementedException();
    }

    public static (bool, bool) operator <(IntervalSet i1, IntervalSet i2)
    {
        return i1.Less(i2);
    }

    public static (bool, bool) operator >(IntervalSet i1, IntervalSet i2)
    {
        return i1.Greater(i2);
    }

    public static (bool, bool) operator <=(IntervalSet i1, IntervalSet i2)
    {
        return i1.LessEqual(i2);
    }

    public static (bool, bool) operator >=(IntervalSet i1, IntervalSet i2)
    {
        return i1.GreaterEqual(i2);
    }

    public static IntervalSet operator -(IntervalSet i1)
    {
        return IntervalSetMath.Neg(i1);
    }

    public static implicit operator IntervalSet(double num)
    {
        return new IntervalSet(num);
    }

    public static implicit operator IntervalSet(Range range)
    {
        return new IntervalSet(range.Min, range.Max);
    }

    public static implicit operator IntervalSet(Interval range)
    {
        return new IntervalSet(range.Min, range.Max) { Def = range.Def, Cont = range.Cont };
    }

    public override bool Equals(object? obj)
    {
        if (obj is IntervalSet its)
            return this == its == TT;
        return false;
    }
    public static IInterval Create(double num)=>new IntervalSet(num);
}