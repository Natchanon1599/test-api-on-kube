using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrototypeLibrary.Interfaces;
using PrototypeLibrary.Models;

namespace PrototypeLibrary.Logging;

// Writes logs to log_req / log_resp (DDL in db/002_create_log_tables.sql) via the shared
// IDataAccess, so it targets whatever database the app is configured for and reuses the
// existing RepositoryException wrapping instead of managing Dapper/connections directly.
// Logging must never break the business flow: if a write fails it falls back to ILogger instead of rethrowing.
public class PostgresLogService : ILogService
{
    private readonly IDataAccess _dataAccess;
    private readonly ILogger<PostgresLogService> _logger;
    private readonly string _appName;

    private const string InsertRequestSql = @"
        INSERT INTO log_req (request, username, created_by, created_date)
        VALUES (@Request, @Username, @CreatedBy, @CreatedDate)
        RETURNING id";

    private const string InsertResponseSql = @"
        INSERT INTO log_resp (request_id, log_level, category, source, message, exception_detail, created_at)
        VALUES (@RequestId, @Level, @Category, @Source, @Message, @ExceptionDetail, @CreatedAt)";

    public PostgresLogService(IDataAccess dataAccess, IConfiguration configuration, ILogger<PostgresLogService> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
        // Use the indexer, not GetValue<T>(): PrototypeLibrary only references
        // Microsoft.Extensions.Configuration.Abstractions, which has no .Binder extension methods.
        _appName = configuration["AppName"] ?? "prototype";
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
            var rows = await _dataAccess.LoadData<long>(InsertRequestSql, entry, ct);
            return rows.First();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LOG-FALLBACK] Could not write to log_req. Original request: {Request}", request);
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
            await _dataAccess.SaveData(InsertResponseSql, entry, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LOG-FALLBACK] Could not write to log_resp. Original log: [{Level}] {Category} {Source} - {Message}",
                entry.Level, entry.Category, entry.Source, entry.Message);
        }
    }
}
