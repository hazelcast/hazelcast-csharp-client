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
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for the reliable topic terminated event.
    /// </summary>
    public sealed class ReliableTopicTerminatedEventArgs : EventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicTerminatedEventArgs"/> class.
        /// </summary>
        /// <param name="sequence">The sequence of the message in the ring buffer.</param>
        /// <param name="state">A state object</param>
        public ReliableTopicTerminatedEventArgs(long sequence, object state)
            : base(state)
        {
            Sequence = sequence;
        }

        /// <summary>
        /// Gets last known sequence.
        /// </summary>
        public long Sequence { get; }
    }
}
