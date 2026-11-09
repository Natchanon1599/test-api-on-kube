namespace PrototypeLibrary.Models;

// One row of log_resp - written after business logic returns, success or error,
// linked back to log_req via RequestId (see DDL in db/002_create_log_tables.sql)
public class LogResponseEntry
{
    public long Id { get; set; }

    public long? RequestId { get; set; }

    // INFO, ERROR
    public string Level { get; set; } = "INFO";

    // Error category, e.g. DB_CONNECTION / DB_QUERY / DATA_FORMAT / VALIDATION / UNKNOWN (null on success)
    public string? Category { get; set; }

    // Source of the log, e.g. "PrototypeBusinessLogic.CreateAsync"
    public string Source { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    // Stack trace / exception detail, if any (null on success)
    public string? ExceptionDetail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
