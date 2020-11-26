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
        private string _clientNamePrefix;

        /// <summary>
        /// Gets the default cluster name, which is <c>"dev"</c>.
        /// </summary>
        /// <returns>The default cluster name, which is <c>"dev"</c>.</returns>
        internal const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets the default client name prefix, which is <c>"hz.client_"</c>.
        /// </summary>
        /// <returns>The default client name prefix, which is <c>"hz.client_"</c>.</returns>
        internal const string DefaultClientNamePrefix = "hz.client_";

        /// <inheritdoc />
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <inheritdoc />
        public string ClientName { get; set; } // null by default

        /// <inheritdoc />
        string IClusterOptions.ClientNamePrefix
        {
            get => string.IsNullOrWhiteSpace(_clientNamePrefix) ? DefaultClientNamePrefix : _clientNamePrefix;
            set => _clientNamePrefix = value;
        }

        /// <inheritdoc />
        int IClusterOptions.WaitForConnectionMilliseconds { get; set; } = 1_000;

        /// <inheritdoc />
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <inheritdoc />
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <inheritdoc />
        [BinderIgnore]
        public SingletonServiceFactory<ILoadBalancer> LoadBalancer { get; }
            = new SingletonServiceFactory<ILoadBalancer> { Creator = () => new RoundRobinLoadBalancer() };

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
                    case "STATIC":
                        LoadBalancer.Creator = () => new StaticLoadBalancer(value.Args);
                        break;
                    default:
                        LoadBalancer.Creator = () => ServiceFactory.CreateInstance<ILoadBalancer>(value.TypeName, value.Args);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <inheritdoc />
        public MessagingOptions Messaging { get; } = new MessagingOptions();

        /// <inheritdoc />
        public NetworkingOptions Networking { get; } = new NetworkingOptions();

        /// <inheritdoc />
        public EventsOptions Events { get; } = new EventsOptions();
    }
}
