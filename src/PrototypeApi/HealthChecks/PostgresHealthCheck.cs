using Microsoft.Extensions.Diagnostics.HealthChecks;
using PrototypeLibrary.Interfaces;

namespace PrototypeApi.HealthChecks;

public class PostgresHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL connection succeeded");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
        }
    }
}
