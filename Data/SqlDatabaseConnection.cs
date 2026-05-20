using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SighApp.Data
{
    public class SqlDatabaseConnection
    {
        private readonly string _connectionString;

        public SqlDatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=SighDb;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
