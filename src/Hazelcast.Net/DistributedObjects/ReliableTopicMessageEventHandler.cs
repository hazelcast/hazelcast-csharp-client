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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a handler for the <see cref="TopicEventTypes.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The reliable topic object type.</typeparam>
    internal class ReliableTopicMessageEventHandler<T> : IReliableTopicMessageEventHandler<T>
    {
        private readonly Func<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>, ValueTask> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicMessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicMessageEventHandler(Action<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>> handler)
        {
            _handler = (sender, args) =>
            {
                handler(sender, args);
                return default;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicMessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicMessageEventHandler(Func<IHReliableTopic<T>, ReliableTopicMessageEventArgs<T>, ValueTask> handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public ValueTask HandleAsync(IHReliableTopic<T> sender, MemberInfo member, long publishTime, T payload, long sequence, object state)
            => _handler(sender, CreateEventArgs(member, publishTime, payload, sequence, state));

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The reliable topic object carried by the message.</param>
        /// <param name="sequence">The sequence of the message in the ring buffer.</param>
        /// <returns>Event arguments.</returns>
        private static ReliableTopicMessageEventArgs<T> CreateEventArgs(MemberInfo member, long publishTime, T payload, long sequence, object state)
            => new ReliableTopicMessageEventArgs<T>(member, publishTime, payload, sequence, state);
    }
}
