using System.Data.Common;
using TestKubeLibrary.Interfaces;

namespace TestKubeLibrary.Data;

// Used when Database:Enabled = false. Any DB call fails fast with a clear message
// (DapperDataAccess wraps it in RepositoryException -> HTTP 503), nothing tries to reach a server.
public class DisabledDbConnectionFactory : IDbConnectionFactory
{
    public DbConnection CreateConnection()
    {
        throw new InvalidOperationException("Database is disabled (Database:Enabled = false)");
    }
}
