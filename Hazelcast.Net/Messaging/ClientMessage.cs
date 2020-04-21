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
using System.Collections;
using System.Collections.Generic;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents a message, or a fragment of a message.
    /// </summary>
    /// <remarks>
    /// <para>A message is composed of frames. The first frame of a message contains, in its
    /// payload, the message type (4 bytes), the correlation id (long), the partition id (int)
    /// or backup acknowledgement count (1 byte), and more fixed data fields. The other frames
    /// contain more data fields.</para>
    /// <para>A message can be fragmented when being carried over the network, see
    /// <see cref="ClientMessageFragmentingExtensions"/> for details on the structure of a fragmented
    /// message. Fragments are assembled back by the <see cref="ClientMessageConnection"/>.</para>
    /// <para>Frames are a linked list controlled by <see cref="FirstFrame"/> and
    /// <see cref="LastFrame"/>. The last frame always has the <see cref="FrameFlags.Final"/>
    /// flag set.</para>
    /// </remarks>
    public class ClientMessage : IEnumerable<Frame>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMessage"/> class.
        /// </summary>
        /// <param name="firstFrame">An optional first frame.</param>
        /// <remarks>
        /// <para>Only the single specified frame is added to the message, regardless
        /// of whether it is part of a linked list and has a next frame.</para>
        /// </remarks>
        public ClientMessage(Frame firstFrame = null)
        {
            FirstFrame = LastFrame = firstFrame;
            if (firstFrame != null)
            {
                firstFrame.Next = null;
                LastFrame.Flags |= FrameFlags.Final;
            }
        }

        /// <summary>
        /// Gets or sets FIXME document IsRetryable
        /// </summary>
        public bool IsRetryable { get; set; }

        /// <summary>
        /// Gets or sets FIXME document OperationName
        /// </summary>
        public string OperationName { get; set; }

        /// <summary>
        /// Gets or sets the first frame of the message.
        /// </summary>
        public Frame FirstFrame { get; private set; }

        /// <summary>
        /// Gets the first frame of the message, or throw.
        /// </summary>
        private Frame FirstFrameOrThrow => FirstFrame ?? throw new InvalidOperationException("Message does not have a first frame.");

        /// <summary>
        /// Gets or sets the last frame of the message.
        /// </summary>
        public Frame LastFrame { get; private set; }

        /// <summary>
        /// Gets or sets the message flags.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// <para>Message flags and Frame flags are carried by the same field.</para>
        /// </remarks>
        public ClientMessageFlags Flags
        {
            get => (ClientMessageFlags) FirstFrameOrThrow.Flags;
            set => FirstFrameOrThrow.Flags = (FrameFlags) value;
        }

        /// <summary>
        /// Appends a single frame to the message.
        /// </summary>
        /// <param name="frame">The frame to append.</param>
        /// <returns>The original message, with the frame appended.</returns>
        /// <remarks>
        /// <para>Only the single specified frame is appended to the message, regardless
        /// of whether it is part of a linked list and has a next frame.</para>
        /// </remarks>
        public ClientMessage Append(Frame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));

            frame.Next = null;

            if (FirstFrame == null)
            {
                FirstFrame = LastFrame = frame;
            }
            else
            {
                LastFrame.Flags &= ~FrameFlags.Final;
                LastFrame.Next = frame;
                LastFrame = frame;
            }

            LastFrame.Flags |= FrameFlags.Final;
            return this;
        }

        /// <summary>
        /// Appends frames to the message.
        /// </summary>
        /// <param name="firstFrame">The first frame to append.</param>
        /// <param name="lastFrame">The last frame to append.</param>
        /// <param name="trustable">Whether the supplied frames can be trusted.</param>
        /// <returns>The original message, with the frames appended.</returns>
        /// <remarks>
        /// <para>Append the specified frame, and all next frames if any.</para>
        /// <para>If <paramref name="trustable"/> is true, it is assumed that <see cref="lastFrame"/>
        /// can be reached from <see cref="firstFrame"/> and is already marked as final.</para>
        /// </remarks>
        public ClientMessage AppendFragment(Frame firstFrame, Frame lastFrame, bool trustable = false)
        {
            if (firstFrame == null) throw new ArgumentNullException(nameof(firstFrame));

            if (LastFrame == null) throw new InvalidOperationException("Empty message.");

            // trust the appended frame chain is safe
            if (!trustable)
            {
                var frame = firstFrame;
                while (frame.Next != null) frame = frame.Next;
                if (frame != lastFrame)
                    throw new ArgumentException("Broken linked list.", nameof(lastFrame));
            }

            LastFrame.Flags &= ~FrameFlags.Final;
            LastFrame.Next = firstFrame;
            LastFrame = lastFrame;

            // trust lastFrame is already final
            if (!trustable)
                LastFrame.Flags |= FrameFlags.Final;

            return this;
        }

        /// <summary>
        /// Determines whether the message is backup-aware.
        /// </summary>
        public bool IsBackupAware => Flags.Has(ClientMessageFlags.BackupAware);

        /// <summary>
        /// Determines whether the message carries a backup event.
        /// </summary>
        public bool IsBackupEvent => Flags.Has(ClientMessageFlags.BackupEvent);

        /// <summary>
        /// Determines whether the message carries an event.
        /// </summary>
        public bool IsEvent => Flags.Has(ClientMessageFlags.Event);

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public int MessageType
        {
            get => FirstFrameOrThrow.ReadMessageType();
            set => FirstFrameOrThrow.WriteMessageType(value);
        }

        /// <summary>
        /// Gets or sets the correlation id.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public long CorrelationId
        {
            get => FirstFrameOrThrow.ReadCorrelationId();
            set => FirstFrameOrThrow.WriteCorrelationId(value);
        }

        /// <summary>
        /// Gets or sets the partition id.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public int PartitionId
        {
            get => FirstFrameOrThrow.ReadPartitionId();
            set => FirstFrameOrThrow.WritePartitionId(value);
        }

        /// <summary>
        /// Gets or sets the fragment id.
        /// </summary>
        /// <remarks>
        /// <para>Getting or setting this property requires that the message has the appropriate first frame.</para>
        /// </remarks>
        public long FragmentId
        {
            get => FirstFrameOrThrow.ReadFragmentId();
            set => FirstFrameOrThrow.WriteFragmentId(value);
        }

        /// <summary>
        /// Determines whether the message carries an exception.
        /// </summary>
        public bool IsException => MessageType == 0; // FIXME message type constants?

        /// <inheritdoc />
        public IEnumerator<Frame> GetEnumerator()
            => new FrameEnumerator(this);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Represents an enumerator of frames.
        /// </summary>
        private class FrameEnumerator : IEnumerator<Frame>
        {
            private readonly ClientMessage _message;
            private bool _started;
#if DEBUG
            private int _index;
#endif

            /// <summary>
            /// Initialize a new instance of the <see cref="FrameEnumerator"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            public FrameEnumerator(ClientMessage message)
            {
                _message = message;
#if DEBUG
                _index = -1;
#endif
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (!_started)
                {
                    _started = true;
                    Current = _message.FirstFrame;
#if DEBUG
                    _index = 0;
#endif
                }
                else
                {
#if DEBUG
                    if (Current != null) _index++;
#endif
                    Current = Current?.Next;
                }

                return Current != null;
            }

            /// <inheritdoc />
            public void Reset()
            {
                Current = null;
                _started = false;
#if DEBUG
                _index = -1;
#endif
            }

            /// <inheritdoc />
            public Frame Current { get; private set; }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            { }

            /// <inheritdoc />
            public override string ToString()
            {
#if DEBUG
                return $"Enumerator at frame[{_index}]: {(Current == null ? "<null>" : Current.ToString())}";
#else
                return $"Enumerator at frame: {(Current == null ? "<null>" : Current.ToString())}";
#endif
            }
        }
    }
}