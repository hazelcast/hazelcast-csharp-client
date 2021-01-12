// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represent topic event handlers.
    /// </summary>
    /// <typeparam name="T">The topic message type.</typeparam>
    public sealed class TopicEventHandlers<T> : EventHandlersBase<ITopicEventHandler<T>>
    {
        /// <summary>
        /// Adds an handler which runs when a message is submitted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public TopicEventHandlers<T> Message(Action<IHTopic<T>, TopicMessageEventArgs<T>> handler)
        {
            Add(new TopicMessageEventHandler<T>(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a message is submitted.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public TopicEventHandlers<T> Message(Func<IHTopic<T>, TopicMessageEventArgs<T>, ValueTask> handler)
        {
            Add(new TopicMessageEventHandler<T>(handler));
            return this;
        }
    }
}
