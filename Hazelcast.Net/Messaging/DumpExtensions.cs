﻿using System.Text;

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
        /// <returns>A readable string representation of the message.</returns>
        public static string Dump(this ClientMessage message)
        {
            string prefix;

            var text = new StringBuilder();

            // trying to do colors but that does not seem to work w/Nunit
            //text.Append("\u001b[31m");

            if (message.MessageType == 0)
            {
                prefix = "EXCEPTION";
                text.AppendLine(prefix);
            }
            else
            {
                prefix = message.IsEvent
                    ? "EVENT"
                    : (message.MessageType & 1) > 0 ? "RESPONSE" : "REQUEST";

                text.AppendLine($"{prefix} [{message.CorrelationId}]");
                text.AppendLine($"TYPE 0x{message.MessageType:x} {message.OperationName}");
            }

            if (prefix == "MESSAGE")
            {
                if (message.FirstFrame.Length >= FrameFields.SizeOf.LengthAndFlags + FrameFields.Offset.PartitionId + FrameFields.SizeOf.PartitionId)
                    text.AppendLine($"PARTID {message.PartitionId}");
            }

            var frame = message.FirstFrame;
            while (frame != null)
            {
                text.Append($"FRAME ");
                text.Append(frame);
                frame = frame.Next;
                if (frame != null)
                    text.AppendLine();
            }

            //text.Append("\u001b[0m");

            return text.ToString();
        }
    }
}
