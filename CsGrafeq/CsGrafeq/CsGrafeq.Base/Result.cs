namespace CsGrafeq;

public class Result<T,E> where E:Exception
{
    public static Result<T, E> Success(T okValue)=> new Result<T, E>(okValue);
    public static Result<T, E> Error(E exception) => new Result<T, E>(exception);
    private readonly bool _isSuccessful;
    private readonly E? _exception=null;
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
    public new static Result<T> Success(T okValue)=> new Result<T>(okValue);
    public static Result<T> Error(string error) => new Result<T>(new Exception(error));
    public new static Result<T> Error(Exception exception) => new Result<T>(exception);
    protected Result(T value) : base(value) { }
    protected Result(Exception error) : base(error) { }
}