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
    /// Represents event data for the <see cref="TopicEventType.Message"/> event.
    /// </summary>
    /// <typeparam name="T">The topic object type.</typeparam>
    public sealed class TopicMessageEventArgs<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicMessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="publishTime">The publish time.</param>
        /// <param name="payload">The object.</param>
        public TopicMessageEventArgs(MemberInfo member, long publishTime, T payload)
        {
            Member = member;
            PublishTime = publishTime;
            Payload = payload;
        }

        /// <summary>
        /// Gets the member.
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the message publish time.
        /// </summary>
        // TODO: consider UTC DateTime
        public long PublishTime { get; }

        /// <summary>
        /// Gets the topic object carried by the message.
        /// </summary>
        public T Payload { get; }
    }

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

    /// <summary>
    /// Provides extension to the <see cref="TopicEventHandlers{T}"/> class.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Adds an handler for <see cref="TopicEventType.Message"/> events.
        /// </summary>
        /// <typeparam name="T">The topic object type.</typeparam>
        /// <param name="handlers">The topic events.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>The topic events.</returns>
        public static TopicEventHandlers<T> Message<T>(this TopicEventHandlers<T> handlers, Action<ITopic<T>, TopicMessageEventArgs<T>> handler)
        {
            handlers.Add(new TopicMessageEventHandler<T>(handler));
            return handlers;
        }
    }
}