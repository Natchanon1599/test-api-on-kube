using TestKubeApi.BusinessLogic;
using TestKubeApi.Interfaces;
using TestKubeApi.Repositories;
using TestKubeApi.Services;
using TestKubeLibrary.Data;
using TestKubeLibrary.Interfaces;
using TestKubeLibrary.Logging;

namespace TestKubeApi.Configuration;

public static class ApplicationServiceConfig
{
    // appsettings "Database:Enabled" (env var: Database__Enabled). Default false = run without any DB.
    public static bool IsDatabaseEnabled(this IConfiguration configuration)
    {
        return configuration.GetValue<bool>("Database:Enabled");
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        bool dbEnabled = configuration.IsDatabaseEnabled();

        // Data access: swap IDbConnectionFactory to target another DB; the rest stays the same.
        // Default = PostgreSQL. DapperDataAccess runs raw SQL over whatever connection the factory gives.
        // DB disabled -> DisabledDbConnectionFactory: DB-backed endpoints return 503, nothing connects.
        if (dbEnabled)
            services.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();
        else
            services.AddScoped<IDbConnectionFactory, DisabledDbConnectionFactory>();
        services.AddScoped<IDataAccess, DapperDataAccess>();

        // External API client (typed HttpClient)
        services.AddHttpClient<ITestKubeService, TestKubeService>();
        services.AddHttpClient<IStoreService, StoreService>();

        // Controller -> BusinessLogic -> (Service | Repository)
        services.AddScoped<ITestKubeRepository, TestKubeRepository>();
        services.AddScoped<ITestKubeBusinessLogic, TestKubeBusinessLogic>();
        services.AddScoped<IStoreBusinessLogic, StoreBusinessLogic>();

        // Logs (request + success/error outcome) go to Postgres tables log_req / log_resp,
        // or to stdout (ILogger) when the DB is disabled.
        if (dbEnabled)
            services.AddScoped<ILogService, PostgresLogService>();
        else
            services.AddScoped<ILogService, ConsoleLogService>();

        return services;
    }
}
