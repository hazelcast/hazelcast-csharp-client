// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Text;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods for dumping objects.
    /// </summary>
    internal static class DumpMessagingExtensions
    {
        /// <summary>
        /// Dumps a client message into a readable string.
        /// </summary>
        /// <param name="message">The client message.</param>
        /// <returns>A readable string representation of the message.</returns>
        public static string Dump(this ClientMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
#if DEBUG
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

                var name = message.OperationName ?? MessageTypeConstants.GetMessageTypeName(message.MessageType);

                text.AppendLine($"{prefix} [{message.CorrelationId}]");
                text.AppendLine($"TYPE 0x{message.MessageType:x} {name}");
            }

            if (prefix == "REQUEST")
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
#else
            return string.Empty;
#endif
        }
    }
}
