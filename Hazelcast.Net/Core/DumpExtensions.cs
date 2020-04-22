using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    public static class DumpExtensions
    {
        /// <summary>
        /// Dumps an array of bytes into a readable format.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <param name="linePrefix">A prefix for each line.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the array.</param>
        /// <returns>A readable string representation of the array of bytes.</returns>
        public static string Dump(this byte[] bytes, string linePrefix, int length = 0)
        {
            if (length > bytes.Length)
                throw new InvalidOperationException(ExceptionMessages.NotEnoughBytes);

            linePrefix += "     ";

            if (length == 0) length = bytes.Length;

            var text = new StringBuilder();
            var i = 0;
            while (i < length)
            {
                text.Append(linePrefix);
                for (var j = 0; j < 8 && i < length; j++, i++)
                {
                    text.AppendFormat("{0:x2} ", bytes[i]);
                }

                if (i < length)
                    text.AppendLine();
            }

            return text.ToString();
        }

        /// <summary>
        /// Dumps an sequence of bytes into a readable format.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="linePrefix">A prefix for each line.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the sequence.</param>
        /// <returns>A readable string representation of the sequence of bytes.</returns>
        public static string Dump(this ReadOnlySequence<byte> bytes, string linePrefix, int length = 0)
        {
            var a = new byte[bytes.Length];
            bytes.CopyTo(a);
            return a.Dump(linePrefix, length);
        }
    }
}
