using System;
using System.Collections.Generic;
using TemplateStaticAnalyser.Models;

namespace TemplateStaticAnalyser
{
    public interface IDatabaseHelper
    {
        List<SqlServerInstance> GetSqlServers();
        List<string> GetServerDatabases(SqlAuthConnectionModel connectionModel);
        Boolean ValidateLogin(SqlAuthConnectionModel connectionModel);
        String ConnectionString(SqlAuthConnectionModel connectionModel, string databaseName);
    }
}