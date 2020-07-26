using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage.IntegrationTests.Azure
{
    public abstract class AzureSqlServerStorageBaseSpec : AnyStorageBaseSpec
    {
        protected static void InitializeAllTests(TestContext context, string dbName, ISqlConnectionFactory sqlConnectionFactory)
        {   
            // string sql = $"CREATE DATABASE {dbName}";
            //
            // using (var connection = sqlConnectionFactory.GetSqlDbConnection())
            // {
            //     connection.Execute(sql);
            // }
        }

        protected static void CleanupAllTests(string dbName, ISqlConnectionFactory sqlConnectionFactory)
        {
            // string sql = $"DROP DATABASE {dbName}";
            //
            // using (var connection = sqlConnectionFactory.GetSqlDbConnection())
            // {
            //     connection.Execute(sql);
            // }
        }
    }
}