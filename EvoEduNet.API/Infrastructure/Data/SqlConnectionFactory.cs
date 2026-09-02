using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace EvoEduNet.API.Infrastructure.Data
{
    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionStringName = "DefaultConnection")
        {
            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionStringSettings == null || string.IsNullOrWhiteSpace(connectionStringSettings.ConnectionString))
            {
                throw new InvalidOperationException($"A ConnectionString '{connectionStringName}' não foi encontrada no App.config.");
            }

            _connectionString = connectionStringSettings.ConnectionString;
        }

        public SqlConnectionFactory(string connectionString, bool isRawConnectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("A connection string não pode ser nula ou vazia.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            return connection;
        }
    }
}
