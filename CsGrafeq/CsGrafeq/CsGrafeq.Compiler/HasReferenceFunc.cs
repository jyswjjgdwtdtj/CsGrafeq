using CsGrafeq.Numeric;

namespace CsGrafeq.Compiler;

public delegate T Function0<T>() where T : IComputableNumber<T>;

public delegate T Function1<T>(T arg1) where T : IComputableNumber<T>;

public delegate T Function2<T>(T arg1, T arg2) where T : IComputableNumber<T>;

public delegate T Function3<T>(T arg1, T arg2, T arg3) where T : IComputableNumber<T>;

public class HasReferenceFunction : IDisposable
{
    private readonly bool Disposed = false;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction(EnglishCharEnum reference)
    {
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

public class HasReferenceFunction0<T> : HasReferenceFunction where T : IComputableNumber<T>
{
    public readonly Function0<T> Function;

    public HasReferenceFunction0(Function0<T> function, EnglishCharEnum reference) : base(reference)
    {
        Function = function;
    }
}

public class HasReferenceFunction1<T> : HasReferenceFunction where T : IComputableNumber<T>
{
    public readonly Function1<T> Function;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction1(Function1<T> function, EnglishCharEnum reference) : base(reference)
    {
        Function = function;
    }
}

public class HasReferenceFunction2<T> : HasReferenceFunction where T : IComputableNumber<T>
{
    public readonly Function2<T> Function;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction2(Function2<T> function, EnglishCharEnum reference) : base(reference)
    {
        Function = function;
    }
}

public class HasReferenceFunction3<T> : HasReferenceFunction where T : IComputableNumber<T>
{
    public readonly Function3<T> Function;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction3(Function3<T> function, EnglishCharEnum reference) : base(reference)
    {
        Function = function;
    }
}