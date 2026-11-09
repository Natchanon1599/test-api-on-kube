using Dapper;
using PrototypeLibrary.Exceptions;
using PrototypeLibrary.Interfaces;

namespace PrototypeLibrary.Data;

// DB-agnostic data access: gets a connection from IDbConnectionFactory and runs SQL via Dapper.
// Database errors are wrapped in RepositoryException so upper layers can catch them cleanly.
public class DapperDataAccess : IDataAccess
{
    private readonly IDbConnectionFactory _connectionFactory;

    static DapperDataAccess()
    {
        // Map snake_case columns (created_at) to PascalCase properties (CreatedAt) automatically
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public DapperDataAccess(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<T>> LoadData<T>(string sql, object? parameters = null, CancellationToken ct = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
            return await connection.QueryAsync<T>(command);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException("Failed to load data from the database", ex);
        }
    }

    public async Task<int> SaveData(string sql, object? parameters = null, CancellationToken ct = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
            return await connection.ExecuteAsync(command);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RepositoryException("Failed to save data to the database", ex);
        }
    }

    public async Task SaveDataTransaction(IEnumerable<(string Sql, object? Parameters)> commands, CancellationToken ct = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            foreach (var (sql, parameters) in commands)
            {
                var command = new CommandDefinition(sql, parameters, transaction, cancellationToken: ct);
                await connection.ExecuteAsync(command);
            }
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw new RepositoryException("Transaction failed and was rolled back", ex);
        }
    }
}
