namespace CsGrafeq;

public class Result<T, E> where E : Exception
{
    private readonly E? _exception;

    private readonly bool _isSuccessful;
    private readonly T? _value;

    protected Result(T value)
    {
        _isSuccessful = true;
        _value = value;
    }

    protected Result(E exception)
    {
        _isSuccessful = false;
        _exception = exception;
    }

    public static Result<T, E> Success(T okValue)
    {
        return new Result<T, E>(okValue);
    }

    public static Result<T, E> Error(E exception)
    {
        return new Result<T, E>(exception);
    }

    public bool Success(out T okValue)
    {
        okValue = _value!;
        return _isSuccessful;
    }

    public bool Error(out E exception)
    {
        exception = _exception!;
        return !_isSuccessful;
    }

    public void Match(Action<T> successAction, Action<E> errorAction)
    {
        if (_isSuccessful)
            successAction(_value!);
        else
            errorAction(_exception!);
    }

    public void IfSuccess(Action<T> successAction)
    {
        if (_isSuccessful)
            successAction(_value!);
    }

    public void IfError(Action<E> errorAction)
    {
        if (!_isSuccessful)
            errorAction(_exception!);
    }

    public void IfErrorThrow()
    {
        if (!_isSuccessful)
            throw _exception!;
    }
}

public class Result<T> : Result<T, Exception>
{
    protected Result(T value) : base(value)
    {
    }

    protected Result(Exception error) : base(error)
    {
    }

    public new static Result<T> Success(T okValue)
    {
        return new Result<T>(okValue);
    }

    public static Result<T> Error(string error)
    {
        return new Result<T>(new Exception(error));
    }

    public new static Result<T> Error(Exception exception)
    {
        return new Result<T>(exception);
    }
}