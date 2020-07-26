using System.Data;

namespace Storage.Interfaces
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetSqlDbConnection();
    }
}