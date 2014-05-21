using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public class DatabaseHelper : IDatabaseHelper
    {
        public List<SqlServerInstance> GetSqlServers()
        {
            var sqlSources = SqlDataSourceEnumerator.Instance.GetDataSources();

            return (from DataRow row in sqlSources.Rows
                    let serverName = row["ServerName"].ToString()
                    let instanceName = row["InstanceName"].ToString()
                    let version = row["Version"].ToString()
                    orderby serverName
                    select new SqlServerInstance
                    {
                        InstanceName = instanceName,
                        ServerName = serverName,
                        Version = version
                    }).ToList();
        }

        public List<String> GetServerDatabases(SqlAuthConnectionModel connectionModel)
        {
            return GetServerDatabases(ConnectionString(connectionModel));
        }

        public Boolean ValidateLogin(SqlAuthConnectionModel connectionModel)
        {
            var connectionString = ConnectionString(connectionModel);
            return ValidateLogin(connectionString);
        }

        private Boolean ValidateLogin(String connectionString)
        {
            try
            {
                using (CreateOpenConnection(connectionString))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<String> GetServerDatabases(String connectionString)
        {
            using (var sqlConnection = CreateOpenConnection(connectionString))
            {
                var allDatabaseNames = sqlConnection.GetSchema("Databases");
                var databaseNames = RemoveExcludedDatabases(allDatabaseNames);
                return databaseNames;
            }
        }
        private SqlConnection CreateOpenConnection(String connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        private List<String> RemoveExcludedDatabases(DataTable databaseList)
        {
            return (from DataRow r in databaseList.Rows
                    let dbName = r["database_name"].ToString()
                    orderby dbName
                    where !ExcludedDatabases.Contains(dbName)
                    select dbName).ToList();
        }

        /// <summary>
        /// The databases we exclude from the database drop down.
        /// </summary>
        /// <value>
        /// The excluded databases.
        /// </value>
        private IEnumerable<String> ExcludedDatabases
        {
            get { return new[] { "master", "tempdb", "model", "msdb" }; }
        }

        public String ConnectionString(SqlAuthConnectionModel connectionModel)
        {
            return new SqlConnectionStringBuilder
            {
                UserID = connectionModel.UserName,
                DataSource = connectionModel.ServerName,
                Password = connectionModel.Password
            }.ConnectionString;
        }
    }
}
