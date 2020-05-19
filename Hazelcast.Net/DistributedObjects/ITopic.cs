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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    ///     Hazelcast provides distribution mechanism for publishing messages that are delivered to multiple subscribers
    ///     which is also known as publish/subscribe (pub/sub) messaging model.
    /// </summary>
    /// <remarks>
    ///     <p>Hazelcast provides distribution mechanism for publishing messages that are delivered to multiple subscribers
    ///     which is also known as publish/subscribe (pub/sub) messaging model. Publish and subscriptions are cluster-wide.
    ///     When a member subscribes for a topic, it is actually registering for messages published by any member in the
    ///     cluster,
    ///     including the new members joined after you added the listener.
    ///     </p>
    ///     Messages are ordered, meaning, listeners(subscribers)
    ///     will process the messages in the order they are actually published. If cluster member M publishes messages
    ///     m1, m2, m3...mn to a topic T, then Hazelcast makes sure that all of the subscribers of topic T will receive
    ///     and process m1, m2, m3...mn in order.
    /// </remarks>
    public interface ITopic<T> : IDistributedObject
    {
        /// <summary>Subscribes to this topic.</summary>
        Task<Guid> SubscribeAsync(Action<TopicEventHandlers<T>> on);

        /// <summary>Stops receiving messages for the given message listener.</summary>
        /// <remarks>
        ///     Stops receiving messages for the given message listener. If the given listener already removed,
        ///     this method does nothing.
        /// </remarks>
        /// <param name="subscriptionId">Id of listener registration.</param>
        Task UnsubscribeAsync(Guid subscriptionId);

        //        /// <summary>Returns the name of this ITopic instance</summary>
        //        /// <returns>name of this instance</returns>
        //        new string GetName();

        /// <summary>Publishes the message to all subscribers of this topic</summary>
        /// <param name="message"></param>
        void Publish(T message);
    }
}