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

using Hazelcast.Core;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="Frame"/> class.
    /// </summary>
    public static class FrameExtensions
    {
        /// <summary>
        /// Reads the message type.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The message type.</returns>
        public static int ReadMessageType(this Frame frame)
            => frame.Bytes.ReadInt32(FrameFields.Offset.MessageType);

        /// <summary>
        /// Writes the message type.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="type">The message type.</param>
        public static void WriteMessageType(this Frame frame, int type)
            => frame.Bytes.WriteInt32(FrameFields.Offset.MessageType, type);

        /// <summary>
        /// Reads the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The correlation id.</returns>
        public static long ReadCorrelationId(this Frame frame)
            => frame.Bytes.ReadInt64(FrameFields.Offset.CorrelationId);

        /// <summary>
        /// Writes the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="correlationId">The correlation id.</param>
        public static void WriteCorrelationId(this Frame frame, long correlationId)
            => frame.Bytes.WriteInt64(FrameFields.Offset.CorrelationId, correlationId);

        /// <summary>
        /// Reads the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The partition id.</returns>
        public static int ReadPartitionId(this Frame frame)
            => frame.Bytes.ReadInt32(FrameFields.Offset.PartitionId);

        /// <summary>
        /// Writes the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="partionId">The partition id.</param>
        public static void WritePartitionId(this Frame frame, int partionId)
            => frame.Bytes.WriteInt32(FrameFields.Offset.PartitionId, partionId);

        /// <summary>
        /// Reads the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The fragment id.</returns>
        public static long ReadFragmentId(this Frame frame)
            => frame.Bytes.ReadInt64(FrameFields.Offset.FragmentId);

        /// <summary>
        /// Writes the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="fragmentId">The fragment id.</param>
        public static void WriteFragmentId(this Frame frame, long fragmentId)
            => frame.Bytes.WriteInt64(FrameFields.Offset.FragmentId, fragmentId);
    }
}