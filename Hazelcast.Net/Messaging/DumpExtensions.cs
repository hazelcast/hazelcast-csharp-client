using System.Text;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    public static class DumpExtensions
    {
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
