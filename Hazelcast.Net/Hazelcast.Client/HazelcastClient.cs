// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Connection;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.NearCache;
using Hazelcast.Net.Ext;
using Hazelcast.Partition.Strategy;
using Hazelcast.Security;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    /// <summary>
    ///     Hazelcast Client enables you to do all Hazelcast operations without
    ///     being a member of the cluster.
    /// </summary>
    /// <remarks>
    ///     Hazelcast Client enables you to do all Hazelcast operations without
    ///     being a member of the cluster. It connects to one of the
    ///     cluster members and delegates all cluster wide operations to it.
    ///     When the connected cluster member dies, client will
    ///     automatically switch to another live member.
    /// </remarks>
    public sealed class HazelcastClient : IHazelcastInstance
    {
        public const string PropPartitioningStrategyClass = "hazelcast.partitioning.strategy.class";
        private static readonly AtomicInteger ClientId = new AtomicInteger();

        private static readonly ConcurrentDictionary<int, HazelcastClientProxy> Clients =
            new ConcurrentDictionary<int, HazelcastClientProxy>();

        private readonly ClientClusterService _clusterService;
        private readonly ClientConfig _config;
        private readonly ClientConnectionManager _connectionManager;
        private readonly IClientExecutionService _executionService;
        private readonly int _id = ClientId.GetAndIncrement();
        private readonly string _instanceName;
        private readonly ClientInvocationService _invocationService;

        private readonly LifecycleService _lifecycleService;
        private readonly ClientListenerService _listenerService;
        private readonly ILoadBalancer _loadBalancer;
        private readonly ClientPartitionService _partitionService;
        private readonly ProxyManager _proxyManager;
        private readonly ISerializationService _serializationService;
        private readonly ConcurrentDictionary<string, object> _userContext;
        private readonly ClientLockReferenceIdGenerator _lockReferenceIdGenerator;
        private readonly Statistics _statistics;
        private readonly NearCacheManager _nearCacheManager;
        private readonly ICredentialsFactory _credentialsFactory;
        private readonly AddressProvider _addressProvider;
        
        private HazelcastClient(ClientConfig config)
        {
            _config = config;
            var groupConfig = config.GetGroupConfig();
            _instanceName = "hz.client_" + _id + (groupConfig != null ? "_" + groupConfig.GetName() : string.Empty);

            _lifecycleService = new LifecycleService(this);
            try
            {
                //TODO make partition strategy parametric                
                var partitioningStrategy = new DefaultPartitioningStrategy();
                _serializationService =
                    new SerializationServiceBuilder().SetManagedContext(new HazelcastClientManagedContext(this,
                        config.GetManagedContext()))
                        .SetConfig(config.GetSerializationConfig())
                        .SetPartitioningStrategy(partitioningStrategy)
                        .SetVersion(SerializationService.SerializerVersion)
                        .Build();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            _proxyManager = new ProxyManager(this);

            //TODO EXECUTION SERVICE
            _executionService = new ClientExecutionService(_instanceName, config.GetExecutorPoolSize());
            _clusterService = new ClientClusterService(this);
            _loadBalancer = config.GetLoadBalancer() ?? new RoundRobinLB();
            
            _addressProvider = new AddressProvider(_config);
            _connectionManager = new ClientConnectionManager(this);
            _invocationService = CreateInvocationService();
            _listenerService = new ClientListenerService(this);
            _userContext = new ConcurrentDictionary<string, object>();
            _partitionService = new ClientPartitionService(this);
            _lockReferenceIdGenerator = new ClientLockReferenceIdGenerator();
            _statistics = new Statistics(this);
            _nearCacheManager = new NearCacheManager(this);
            _credentialsFactory = InitCredentialsFactory(config);
        }

        /// <inheritdoc />
        public string GetName()
        {
            return _instanceName;
        }

        /// <inheritdoc />
        public IQueue<T> GetQueue<T>(string name)
        {
            return GetDistributedObject<IQueue<T>>(ServiceNames.Queue, name);
        }

        /// <inheritdoc />
        public IRingbuffer<T> GetRingbuffer<T>(string name)
        {
            return GetDistributedObject<IRingbuffer<T>>(ServiceNames.Ringbuffer, name);
        }

        /// <inheritdoc />
        public ITopic<T> GetTopic<T>(string name)
        {
            return GetDistributedObject<ITopic<T>>(ServiceNames.Topic, name);
        }

        /// <inheritdoc />
        public IHSet<T> GetSet<T>(string name)
        {
            return GetDistributedObject<IHSet<T>>(ServiceNames.Set, name);
        }

        /// <inheritdoc />
        public IHList<T> GetList<T>(string name)
        {
            return GetDistributedObject<IHList<T>>(ServiceNames.List, name);
        }

        /// <inheritdoc />
        public IMap<TKey, TValue> GetMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMap<TKey, TValue>>(ServiceNames.Map, name);
        }

        /// <inheritdoc />
        public IMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name);
        }

        /// <inheritdoc />
        public IReplicatedMap<TKey, TValue> GetReplicatedMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IReplicatedMap<TKey, TValue>>(ServiceNames.ReplicatedMap, name);
        }

        /// <inheritdoc />
        public ILock GetLock(string key)
        {
            return GetDistributedObject<ILock>(ServiceNames.Lock, key);
        }

        /// <inheritdoc />
        public ICluster GetCluster()
        {
            return new ClientClusterProxy(_clusterService);
        }

        /// <inheritdoc />
        public IEndpoint GetLocalEndpoint()
        {
            return _clusterService.GetLocalClient();
        }

        /// <inheritdoc />
        public ITransactionContext NewTransactionContext()
        {
            return NewTransactionContext(TransactionOptions.GetDefault());
        }

        /// <inheritdoc />
        public ITransactionContext NewTransactionContext(TransactionOptions options)
        {
            return new TransactionContextProxy(this, options);
        }

        /// <inheritdoc />
        public IIdGenerator GetIdGenerator(string name)
        {
            return GetDistributedObject<IIdGenerator>(ServiceNames.IdGenerator, name);
        }

        /// <inheritdoc />
        public IAtomicLong GetAtomicLong(string name)
        {
            return GetDistributedObject<IAtomicLong>(ServiceNames.AtomicLong, name);
        }

        /// <inheritdoc />
        public ICountDownLatch GetCountDownLatch(string name)
        {
            return GetDistributedObject<ICountDownLatch>(ServiceNames.CountDownLatch, name);
        }

        /// <inheritdoc />
        public IPNCounter GetPNCounter(string name)
        {
            return GetDistributedObject<IPNCounter>(ServiceNames.PNCounter, name);
        }

        /// <inheritdoc />
        public ISemaphore GetSemaphore(string name)
        {
            return GetDistributedObject<ISemaphore>(ServiceNames.Semaphore, name);
        }

        /// <inheritdoc />
        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            return _proxyManager.GetDistributedObjects();
        }

        /// <inheritdoc />
        public string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener)
        {
            return _proxyManager.AddDistributedObjectListener(distributedObjectListener);
        }

        /// <inheritdoc />
        public bool RemoveDistributedObjectListener(string registrationId)
        {
            return _proxyManager.RemoveDistributedObjectListener(registrationId);
        }

        /// <inheritdoc />
        public IClientService GetClientService()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public ILifecycleService GetLifecycleService()
        {
            return _lifecycleService;
        }

        /// <inheritdoc />
        public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
        {
            var clientProxy = _proxyManager.GetOrCreateProxy<T>(serviceName, name);
            return (T) ((IDistributedObject) clientProxy);
        }

        /// <inheritdoc />
        public ConcurrentDictionary<string, object> GetUserContext()
        {
            return _userContext;
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            GetLifecycleService().Shutdown();
        }

        /// <summary>
        ///     Gets all Hazelcast clients.
        /// </summary>
        /// <returns>ICollection&lt;IHazelcastInstance&gt;</returns>
        public static ICollection<IHazelcastInstance> GetAllHazelcastClients()
        {
            return (ICollection<IHazelcastInstance>) Clients.Values;
        }

        /// <summary>
        /// Gets the configured <see cref="ILoadBalancer"/> instance
        /// </summary>
        /// <returns></returns>
        public ILoadBalancer GetLoadBalancer()
        {
            return _loadBalancer;
        }

        /// <summary>
        /// Not supported yet.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public IClientPartitionService GetPartitionService()
        {
            throw new NotSupportedException("not supported yet");
        }

        /// <summary>
        ///     Creates a new hazelcast client using default configuration.
        /// </summary>
        /// <remarks>
        ///     Creates a new hazelcast client using default configuration.
        /// </remarks>
        /// <returns>IHazelcastInstance.</returns>
        /// <example>
        ///     <code>
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient();
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        /// </example>
        public static IHazelcastInstance NewHazelcastClient()
        {
            return NewHazelcastClient(XmlClientConfigBuilder.Build());
        }

        /// <summary>
        ///     Creates a new hazelcast client using the given configuration xml file
        /// </summary>
        /// <param name="configFile">The configuration file with full or relative path.</param>
        /// <returns>IHazelcastInstance.</returns>
        /// <example>
        ///     <code>
        ///     //Full path
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(@"C:\Users\user\Hazelcast.Net\hazelcast-client.xml");
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        ///     
        ///     //relative path
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(@"..\Hazelcast.Net\Resources\hazelcast-client.xml");
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        /// </example>
        public static IHazelcastInstance NewHazelcastClient(string configFile)
        {
            return NewHazelcastClient(XmlClientConfigBuilder.Build(configFile));
        }

        /// <summary>
        ///     Creates a new hazelcast client using the given configuration object created programmaticly.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>IHazelcastInstance.</returns>
        /// <code>
        ///     var clientConfig = new ClientConfig();
        ///     //configure clientConfig ...
        ///     var hazelcastInstance = Hazelcast.NewHazelcastClient(clientConfig);
        ///     var myMap = hazelcastInstance.GetMap("myMap");
        /// </code>
        public static IHazelcastInstance NewHazelcastClient(ClientConfig config)
        {
            if (config == null)
            {
                config = XmlClientConfigBuilder.Build();
            }
            var client = new HazelcastClient(config);
            client.Start();
            var proxy = new HazelcastClientProxy(client);
            Clients.TryAdd(client._id, proxy);
            return proxy;
        }

        /// <summary>
        ///     Shutdowns all Hazelcast Clients .
        /// </summary>
        public static void ShutdownAll()
        {
            foreach (var proxy in Clients.Values)
            {
                try
                {
                    proxy.GetClient().GetLifecycleService().Shutdown();
                }
                catch
                {
                    // ignored
                }
            }
            Clients.Clear();
        }

        internal void DoShutdown()
        {
            HazelcastClientProxy _out;
            Clients.TryRemove(_id, out _out);
            _statistics.Destroy();
            _executionService.Shutdown();
            _partitionService.Stop();
            _connectionManager.Shutdown();
            _proxyManager.Shutdown();
            _invocationService.Shutdown();
            _nearCacheManager.Shutdown();
            _listenerService.Dispose();
            _serializationService.Destroy();
            _credentialsFactory.Destroy();
        }

        internal IClientClusterService GetClientClusterService()
        {
            return _clusterService;
        }

        internal ClientConfig GetClientConfig()
        {
            return _config;
        }

        internal IClientExecutionService GetClientExecutionService()
        {
            return _executionService;
        }

        internal IClientPartitionService GetClientPartitionService()
        {
            return _partitionService;
        }

        internal ClientConnectionManager GetConnectionManager()
        {
            return _connectionManager;
        }

        internal IClientInvocationService GetInvocationService()
        {
            return _invocationService;
        }

        internal IClientListenerService GetListenerService()
        {
            return _listenerService;
        }

        internal ISerializationService GetSerializationService()
        {
            return _serializationService;
        }

        internal ClientLockReferenceIdGenerator GetLockReferenceIdGenerator()
        {
            return _lockReferenceIdGenerator;
        }

        internal NearCacheManager GetNearCacheManager()
        {
            return _nearCacheManager;
        }

        internal Statistics GetStatistics()
        {
            return _statistics;
        }

        internal AddressProvider GetAddressProvider()
        {
            return _addressProvider;
        }
        
        internal ICredentialsFactory GetCredentialsFactory()
        {
            return _credentialsFactory;
        }

        private ClientInvocationService CreateInvocationService()
        {
            return _config.GetNetworkConfig().IsSmartRouting()
                ? (ClientInvocationService) new ClientSmartInvocationService(this)
                : new ClientNonSmartInvocationService(this);
        }

        private void Start()
        {
            _lifecycleService.SetStarted();
            try
            {
                _invocationService.Start();
                _connectionManager.Start();
                _clusterService.Start();
                _proxyManager.Init(_config);
                _listenerService.Start();
                _loadBalancer.Init(GetCluster(), _config);
                _partitionService.Start();
                _statistics.Start();
            }
            catch (InvalidOperationException)
            {
                //there was an authentication failure (todo: perhaps use an AuthenticationException
                // ??)
                _lifecycleService.Shutdown();
                throw;
            }
        }


        private ICredentialsFactory InitCredentialsFactory(ClientConfig config)
        {
            var securityConfig = config.GetSecurityConfig();
            ValidateSecurityConfig(securityConfig);
            var c = GetCredentialsFromFactory(config);
            if (c == null) {
                return new DefaultCredentialsFactory(securityConfig, config.GetGroupConfig());
            }
            return c;
        }
        
        private void ValidateSecurityConfig(ClientSecurityConfig securityConfig)
        {
            var configuredViaCredentials = securityConfig.GetCredentials() != null
                                               || securityConfig.GetCredentialsClassName() != null;

            var factoryConfig = securityConfig.GetCredentialsFactoryConfig();
            var configuredViaCredentialsFactory = factoryConfig.GetClassName() != null
                                                      || factoryConfig.GetImplementation() != null;

            if (configuredViaCredentials && configuredViaCredentialsFactory) {
                throw new ConfigurationException("Ambiguous Credentials config. Set only one of ICredentials or ICredentialsFactory");
            }
        }

 
        private ICredentialsFactory GetCredentialsFromFactory(ClientConfig config) {
            var credentialsFactoryConfig = config.GetSecurityConfig().GetCredentialsFactoryConfig();
            var factory = credentialsFactoryConfig.GetImplementation();
            if (factory == null) {
                var factoryClassName = credentialsFactoryConfig.GetClassName();
                if (factoryClassName != null) {
                    try {
                        var type = Type.GetType(factoryClassName);
                        if (type != null)
                        {
                            factory = Activator.CreateInstance(type) as ICredentialsFactory;
                        }
                    } catch (Exception e) {
                        throw ExceptionUtil.Rethrow(e);
                    }
                }
            }
            if (factory == null) {
                return null;
            }
            factory.Configure(config.GetGroupConfig(), credentialsFactoryConfig.GetProperties());
            return factory;
        }
    }
}