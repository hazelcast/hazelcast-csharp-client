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
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Networking;

namespace Hazelcast
{
    public partial class HazelcastOptions : IClusterOptions // ClusterName, Subscribers
    {
        /// <summary>
        /// Gets the default cluster name, which is <c>"dev"</c>.
        /// </summary>
        /// <returns>The default cluster name, which is <c>"dev"</c>.</returns>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets the default client name prefix, which is <c>"hz.client_"</c>.
        /// </summary>
        /// <returns>The default client name prefix, which is <c>"hz.client_"</c>.</returns>
        public const string DefaultClientNamePrefix = "hz.client_";

        /// <summary>
        /// Gets or sets the cluster name.
        /// </summary>
        /// <returns>The cluster name.</returns>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets or sets the client name.
        /// </summary>
        /// <returns>The client name.</returns>
        /// <remarks>
        /// <para>When <c>null</c>, the client name is derived from <see cref="ClientNamePrefix"/>.</para>
        /// </remarks>
        public string ClientName { get; set; } // null by default

        /// <summary>
        /// Gets or sets the client name prefix.
        /// </summary>
        /// <returns>The client name prefix.</returns>
        public string ClientNamePrefix
        {
            get => string.IsNullOrWhiteSpace(_clientNamePrefix) ? DefaultClientNamePrefix : _clientNamePrefix;
            set => _clientNamePrefix = value;
        }

        /// <summary>
        /// Gets the client labels.
        /// </summary>
        /// <returns>The client labels.</returns>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the authentication options.
        /// </summary>
        /// <returns>The authentication options.</returns>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the service factory for <see cref="ILoadBalancer"/>.
        /// </summary>
        /// <returns>The service factory for <see cref="ILoadBalancer"/>.</returns>
        /// <remarks>
        /// <para>A service factory is initialized with a creator function, that creates an
        /// instance of the service. For instance:
        /// <code>
        /// options.LoadBalancer.Creator = () => new RandomLoadBalancer();
        /// </code></para>
        /// </remarks>
        [BinderIgnore]
        public SingletonServiceFactory<ILoadBalancer> LoadBalancer { get; }
            = new SingletonServiceFactory<ILoadBalancer> { Creator = () => new RandomLoadBalancer() };

        [BinderName("loadBalancer")]
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private InjectionOptions LoadBalancerBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set
            {
                var typeName = value.TypeName;
                if (string.IsNullOrWhiteSpace(typeName))
                    throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));

                switch (typeName.ToUpperInvariant())
                {
                    case "RANDOM":
                        LoadBalancer.Creator = () => new RandomLoadBalancer();
                        break;
                    case "ROUNDROBIN":
                        LoadBalancer.Creator = () => new RoundRobinLoadBalancer();
                        break;
                    default:
                        LoadBalancer.Creator = () => ServiceFactory.CreateInstance<ILoadBalancer>(value.TypeName, value.Args);
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the heartbeat options.
        /// </summary>
        /// <returns>The heartbeat options.</returns>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the messaging options.
        /// </summary>
        /// <returns>The messaging options.</returns>
        public MessagingOptions Messaging { get; } = new MessagingOptions();

        /// <summary>
        /// Gets the networking options.
        /// </summary>
        /// <returns>The networking options.</returns>
        public NetworkingOptions Networking { get; } = new NetworkingOptions();

        /// <summary>
        /// Gets the events options.
        /// </summary>
        /// <returns>The events options.</returns>
        public EventsOptions Events { get; } = new EventsOptions();
    }
}
