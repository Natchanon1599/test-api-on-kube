using System.Data.Common;

namespace TestKubeLibrary.Interfaces;

// The single DB-specific seam. Swap the implementation (and the ADO.NET provider package)
// to target another database; everything above this talks to DbConnection / Dapper only.
public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}
