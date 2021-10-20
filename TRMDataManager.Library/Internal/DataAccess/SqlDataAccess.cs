using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace TRMDataManager.Library.Internal.DataAccess
{
    internal class SqlDataAccess
    {
        public string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
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
    }
}
