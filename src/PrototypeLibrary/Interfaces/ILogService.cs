namespace PrototypeLibrary.Interfaces;

// Writes to log_req (before business logic runs) and log_resp (after it returns, success or error).
public interface ILogService
{
    // Call before business logic runs. Returns the new log_req row id (0 if the write failed).
    Task<long> LogRequestAsync(string? request, string? username, CancellationToken ct = default);

    // Call after business logic returns successfully.
    Task LogSuccessAsync(long? requestId, string source, string message, CancellationToken ct = default);

    // Call from a catch block when the call failed.
    Task LogErrorAsync(long? requestId, string source, string message, Exception? ex = null, string? category = null, CancellationToken ct = default);
}
