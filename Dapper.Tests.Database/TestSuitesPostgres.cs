using System;
using System.IO;
using System.Net.Sockets;
using Dapper.Database;
using Npgsql;
using Xunit;

namespace Dapper.Tests.Database
{
    [Trait("Provider", "Postgres")]
    public partial class PostgresTestSuite : TestSuite
    {
        private const string DbName = "test";
        public static string ConnectionString =>
            IsAppVeyor
                ? $"Server=localhost;Port=5432;User Id=postgres;Password=Password12!;Database={DbName}"
                : $"Server=localhost;Port=5432;User Id=postgres;Password=Password12!;Database={DbName}";

        protected override void CheckSkip()
        {
            if (_skip) throw new SkipTestException("Skipping Postgres Tests - no server.");
        }

        public override ISqlDatabase GetSqlDatabase()
        {
            CheckSkip();
            return new SqlDatabase(new StringConnectionService<NpgsqlConnection>(ConnectionString));
        }

        public override Provider GetProvider() => Provider.Postgres;

        private static readonly bool _skip;

        static PostgresTestSuite()
        {
            Environment.SetEnvironmentVariable("NoCache", "True");

            ResetDapperTypes();
            SqlMapper.AddTypeHandler<Guid>(new GuidTypeHandler());
            try
            {
                // Ensure the 'test' database is created.
                // We have to do this outside the main SQL script for two reasons:
                //  1. PostgreSQL CREATE DATABASE does not have an IF NOT EXISTS option
                //  2. PostgreSQL CREATE DATABASE cannot be executed inside a transaction block.
                // @see https://stackoverflow.com/questions/18389124/simulate-create-database-if-not-exists-for-postgresql
                using (var connection = new NpgsqlConnection($"Server=localhost;Port=5432;User Id=postgres;Password=Password12!;"))
                {
                    connection.Open();

                    try
                    {
                        connection.Execute("CREATE DATABASE test");
                    }
                    catch (NpgsqlException e) when (e.Message.StartsWith("42P04", StringComparison.OrdinalIgnoreCase))
                    {
                        // Database "test" already exists.  Fall through.
                    }
                }

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var awfile = File.ReadAllText(".\\Scripts\\postgresawlite.sql");
                    connection.Execute(awfile);
                    connection.Execute("delete from Person;");

                }
            }

            catch (SocketException e)
            {
                if (e.Message.Contains("No connection could be made because the target machine actively refused it"))
                    _skip = true;
                else
                    throw;
            }
        }
    }
}
