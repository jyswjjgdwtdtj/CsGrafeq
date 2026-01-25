namespace CsGrafeq.Numeric;

public interface IHasOperatorNumber<T> where T : IHasOperatorNumber<T>,allows ref struct
{
    static IDictionary<string, Delegate> HasOperatorNumberMethodDictionary { get; } = new Dictionary<string, Delegate>
    {
        { "add", Add },
        { "subtract", Subtract },
        { "multiply", Multiply },
        { "divide", Divide },
        { "modulo", Modulo },
        { "neg", Neg }
    };

    static IDictionary<string, nint> HasOperatorNumberPtrMethodDictionary { get; } = new Dictionary<string, nint>
    {
        /*{ "add", new IntPtr((delegate*<T,T,T>)(&IHasOperatorNumber<T>.Add)) },
        { "subtract", new IntPtr((delegate*<T,T,T>)(&IHasOperatorNumber<T>.Subtract)) },
        { "multiply", new IntPtr((delegate*<T, T, T>)(&IHasOperatorNumber<T>.Multiply)) },
        { "divide", new IntPtr((delegate*<T,T,T>)(&IHasOperatorNumber<T>.Divide)) },
        { "modulo", new IntPtr((delegate*<T, T, T>)(&IHasOperatorNumber<T>.Modulo)) },
        { "neg", new IntPtr((delegate*<T,T>)(&IHasOperatorNumber<T>.Neg)) },*/
    };

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