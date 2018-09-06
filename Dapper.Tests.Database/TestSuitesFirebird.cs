﻿using System;
using System.IO;
using Dapper.Database;
using FirebirdSql.Data.FirebirdClient;
using Xunit;

namespace Dapper.Tests.Database
{

    [Trait("Provider", "Firebird")]
    public class FirebirdTestSuite : TestSuite
    {
        public static string ConnectionString => $"DataSource=localhost;User=SYSDBA;Password=Password12!;";

        protected override void CheckSkip()
        {
            if (_skip) throw new SkipTestException("Skipping Firebird Tests - no server.");
        }

        public override ISqlDatabase GetSqlDatabase()
        {
            CheckSkip();
            var filename = Directory.GetCurrentDirectory() + "\\Test.Db.fdb";
            return new SqlDatabase(new StringConnectionService<FbConnection>($"Database={filename};{ConnectionString}"));
        }

        public override Provider GetProvider() => Provider.Firebird;

        private static readonly bool _skip;

        static FirebirdTestSuite()
        {
            Environment.SetEnvironmentVariable("NoCache", "True");

            var init = false;
            ResetDapperTypes();
            SqlMapper.AddTypeHandler<Guid>(new GuidTypeHandler());
            //SqlMapper.AddTypeHandler<decimal>(new NumericTypeHandler());

            //if (File.Exists(FileName))
            //{
            //    File.Delete(FileName);
            //}

            var commandText = string.Empty;

            try
            {
                var filename = Directory.GetCurrentDirectory() + "\\Test.Db.sfdb";

                using ( var connection = new FbConnection($"Database={filename};{ConnectionString}") )
                {
                    connection.Open();

                    if ( init )
                    {
                        var file = File.OpenText(".\\Scripts\\firebirdawlite.sql");
                        var line = string.Empty;

                        while ( (line = file.ReadLine()) != null )
                        {
                            if ( line.Equals("GO", StringComparison.OrdinalIgnoreCase) )
                            {
                                if ( !string.IsNullOrEmpty(commandText) )
                                    connection.Execute(commandText);
                                commandText = string.Empty;
                            }
                            else
                            {
                                commandText += "\r\n" + line;
                            }
                        }
                    }
                    connection.Execute("delete from Person");
                }

            }
            catch ( FbException ex )
            {
                if ( ex.Message.Contains("Unable to complete network request") )
                {
                    _skip = true;
                }
                else
                {
                    throw;
                }
            }

        }
    }

}
