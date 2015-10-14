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
        /// <summary>
        /// The Group Configuration properties like:
        /// Name and Password that is used to connect to the cluster.
        /// </summary>
        /// <remarks>
        /// The Group Configuration properties like:
        /// Name and Password that is used to connect to the cluster.
        /// </remarks>
        private GroupConfig groupConfig = new GroupConfig();

        /// <summary>
        /// The Security Configuration for custom Credentials:
        /// Name and Password that is used to connect to the cluster.
        ///     Can be used instead of
        ///     <see cref="GroupConfig">GroupConfig</see>
        ///     in Hazelcast EE.
        /// </summary>
        private ICredentials credentials;

        /// <summary>
        /// The Network Configuration properties like:
        /// addresses to connect, smart-routing, socket-options...
        /// </summary>
        /// <remarks>
        /// The Network Configuration properties like:
        /// addresses to connect, smart-routing, socket-options...
        /// </remarks>
        private ClientNetworkConfig networkConfig = new ClientNetworkConfig();

        /// <summary>Used to distribute the operations to multiple Endpoints.</summary>
        private ILoadBalancer loadBalancer = new RoundRobinLB();

        /// <summary>List of listeners that Hazelcast will automatically add as a part of initialization process.</summary>
        /// <remarks>
        /// List of listeners that Hazelcast will automatically add as a part of initialization process.
        /// Currently only supports
        /// <see cref="Hazelcast.Core.LifecycleListener">Hazelcast.Core.LifecycleListener</see>
        /// .
        /// </remarks>
        private IList<ListenerConfig> listenerConfigs = new List<ListenerConfig>();

        /// <summary>pool-size for internal ExecutorService which handles responses etc.</summary>
        private int executorPoolSize = -1;

        private SerializationConfig serializationConfig = new SerializationConfig();

        private IList<ProxyFactoryConfig> proxyFactoryConfigs = new List<ProxyFactoryConfig>();

        private IManagedContext managedContext = null;

        private IDictionary<string, NearCacheConfig> nearCacheConfigMap = new Dictionary<string, NearCacheConfig>();

        private IConfigPatternMatcher configPatternMatcher = new MatchingPointConfigPatternMatcher();

        private string licenseKey;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientConfig));

        public virtual string GetLicenseKey()
        {
            return licenseKey;
        }

        public virtual ClientConfig SetLicenseKey(string licenseKey)
        {
            this.licenseKey = licenseKey;
            return this;
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
            return networkConfig;
        }

        public virtual void SetNetworkConfig(ClientNetworkConfig networkConfig)
        {
            this.networkConfig = networkConfig;
        }

        public virtual ClientConfig AddNearCacheConfig(NearCacheConfig nearCacheConfig)
        {
            nearCacheConfigMap.Add(nearCacheConfig.GetName(), nearCacheConfig);
            return this;
        }

        public virtual ClientConfig AddNearCacheConfig(string mapName, NearCacheConfig nearCacheConfig)
        {
            nearCacheConfigMap.Add(mapName, nearCacheConfig);
            return this;
        }

        public virtual ClientConfig AddListenerConfig(ListenerConfig listenerConfig)
        {
            GetListenerConfigs().Add(listenerConfig);
            return this;
        }

        public virtual ClientConfig AddProxyFactoryConfig(ProxyFactoryConfig proxyFactoryConfig)
        {
            this.proxyFactoryConfigs.Add(proxyFactoryConfig);
            return this;
        }

        public virtual NearCacheConfig GetNearCacheConfig(string mapName)
        {
            return LookupByPattern(nearCacheConfigMap, mapName);
        }

        public virtual IDictionary<string, NearCacheConfig> GetNearCacheConfigMap()
        {
            return nearCacheConfigMap;
        }

        public virtual ClientConfig SetNearCacheConfigMap(IDictionary<string, NearCacheConfig> nearCacheConfigMap)
        {
            this.nearCacheConfigMap = nearCacheConfigMap;
            return this;
        }

        public virtual ICredentials GetCredentials()
        {
            if (credentials == null)
            {
                SetCredentials(new UsernamePasswordCredentials(GetGroupConfig().GetName(),
                    GetGroupConfig().GetPassword()));
            }
            return credentials;
        }

        public virtual ClientConfig SetCredentials(ICredentials credentials)
        {
            this.credentials = credentials;
            return this;
        }

        public virtual GroupConfig GetGroupConfig()
        {
            return groupConfig;
        }

        public virtual ClientConfig SetGroupConfig(GroupConfig groupConfig)
        {
            this.groupConfig = groupConfig;
            return this;
        }

        public virtual IList<ListenerConfig> GetListenerConfigs()
        {
            return listenerConfigs;
        }

        public virtual ClientConfig SetListenerConfigs(IList<ListenerConfig> listenerConfigs)
        {
            this.listenerConfigs = listenerConfigs;
            return this;
        }

        internal virtual ILoadBalancer GetLoadBalancer()
        {
            return loadBalancer;
        }

        internal virtual ClientConfig SetLoadBalancer(ILoadBalancer loadBalancer)
        {
            this.loadBalancer = loadBalancer;
            return this;
        }

        public virtual IManagedContext GetManagedContext()
        {
            return managedContext;
        }

        public virtual ClientConfig SetManagedContext(IManagedContext managedContext)
        {
            this.managedContext = managedContext;
            return this;
        }

        public virtual int GetExecutorPoolSize()
        {
            return executorPoolSize;
        }

        public virtual ClientConfig SetExecutorPoolSize(int executorPoolSize)
        {
            this.executorPoolSize = executorPoolSize;
            return this;
        }

        public virtual IList<ProxyFactoryConfig> GetProxyFactoryConfigs()
        {
            return proxyFactoryConfigs;
        }

        public virtual ClientConfig SetProxyFactoryConfigs(IList<ProxyFactoryConfig> proxyFactoryConfigs)
        {
            this.proxyFactoryConfigs = proxyFactoryConfigs;
            return this;
        }

        public virtual SerializationConfig GetSerializationConfig()
        {
            return serializationConfig;
        }

        public virtual ClientConfig SetSerializationConfig(SerializationConfig serializationConfig)
        {
            this.serializationConfig = serializationConfig;
            return this;
        }

        private T LookupByPattern<T>(IDictionary<string, T> map, string name)
        {
            T t;
            if (map.TryGetValue(name, out t))
            {
                return t;
            }

            var key = configPatternMatcher.Matches(map.Keys, name);
            if (key != null) return map[key];

            if ("default" != name && !name.StartsWith("hz:"))
            {
                Logger.Finest("No configuration found for " + name + ", using default config.");
            }
            return default(T);
        }

        public void SetConfigPatternMatcher(IConfigPatternMatcher matchingPointConfigPatternMatcher)
        {
            this.configPatternMatcher = matchingPointConfigPatternMatcher;
        }
    }
}
