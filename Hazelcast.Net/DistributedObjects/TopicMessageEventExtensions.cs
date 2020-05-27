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
using Hazelcast.Data.Topic;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Provides extension to the <see cref="TopicEventHandlers{T}"/> class.
    /// </summary>
    public static class TopicMessageEventExtensions
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