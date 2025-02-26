// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.ExceptionServices;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    internal abstract partial class SocketConnectionBase
    {
        /// <summary>
        /// (internal for tests only)
        /// Represents the state of the reading loop.
        /// </summary>
        internal sealed class ReadPipeState : IBufferReference<ReadOnlySequence<byte>>
        {
            /// <summary>
            /// Gets or sets the pipe reader.
            /// </summary>
            public PipeReader Reader { get; set; }

            /// <summary>
            /// Gets or sets the current buffer.
            /// </summary>
            public ReadOnlySequence<byte> Buffer { get; set; }

            /// <summary>
            /// Determines whether reading has failed.
            /// </summary>
            public bool Failed { get; private set; }

            /// <summary>
            /// Gets the optional exception that caused the failure.
            /// </summary>
            public ExceptionDispatchInfo Exception { get; private set; }

            /// <summary>
            /// Captures an exception and registers the failure.
            /// </summary>
            /// <param name="e">The exception.</param>
            public void CaptureExceptionAndFail(Exception e)
            {
                // this should never happen, and we cannot do much about it
                // (has already failed... should not be notified again)
                if (Exception != null) return;

                Failed = true;
                if (e != null) Exception = ExceptionDispatchInfo.Capture(e);
            }
        }
    }
}
