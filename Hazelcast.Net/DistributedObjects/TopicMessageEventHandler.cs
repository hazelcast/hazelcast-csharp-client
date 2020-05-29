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
using Hazelcast.Data;
using Hazelcast.Data.Topic;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a handler for the <see cref="TopicEventType.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
    internal class TopicMessageEventHandler<T> : ITopicEventHandler<T>
    {
        private readonly Action<ITopic<T>, TopicMessageEventArgs<T>> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicMessageEventHandler{T}"/> class.
        /// </summary>
        /// <param name="handler">An action to execute</param>
        public TopicMessageEventHandler(Action<ITopic<T>, TopicMessageEventArgs<T>> handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public TopicEventType EventType => TopicEventType.Message;

        /// <inheritdoc />
        public void Handle(ITopic<T> sender, MemberInfo member, long publishTime, T payload)
            => _handler(sender, CreateEventArgs(member, publishTime, payload));

        /// <summary>
        /// Creates event arguments.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The topic object carried by the message.</param>
        /// <returns>Event arguments.</returns>
        private static TopicMessageEventArgs<T> CreateEventArgs(MemberInfo member, long publishTime, T payload)
            => new TopicMessageEventArgs<T>(member, publishTime, payload);
    }
}
