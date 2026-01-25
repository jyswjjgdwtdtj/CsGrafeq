using System.Diagnostics.CodeAnalysis;

namespace CsGrafeq.Utilities;

public class ThrowHelper
{
    [DoesNotReturn]
    public static TResult Throw<TException, TResult>(TException exception) where TException : Exception where TResult : allows ref struct
    {
        throw exception;
    }

    public static void Throw<TException>(TException exception) where TException : Exception
    {
        throw exception;
    }

    [DoesNotReturn]
    public static TResult Throw<TException, TResult>() where TException : Exception, new() where TResult : allows ref struct
    {
        throw new TException();
    }

    [DoesNotReturn]
    public static TResult Throw<TResult>(string message) where TResult : allows ref struct
    {
        throw new Exception(message);
    }

    public static void Throw(string message)
    {
        throw new Exception(message);
    }
}