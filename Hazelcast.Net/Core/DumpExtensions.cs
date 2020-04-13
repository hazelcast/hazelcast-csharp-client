using System;
using System.Buffers;
using System.Linq;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    public static class DumpExtensions
    {
        /// <summary>
        /// Dumps an array of bytes into a readable string.
        /// </summary>
        /// <param name="bytes">The array of bytes.</param>
        /// <param name="prefix">A prefix.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the array.</param>
        /// <returns>A readable string representation of the array of bytes.</returns>
        public static string Dump(this byte[] bytes, string prefix, int length = 0)
        {
            if (length > bytes.Length)
                throw new InvalidOperationException(ExceptionMessages.NotEnoughBytes);

            return prefix + string.Join(" ", bytes.Take(length > 0 ? length : bytes.Length).Select(x => $"{x:x2}"));
        }

        /// <summary>
        /// Dumps an sequence of bytes into a readable string.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="prefix">A prefix.</param>
        /// <param name="length">The number of bytes to dump, or zero to dump all bytes in the sequence.</param>
        /// <returns>A readable string representation of the sequence of bytes.</returns>
        public static string Dump(this ReadOnlySequence<byte> bytes, string prefix, int length = 0)
        {
            var a = new byte[bytes.Length];
            bytes.CopyTo(a);
            return prefix + string.Join(" ", a.Take(length > 0 ? length : (int)bytes.Length).Select(x => $"{x:x2}"));
        }
    }
}
