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

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents an iterator over linked lists of <see cref="Frame"/>.
    /// </summary>
    /// <remarks>
    /// <para>This iterator works slightly differently from traditional .NET
    /// iterators. It is immediately positioned over the first <see cref="Frame"/>,
    /// and one moves to next frames by invoking <see cref="Take"/> to take
    /// frames out. The iteration is complete when <see cref="Take"/> returns
    /// null.</para>
    /// </remarks>
    public class FrameIterator
    {
        private Frame _frame;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameIterator"/> class for a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public FrameIterator(ClientMessage message)
        {
            _frame = message.FirstFrame;
        }

        /// <summary>
        /// Takes the current frame and moves to the next frame.
        /// </summary>
        /// <returns>The current frame, or null if the end of the list has been reached.</returns>
        public Frame Take()
        {
            var result = _frame;
            _frame = _frame?.Next;
            return result;
        }

        /// <summary>
        /// Determines whether the iterator points to a frame, or has reached the end of the list.
        /// </summary>
        public bool HasCurrent => _frame != null;

        /// <summary>
        /// Returns the current frame without advancing the iterator.
        /// </summary>
        /// <returns></returns>
        public Frame Peek() => _frame;

        /// <summary>
        /// Skips the current frame it is a "null frame".
        /// </summary>
        /// <returns></returns>
        public bool SkipNull()
        {
            var isNull = _frame != null && _frame.IsNull;
            if (isNull) Take();
            return isNull;
        }

        /// <summary>
        /// Determines whether the current frame is an "end of structure" frame.
        /// </summary>
        public bool CurrentIsEndStruct => _frame != null && _frame.IsEndStruct;

        /// <summary>
        /// Advances the iterator by skipping all frames until the end of a structure.
        /// </summary>
        public void SkipToStructEnd()
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            var numberOfExpectedEndFrames = 1;

            while (numberOfExpectedEndFrames != 0)
            {
                var frame = Take();
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