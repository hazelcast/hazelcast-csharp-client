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
    /// Represents a handler for the <see cref="TopicEventTypes.Terminated"/> event.
    /// </summary>
    /// <typeparam name="T">The reliable topic object type.</typeparam>
    internal class ReliableTopicTerminatedEventHandler<T> : IReliableTopicTerminatedEventHandler<T>
    {
        private readonly Func<IHReliableTopic<T>, ReliableTopicTerminatedEventArgs, ValueTask> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicTerminatedEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicTerminatedEventHandler(Action<IHReliableTopic<T>, ReliableTopicTerminatedEventArgs> handler)
        {
            _handler = (sender, args) =>
            {
                handler(sender, args);
                return default;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableTopicTerminatedEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public ReliableTopicTerminatedEventHandler(Func<IHReliableTopic<T>, ReliableTopicTerminatedEventArgs, ValueTask> handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public ValueTask HandleAsync(IHReliableTopic<T> sender, long sequence, object state)
            => _handler(sender, CreateEventArgs(sequence, state));

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="sequence">The last known sequence in the ring buffer.</param>
        /// <returns>Event arguments.</returns>
        private static ReliableTopicTerminatedEventArgs CreateEventArgs(long sequence, object state)
            => new ReliableTopicTerminatedEventArgs(sequence, state);
    }
}
