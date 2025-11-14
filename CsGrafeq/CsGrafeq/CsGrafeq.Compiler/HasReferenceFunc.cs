namespace CsGrafeq.Compiler;

public class HasReferenceFunction<T> : IDisposable where T : Delegate
{
    private readonly bool Disposed = false;
    public readonly T Function;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction(T function, EnglishCharEnum reference)
    {
        Function = function;
        if (function == null)
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

public class HasReferenceFunction : IDisposable
{
    private readonly bool Disposed = false;
    public readonly nint Function;
    public readonly EnglishCharEnum Reference;

    public HasReferenceFunction(nint function, EnglishCharEnum reference)
    {
        Function = function;
        if (function == nint.Zero)
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