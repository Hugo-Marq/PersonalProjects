using Microsoft.Data.SqlClient;

namespace QuickRoomSolutions.Connections
{
    public class SqlServerDbConnection
    {
        private string _connectionString;

        
        //private SqlConnection _connection;

      
        public SqlServerDbConnection()
        {
            
            var host = Environment.GetEnvironmentVariable("SQL_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("SQL_PORT") ?? "";
            var db = Environment.GetEnvironmentVariable("SQL_DB") ?? "QuickRoomSolutionDatabase";
            var user = Environment.GetEnvironmentVariable("SQL_USER") ?? "";
            var password = Environment.GetEnvironmentVariable("SQL_PASSWORD") ?? "";
            var security = Environment.GetEnvironmentVariable("SQL_SECURITY") ?? "true";
            var trustServerCertificate = Environment.GetEnvironmentVariable("SQL_CERTIFICATE") ?? "true";

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                host = !string.IsNullOrEmpty(port) ? host + "," + port : host;
                _connectionString = $"Data Source={host};Initial Catalog={db};User ID={user};Password={password};";
            }
            else
            {
                _connectionString = $"Data Source={host};Initial Catalog={db};Integrated Security={security};TrustServerCertificate={trustServerCertificate}";
            }

            //_connection = new SqlConnection(_connectionString);
            //_connection.Open();

        }

        //public SqlConnection GetConnection()
        //{
        //    return _connection;
        //}

        public string GetConnectionString()
        {
            return _connectionString;
;
        }

    }
}
