using System.Text.Json;
using Npgsql;
using TestKubeLibrary.Exceptions;

namespace TestKubeApi.BusinessLogic;

// Maps a caught exception to a coarse category stored on the log row.
public static class ErrorClassifier
{
    public const string Validation = "VALIDATION";
    public const string DbConnection = "DB_CONNECTION";
    public const string DbQuery = "DB_QUERY";
    public const string DataFormat = "DATA_FORMAT";
    public const string Unknown = "UNKNOWN";

    public static string Classify(Exception ex)
    {
        // Data-layer errors are wrapped in RepositoryException; inspect the real cause
        var cause = ex is RepositoryException && ex.InnerException is not null ? ex.InnerException : ex;

        return cause switch
        {
            ValidationException => Validation,
            PostgresException => DbQuery,        // SQL error from server (bad query, constraint) - derives from NpgsqlException
            NpgsqlException => DbConnection,     // could not reach / talk to the database
            FormatException or JsonException or InvalidCastException => DataFormat,
            _ => Unknown
        };
    }
}
