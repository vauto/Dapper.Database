#if !NETCOREAPP1_0
using System;
using System.Data;
using System.Data.Common;
using RealOracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace Dapper.Tests.Database.OracleClient
{
    /// <summary>
    /// Wrapper for <see cref="RealOracleConnection"/> that creates safe <see cref="OracleCommand"/> objects.
    /// </summary>
    public class OracleConnection : DbConnection
    {
        /// <summary>
        /// The wrapped connection.
        /// </summary>
        internal RealOracleConnection Connection { get; }

        internal OracleConnection(RealOracleConnection connection) => Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        public OracleConnection() : this(new RealOracleConnection())
        {
        }

        public OracleConnection(string connectionString) : this(new RealOracleConnection(connectionString))
        {
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => Connection.BeginTransaction(isolationLevel);


        public override void ChangeDatabase(string databaseName) => Connection.ChangeDatabase(databaseName);

        public override void Close() => Connection.Close();

        public new OracleCommand CreateCommand() => new OracleCommand(this);

        protected override DbCommand CreateDbCommand() => new OracleCommand(this);

        public override void Open() => Connection.Open();

        public override string ConnectionString
        {
            get => Connection.ConnectionString;
            set => Connection.ConnectionString = value;
        }

        public override int ConnectionTimeout => Connection.ConnectionTimeout;
        public override string Database => Connection.Database;
        public override ConnectionState State => Connection.State;
        public override string DataSource => Connection.DataSource;
        public override string ServerVersion => Connection.ServerVersion;
    }
}
#endif
