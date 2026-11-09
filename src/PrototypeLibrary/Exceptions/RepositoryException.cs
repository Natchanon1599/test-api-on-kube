namespace PrototypeLibrary.Exceptions;

// Wraps exceptions from the data access layer (e.g. Npgsql/Oracle) so upper
// layers don't have to reference provider-specific exception types.
public class RepositoryException : AppException
{
    public RepositoryException(string message, Exception innerException)
        : base(message, innerException) { }
}
