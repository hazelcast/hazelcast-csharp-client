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
using Hazelcast.Client;
using Hazelcast.Logging;

namespace Hazelcast.Config
{
    public class Configuration
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(Configuration));

        public IConfigPatternMatcher ConfigPatternMatcher { get; set; } = new MatchingPointConfigPatternMatcher();

        public const string DefaultClusterName = "dev";

        public string ClusterName { get; set; } = DefaultClusterName;
        public string InstanceName { get; set; }
        public bool BackupAckToClientEnabled { get; set; } = true;
        public ILoadBalancer LoadBalancer { get; set; }

        public IDictionary<string, string> Properties { get; set; } = new ConcurrentDictionary<string, string>();
        public ISet<string> Labels { get; set; } = new HashSet<string>();
        public NetworkConfig NetworkConfig { get; set; } = new NetworkConfig();
        public SecurityConfig SecurityConfig { get; set; } = new SecurityConfig();
        public IList<ListenerConfig> ListenerConfigs { get; set; } = new List<ListenerConfig>();
        public SerializationConfig SerializationConfig { get; set; } = new SerializationConfig();

        public IDictionary<string, NearCacheConfig> NearCacheConfigs { get; set; } =
            new ConcurrentDictionary<string, NearCacheConfig>();

        public ConnectionStrategyConfig ConnectionStrategyConfig { get; set; } = new ConnectionStrategyConfig();

        public Configuration ConfigureProperties(Action<IDictionary<string, string>> configAction)
        {
            configAction(Properties);
            return this;
        }

        public Configuration ConfigureLabels(Action<ISet<string>> configAction)
        {
            configAction(Labels);
            return this;
        }

        public Configuration ConfigureNetwork(Action<NetworkConfig> configAction)
        {
            configAction(NetworkConfig);
            return this;
        }

        public Configuration ConfigureSecurity(Action<SecurityConfig> configAction)
        {
            configAction(SecurityConfig);
            return this;
        }

        public Configuration ConfigureListeners(Action<IList<ListenerConfig>> configAction)
        {
            configAction(ListenerConfigs);
            return this;
        }

        public Configuration ConfigureSerialization(Action<SerializationConfig> configAction)
        {
            configAction(SerializationConfig);
            return this;
        }

        public Configuration ConfigureNearCache(string name, Action<NearCacheConfig> configAction)
        {
            var nameOrDefault = name ?? "default";
            var nearCacheConfig =
                ((ConcurrentDictionary<string, NearCacheConfig>) NearCacheConfigs).GetOrAdd(nameOrDefault,
                    n => new NearCacheConfig(n));
            configAction(nearCacheConfig);
            return this;
        }

        public Configuration ConfigureConnectionStrategy(Action<ConnectionStrategyConfig> configAction)
        {
            configAction(ConnectionStrategyConfig);
            return this;
        }

        public NearCacheConfig GetNearCacheConfig(string mapName)
        {
            return LookupByPattern(NearCacheConfigs, mapName);
        }

        private T LookupByPattern<T>(IDictionary<string, T> map, string name)
        {
            T t;
            if (map.TryGetValue(name, out t))
            {
                return t;
            }

            var key = ConfigPatternMatcher.Matches(map.Keys, name);
            if (key != null) return map[key];

            if ("default" != name && !name.StartsWith("hz:"))
            {
                Logger.Finest("No configuration found for " + name + ", using default config.");
            }
            return default(T);
        }
    }
}