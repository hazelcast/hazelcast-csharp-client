using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Hazelcast.Messaging;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    public static class DumpExtensions
    {
        /// <summary>
        /// Defines exception messages.
        /// </summary>
        private static class ExceptionMessage
        {
            public const string NotEnoughBytes = "Not enough bytes.";
        }

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
                throw new InvalidOperationException(ExceptionMessage.NotEnoughBytes);

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

        /// <summary>
        /// Dumps a client message into a readable string.
        /// </summary>
        /// <param name="message">The client message.</param>
        /// <param name="prefix">A prefix.</param>
        /// <returns>A readable string representation of the message.</returns>
        public static string Dump(this ClientMessage message, string prefix = "MESSAGE")
        {
            var text = new StringBuilder();
            text.AppendLine(prefix);
            var frame = message.FirstFrame;
            while (frame != null)
            {
                var flagNames = ((frame.Flags & FrameFlags.AllFlags) > 0 
                                        ? frame.Flags.ToString() 
                                        : "") +
                                      (((ClientMessageFlags) frame.Flags & ClientMessageFlags.AllFlags) > 0 
                                        ? ((ClientMessageFlags)frame.Flags).ToString() 
                                        : "");

                text.Append("  FRAME ");
                text.Append(frame.Length);
                text.Append(" ");
                text.Append($"0x{frame.Flags:x} {flagNames}");
                text.AppendLine();
                frame = frame.Next;
            }

            return text.ToString();
        }
    }
}
