namespace CsGrafeq.Result;


public class ResultWithException<TSuccess> : Result<TSuccess, Exception>
{
    protected ResultWithException(TSuccess value, string? message) : base(value, message)
    {
    }

    protected ResultWithException(Exception error) : base(error)
    {
    }

    public new static ResultWithException<TSuccess> Success(TSuccess okValue, string? message = null)
    {
        return new ResultWithException<TSuccess>(okValue, message);
    }

    public static ResultWithException<TSuccess> Failure(string error)
    {
        return new ResultWithException<TSuccess>(new Exception(error));
    }

    public new static ResultWithException<TSuccess> Failure(Exception exception)
    {
        return new ResultWithException<TSuccess>(exception);
    }
}