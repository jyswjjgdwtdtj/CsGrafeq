namespace CsGrafeq.Result;


public class Result<TSuccess, TError>
{
    private readonly TError? _exception;

    private readonly string? _successMessage;
    private readonly TSuccess? _value;

    protected Result(TSuccess value, string? message = null)
    {
        IsSuccessful = true;
        _value = value;
        _successMessage = message;
    }

    protected Result(TError exception)
    {
        IsSuccessful = false;
        _exception = exception;
    }

    public bool IsSuccessful { get; }

    public bool IsError => !IsSuccessful;

    public static Result<TSuccess, TError> Success(TSuccess okValue, string? message = null)
    {
        return new Result<TSuccess, TError>(okValue, message);
    }

    public static Result<TSuccess, TError> Failure(TError exception)
    {
        return new Result<TSuccess, TError>(exception);
    }

    public bool Success(out TSuccess okValue, out string? message)
    {
        okValue = _value!;
        message = _successMessage;
        return IsSuccessful;
    }

    public TSuccess Success()
    {
        return IsSuccessful ? _value! : throw new InvalidOperationException("Result does not contain a successful value.");
    }


    public bool Error(out TError exception)
    {
        exception = _exception!;
        return !IsSuccessful;
    }

    public TError? Error()
    {
        return IsSuccessful ? default : _exception;
    }

    public void Match(Action<TSuccess> successAction, Action<TError> errorAction)
    {
        if (IsSuccessful)
            successAction(_value!);
        else
            errorAction(_exception!);
    }

    public void Match(Action<TSuccess, string?> successAction, Action<TError> errorAction)
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

    public void IfFailed(Action<TError> errorAction)
    {
        if (!IsSuccessful)
            errorAction(_exception!);
    }
    public static implicit operator Result<TSuccess, TError>(TSuccess value) => Success(value);
    public static implicit operator Result<TSuccess, TError>(TError error) => Failure(error);
}