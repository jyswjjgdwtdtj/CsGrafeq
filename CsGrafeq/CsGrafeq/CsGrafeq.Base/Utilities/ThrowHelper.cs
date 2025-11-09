using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CsGrafeq.Utilities
{
    public class ThrowHelper
    {
        [DoesNotReturn]
        public static TResult Throw<TException, TResult>(TException exception) where TException : Exception
        {
            throw exception;
        }

        public static void Throw<TException>(TException exception) where TException : Exception
        {
            throw exception;
        }

        [DoesNotReturn]
        public static TResult Throw<TException, TResult>() where TException : Exception, new()
        {
            throw new TException();
        }

        [DoesNotReturn]
        public static TResult Throw<TResult>(string message)
        {
            throw new Exception(message);
        }

        public static void Throw(string message)
        {
            throw new Exception(message);
        }
    }
}
