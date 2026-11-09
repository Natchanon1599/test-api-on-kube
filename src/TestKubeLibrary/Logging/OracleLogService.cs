using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using TestKubeLibrary.Interfaces;
using TestKubeLibrary.Models;

namespace TestKubeLibrary.Logging;

// Writes logs to Oracle tables LOG_REQ / LOG_RESP (mirrors db/002_create_log_tables.sql).
// Logging must never break the business flow: if the Oracle write fails
// (connection down, missing table, etc.) it is caught and falls back to ILogger instead of rethrowing.
public class OracleLogService : ILogService
{
    private readonly string _connectionString;
    private readonly ILogger<OracleLogService> _logger;
    private readonly string _appName;

    private const string InsertRequestSql = @"
        INSERT INTO LOG_REQ (REQUEST, USERNAME, CREATED_BY, CREATED_DATE)
        VALUES (:request, :username, :createdBy, :createdDate)
        RETURNING ID INTO :id";

    private const string InsertResponseSql = @"
        INSERT INTO LOG_RESP (REQUEST_ID, LOG_LEVEL, CATEGORY, SOURCE, MESSAGE, EXCEPTION_DETAIL, CREATED_AT)
        VALUES (:requestId, :level, :category, :source, :message, :exceptionDetail, :createdAt)";

    public OracleLogService(IConfiguration configuration, ILogger<OracleLogService> logger)
    {
        _connectionString = configuration.GetConnectionString("OracleLogs")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:OracleLogs in configuration");
        _logger = logger;
        _appName = configuration["AppName"] ?? "testkube";
    }

    public async Task<long> LogRequestAsync(string? request, string? username, CancellationToken ct = default)
    {
        var entry = new LogRequestEntry
        {
            Request = request,
            Username = username,
            CreatedBy = _appName
        };

        try
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = new OracleCommand(InsertRequestSql, connection);
            command.Parameters.Add(new OracleParameter("request", (object?)entry.Request ?? DBNull.Value));
            command.Parameters.Add(new OracleParameter("username", (object?)entry.Username ?? DBNull.Value));
            command.Parameters.Add(new OracleParameter("createdBy", entry.CreatedBy));
            command.Parameters.Add(new OracleParameter("createdDate", entry.CreatedDate));

            var idParameter = new OracleParameter("id", OracleDbType.Int64)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            command.Parameters.Add(idParameter);

            await command.ExecuteNonQueryAsync(ct);

            var oracleDecimal = ((Oracle.ManagedDataAccess.Types.OracleDecimal)idParameter.Value);
            return oracleDecimal.IsNull ? 0 : (long)oracleDecimal.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LOG-FALLBACK] Could not write to LOG_REQ. Original request: {Request}", request);
            return 0;
        }
    }

    public Task LogSuccessAsync(long? requestId, string source, string message, CancellationToken ct = default)
    {
        var entry = new LogResponseEntry
        {
            RequestId = requestId,
            Level = "INFO",
            Source = source,
            Message = message
        };

        return WriteResponseAsync(entry, ct);
    }

    public Task LogErrorAsync(long? requestId, string source, string message, Exception? ex = null, string? category = null, CancellationToken ct = default)
    {
        var entry = new LogResponseEntry
        {
            RequestId = requestId,
            Level = "ERROR",
            Category = category,
            Source = source,
            Message = message,
            ExceptionDetail = ex?.ToString()
        };

        return WriteResponseAsync(entry, ct);
    }

    private async Task WriteResponseAsync(LogResponseEntry entry, CancellationToken ct)
    {
        try
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = new OracleCommand(InsertResponseSql, connection);
            command.Parameters.Add(new OracleParameter("requestId", (object?)entry.RequestId ?? DBNull.Value));
            command.Parameters.Add(new OracleParameter("level", entry.Level));
            command.Parameters.Add(new OracleParameter("category", (object?)entry.Category ?? DBNull.Value));
            command.Parameters.Add(new OracleParameter("source", entry.Source));
            command.Parameters.Add(new OracleParameter("message", entry.Message));
            command.Parameters.Add(new OracleParameter("exceptionDetail", (object?)entry.ExceptionDetail ?? DBNull.Value));
            command.Parameters.Add(new OracleParameter("createdAt", entry.CreatedAt));

            await command.ExecuteNonQueryAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LOG-FALLBACK] Could not write to LOG_RESP. Original log: [{Level}] {Category} {Source} - {Message}",
                entry.Level, entry.Category, entry.Source, entry.Message);
        }
    }
}
