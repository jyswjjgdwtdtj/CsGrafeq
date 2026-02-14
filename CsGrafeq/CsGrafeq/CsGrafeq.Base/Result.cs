namespace CsGrafeq;

public class Result<TSuccess, TException> where TException : Exception
{
    private readonly TException? _exception;

    private readonly string? _successMessage;
    private readonly TSuccess? _value;

    protected Result(TSuccess value, string? message = null)
    {
        IsSuccessful = true;
        _value = value;
        _successMessage = message;
    }

    protected Result(TException exception)
    {
        IsSuccessful = false;
        _exception = exception;
    }

    public bool IsSuccessful { get; }

    public bool IsError => !IsSuccessful;

    public static Result<TSuccess, TException> Success(TSuccess okValue, string? message = null)
    {
        return new Result<TSuccess, TException>(okValue, message);
    }

    public static Result<TSuccess, TException> Failure(TException exception)
    {
        return new Result<TSuccess, TException>(exception);
    }

    public bool Success(out TSuccess okValue, out string? message)
    {
        okValue = _value!;
        message = _successMessage;
        return IsSuccessful;
    }


    public bool Error(out TException exception)
    {
        exception = _exception!;
        return !IsSuccessful;
    }

    public TException? Error()
    {
        return IsSuccessful ? null : _exception;
    }

    public void Throw()
    {
        if (!IsSuccessful)
            throw _exception!;
    }

    public void Match(Action<TSuccess> successAction, Action<TException> errorAction)
    {
        if (IsSuccessful)
            successAction(_value!);
        else
            errorAction(_exception!);
    }

    public void Match(Action<TSuccess, string?> successAction, Action<TException> errorAction)
    {
        if (IsSuccessful)
            successAction(_value!, _successMessage);
        else
            errorAction(_exception!);
    }

    public void IfSuccessful(Action<TSuccess> successAction)
    {
        if (IsSuccessful)
            successAction(_value!);
    }

    public void IfFailed(Action<TException> errorAction)
    {
        if (!IsSuccessful)
            errorAction(_exception!);
    }

    public void IfErrorThenThrow()
    {
        if (!IsSuccessful)
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

    public static Result<TSuccess> Failure(string error)
    {
        return new Result<TSuccess>(new Exception(error));
    }

    public new static Result<TSuccess> Failure(Exception exception)
    {
        return new Result<TSuccess>(exception);
    }
}