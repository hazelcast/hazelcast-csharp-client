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

using AsyncTests1.Core;

namespace AsyncTests1.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="Frame"/> class.
    /// </summary>
    public static class FrameExtensions
    {
        /// <summary>
        /// Defines constants representing the size of frame elements.
        /// </summary>
        public static class SizeOf
        {
            /// <summary>
            /// Gets the size of the message type field.
            /// </summary>
            public const int MessageType = sizeof(int);

            /// <summary>
            /// Gets the size of the fragment id field.
            /// </summary>
            public const int FragmentId = sizeof(long);

            /// <summary>
            /// Gets the size of the correlation id field.
            /// </summary>
            public const int CorrelationId = sizeof(long);

            /// <summary>
            /// Gets the size of the partition id field.
            /// </summary>
            public const int PartitionId = sizeof(int);

            /// <summary>
            /// Gets the size of the response backup acknowledgement field.
            /// </summary>
            public const int ResponseBackupAcks = sizeof(byte);
        }

        /// <summary>
        /// Defines constants representing the offset of frame elements.
        /// </summary>
        public static class Offset
        {
            // structure is
            // type (int) | correlationId (long) | partitionId (int)
            //                                   | responseBackupAcks (???)
            // fragmentId (???)

            /// <summary>
            /// Gets the offset of the message type field.
            /// </summary>
            public const int MessageType = 0;

            /// <summary>
            /// Gets the offset of the fragment id field.
            /// </summary>
            public const int FragmentId = 0;

            /// <summary>
            /// Gets the offset of the correlation id field.
            /// </summary>
            public const int CorrelationId = SizeOf.MessageType;

            /// <summary>
            /// Gets the offset of the partition id field.
            /// </summary>
            public const int PartitionId = CorrelationId + SizeOf.CorrelationId;

            /// <summary>
            /// Gets the offset of the response backup acknowledgement field.
            /// </summary>
            public const int ResponseBackupAcks = CorrelationId + SizeOf.CorrelationId;
        }

        /// <summary>
        /// Reads the message type.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The message type.</returns>
        public static int ReadMessageType(this Frame frame)
            => frame.Bytes.ReadInt32(Offset.MessageType);

        /// <summary>
        /// Writes the message type.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="type">The message type.</param>
        public static void WriteMessageType(this Frame frame, int type)
            => frame.Bytes.WriteInt32(Offset.MessageType, type);

        /// <summary>
        /// Reads the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The correlation id.</returns>
        public static long ReadCorrelationId(this Frame frame)
            => frame.Bytes.ReadInt64(Offset.CorrelationId);

        /// <summary>
        /// Writes the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="correlationId">The correlation id.</param>
        public static void WriteCorrelationId(this Frame frame, long correlationId)
            => frame.Bytes.WriteInt64(Offset.CorrelationId, correlationId);

        /// <summary>
        /// Reads the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The partition id.</returns>
        public static int ReadPartitionId(this Frame frame)
            => frame.Bytes.ReadInt32(Offset.PartitionId);

        /// <summary>
        /// Writes the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="partionId">The partition id.</param>
        public static void WritePartitionId(this Frame frame, int partionId)
            => frame.Bytes.WriteInt32(Offset.PartitionId, partionId);

        /// <summary>
        /// Reads the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The fragment id.</returns>
        public static long ReadFragmentId(this Frame frame)
            => frame.Bytes.ReadInt64(Offset.FragmentId);

        /// <summary>
        /// Writes the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="fragmentId">The fragment id.</param>
        public static void WriteFragmentId(this Frame frame, long fragmentId)
            => frame.Bytes.WriteInt64(Offset.FragmentId, fragmentId);
    }
}