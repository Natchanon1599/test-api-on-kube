using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PrototypeLibrary.Interfaces;

namespace PrototypeLibrary.Data;

// PostgreSQL implementation (default). To use another DB, add a sibling factory
// (e.g. SqlConnectionFactory returning SqlConnection) and swap it in ApplicationServiceConfig.
public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default in configuration");
    }

    public DbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
