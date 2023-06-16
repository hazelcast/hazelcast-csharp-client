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

using System.Diagnostics.CodeAnalysis;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents event data for the reliable topic message.
    /// </summary>
    /// <typeparam name="T">The reliable topic object type.</typeparam>
    public sealed class ReliableTopicMessageEventArgs<T> : EventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicMessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The object.</param>
        /// <param name="sequence">The sequence of the message in the ring buffer.</param>
        /// <param name="state">A state object</param>
        public ReliableTopicMessageEventArgs(MemberInfo member, long publishTime, T payload, long sequence, object state)
            : base(state)
        {
            Member = member;
            PublishTime = publishTime;
            Payload = payload;
            Sequence = sequence;
        }

        /// <summary>
        /// Gets the member that triggered the event.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the message publish time.
        /// </summary>
        public long PublishTime { get; }

        /// <summary>
        /// Gets the topic object carried by the message.
        /// </summary>
        public T Payload { get; }

        /// <summary>
        /// Gets sequence of the message in the ring buffer.
        /// </summary>
        public long Sequence { get; }

        /// <summary>
        /// Sets or gets an exception terminates the subscription unless
        /// the event is canceled by setting Cancel to <code>false</code>
        /// </summary>
        public bool Cancel { get; set; } = true;
    }
}
