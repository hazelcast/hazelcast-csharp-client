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
        /// <param name="linePrefix">A prefix for each line.</param>
        /// <returns>A readable string representation of the message.</returns>
        public static string Dump(this ClientMessage message, string linePrefix)
        {
            string prefix;

            linePrefix += "     ";

            var text = new StringBuilder();

            if (message.MessageType == 0)
            {
                prefix = "EXCEPTION";
                text.AppendLine(linePrefix + prefix);
            }
            else
            {
                prefix = message.IsEvent
                    ? "EVENT"
                    : (message.MessageType & 1) > 0 ? "RESPONSE" : "REQUEST";

                text.AppendLine($"{linePrefix}{prefix} [{message.CorrelationId}]");
                text.AppendLine($"{linePrefix}TYPE 0x{message.MessageType:x} {message.OperationName}");
            }

            if (prefix == "MESSAGE")
            {
                if (message.FirstFrame.Length >= FrameFields.SizeOf.LengthAndFlags + FrameFields.Offset.PartitionId + FrameFields.SizeOf.PartitionId)
                    text.AppendLine($"{linePrefix}PARTID {message.PartitionId}");
            }

            var frame = message.FirstFrame;
            while (frame != null)
            {
                text.Append($"{linePrefix}FRAME ");
                text.Append(frame);
                frame = frame.Next;
                if (frame != null)
                    text.AppendLine();
            }

            return text.ToString();
        }
    }
}
