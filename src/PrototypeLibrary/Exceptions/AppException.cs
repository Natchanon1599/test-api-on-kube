namespace PrototypeLibrary.Exceptions;

// Base class for exceptions intentionally thrown by the app, so upper layers
// can catch app errors separately from .NET/provider exceptions.
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
    protected AppException(string message, Exception innerException) : base(message, innerException) { }
}
