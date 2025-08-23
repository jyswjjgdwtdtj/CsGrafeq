namespace CsGrafeq.Numeric;

public interface IHasOperatorNumber<T> where T : IHasOperatorNumber<T>
{
    static T Add(T left, T right)
    {
        return left + right;
    }

    static T Subtract(T left, T right)
    {
        return left - right;
    }

    static T Multiply(T left, T right)
    {
        return left * right;
    }

    static T Divide(T left, T right)
    {
        return left / right;
    }

    static T Modulo(T left, T right)
    {
        return left % right;
    }

    static T Neg(T num)
    {
        return -num;
    }

    static abstract T operator +(T left, T right);
    static abstract T operator -(T left, T right);
    static abstract T operator *(T left, T right);
    static abstract T operator /(T left, T right);
    static abstract T operator %(T left, T right);
    static abstract T operator -(T num);
}