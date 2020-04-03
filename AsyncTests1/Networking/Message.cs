// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a message.
    /// </summary>
    /// <remarks>
    /// <para>A message is composed of frames FIXME elaborate</para>
    /// <para>Frames are a linked list.</para>
    /// </remarks>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="firstFrame">An optional first frame.</param>
        public Message(Frame firstFrame = null)
        {
            FirstFrame = LastFrame = firstFrame;
            if (firstFrame != null) firstFrame.Next = null;
        }

        /// <summary>
        /// Gets or sets FIXME document
        /// </summary>
        public bool IsRetryable { get; set; }

        /// <summary>
        /// Gets or sets FIXME document
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets the first frame of the message.
        /// </summary>
        public Frame FirstFrame { get; private set; }

        /// <summary>
        /// Gets or sets the last frame of the message.
        /// </summary>
        public Frame LastFrame { get; private set; }

        /// <summary>
        /// Gets the message flags.
        /// </summary>
        public MessageFlags Flags => FirstFrame == null 
            ? MessageFlags.Default 
            : (MessageFlags) FirstFrame?.Flags;

        /// <summary>
        /// Appends a frame to the message.
        /// </summary>
        /// <param name="frame">The frame to append.</param>
        /// <returns>The original message, with the frame appended.</returns>
        public Message Append(Frame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            frame.Next = null;

            if (FirstFrame == null)
            {
                FirstFrame = LastFrame = frame;
            }
            else
            {
                LastFrame.Next = frame;
                LastFrame = frame;
            }

            return this;
        }

        /// <summary>
        /// Appends a message fragment to the message.
        /// </summary>
        /// <param name="fragment">The message to append.</param>
        /// <returns>The original message, with the fragment appended.</returns>
        public Message Append(Message fragment)
        {
            if (fragment == null) throw new ArgumentNullException(nameof(fragment));

            // skip the first frame of the fragment,
            // the first frame is just an empty Begin frame marking the segment

            LastFrame.Next = fragment.FirstFrame.Next;
            LastFrame = fragment.LastFrame;

            return this;
        }

        /// <summary>
        /// Determines whether the message is backup-aware.
        /// </summary>
        public bool IsBackupAware => Flags.Has(MessageFlags.BackupAware);

        /// <summary>
        /// Determines whether the message carries a backup event.
        /// </summary>
        public bool IsBackupEvent => Flags.Has(MessageFlags.BackupEvent);

        /// <summary>
        /// Determines whether the message carries an event.
        /// </summary>
        public bool IsEvent => Flags.Has(MessageFlags.Event);

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public int MessageType
        {
            get => FirstFrame.ReadMessageType();
            set => FirstFrame.WriteMessageType(value);
        }

        /// <summary>
        /// Gets or sets the correlation id.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public long CorrelationId
        {
            get => FirstFrame.ReadCorrelationId();
            set => FirstFrame.WriteCorrelationId(value);
        }

        /// <summary>
        /// Gets or sets the partition id.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public int PartitionId
        {
            get => FirstFrame.ReadPartitionId();
            set => FirstFrame.WritePartitionId(value);
        }

        /// <summary>
        /// Determines whether the message carries an exception.
        /// </summary>
        public bool IsException => MessageType == 0; // FIXME values, consts ???
    }
}