using System;
using System.Data;
#if ORACLE
using Dapper.Tests.Database.OracleClient;
#endif

namespace Dapper.Tests.Database
{
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        /// <summary>
        /// Flips bytes' endianness.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private byte[] FlipGuidBytes(byte[] b) => new[]{ b[3], b[2], b[1], b[0], b[5], b[4], b[7], b[6], b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15] };

        public override Guid Parse(object value)
        {
            switch (value)
            {
                case string s:
                    return new Guid(s);
                case byte[] b:
                    return new Guid(FlipGuidBytes(b));
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
                    parameter.Value = FlipGuidBytes(value.ToByteArray());
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
