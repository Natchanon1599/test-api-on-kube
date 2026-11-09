using PrototypeApi.BusinessLogic;
using PrototypeApi.Interfaces;
using PrototypeApi.Repositories;
using PrototypeApi.Services;
using PrototypeLibrary.Data;
using PrototypeLibrary.Interfaces;
using PrototypeLibrary.Logging;

namespace PrototypeApi.Configuration;

public static class ApplicationServiceConfig
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Data access: swap IDbConnectionFactory to target another DB; the rest stays the same.
        // Default = PostgreSQL. DapperDataAccess runs raw SQL over whatever connection the factory gives.
        services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IDataAccess, DapperDataAccess>();

        // External API client (typed HttpClient)
        services.AddHttpClient<IPrototypeService, PrototypeService>();

        // Controller -> BusinessLogic -> (Service | Repository)
        services.AddScoped<IPrototypeRepository, PrototypeRepository>();
        services.AddScoped<IPrototypeBusinessLogic, PrototypeBusinessLogic>();

        // Logs (request + success/error outcome) go to Postgres tables log_req / log_resp
        services.AddScoped<ILogService, PostgresLogService>();

        return services;
    }
}
