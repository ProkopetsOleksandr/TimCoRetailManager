using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace TRMDataManager.Library.Internal.DataAccess
{
    public class SqlDataAccess : IDisposable, ISqlDataAccess
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqlDataAccess> _logger;
        private bool IsConnectionClosed = false;

        public SqlDataAccess(IConfiguration configuration, ILogger<SqlDataAccess> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GetConnectionString(string name)
        {
            return _configuration.GetConnectionString(name);
        }

        public List<T> LoadData<T, U>(string storeProcedure, U parameters, string connectionStringName)
        {
            var connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                return connection
                    .Query<T>(storeProcedure, parameters, commandType: CommandType.StoredProcedure)
                    .ToList();
            }
        }

        public void SaveData<T>(string storeProcedure, T parameters, string connectionStringName)
        {
            var connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                connection.Execute(storeProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void SaveDataInTransaction<T>(string storeProcedure, T parameters)
        {
            _dbConnection.Execute(storeProcedure, parameters,
                commandType: CommandType.StoredProcedure, transaction: _dbTransaction);
        }

        public List<T> LoadDataInTransaction<T, U>(string storeProcedure, U parameters)
        {
            return _dbConnection
                    .Query<T>(storeProcedure, parameters,
                    commandType: CommandType.StoredProcedure, transaction: _dbTransaction).ToList();
        }

        private IDbConnection _dbConnection;
        private IDbTransaction _dbTransaction;

        public void StartTransaction(string connectionStringName)
        {
            var connectionString = GetConnectionString(connectionStringName);

            _dbConnection = new SqlConnection(connectionString);
            _dbConnection.Open();
            _dbTransaction = _dbConnection.BeginTransaction();

            IsConnectionClosed = false;
        }

        public void CommitTransaction()
        {
            _dbTransaction?.Commit();
            _dbConnection?.Close();

            IsConnectionClosed = true;
        }

        public void RollbackTransaction()
        {
            _dbTransaction?.Rollback();
            _dbConnection?.Close();

            IsConnectionClosed = true;
        }

        public void Dispose()
        {
            if (!IsConnectionClosed)
            {
                try
                {
                    CommitTransaction();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Commin transaction failed in the dispose method.");
                }
            }

            _dbConnection = null;
            _dbConnection = null;
        }
    }
}
