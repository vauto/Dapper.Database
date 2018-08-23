using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Tests.Database.Extensions
{
    public static class GuidExtensions
    {
        /// <summary>
        /// Flips bytes' endianness.
        /// </summary>
        /// <param name="b">the bytes in one order</param>
        /// <returns>the bytes flipped the other way</returns>
        private static byte[] FlipGuidBytes(this byte[] b) => new[] { b[3], b[2], b[1], b[0], b[5], b[4], b[7], b[6], b[8], b[9], b[10], b[11], b[12], b[13], b[14], b[15] };

        public static byte[] ToByteArray(this Guid guid, Endianness endianness = Endianness.BigEndian)
        {
            switch (endianness)
            {
                case Endianness.BigEndian:
                    return guid.ToByteArray().FlipGuidBytes();
                case Endianness.LittleEndian:
                    return guid.ToByteArray();
                default:
                    throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null);
            }
        }

        /// <summary>
        /// Converts a byte array to a Guid, if possible.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="endianness"></param>
        /// <returns></returns>
        public static Guid ToGuid(this byte[] bytes, Endianness endianness = Endianness.BigEndian)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length != 16) throw new ArgumentException($"Invalid byte array length {bytes.Length}.", nameof(bytes));

            switch (endianness)
            {
                case Endianness.BigEndian:
                    return new Guid(bytes.FlipGuidBytes());
                case Endianness.LittleEndian:
                    return new Guid(bytes);
                default:
                    throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null);
            }
        }
    }

    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

}
