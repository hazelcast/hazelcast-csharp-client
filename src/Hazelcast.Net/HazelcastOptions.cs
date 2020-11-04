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
    /// <summary>
    /// Represents the Hazelcast client options.
    /// </summary>
    public sealed partial class HazelcastOptions
    {
        private string _clientNamePrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        public HazelcastOptions()
        {
            Subscribers = new List<IHazelcastClientEventSubscriber>();
            SubscribersBinder = new CollectionBinder<InjectionOptions>(x
                => Subscribers.Add(new HazelcastClientEventSubscriber(x.TypeName)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastOptions"/> class.
        /// </summary>
        private HazelcastOptions(HazelcastOptions other)
        {
            ClientName = other.ClientName;
            ClusterName = other.ClusterName;
            Subscribers = new List<IHazelcastClientEventSubscriber>(other.Subscribers);
            Labels = new HashSet<string>(other.Labels);
            LoggerFactory = other.LoggerFactory.Clone();

            Core = other.Core.Clone();
            Heartbeat = other.Heartbeat.Clone();
            Networking = other.Networking.Clone();
            Authentication = other.Authentication.Clone();
            LoadBalancing = other.LoadBalancing.Clone();
            Serialization = other.Serialization.Clone();
            NearCache = other.NearCache.Clone();
            Messaging = other.Messaging.Clone();
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <remarks>
        /// <para>In dependency-injection scenario the service provider may be available,
        /// so that service factories can return injected services.</para>
        /// </remarks>
        public IServiceProvider ServiceProvider { get; internal set; }

        /// <summary>
        /// Gets the core options.
        /// </summary>
        [BinderIgnore(false)]
        internal CoreOptions Core { get; } = new CoreOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal HazelcastOptions Clone() => new HazelcastOptions(this);
    }
}
