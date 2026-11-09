namespace PrototypeLibrary.Models;

// One row of log_req - written before business logic runs (see DDL in db/002_create_log_tables.sql)
public class LogRequestEntry
{
    public long Id { get; set; }

    public string? Request { get; set; }

    public string? Username { get; set; }

    // App name, from appsettings "AppName"
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
