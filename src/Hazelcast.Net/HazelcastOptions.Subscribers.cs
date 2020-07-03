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
using System.Collections.Generic;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast
{
    public partial class HazelcastOptions // Subscribers
    {
        /// <summary>
        /// Gets the subscribers.
        /// </summary>
        [BinderIgnore]
        public IList<IHazelcastClientEventSubscriber> Subscribers { get; }

        // used for configuration binding
        [BinderName("subscribers")]
        [BinderIgnore(false)]
        private CollectionBinder<InjectionOptions> SubscribersBinder { get; set; }

        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <param name="on">An action defining event handlers.</param>
        /// <returns>The options.</returns>
        public HazelcastOptions AddSubscriber(Action<HazelcastClientEventHandlers> on)
        {
            Subscribers.Add(new HazelcastClientEventSubscriber((hazelcastClient, cancellationToken)
                => hazelcastClient.SubscribeAsync(on, cancellationToken)));
            return this;
        }

        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <returns>The options.</returns>
        public HazelcastOptions AddSubscriber(IHazelcastClientEventSubscriber subscriber)
        {
            Subscribers.Add(new HazelcastClientEventSubscriber(subscriber));
            return this;
        }

        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <typeparam name="T">The type of the subscriber.</typeparam>
        /// <returns>The options.</returns>
        public HazelcastOptions AddSubscriber<T>()
            where T : IHazelcastClientEventSubscriber
        {
            Subscribers.Add(new HazelcastClientEventSubscriber(typeof(T)));
            return this;
        }

        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <param name="type">The type of the subscriber.</param>
        /// <returns>The options.</returns>
        public HazelcastOptions AddSubscriber(Type type)
        {
            Subscribers.Add(new HazelcastClientEventSubscriber(type));
            return this;
        }

        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <param name="typename">The name of the type of the subscriber.</param>
        /// <returns>The options.</returns>
        public HazelcastOptions AddSubscriber(string typename)
        {
            Subscribers.Add(new HazelcastClientEventSubscriber(typename));
            return this;
        }
    }
}