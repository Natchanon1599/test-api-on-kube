using Microsoft.Extensions.Logging;
using TestKubeLibrary.Interfaces;

namespace TestKubeLibrary.Logging;

// Used when Database:Enabled = false. Writes request/response logs to ILogger (stdout)
// instead of log_req / log_resp, so `kubectl logs` / `docker logs` still show them.
public class ConsoleLogService : ILogService
{
    private static long _nextRequestId;
    private readonly ILogger<ConsoleLogService> _logger;

    public ConsoleLogService(ILogger<ConsoleLogService> logger)
    {
        _logger = logger;
    }

    public Task<long> LogRequestAsync(string? request, string? username, CancellationToken ct = default)
    {
        long id = Interlocked.Increment(ref _nextRequestId);
        _logger.LogInformation("[REQ {RequestId}] user={Username} request={Request}", id, username, request);
        return Task.FromResult(id);
    }

    public Task LogSuccessAsync(long? requestId, string source, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[RESP {RequestId}] INFO {Source} - {Message}", requestId, source, message);
        return Task.CompletedTask;
    }

    public Task LogErrorAsync(long? requestId, string source, string message, Exception? ex = null, string? category = null, CancellationToken ct = default)
    {
        _logger.LogError(ex, "[RESP {RequestId}] ERROR {Category} {Source} - {Message}", requestId, category, source, message);
        return Task.CompletedTask;
    }
}
