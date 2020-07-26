using System.Data;
using System.Data.SqlClient;
using Storage.Interfaces;

namespace Storage.Azure
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private IDbConnection _connection;
        private string _connectionString;

        public SqlConnectionFactory(IDbConnection singleConnection)
        {
            _connection = singleConnection;
        }

        public SqlConnectionFactory(string sqlConnectionString)
        {
            _connectionString = sqlConnectionString;
        }

        public IDbConnection GetSqlDbConnection()
        {
            if (_connection != null)
            {
                return _connection;
            }
            
            return new SqlConnection(_connectionString);
        }
    }
}