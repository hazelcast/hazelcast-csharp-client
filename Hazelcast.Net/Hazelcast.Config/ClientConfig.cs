// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>
    /// Main configuration to setup a Hazelcast Client.
    /// </summary>
    public class ClientConfig
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientConfig));

        private IConfigPatternMatcher _configPatternMatcher = new MatchingPointConfigPatternMatcher();

        /// <summary>
        /// The Security Configuration for custom Credentials:
        /// Name and Password that is used to connect to the cluster.
        ///     Can be used instead of
        ///     <see cref="GroupConfig">GroupConfig</see>
        ///     in Hazelcast EE.
        /// </summary>
        private ICredentials _credentials;

        /// <summary>pool-size for internal ExecutorService which handles responses etc.</summary>
        private int _executorPoolSize = -1;

        /// <summary>
        /// The Group Configuration properties like:
        /// Name and Password that is used to connect to the cluster.
        /// </summary>
        /// <remarks>
        /// The Group Configuration properties like:
        /// Name and Password that is used to connect to the cluster.
        /// </remarks>
        private GroupConfig _groupConfig = new GroupConfig();

        /// <summary>List of listeners that Hazelcast will automatically add as a part of initialization process.</summary>
        /// <remarks>
        /// List of listeners that Hazelcast will automatically add as a part of initialization process.
        /// Currently only supports
        /// <see cref="Hazelcast.Core.LifecycleListener">Hazelcast.Core.LifecycleListener</see>
        /// .
        /// </remarks>
        private IList<ListenerConfig> _listenerConfigs = new List<ListenerConfig>();

        /// <summary>Used to distribute the operations to multiple Endpoints.</summary>
        private ILoadBalancer _loadBalancer = new RoundRobinLB();

        private IManagedContext _managedContext;

        private IDictionary<string, NearCacheConfig> _nearCacheConfigMap = new Dictionary<string, NearCacheConfig>();

        /// <summary>
        /// The Network Configuration properties like:
        /// addresses to connect, smart-routing, socket-options...
        /// </summary>
        /// <remarks>
        /// The Network Configuration properties like:
        /// addresses to connect, smart-routing, socket-options...
        /// </remarks>
        private ClientNetworkConfig _networkConfig = new ClientNetworkConfig();

        private IList<ProxyFactoryConfig> _proxyFactoryConfigs = new List<ProxyFactoryConfig>();

        private SerializationConfig _serializationConfig = new SerializationConfig();

        /// <summary>
        /// Helper method to add a new ListenerConfig.
        /// </summary>
        /// <param name="listenerConfig">ListnerConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddListenerConfig(ListenerConfig listenerConfig)
        {
            GetListenerConfigs().Add(listenerConfig);
            return this;
        }

        /// <summary>
        /// Helper method to add a new NearCacheConfig.
        /// </summary>
        /// <param name="nearCacheConfig">NearCacheConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddNearCacheConfig(NearCacheConfig nearCacheConfig)
        {
            _nearCacheConfigMap.Add(nearCacheConfig.GetName(), nearCacheConfig);
            return this;
        }

        /// <summary>
        /// Helper method to add a new NearCacheConfig.
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
        /// Helper method to add a new <see cref="ProxyFactoryConfig"/>.
        /// </summary>
        /// <param name="proxyFactoryConfig">ProxyFactoryConfig</param>
        /// <returns>configured <see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig AddProxyFactoryConfig(ProxyFactoryConfig proxyFactoryConfig)
        {
            _proxyFactoryConfigs.Add(proxyFactoryConfig);
            return this;
        }

        /// <summary>
        /// Gets <see cref="ICredentials"/>.
        /// </summary>
        /// <returns>Credentials</returns>
        public virtual ICredentials GetCredentials()
        {
            if (_credentials == null)
            {
                SetCredentials(new UsernamePasswordCredentials(GetGroupConfig().GetName(),
                    GetGroupConfig().GetPassword()));
            }
            return _credentials;
        }

        /// <summary>
        /// Gets pool-size for internal ExecutorService which handles responses etc.
        /// </summary>
        public virtual int GetExecutorPoolSize()
        {
            return _executorPoolSize;
        }

        /// <summary>
        /// Gets <see cref="GroupConfig"/>.
        /// </summary>
        /// <returns><see cref="GroupConfig"/></returns>
        public virtual GroupConfig GetGroupConfig()
        {
            return _groupConfig;
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
        /// Gets <see cref="IManagedContext"/>.
        /// </summary>
        /// <returns><see cref="IManagedContext"/></returns>
        public virtual IManagedContext GetManagedContext()
        {
            return _managedContext;
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
        /// Gets <see cref="ProxyFactoryConfig"/>.
        /// </summary>
        /// <returns><see cref="ProxyFactoryConfig"/></returns>
        public virtual IList<ProxyFactoryConfig> GetProxyFactoryConfigs()
        {
            return _proxyFactoryConfigs;
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
        /// Sets <see cref="IConfigPatternMatcher"/>.
        /// </summary>
        /// <param name="matchingPointConfigPatternMatcher"><see cref="IConfigPatternMatcher"/></param>
        /// <returns><see cref="SerializationConfig"/></returns>
        public ClientConfig SetConfigPatternMatcher(IConfigPatternMatcher matchingPointConfigPatternMatcher)
        {
            _configPatternMatcher = matchingPointConfigPatternMatcher;
            return this;
        }

        /// <summary>
        /// Sets <see cref="ICredentials"/> object.
        /// </summary>
        /// <param name="credentials"><see cref="ICredentials"/> to be set</param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetCredentials(ICredentials credentials)
        {
            _credentials = credentials;
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

        /// <summary>
        /// Sets <see cref="GroupConfig"/> object.
        /// </summary>
        /// <param name="groupConfig"><see cref="GroupConfig"/> to be set</param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetGroupConfig(GroupConfig groupConfig)
        {
            _groupConfig = groupConfig;
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
        /// Sets <see cref="IManagedContext"/> object.
        /// </summary>
        /// <param name="managedContext"><see cref="IManagedContext"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetManagedContext(IManagedContext managedContext)
        {
            _managedContext = managedContext;
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
        /// Sets <see cref="ProxyFactoryConfig"/>.
        /// </summary>
        /// <param name="proxyFactoryConfigs"><see cref="ProxyFactoryConfig"/></param>
        /// <returns><see cref="ClientConfig"/> for chaining</returns>
        public virtual ClientConfig SetProxyFactoryConfigs(IList<ProxyFactoryConfig> proxyFactoryConfigs)
        {
            _proxyFactoryConfigs = proxyFactoryConfigs;
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