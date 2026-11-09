namespace PrototypeLibrary.Interfaces;

// Generic SQL data access - callers pass raw SQL + parameters (run via Dapper).
// DB-agnostic: the concrete database is chosen by the injected IDbConnectionFactory.
public interface IDataAccess
{
    // Query and map each row to T (SELECT)
    Task<IEnumerable<T>> LoadData<T>(string sql, object? parameters = null, CancellationToken ct = default);

    // Execute a non-query (INSERT/UPDATE/DELETE); returns rows affected
    Task<int> SaveData(string sql, object? parameters = null, CancellationToken ct = default);

    // Run several commands in a single transaction - all succeed or all roll back
    Task SaveDataTransaction(IEnumerable<(string Sql, object? Parameters)> commands, CancellationToken ct = default);
}
