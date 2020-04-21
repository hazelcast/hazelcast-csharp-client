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
            text.AppendLine($"{prefix} 0x{message.MessageType:x}");
            var frame = message.FirstFrame;
            while (frame != null)
            {
                text.Append("  FRAME ");
                text.AppendLine(frame.ToString());
                frame = frame.Next;
            }

            return text.ToString();
        }
    }
}
