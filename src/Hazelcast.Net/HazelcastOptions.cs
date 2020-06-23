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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Events;

namespace Hazelcast
{
    /// <summary>
    /// Main options to setup a Hazelcast Client.
    /// </summary>
    public sealed partial class HazelcastOptions
    {
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
            AsyncStart = other.AsyncStart;

            Core = Core.Clone();
            Logging = Logging.Clone();
            Heartbeat = Heartbeat.Clone();
            Networking = Networking.Clone();
            Authentication = Authentication.Clone();
            LoadBalancing = LoadBalancing.Clone();
            Serialization = Serialization.Clone();
            NearCache = NearCache.Clone();
            Messaging = Messaging.Clone();
        }

        /// <summary>
        /// Gets the default client name prefix.
        /// </summary>
        public string DefaultClientNamePrefix => "hz.client_";

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <remarks>
        /// <para>In dependency-injection scenario the service provider may be available,
        /// so that service factories can return injected services.</para>
        /// </remarks>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets the core options.
        /// </summary>
        public CoreOptions Core { get; } = new CoreOptions();

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal HazelcastOptions Clone() => new HazelcastOptions(this);
    }
}
