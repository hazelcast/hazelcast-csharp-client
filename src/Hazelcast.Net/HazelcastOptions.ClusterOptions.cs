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
    // beware! the IClusterOptions is internal, and therefore its /// documentation
    // is NOT inherited = do NOT use <inheritdoc /> here but DO duplicate the docs

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

        /// <summary>
        /// Gets or sets the name of the cluster.
        /// </summary>
        /// <remarks>
        /// <para>This must match the name of the cluster that the client is going to connect to.</para>
        /// </remarks>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets or sets the name of the client.
        /// </summary>
        /// <remarks>
        /// <para>This is optional. If no client name is specified, a name will be generated.</para>
        /// </remarks>
        public string ClientName { get; set; } // null by default

        /// <inheritdoc />
        string IClusterOptions.ClientNamePrefix
        {
            get => string.IsNullOrWhiteSpace(_clientNamePrefix) ? DefaultClientNamePrefix : _clientNamePrefix;
            set => _clientNamePrefix = value;
        }

        /// <inheritdoc />
        int IClusterOptions.WaitForConnectionMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets the set of client labels.
        /// </summary>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Gets the <see cref="AuthenticationOptions"/>.
        /// </summary>
        public AuthenticationOptions Authentication { get; } = new AuthenticationOptions();

        /// <summary>
        /// Gets the <see cref="SingletonServiceFactory{TService}"/> for the <see cref="ILoadBalancer"/>.
        /// </summary>
        /// <remarks>
        /// <para>When set in the configuration file, it is defined as an injected type, for instance:
        /// <code>
        /// "loadBalancer":
        /// {
        ///   "typeName": "My.LoadBalancer",
        ///   "args":
        ///   {
        ///     "foo": 42
        ///   }
        /// }
        /// </code>
        /// where <c>typeName</c> is the name of the type, and <c>args</c> is an optional dictionary
        /// of arguments for the type constructor.</para>
        /// <para>In addition to custom type names, <c>typeName</c> can be any of the
        /// predefined <c>Random</c>, <c>RoundRobin</c> or <c>Static</c> values.</para>
        /// <para>The default load balancer is the <see cref="RoundRobinLoadBalancer"/>.</para>
        /// </remarks>
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

                LoadBalancer.Creator = typeName.ToUpperInvariant() switch
                {
                    "RANDOM" => () => new RandomLoadBalancer(),
                    "ROUNDROBIN" => () => new RoundRobinLoadBalancer(),
                    "STATIC" => () => new StaticLoadBalancer(value.Args),
                    _ => () => ServiceFactory.CreateInstance<ILoadBalancer>(value.TypeName, value.Args)
                };
            }
        }

        /// <summary>
        /// Gets the <see cref="HeartbeatOptions"/>.
        /// </summary>
        public HeartbeatOptions Heartbeat { get; } = new HeartbeatOptions();

        /// <summary>
        /// Gets the <see cref="MessagingOptions"/>.
        /// </summary>
        public MessagingOptions Messaging { get; } = new MessagingOptions();

        /// <summary>
        /// Gets the <see cref="NetworkingOptions"/>.
        /// </summary>
        public NetworkingOptions Networking { get; } = new NetworkingOptions();

        /// <summary>
        /// Gets the <see cref="EventsOptions"/>.
        /// </summary>
        public EventsOptions Events { get; } = new EventsOptions();
    }
}
