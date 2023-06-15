// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for the reliable topic exception event.
    /// </summary>
    public sealed class ReliableTopicExceptionEventArgs : EventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="sequence">The sequence of the message in the ring buffer.</param>
        /// <param name="state">A state object</param>
        public ReliableTopicExceptionEventArgs(Exception exception, long sequence, object state)
            : base(state)
        {
            Sequence = sequence;
            Exception = exception;
        }
        
        /// <summary>
        /// Gets exception that triggers the event.
        /// </summary>
        public Exception Exception { get; }
        
        /// <summary>
        /// Gets last known sequence.
        /// </summary>
        public long Sequence { get; }

        /// <summary>
        /// Sets or gets an exception terminates the subscription unless
        /// the event is canceled by setting Cancel to <code>false</code>
        /// </summary>
        public bool Cancel { get; set; } = true;
    }
}
