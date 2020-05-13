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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Main configuration to setup a Hazelcast Client.
    /// </summary>
    public sealed partial class HazelcastConfiguration
    {
        /// <summary>
        /// Gets the default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets or sets the unique name of the cluster.
        /// </summary>
        public string ClusterName { get; set; } = DefaultClusterName;

        /// <summary>
        /// Gets the instance name. TODO: what is it?
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the FIXME ??? + why is it concurrent ???
        /// </summary>
        public IDictionary<string, string> Properties { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Gets or sets the FIXME ???
        /// </summary>
        public ISet<string> Labels { get; } = new HashSet<string>();

        /// <summary>
        /// Whether to start the client asynchronously. TODO: still used?
        /// </summary>
        public bool AsyncStart { get; set; }

        /// <summary>
        /// Gets the logging configuration.
        /// </summary>
        public LoggingConfiguration Logging { get; } = new LoggingConfiguration();

        /// <summary>
        /// Gets the networking configuration.
        /// </summary>
        public NetworkingConfiguration Networking { get; } = new NetworkingConfiguration();

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        public SecurityConfiguration Security { get; } = new SecurityConfiguration();

        /// <summary>
        /// Gets the load balancing configuration.
        /// </summary>
        public LoadBalancingConfiguration LoadBalancing { get; } = new LoadBalancingConfiguration();

        /// <summary>
        /// Gets the serialization configuration.
        /// </summary>
        public SerializationConfiguration Serialization { get; set; } = new SerializationConfiguration();

        /// <summary>
        /// Gets the NearCache configurations.
        /// </summary>
        public IDictionary<string, NearCacheConfig> NearCache { get; } = new Dictionary<string, NearCacheConfig>();

        /// <summary>
        /// Gets or sets the NearCache configuration pattern matcher.
        /// </summary>
        public IConfigPatternMatcher NearCacheConfigurationPatternMatcher { get; set; } = new MatchingPointConfigPatternMatcher();

        /// <summary>
        /// Looks a NearCache configuration up by its name, using a pattern.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NearCacheConfig LookupNearCacheConfiguration(string name)
        {
            if (NearCache.TryGetValue(name, out var configuration))
                return configuration;

            var key = NearCacheConfigurationPatternMatcher.Matches(NearCache.Keys, name);
            return key == null ? null : NearCache[key];
        }
    }
}