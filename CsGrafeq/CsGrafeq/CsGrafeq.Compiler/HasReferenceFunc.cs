using CsGrafeq.Numeric;

namespace CsGrafeq.Compiler;

public delegate T Function0<T>() where T : IComputableNumber<T>;

public delegate T Function1<T>(T arg1) where T : IComputableNumber<T>;

public delegate T Function2<T>(T arg1, T arg2) where T : IComputableNumber<T>;

public delegate T Function3<T>(T arg1, T arg2, T arg3) where T : IComputableNumber<T>;

public class HasReferenceFunction<T> : IDisposable where T:Delegate
{
    public readonly T Function;
    private readonly bool Disposed = false;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction(T function,EnglishCharEnum reference)
    {
        this.Function = function;
        if(function==null)
            throw new ArgumentNullException(nameof(function));
        Reference = reference;
        EnglishChar.Instance.AddReference(reference);
    }

    public void Dispose()
    {
        if (!Disposed)
        {
            EnglishChar.Instance.RemoveReference(Reference);
            GC.SuppressFinalize(this);
        }
    }

    ~HasReferenceFunction()
    {
        Dispose();
    }
}
public class HasReferenceFunction0<T>(Function0<T> function, EnglishCharEnum reference) : HasReferenceFunction<Function0<T>>(function, reference) where T : IComputableNumber<T>
{
}
public class HasReferenceFunction1<T>(Function1<T> function, EnglishCharEnum reference) : HasReferenceFunction<Function1<T>>(function, reference) where T : IComputableNumber<T>
{
}
public class HasReferenceFunction2<T>(Function2<T> function, EnglishCharEnum reference) : HasReferenceFunction<Function2<T>>(function, reference) where T : IComputableNumber<T>
{
}
public class HasReferenceFunction3<T>(Function3<T> function, EnglishCharEnum reference) : HasReferenceFunction<Function3<T>>(function, reference) where T : IComputableNumber<T>
{
}