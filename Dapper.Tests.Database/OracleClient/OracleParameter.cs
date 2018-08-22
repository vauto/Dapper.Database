using System;
using System.Data;

#if !NETCOREAPP1_0
using RealOracleParameter = Oracle.ManagedDataAccess.Client.OracleParameter;
#endif

#if !NETCOREAPP1_0
namespace Dapper.Tests.Database.OracleClient
{
    public class OracleParameter : System.Data.Common.DbParameter
    {
        internal RealOracleParameter RealParameter { get; }

        internal OracleParameter(RealOracleParameter parameter) => RealParameter = parameter ?? throw new ArgumentNullException(nameof(parameter));

        public override void ResetDbType() => RealParameter.ResetDbType();

        public override DbType DbType
        {
            get => RealParameter.DbType;
            set
            {
                switch (value)
                {
                    case DbType.Guid:
                        // Oracle does not support DbType.Guid.
                        // Convention is to use binary (endianness up to the TypeHandler).
                        RealParameter.DbType = DbType.Binary;
                        break;
                    default:
                        // Let Oracle sort the rest out
                        RealParameter.DbType = value;
                        break;
                }
            }
        }

        public override ParameterDirection Direction
        {
            get => RealParameter.Direction;
            set => RealParameter.Direction = value;
        }

        public override bool IsNullable
        {
            get => RealParameter.IsNullable;
            set => RealParameter.IsNullable = value;
        }

        public override string ParameterName
        {
            get => RealParameter.ParameterName;
            set => RealParameter.ParameterName = value;
        }

        public override string SourceColumn
        {
            get => RealParameter.SourceColumn;
            set => RealParameter.SourceColumn = value;
        }

        public override object Value
        {
            get => RealParameter.Value;
            set
            {
                switch (value)
                {
                    case Guid _:
                        throw new ArgumentException("Oracle does not like guids");
                    default:
                        RealParameter.Value = value;
                        break;
                }

            }
        }

        public override bool SourceColumnNullMapping
        {
            get => RealParameter.SourceColumnNullMapping;
            set => RealParameter.SourceColumnNullMapping = value;
        }

        public override int Size
        {
            get => RealParameter.Size;
            set => RealParameter.Size = value;
        }

        public override byte Precision
        {
            get => RealParameter.Precision;
            set => RealParameter.Precision = value;
        }

        public override byte Scale
        {
            get => RealParameter.Scale;
            set => RealParameter.Scale = value;
        }

        public override DataRowVersion SourceVersion
        {
            get => RealParameter.SourceVersion;
            set => RealParameter.SourceVersion = value;
        }
    }
}


#endif
