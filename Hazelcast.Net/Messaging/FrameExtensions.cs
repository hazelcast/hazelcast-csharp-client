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
using System.Collections.Generic;
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
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.ReadInt(FrameFields.Offset.MessageType);

        /// <summary>
        /// Writes the message type.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="type">The message type.</param>
        public static void WriteMessageType(this Frame frame, int type)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.WriteInt(FrameFields.Offset.MessageType, type);

        /// <summary>
        /// Reads the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The correlation id.</returns>
        public static long ReadCorrelationId(this Frame frame)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.ReadLong(FrameFields.Offset.CorrelationId);

        /// <summary>
        /// Writes the correlation id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="correlationId">The correlation id.</param>
        public static void WriteCorrelationId(this Frame frame, long correlationId)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.WriteLong(FrameFields.Offset.CorrelationId, correlationId);

        /// <summary>
        /// Reads the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The partition id.</returns>
        public static int ReadPartitionId(this Frame frame)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.ReadInt(FrameFields.Offset.PartitionId);

        /// <summary>
        /// Writes the partition id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="partionId">The partition id.</param>
        public static void WritePartitionId(this Frame frame, int partionId)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.WriteInt(FrameFields.Offset.PartitionId, partionId);

        /// <summary>
        /// Reads the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <returns>The fragment id.</returns>
        public static long ReadFragmentId(this Frame frame)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.ReadLong(FrameFields.Offset.FragmentId);

        /// <summary>
        /// Writes the fragment id.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="fragmentId">The fragment id.</param>
        public static void WriteFragmentId(this Frame frame, long fragmentId)
            => (frame ?? throw new ArgumentNullException(nameof(frame))).Bytes.WriteLong(FrameFields.Offset.FragmentId, fragmentId);

        /// <summary>
        /// Takes the current frame and moves to the next frame.
        /// </summary>
        /// <returns>The current frame, or null if the end of the list has been reached.</returns>
        public static Frame Take(this IEnumerator<Frame> frames)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            // if current is null maybe we haven't started yet - start
            // (if it's null because we've reached the end, nothing happens)
            if (frames.Current == null) frames.MoveNext();

            // capture and return the current frame, move to next
            var frame = frames.Current;
            frames.MoveNext();
            return frame;
        }

        /// <summary>
        /// Skips the current frame it is a "null frame".
        /// </summary>
        /// <returns></returns>
        public static bool SkipNull(this IEnumerator<Frame> frames)
        {
            if (frames == null) throw new ArgumentNullException(nameof(frames));

            var isNull = frames.Current != null && frames.Current.IsNull;
            if (isNull) frames.Take();
            return isNull;
        }

        /// <summary>
        /// Determines whether the current frame is an "end of structure" frame.
        /// </summary>
        public static bool AtStructEnd(this IEnumerator<Frame> frames)
            => (frames ?? throw new ArgumentNullException(nameof(frames))).Current != null && frames.Current.IsEndStruct;

        /// <summary>
        /// Advances the iterator by skipping all frames until the end of a structure.
        /// </summary>
        public static void SkipToStructEnd(this IEnumerator<Frame> frames)
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            var numberOfExpectedEndFrames = 1;

            while (numberOfExpectedEndFrames != 0)
            {
                var frame = frames.Take();
                if (frame == null)
                    throw new InvalidOperationException("Reached end of message.");

                if (frame.IsEndStruct)
                {
                    numberOfExpectedEndFrames--;
                }
                else if (frame.IsBeginStruct)
                {
                    numberOfExpectedEndFrames++;
                }
            }
        }
    }
}
