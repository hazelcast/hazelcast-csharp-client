// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Partition.Strategy;
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
        private readonly IClientInvocationService _invocationService;

        //private readonly ThreadGroup threadGroup;

        private readonly LifecycleService _lifecycleService;
        private readonly ClientListenerService _listenerService;
        private readonly ILoadBalancer _loadBalancer;
        private readonly ClientPartitionService _partitionService;
        private readonly ProxyManager _proxyManager;
        private readonly ISerializationService _serializationService;
        private readonly ConcurrentDictionary<string, object> _userContext;

        private HazelcastClient(ClientConfig config)
        {
            _config = config;
            var groupConfig = config.GetGroupConfig();
            _instanceName = "hz.client_" + _id + (groupConfig != null ? "_" + groupConfig.GetName() : string.Empty);

            //threadGroup = new ThreadGroup(instanceName);
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
            _connectionManager = new ClientConnectionManager(this);
            _invocationService = GetInvocationService(config);
            _listenerService = new ClientListenerService(this);
            _userContext = new ConcurrentDictionary<string, object>();
            _loadBalancer.Init(GetCluster(), config);
            _proxyManager.Init(config);
            _partitionService = new ClientPartitionService(this);
        }

        public string GetName()
        {
            return _instanceName;
        }

        public IQueue<T> GetQueue<T>(string name)
        {
            return GetDistributedObject<IQueue<T>>(ServiceNames.Queue, name);
        }

        public IRingbuffer<T> GetRingbuffer<T>(string name)
        {
            return GetDistributedObject<IRingbuffer<T>>(ServiceNames.Ringbuffer, name);
        }

        public ITopic<T> GetTopic<T>(string name)
        {
            return GetDistributedObject<ITopic<T>>(ServiceNames.Topic, name);
        }

        public IHSet<T> GetSet<T>(string name)
        {
            return GetDistributedObject<IHSet<T>>(ServiceNames.Set, name);
        }

        public IHList<T> GetList<T>(string name)
        {
            return GetDistributedObject<IHList<T>>(ServiceNames.List, name);
        }

        public IMap<TKey, TValue> GetMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMap<TKey, TValue>>(ServiceNames.Map, name);
        }

        public IMultiMap<TKey, TValue> GetMultiMap<TKey, TValue>(string name)
        {
            return GetDistributedObject<IMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name);
        }

        public ILock GetLock(string key)
        {
            return GetDistributedObject<ILock>(ServiceNames.Lock, key);
        }

        public ICluster GetCluster()
        {
            return new ClientClusterProxy(_clusterService);
        }

        public IEndpoint GetLocalEndpoint()
        {
            return _clusterService.GetLocalClient();
        }

        public ITransactionContext NewTransactionContext()
        {
            return NewTransactionContext(TransactionOptions.GetDefault());
        }

        public ITransactionContext NewTransactionContext(TransactionOptions options)
        {
            return new TransactionContextProxy(this, options);
        }

        public IIdGenerator GetIdGenerator(string name)
        {
            return GetDistributedObject<IIdGenerator>(ServiceNames.IdGenerator, name);
        }

        public IAtomicLong GetAtomicLong(string name)
        {
            return GetDistributedObject<IAtomicLong>(ServiceNames.AtomicLong, name);
        }

        public ICountDownLatch GetCountDownLatch(string name)
        {
            return GetDistributedObject<ICountDownLatch>(ServiceNames.CountDownLatch, name);
        }

        public ISemaphore GetSemaphore(string name)
        {
            return GetDistributedObject<ISemaphore>(ServiceNames.Semaphore, name);
        }

        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            try
            {
                var request = ClientGetDistributedObjectsCodec.EncodeRequest();
                var task = _invocationService.InvokeOnRandomTarget(request);
                var response = ThreadUtil.GetResult(task);
                var result = ClientGetDistributedObjectsCodec.DecodeResponse(response).infoCollection;
                foreach (var data in result)
                {
                    var o = _serializationService.ToObject<DistributedObjectInfo>(data);
                    GetDistributedObject<IDistributedObject>(o.GetServiceName(), o.GetName());
                }
                return _proxyManager.GetDistributedObjects();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener)
        {
            return _proxyManager.AddDistributedObjectListener(distributedObjectListener);
        }

        public bool RemoveDistributedObjectListener(string registrationId)
        {
            return _proxyManager.RemoveDistributedObjectListener(registrationId);
        }

        public IClientService GetClientService()
        {
            throw new NotSupportedException();
        }

        public ILifecycleService GetLifecycleService()
        {
            return _lifecycleService;
        }

        public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
        {
            var clientProxy = _proxyManager.GetOrCreateProxy<T>(serviceName, name);
            return (T) ((IDistributedObject) clientProxy);
        }

        public ConcurrentDictionary<string, object> GetUserContext()
        {
            return _userContext;
        }

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

        public ILoadBalancer GetLoadBalancer()
        {
            return _loadBalancer;
        }

        //    @Override
        public IClientPartitionService GetPartitionService()
        {
            throw new NotSupportedException("not supported yet");
            //return new PartitionServiceProxy(partitionService);
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
            _executionService.Shutdown();
            _partitionService.Stop();
            _connectionManager.Shutdown();
            _proxyManager.Destroy();
            _invocationService.Shutdown();
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

        private IClientInvocationService GetInvocationService(ClientConfig config)
        {
            return config.GetNetworkConfig().IsSmartRouting()
                ? (IClientInvocationService) new ClientSmartInvocationService(this)
                : new ClientNonSmartInvocationService(this);
        }

        private void Start()
        {
            _lifecycleService.SetStarted();
            try
            {
                _connectionManager.Start();
                _clusterService.Start();
                _partitionService.Start();
            }
            catch (InvalidOperationException)
            {
                //there was an authentication failure (todo: perhaps use an AuthenticationException
                // ??)
                _lifecycleService.Shutdown();
                throw;
            }
        }
    }
}