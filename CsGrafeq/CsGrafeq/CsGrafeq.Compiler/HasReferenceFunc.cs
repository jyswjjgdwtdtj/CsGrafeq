using CsGrafeq.Variables;

namespace CsGrafeq.Compiler;

public class HasReferenceFunction<T> :IHasReference, IDisposable where T : Delegate
{
    private readonly bool _disposed = false;
    public readonly T Function;

    public bool IsActive
    {
        get;
        set
        {
            if (field != value)
            {
                Console.WriteLine(value);
                field = value;
                VarRecorder.Instance.RefreshReferences();
            }   
        }
    } = true;

    public VariablesEnum References { get; }

    public HasReferenceFunction(T function, VariablesEnum references)
    {
        Function = function;
        if (function == null)
            throw new ArgumentNullException(nameof(function));
        References = references;
        VarRecorder.Instance.Attach(this);
        VarRecorder.Instance.RefreshReferences();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            IsActive = false;
            VarRecorder.Instance.Detach(this);
            VarRecorder.Instance.RefreshReferences();
            GC.SuppressFinalize(this);
        }
    }

    ~HasReferenceFunction()
    {
        Dispose();
    }
}