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

        public virtual ClientConfig AddListenerConfig(ListenerConfig listenerConfig)
        {
            GetListenerConfigs().Add(listenerConfig);
            return this;
        }

        public virtual ClientConfig AddNearCacheConfig(NearCacheConfig nearCacheConfig)
        {
            _nearCacheConfigMap.Add(nearCacheConfig.GetName(), nearCacheConfig);
            return this;
        }

        public virtual ClientConfig AddNearCacheConfig(string mapName, NearCacheConfig nearCacheConfig)
        {
            _nearCacheConfigMap.Add(mapName, nearCacheConfig);
            return this;
        }

        public virtual ClientConfig AddProxyFactoryConfig(ProxyFactoryConfig proxyFactoryConfig)
        {
            _proxyFactoryConfigs.Add(proxyFactoryConfig);
            return this;
        }

        public virtual ICredentials GetCredentials()
        {
            if (_credentials == null)
            {
                SetCredentials(new UsernamePasswordCredentials(GetGroupConfig().GetName(),
                    GetGroupConfig().GetPassword()));
            }
            return _credentials;
        }

        public virtual int GetExecutorPoolSize()
        {
            return _executorPoolSize;
        }

        public virtual GroupConfig GetGroupConfig()
        {
            return _groupConfig;
        }

        public virtual IList<ListenerConfig> GetListenerConfigs()
        {
            return _listenerConfigs;
        }

        public virtual IManagedContext GetManagedContext()
        {
            return _managedContext;
        }

        public virtual NearCacheConfig GetNearCacheConfig(string mapName)
        {
            return LookupByPattern(_nearCacheConfigMap, mapName);
        }

        public virtual IDictionary<string, NearCacheConfig> GetNearCacheConfigMap()
        {
            return _nearCacheConfigMap;
        }

        //public virtual ClientSecurityConfig GetSecurityConfig()
        //{
        //    return securityConfig;
        //}

        //public virtual void SetSecurityConfig(ClientSecurityConfig securityConfig)
        //{
        //    this.securityConfig = securityConfig;
        //}

        public virtual ClientNetworkConfig GetNetworkConfig()
        {
            return _networkConfig;
        }

        public virtual IList<ProxyFactoryConfig> GetProxyFactoryConfigs()
        {
            return _proxyFactoryConfigs;
        }

        public virtual SerializationConfig GetSerializationConfig()
        {
            return _serializationConfig;
        }

        public void SetConfigPatternMatcher(IConfigPatternMatcher matchingPointConfigPatternMatcher)
        {
            _configPatternMatcher = matchingPointConfigPatternMatcher;
        }

        public virtual ClientConfig SetCredentials(ICredentials credentials)
        {
            _credentials = credentials;
            return this;
        }

        public virtual ClientConfig SetExecutorPoolSize(int executorPoolSize)
        {
            _executorPoolSize = executorPoolSize;
            return this;
        }

        public virtual ClientConfig SetGroupConfig(GroupConfig groupConfig)
        {
            _groupConfig = groupConfig;
            return this;
        }

        public virtual ClientConfig SetListenerConfigs(IList<ListenerConfig> listenerConfigs)
        {
            _listenerConfigs = listenerConfigs;
            return this;
        }

        public virtual ClientConfig SetManagedContext(IManagedContext managedContext)
        {
            _managedContext = managedContext;
            return this;
        }

        public virtual ClientConfig SetNearCacheConfigMap(IDictionary<string, NearCacheConfig> nearCacheConfigMap)
        {
            _nearCacheConfigMap = nearCacheConfigMap;
            return this;
        }

        public virtual void SetNetworkConfig(ClientNetworkConfig networkConfig)
        {
            _networkConfig = networkConfig;
        }

        public virtual ClientConfig SetProxyFactoryConfigs(IList<ProxyFactoryConfig> proxyFactoryConfigs)
        {
            _proxyFactoryConfigs = proxyFactoryConfigs;
            return this;
        }

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