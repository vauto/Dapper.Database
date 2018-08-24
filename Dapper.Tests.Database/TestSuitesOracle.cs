using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Dapper.Database;
using Xunit;

#if ORACLE
using Oracle.ManagedDataAccess.Client;
using OracleConnection = Dapper.Tests.Database.OracleClient.OracleConnection;
#endif

namespace Dapper.Tests.Database
{

#if ORACLE
    [Trait("Provider", "Oracle")]
    public partial class OracleTestSuite : TestSuite
    {
        private const string DbName = "test";
        public static string ConnectionString =>
            IsAppVeyor
                ? $"User Id=DapperContribTests;Password=Password12!;Data Source=localhost:1521/{DbName}"
                : $"User Id=DapperContribTests;Password=Password12!;Data Source=localhost:1521/ORCLPDB1.localdomain"; // FIXME need good service name like "test"

        public override ISqlDatabase GetSqlDatabase()
        {
            if(_skip) throw new SkipTestException("Skipping Oracle Tests - no server.");
            return new SqlDatabase(new StringConnectionService<OracleConnection>(ConnectionString));
        }


        public override Provider GetProvider() => Provider.Oracle;

        private static readonly bool _skip;

        private static readonly Regex CommandSeparator = new Regex("^/\r?\n", RegexOptions.Multiline);

        static OracleTestSuite()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();

                    var awfile = File.ReadAllText(".\\Scripts\\oracleawlite.sql");

                    // Because the Oracle driver does not support multiple statements in a single IDbCommand, we have to manually split the file.
                    // The file is marked with lines with just forward slashes ("/"), which is the way SQL*Plus and other tools recognize the end of a command in such situations, so just use that.
                    // (It also helps the ability to debug the script in SQL*Plus or another tool.)
                    foreach (var command in CommandSeparator.Split(awfile))
                    {
                        // don't execute blank commands (e.g. last line)
                        if (string.IsNullOrWhiteSpace(command))
                            continue;
                        // don't execute anything starting with a comment indicating use of SQL*Plus
                        if (command.StartsWith("/*SQLPLUS*/", StringComparison.OrdinalIgnoreCase))
                            continue;

                        connection.Execute(command);
                    }

                    connection.Execute("delete from Person");
                }
            }
            catch (OracleException e) when (e.Message.StartsWith("ORA-125", StringComparison.OrdinalIgnoreCase))
            {
                // All ORA- errors (12500-12599) are TNS errors indicating connectivity.
                _skip = true;

            }
            catch (SocketException e) when (e.Message.Contains("No connection could be made because the target machine actively refused it"))
            {
                _skip = true;
            }
        }
    }
#endif

}
