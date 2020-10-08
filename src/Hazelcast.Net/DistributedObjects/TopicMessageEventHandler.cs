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
using System.Threading.Tasks;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a handler for the <see cref="TopicEventTypes.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
    internal class TopicMessageEventHandler<T> : ITopicEventHandler<T>
    {
        private readonly Func<IHTopic<T>, TopicMessageEventArgs<T>, ValueTask> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicMessageEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public TopicMessageEventHandler(Action<IHTopic<T>, TopicMessageEventArgs<T>> handler)
        {
            _handler = (sender, args) =>
            {
                handler(sender, args);
                return default;
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicMessageEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public TopicMessageEventHandler(Func<IHTopic<T>, TopicMessageEventArgs<T>, ValueTask> handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public TopicEventTypes EventType => TopicEventTypes.Message;

        /// <inheritdoc />
        public ValueTask HandleAsync(IHTopic<T> sender, MemberInfo member, long publishTime, T payload, object state)
            => _handler(sender, CreateEventArgs(member, publishTime, payload, state));

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The topic object carried by the message.</param>
        /// <returns>Event arguments.</returns>
        private static TopicMessageEventArgs<T> CreateEventArgs(MemberInfo member, long publishTime, T payload, object state)
            => new TopicMessageEventArgs<T>(member, publishTime, payload, state);
    }
}
