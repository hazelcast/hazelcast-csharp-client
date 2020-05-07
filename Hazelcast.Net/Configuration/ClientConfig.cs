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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Main configuration to setup a Hazelcast Client.
    /// </summary>
    public class ClientConfig
    {
        private static readonly ILogger Logger = Services.Get.LoggerFactory().CreateLogger<ClientConfig>();

        private IConfigPatternMatcher _configPatternMatcher = new MatchingPointConfigPatternMatcher();

        private ClientSecurityConfig _securityConfig = new ClientSecurityConfig();
        private ClientNetworkConfig _networkConfig = new ClientNetworkConfig();
        private SerializationConfig _serializationConfig = new SerializationConfig();
        private LoadBalancingConfig _loadBalancingConfig = new LoadBalancingConfig();

        private IDictionary<string, NearCacheConfig> _nearCacheConfigMap = new Dictionary<string, NearCacheConfig>();

        private string _clusterName = DefaultClusterName; // cluster to connect to
        private int _executorPoolSize = -1; // for the executor service which handles responses etc

        // FIXME need to merge w/ Asim work on config!

        /// <summary>
        /// Gets the default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// Gets the instance name. //TODO: what is it?
        /// </summary>
        public string InstanceName { get; set; }

        #region Events

        private readonly List<IClusterEventSubscriber> _clusterEventSubscribers = new List<IClusterEventSubscriber>();

        // FIXME now we must read this somewhere when setting up the cluster
        public List<IClusterEventSubscriber> ClusterEventSubscribers => _clusterEventSubscribers;

        public virtual ClientConfig AddClusterEventSubscriber(Func<Cluster, Task> subscribeAsync)
        {
            _clusterEventSubscribers.Add(new ClusterEventSubscriber(subscribeAsync));
            return this;
        }

        public virtual ClientConfig AddClusterEventSubscriber(IClusterEventSubscriber subscriber)
        {
            _clusterEventSubscribers.Add(new ClusterEventSubscriber(subscriber));
            return this;
        }

        public virtual ClientConfig AddClusterEventSubscriber<T>()
            where T : IClusterEventSubscriber
        {
            _clusterEventSubscribers.Add(new ClusterEventSubscriber(typeof(T)));
            return this;
        }

        public virtual ClientConfig AddClusterEventSubscriber(Type type)
        {
            _clusterEventSubscribers.Add(new ClusterEventSubscriber(type));
            return this;
        }

        public virtual ClientConfig AddClusterEventSubscriber(string typename)
        {
            _clusterEventSubscribers.Add(new ClusterEventSubscriber(typename));
            return this;
        }

        #endregion

        /// <summary>
        /// Helper method to Add a new NearCacheConfig.
        /// </summary>
        /// <param name="nearCacheConfig">NearCacheConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddNearCacheConfig(NearCacheConfig nearCacheConfig)
        {
            _nearCacheConfigMap.Add(nearCacheConfig.GetName(), nearCacheConfig);
            return this;
        }

        /// <summary>
        /// Helper method to Add a new NearCacheConfig.
        /// </summary>
        /// <param name="mapName">name of the IMap / ICache that Near Cache config will be applied to</param>
        /// <param name="nearCacheConfig">NearCacheConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddNearCacheConfig(string mapName, NearCacheConfig nearCacheConfig)
        {
            _nearCacheConfigMap.Add(mapName, nearCacheConfig);
            return this;
        }

        /// <summary>
        /// Gets pool-size for internal ExecutorService which handles responses etc.
        /// </summary>
        public virtual int GetExecutorPoolSize()
        {
            return _executorPoolSize;
        }

        /// <summary>
        /// Gets the cluster name.
        /// </summary>
        /// <returns>The current cluster name.</returns>
        public string GetClusterName()
        {
            return _clusterName;
        }

        public ConnectionStrategyConfig GetConnectionStrategyConfig()
        {
            return _connectionStrategyConfig;
        }

        public ClientConfig SetConnectionStrategyConfig(ConnectionStrategyConfig connectionStrategyConfig)
        {
            _connectionStrategyConfig = connectionStrategyConfig
                                        ?? throw new ArgumentNullException(nameof(connectionStrategyConfig));
            return this;
        }

        /// <summary>
        /// Gets <see cref="NearCacheConfig"/>.
        /// </summary>
        /// <param name="mapName">name of map</param>
        /// <returns><see cref="NearCacheConfig"/></returns>
        public virtual NearCacheConfig GetNearCacheConfig(string mapName)
        {
            return LookupByPattern(_nearCacheConfigMap, mapName);
        }

        /// <summary>
        /// Gets dictionary of all configured <see cref="NearCacheConfig"/>'s with the name key and configuration as the value.
        /// </summary>
        /// <returns>Dictionary of NearCacheConfig</returns>
        public virtual IDictionary<string, NearCacheConfig> GetNearCacheConfigMap()
        {
            return _nearCacheConfigMap;
        }

        /// <summary>
        /// Gets <see cref="ClientNetworkConfig"/>.
        /// </summary>
        /// <returns><see cref="ClientNetworkConfig"/></returns>
        public virtual ClientNetworkConfig GetNetworkConfig()
        {
            return _networkConfig;
        }

        /// <summary>
        /// Gets <see cref="SerializationConfig"/>.
        /// </summary>
        /// <returns><see cref="SerializationConfig"/></returns>
        public virtual SerializationConfig GetSerializationConfig()
        {
            return _serializationConfig;
        }

        /// <summary>
        /// Gets <see cref="ClientSecurityConfig"/>.
        /// </summary>
        /// <returns><see cref="ClientSecurityConfig"/></returns>
        public ClientSecurityConfig GetSecurityConfig()
        {
            return _securityConfig;
        }

        /// <summary>
        /// Sets <see cref="IConfigPatternMatcher"/>.
        /// </summary>
        /// <param name="matchingPointConfigPatternMatcher"><see cref="IConfigPatternMatcher"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public ClientConfig SetConfigPatternMatcher(IConfigPatternMatcher matchingPointConfigPatternMatcher)
        {
            _configPatternMatcher = matchingPointConfigPatternMatcher;
            return this;
        }

        /// <summary>
        /// Sets pool-size for internal ExecutorService which handles responses etc.
        /// </summary>
        /// <param name="executorPoolSize">executor pool size</param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetExecutorPoolSize(int executorPoolSize)
        {
            _executorPoolSize = executorPoolSize;
            return this;
        }

        ///<summary>
        /// Sets the cluster name uniquely identifying the hazelcast cluster. This name is used in different scenarios, such as identifying cluster for WAN publisher.
        /// </summary>
        public ClientConfig SetClusterName(string clusterName)
        {
            _clusterName = clusterName ?? throw new ArgumentNullException(nameof(clusterName));
            return this;
        }

        /// <summary>
        /// Sets all near cache configs.
        /// </summary>
        /// <param name="nearCacheConfigMap">Dictionary of map-name to NearCacheConfig</param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetNearCacheConfigMap(IDictionary<string, NearCacheConfig> nearCacheConfigMap)
        {
            _nearCacheConfigMap = nearCacheConfigMap;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ClientNetworkConfig"/>.
        /// </summary>
        /// <param name="networkConfig"><see cref="ClientNetworkConfig"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetNetworkConfig(ClientNetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
            return this;
        }

        /// <summary>
        /// Sets <see cref="SerializationConfig"/>.
        /// </summary>
        /// <param name="serializationConfig"><see cref="SerializationConfig"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetSerializationConfig(SerializationConfig serializationConfig)
        {
            _serializationConfig = serializationConfig;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ClientSecurityConfig"/>.
        /// </summary>
        /// <param name="securityConfig"><see cref="ClientSecurityConfig"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public ClientConfig SetSecurityConfig(ClientSecurityConfig securityConfig)
        {
            _securityConfig = securityConfig;
            return this;
        }

        public ClientConfig AddLabel(string label)
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            Labels.Add(label);
            return this;
        }

        private readonly ISet<string> _labels = new HashSet<string>();
        private ConnectionStrategyConfig _connectionStrategyConfig = new ConnectionStrategyConfig();

        public ISet<string> Labels
        {
            get => _labels;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _labels.Clear();
                value.All(_labels.Add);
            }
        }

        public LoadBalancingConfig GetLoadBalancingConfig() => _loadBalancingConfig;

        public ClientConfig SetLoadBalancingConfig(LoadBalancingConfig config)
        {
            _loadBalancingConfig = config;
            return this;
        }

        private T LookupByPattern<T>(IDictionary<string, T> map, string name)
        {
            T t;
            if (map.TryGetValue(name, out t))
            {
                return t;
            }

            var key = _configPatternMatcher.Matches(map.Keys, name);
            if (key != null) return map[key];

            if ("default" != name && !name.StartsWith("hz:"))
            {
                Logger.LogDebug("No configuration found for " + name + ", using default config.");
            }
            return default(T);
        }
    }
}