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
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Security;
using Hazelcast.Util;
using static Hazelcast.Util.ValidationUtil;

namespace Hazelcast.Config
{
    /// <summary>
    /// Main configuration to setup a Hazelcast Client.
    /// </summary>
    public class ClientConfig
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientConfig));

        private IConfigPatternMatcher _configPatternMatcher = new MatchingPointConfigPatternMatcher();

        /// <summary>
        /// The Security Configuration for custom Credentials:
        /// Name and Password that is used to connect to the cluster.
        /// </summary>
        private ClientSecurityConfig _securityConfig = new ClientSecurityConfig();

        /// <summary>
        /// Default cluster name.
        /// </summary>
        public const string DefaultClusterName = "dev";

        /// <summary>
        /// The cluster name to connect to.
        /// </summary>
        private string _clusterName = DefaultClusterName;


        /// <summary>pool-size for internal ExecutorService which handles responses etc.</summary>
        private int _executorPoolSize = -1;

        /// <summary>List of listeners that Hazelcast will automatically Add as a part of initialization process.</summary>
        /// <remarks>
        /// List of listeners that Hazelcast will automatically Add as a part of initialization process.
        /// Currently only supports
        /// <see cref="Hazelcast.Core.LifecycleListener">Hazelcast.Core.LifecycleListener</see>
        /// .
        /// </remarks>
        private IList<ListenerConfig> _listenerConfigs = new List<ListenerConfig>();

        /// <summary>Used to distribute the operations to multiple Endpoints.</summary>
        private ILoadBalancer _loadBalancer = new RoundRobinLB();


        private IDictionary<string, NearCacheConfig> _nearCacheConfigMap = new Dictionary<string, NearCacheConfig>();

        /// <summary>
        /// The Network Configuration properties like:
        /// addresses to connect, smart-routing, socket-options...
        /// </summary>
        private ClientNetworkConfig _networkConfig = new ClientNetworkConfig();


        private SerializationConfig _serializationConfig = new SerializationConfig();
        
        public string InstanceName { get; set; }

        /// <summary>
        /// Helper method to Add a new ListenerConfig.
        /// </summary>
        /// <param name="listenerConfig">ListenerConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddListenerConfig(ListenerConfig listenerConfig)
        {
            GetListenerConfigs().Add(listenerConfig);
            return this;
        }

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
            IsNotNull(connectionStrategyConfig, "connectionStrategyConfig");
            _connectionStrategyConfig = connectionStrategyConfig;
            return this;
        }


        /// <summary>
        /// Gets list of configured <see cref="ListenerConfig"/>.
        /// </summary>
        /// <returns>list of configured <see cref="ListenerConfig"/></returns>
        public virtual IList<ListenerConfig> GetListenerConfigs()
        {
            return _listenerConfigs;
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
        /// Sets <see cref="ListenerConfig"/> object.
        /// </summary>
        /// <param name="listenerConfigs"><see cref="ListenerConfig"/> to be set</param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetListenerConfigs(IList<ListenerConfig> listenerConfigs)
        {
            _listenerConfigs = listenerConfigs;
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
            IsNotNull(label, "labels");        
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
                IsNotNull(value, "labels");
                _labels.Clear();
                value.All(_labels.Add);
            }
        }

        internal virtual ILoadBalancer GetLoadBalancer()
        {
            return _loadBalancer;
        }

        internal virtual ClientConfig SetLoadBalancer(ILoadBalancer loadBalancer)
        {
            _loadBalancer = loadBalancer;
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
                Logger.Finest("No configuration found for " + name + ", using default config.");
            }
            return default(T);
        }
    }
}