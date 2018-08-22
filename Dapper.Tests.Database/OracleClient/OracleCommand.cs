#if !NETCOREAPP1_0
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using RealOracleCommand = Oracle.ManagedDataAccess.Client.OracleCommand;
using RealOracleConnection = Oracle.ManagedDataAccess.Client.OracleConnection;

namespace Dapper.Tests.Database.OracleClient
{
    /// <summary>
    /// Wrapper of <see cref="RealOracleCommand"/> whose sole purpose is to massage standard Dapper SQL into Oracle SQL.
    /// </summary>
    /// <remarks>
    /// Of all the ADO.NET drivers, ODP.NET is the only one that doesn't parse MSSQL-style bind variables (e.g. <c>@foo</c>).
    /// This wraps ODP.NET so that tests can use ODP.NET without fear.
    /// </remarks>
    public class OracleCommand : DbCommand
    {
        private readonly RealOracleCommand _command;
        private OracleConnection _connection;

        /// <summary>
        /// Called from <see cref="OracleConnection.CreateCommand"/>.
        /// </summary>
        /// <param name="connection"></param>
        internal OracleCommand(OracleConnection connection)
        {
            _connection = connection;
            _command = connection.Connection.CreateCommand();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _command?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void Cancel() => _command.Cancel();

        protected override DbParameter CreateDbParameter() => _command.CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _command.ExecuteReader(behavior);

        public override int ExecuteNonQuery() => _command.ExecuteNonQuery();

        public override object ExecuteScalar() => _command.ExecuteScalar();

        public override void Prepare() => _command.Prepare();

        private string _rawCommandText;

        public override string CommandText
        {
            get => _rawCommandText;
            set
            {
                if (_rawCommandText == value) return;

                _rawCommandText = value;
                _command.CommandText = value?.Replace('@', ':'); // FIXME more granular
            }
        }

        public override int CommandTimeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _command.CommandType;
            set => _command.CommandType = value;
        }

        public new OracleConnection Connection
        {
            get => _connection;
            set
            {
                if (_connection == value) return;

                _connection = value;
                _command.Connection = _connection?.Connection;
            }
        }

        protected override DbConnection DbConnection
        {
            get => Connection;
            set
            {
                switch (value)
                {
                    case null:
                        Connection = null;
                        break;
                    case OracleConnection connection:
                        Connection = connection;
                        break;
                    case RealOracleConnection connection:
                        if (Connection?.Connection != connection)
                        {
                            Connection = new OracleConnection(connection);
                        }
                        break;
                    default:
                        throw new InvalidCastException($"Cannot cast connection of type {value.GetType()} to {typeof(RealOracleConnection)}.");
                }
            }
        }

        protected override DbParameterCollection DbParameterCollection => _command.Parameters;

        public new OracleParameterCollection Parameters => _command.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _command.Transaction;
            set => _command.Transaction = (OracleTransaction)value;
        }

        public new OracleTransaction Transaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _command.UpdatedRowSource;
            set => _command.UpdatedRowSource = value;
        }

        public override bool DesignTimeVisible
        {
            get => _command.DesignTimeVisible;
            set => _command.DesignTimeVisible = value;
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken) => _command.ExecuteScalarAsync(cancellationToken);
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => _command.ExecuteNonQueryAsync(cancellationToken);
        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) => _command.ExecuteReaderAsync(behavior, cancellationToken);
    }
}
#endif
