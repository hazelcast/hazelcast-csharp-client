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

using System.Collections;
using System.Collections.Generic;

namespace Hazelcast.Messaging
{
    public partial class ClientMessage // FrameEnumerator
    {
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