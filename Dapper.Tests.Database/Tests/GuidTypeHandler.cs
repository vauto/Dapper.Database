using System;
using System.Data;
using Dapper.Tests.Database.Extensions;
#if ORACLE
using Dapper.Tests.Database.OracleClient;
#endif

namespace Dapper.Tests.Database
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            switch (value)
            {
                case string s:
                    return new Guid(s);
                case byte[] b:
                    return GuidExtensions.ToGuid(b);
                default:
                    return (Guid)value;
            }
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
#if ORACLE
            switch (parameter)
            {
                case OracleParameter _:
                    // Oracle does not like Guids.
                    parameter.Value = GuidExtensions.ToByteArray(value);
                    return;
            }
#endif
            // Everyone else...
            parameter.Value = value;
        }
    }


    public class NumericTypeHandler : SqlMapper.TypeHandler<decimal>
    {
        public override decimal Parse(object value)
        {
            return Convert.ToDecimal(value);
        }

        public override void SetValue(IDbDataParameter parameter, decimal value)
        {
            parameter.Value = value;
        }
    }
}
