using System;
using System.Data;

namespace Dapper.Tests.Database.Tests
{
    /// <summary>
    /// Oracle's driver has a complicated relationship with Guids.
    /// It tries to ignore them, but apparently allows them for EF queries.
    /// When it does, it uses the builtin <see cref="Guid(byte[])"/> constructor,
    /// which is little-endian.
    /// So Oracle basically treats them as little-endian when it actually does deal with them...
    /// </summary>
    public class LittleEndianGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            switch (value)
            {
                case string s:
                    return new Guid(s);
                case byte[] b:
                    return new Guid(b);
                case Guid g:
                    return g;
                case null:
                    throw new ArgumentNullException(nameof(value));
                default:
                    throw new InvalidCastException($"Cannot convert value of type {value.GetType()} to Guid.");
            }
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToByteArray();
        }
    }
}
