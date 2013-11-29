//using Hazelcast.Core;

using System.Collections.Generic;
using Hazelcast.Client;
using Hazelcast.Core;
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    public class ClientConfig
    {
        /// <summary>List of the initial set of addresses.</summary>
        /// <remarks>
        ///     List of the initial set of addresses.
        ///     IClient will use this list to find a running IMember, connect to it.
        /// </remarks>
        private readonly List<string> addressList = new List<string>(10);

        /// <summary>
        ///     While client is trying to connect initially to one of the members in the
        ///     <see cref="addressList">addressList</see>
        ///     ,
        ///     all might be not available. Instead of giving up, throwing Exception and stopping client, it will
        ///     attempt to retry as much as
        ///     <see cref="connectionAttemptLimit">connectionAttemptLimit</see>
        ///     times.
        /// </summary>
        private int connectionAttemptLimit = 2;

        /// <summary>Period for the next attempt to find a member to connect.</summary>
        /// <remarks>
        ///     Period for the next attempt to find a member to connect. (see
        ///     <see cref="connectionAttemptLimit">connectionAttemptLimit</see>
        ///     ).
        /// </remarks>
        private int connectionAttemptPeriod = 3000;

        /// <summary>limit for the Pool size that is used to pool the connections to the members.</summary>
        /// <remarks>limit for the Pool size that is used to pool the connections to the members.</remarks>
        private int connectionPoolSize = 100;

        /// <summary>IClient will be sending heartbeat messages to members and this is the timeout.</summary>
        /// <remarks>
        ///     IClient will be sending heartbeat messages to members and this is the timeout. If there is no any message
        ///     passing between client and member within the
        ///     <see cref="connectionTimeout">connectionTimeout</see>
        ///     milliseconds the connection
        ///     will be closed.
        /// </remarks>
        private int connectionTimeout = 60000;

        /// <summary>
        ///     Can be used instead of
        ///     <see cref="GroupConfig">GroupConfig</see>
        ///     in Hazelcast EE.
        /// </summary>
        private ICredentials credentials;

        /// <summary>Period for the next attempt to find a member to connect.</summary>
        /// <remarks>
        ///     Period for the next attempt to find a member to connect. (see
        ///     <see cref="connectionAttemptLimit">connectionAttemptLimit</see>
        ///     ).
        /// </remarks>
        private int executorPoolSize = -1;

        /// <summary>
        ///     The Group Configuration properties like:
        ///     Name and Password that is used to connect to the cluster.
        /// </summary>
        /// <remarks>
        ///     The Group Configuration properties like:
        ///     Name and Password that is used to connect to the cluster.
        /// </remarks>
        private GroupConfig groupConfig = new GroupConfig();

        /// <summary>List of listeners that Hazelcast will automatically add as a part of initialization process.</summary>
        /// <remarks>
        ///     List of listeners that Hazelcast will automatically add as a part of initialization process.
        ///     Currently only supports
        ///     <see cref="Hazelcast.Core.LifecycleListener">Hazelcast.Core.ILifecycleListener</see>
        ///     .
        /// </remarks>
        private List<ListenerConfig> listenerConfigs = new List<ListenerConfig>();

        /// <summary>Used to distribute the operations to multiple Endpoints.</summary>
        /// <remarks>Used to distribute the operations to multiple Endpoints.</remarks>
        private LoadBalancer loadBalancer = new RoundRobinLB();

        private IManagedContext managedContext;

        private IDictionary<string, NearCacheConfig> nearCacheConfigMap = new Dictionary<string, NearCacheConfig>();

        private IList<ProxyFactoryConfig> proxyFactoryConfigs =
            new List<ProxyFactoryConfig>();

        /// <summary>If true, client will redo the operations that were executing on the server and client lost the connection.</summary>
        /// <remarks>
        ///     If true, client will redo the operations that were executing on the server and client lost the connection.
        ///     This can be because of network, or simply because the member died. However it is not clear whether the
        ///     application is performed or not. For idempotent operations this is harmless, but for non idempotent ones
        ///     retrying can cause to undesirable effects. Note that the redo can perform on any member.
        ///     <p />
        ///     If false, the operation will throw
        ///     <see cref="Hazelcast.Net.Ext.RuntimeException">Hazelcast.Net.Ext.RuntimeException</see>
        ///     that is wrapping
        ///     <see cref="System.IO.IOException">System.IO.IOException</see>
        ///     .
        /// </remarks>
        private bool redoOperation;

        private SerializationConfig serializationConfig = new SerializationConfig();

        /// <summary>If true, client will route the key based operations to owner of the key at the best effort.</summary>
        /// <remarks>
        ///     If true, client will route the key based operations to owner of the key at the best effort.
        ///     Note that it uses a cached version of PartitionService.Partitions() and doesn't
        ///     guarantee that the operation will always be executed on the owner. The cached table is updated every second.
        /// </remarks>
        private bool smartRouting = true;

        /// <summary>Will be called with the Socket, each time client creates a connection to any IMember.</summary>
        /// <remarks>Will be called with the Socket, each time client creates a connection to any IMember.</remarks>
        private SocketInterceptorConfig socketInterceptorConfig;

        private SocketOptions socketOptions = new SocketOptions();

        //    private ClassLoader classLoader = null;
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
            proxyFactoryConfigs.Add(proxyFactoryConfig);
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

        public virtual bool IsSmartRouting()
        {
            return smartRouting;
        }

        public virtual ClientConfig SetSmartRouting(bool smartRouting)
        {
            this.smartRouting = smartRouting;
            return this;
        }

        public virtual int GetConnectionPoolSize()
        {
            return connectionPoolSize;
        }

        public virtual ClientConfig SetConnectionPoolSize(int connectionPoolSize)
        {
            this.connectionPoolSize = connectionPoolSize;
            return this;
        }

        public virtual SocketInterceptorConfig GetSocketInterceptorConfig()
        {
            return socketInterceptorConfig;
        }

        public virtual ClientConfig SetSocketInterceptorConfig(SocketInterceptorConfig socketInterceptorConfig)
        {
            this.socketInterceptorConfig = socketInterceptorConfig;
            return this;
        }

        public virtual int GetConnectionAttemptPeriod()
        {
            return connectionAttemptPeriod;
        }

        public virtual ClientConfig SetConnectionAttemptPeriod(int connectionAttemptPeriod)
        {
            this.connectionAttemptPeriod = connectionAttemptPeriod;
            return this;
        }

        public virtual int GetConnectionAttemptLimit()
        {
            return connectionAttemptLimit;
        }

        public virtual ClientConfig SetConnectionAttemptLimit(int connectionAttemptLimit)
        {
            this.connectionAttemptLimit = connectionAttemptLimit;
            return this;
        }

        public virtual int GetConnectionTimeout()
        {
            return connectionTimeout;
        }

        public virtual ClientConfig SetConnectionTimeout(int connectionTimeout)
        {
            this.connectionTimeout = connectionTimeout;
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

        public virtual ClientConfig AddAddress(params string[] addresses)
        {
            addressList.AddRange(addresses);
            return this;
        }

        // required for spring module
        public virtual ClientConfig SetAddresses(IList<string> addresses)
        {
            addressList.Clear();
            addressList.AddRange(addresses);
            return this;
        }

        public virtual IList<string> GetAddresses()
        {
            if (addressList.Count == 0)
            {
                AddAddress("localhost");
            }
            return addressList;
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
            this.listenerConfigs = new List<ListenerConfig>(listenerConfigs);
            return this;
        }

        public virtual LoadBalancer GetLoadBalancer()
        {
            return loadBalancer;
        }

        public virtual ClientConfig SetLoadBalancer(LoadBalancer loadBalancer)
        {
            this.loadBalancer = loadBalancer;
            return this;
        }

        public virtual bool IsRedoOperation()
        {
            return redoOperation;
        }

        public virtual ClientConfig SetRedoOperation(bool redoOperation)
        {
            this.redoOperation = redoOperation;
            return this;
        }

        public virtual SocketOptions GetSocketOptions()
        {
            return socketOptions;
        }

        public virtual ClientConfig SetSocketOptions(SocketOptions socketOptions)
        {
            this.socketOptions = socketOptions;
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

        public virtual ClientConfig SetProxyFactoryConfigs(
            IList<ProxyFactoryConfig> proxyFactoryConfigs)
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

        private static T LookupByPattern<T>(IDictionary<string, T> map, string name)
        {
            T t;
            map.TryGetValue(name, out t);
            if (t == null)
            {
                ICollection<string> tNames = map.Keys;
                foreach (string pattern in tNames)
                {
                    if (NameMatches(name, pattern))
                    {
                        T value;
                        map.TryGetValue(pattern, out value);
                        return value;
                    }
                }
            }
            return t;
        }

        private static bool NameMatches(string name, string pattern)
        {
            int index = pattern.IndexOf('*');
            if (index == -1)
            {
                return name.Equals(pattern);
            }
            string firstPart = pattern.Substring(0, index);
            int indexFirstPart = name.IndexOf(firstPart, 0);
            if (indexFirstPart == -1)
            {
                return false;
            }
            string secondPart = pattern.Substring(index + 1);
            int indexSecondPart = name.IndexOf(secondPart, index + 1);
            return indexSecondPart != -1;
        }
    }
}