namespace CsGrafeq;

public class Result<TSuccess, TException> where TException : Exception
{
    private readonly TException? _exception;

    private readonly bool _isSuccessful;
    private readonly string? _successMessage;
    private readonly TSuccess? _value;
    
    public bool IsSuccessful => _isSuccessful;
    public bool IsError => !_isSuccessful;

    protected Result(TSuccess value, string? message = null)
    {
        _isSuccessful = true;
        _value = value;
        _successMessage = message;
    }

    protected Result(TException exception)
    {
        _isSuccessful = false;
        _exception = exception;
    }

    public static Result<TSuccess, TException> Success(TSuccess okValue, string? message = null)
    {
        return new Result<TSuccess, TException>(okValue, message);
    }

    public static Result<TSuccess, TException> Error(TException exception)
    {
        return new Result<TSuccess, TException>(exception);
    }

    public bool Success(out TSuccess okValue, out string? message)
    {
        okValue = _value!;
        message = _successMessage;
        return _isSuccessful;
    }


    public bool Error(out TException exception)
    {
        exception = _exception!;
        return !_isSuccessful;
    }

    public TException? Error()
    {
        return _isSuccessful ? null : _exception;
    }

    public void Throw()
    {
        if (!_isSuccessful)
            throw _exception!;
    }

    public void Match(Action<TSuccess> successAction, Action<TException> errorAction)
    {
        if (_isSuccessful)
            successAction(_value!);
        else
            errorAction(_exception!);
    }

    public void Match(Action<TSuccess, string?> successAction, Action<TException> errorAction)
    {
        if (_isSuccessful)
            successAction(_value!, _successMessage);
        else
            errorAction(_exception!);
    }

    public void IfSuccess(Action<TSuccess> successAction)
    {
        if (_isSuccessful)
            successAction(_value!);
    }

    public void IfError(Action<TException> errorAction)
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

public class Result<TSuccess> : Result<TSuccess, Exception>
{
    protected Result(TSuccess value, string? message) : base(value, message)
    {
    }

    protected Result(Exception error) : base(error)
    {
    }

    public new static Result<TSuccess> Success(TSuccess okValue, string? message = null)
    {
        return new Result<TSuccess>(okValue, message);
    }

    public static Result<TSuccess> Error(string error)
    {
        return new Result<TSuccess>(new Exception(error));
    }

    public new static Result<TSuccess> Error(Exception exception)
    {
        return new Result<TSuccess>(exception);
    }
}