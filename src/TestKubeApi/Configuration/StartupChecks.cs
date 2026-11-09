using TestKubeLibrary.Interfaces;

namespace TestKubeApi.Configuration;

public static class StartupChecks
{
    public static async Task EnsureDatabaseIsReachableAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[STARTUP] Cannot connect to PostgreSQL, aborting startup: {ex.Message}");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}
