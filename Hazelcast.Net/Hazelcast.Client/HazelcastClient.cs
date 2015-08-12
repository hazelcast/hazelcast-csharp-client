using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Request.Base;
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

        private readonly ClientClusterService clusterService;
        private readonly ClientConfig config;
        private readonly IClientConnectionManager connectionManager;
        private readonly ClientExecutionService executionService;
        private readonly int id = ClientId.GetAndIncrement();
        private readonly string instanceName;
        private readonly ClientInvocationService invocationService;
        //private readonly ThreadGroup threadGroup;

        private readonly LifecycleService lifecycleService;
        private readonly ClientPartitionService partitionService;
        private readonly ProxyManager proxyManager;
        private readonly ISerializationService serializationService;
        private readonly ConcurrentDictionary<string, object> userContext;

        private HazelcastClient(ClientConfig config)
        {
            this.config = config;
            var groupConfig = config.GetGroupConfig();
            instanceName = "hz.client_" + id + (groupConfig != null ? "_" + groupConfig.GetName() : string.Empty);

            BeforeStart(config);
            //threadGroup = new ThreadGroup(instanceName);
            lifecycleService = new LifecycleService(this);
            try
            {
                string partitioningStrategyClassName = null;
                //TODO make partition strategy parametric                
                //Runtime.GetProperty(PropPartitioningStrategyClass);
                IPartitioningStrategy partitioningStrategy;
                if (partitioningStrategyClassName != null && partitioningStrategyClassName.Length > 0)
                {
                    partitioningStrategy = null;
                }
                else
                {
                    //new Instance for partitioningStrategyClassName;
                    partitioningStrategy = new DefaultPartitioningStrategy();
                }
                serializationService =
                    new SerializationServiceBuilder().SetManagedContext(new HazelcastClientManagedContext(this,
                        config.GetManagedContext()))
                        .SetConfig(config.GetSerializationConfig())
                        .SetPartitioningStrategy(partitioningStrategy)
                        .Build();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            proxyManager = new ProxyManager(this);

            //TODO EXECUTION SERVICE
            executionService = new ClientExecutionService(instanceName, config.GetExecutorPoolSize());
            clusterService = new ClientClusterService(this);
            var loadBalancer = config.GetLoadBalancer();
            if (loadBalancer == null)
            {
                loadBalancer = new RoundRobinLB();
            }

            connectionManager = new ClientConnectionManager(this, loadBalancer,
                config.GetNetworkConfig().IsSmartRouting());

            invocationService = new ClientInvocationService(this);
            userContext = new ConcurrentDictionary<string, object>();
            loadBalancer.Init(GetCluster(), config);
            proxyManager.Init(config);
            partitionService = new ClientPartitionService(this);
        }

        private static void BeforeStart(ClientConfig clientConfig)
        {
            var licenseKey = clientConfig.GetLicenseKey();
            var list = new List<LicenseType>
            {
                LicenseType.ENTERPRISE
              //  LicenseType.ENTERPRISE_SECURITY_ONLY
            };
            LicenseExtractor.CheckLicenseKey(licenseKey, list);
        }

        #region HazelcastInstance

        public string GetName()
        {
            return instanceName;
        }

        public IQueue<E> GetQueue<E>(string name)
        {
            return GetDistributedObject<IQueue<E>>(ServiceNames.Queue, name);
        }

        public ITopic<E> GetTopic<E>(string name)
        {
            return GetDistributedObject<ITopic<E>>(ServiceNames.Topic, name);
        }

        public IHSet<E> GetSet<E>(string name)
        {
            return GetDistributedObject<IHSet<E>>(ServiceNames.Set, name);
        }

        public IHList<E> GetList<E>(string name)
        {
            return GetDistributedObject<IHList<E>>(ServiceNames.List, name);
        }

        public IMap<K, V> GetMap<K, V>(string name)
        {
            return GetDistributedObject<IMap<K, V>>(ServiceNames.Map, name);
        }

        public IMultiMap<K, V> GetMultiMap<K, V>(string name)
        {
            return GetDistributedObject<IMultiMap<K, V>>(ServiceNames.MultiMap, name);
        }

        public ILock GetLock(string key)
        {
            return GetDistributedObject<ILock>(ServiceNames.Lock, key);
        }

        public ICluster GetCluster()
        {
            return new ClientClusterProxy(clusterService);
        }

        public IEndpoint GetLocalEndpoint()
        {
            return clusterService.GetLocalClient();
        }

        //public IExecutorService GetExecutorService(string name)
        //{
        //    //TODO FIXME
        //    throw new NotSupportedException("Not implemented yet");
        //    //return GetDistributedObject<ClientE>(ServiceNames.DistributedExecutor, name);
        //}

        ///// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        //public T ExecuteTransaction<T>(ITransactionalTask<T> task)
        //{
        //    return ExecuteTransaction(TransactionOptions.GetDefault(), task);
        //}

        ///// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
        //public T ExecuteTransaction<T>(TransactionOptions options, ITransactionalTask<T> task)
        //{
        //    ITransactionContext context = NewTransactionContext(options);
        //    context.BeginTransaction();
        //    try
        //    {
        //        T value = task.Execute(context);
        //        context.CommitTransaction();
        //        return value;
        //    }
        //    catch (Exception e)
        //    {
        //        context.RollbackTransaction();
        //        //TODO FIX EXEPTION
        //        if (e is TransactionException)
        //        {
        //            throw e;
        //        }
        //        if (e.InnerException is TransactionException)
        //        {
        //            throw e.InnerException;
        //        }
        //        if (e is SystemException)
        //        {
        //            throw e;
        //        }
        //        throw new TransactionException(e);
        //    }
        //}

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
                var request = ClientGetDistributedObjectCodec.EncodeRequest();
                var task = invocationService.InvokeOnRandomTarget(request);
                var response = ThreadUtil.GetResult(task);
                var result = ClientGetDistributedObjectCodec.DecodeResponse(response).infoCollection;
                foreach (var data in result)
                {
                    var o = serializationService.ToObject<DistributedObjectInfo>(data);
                    GetDistributedObject<IDistributedObject>(o.GetServiceName(), o.GetName());
                }
                return proxyManager.GetDistributedObjects<IDistributedObject>();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public string AddDistributedObjectListener(IDistributedObjectListener distributedObjectListener)
        {
            return proxyManager.AddDistributedObjectListener(distributedObjectListener);
        }

        public bool RemoveDistributedObjectListener(string registrationId)
        {
            return proxyManager.RemoveDistributedObjectListener(registrationId);
        }

        //    @Override
        public IClientPartitionService GetPartitionService()
        {
            throw new NotSupportedException("not supported yet");
            //return new PartitionServiceProxy(partitionService);
        }

        public IClientService GetClientService()
        {
            throw new NotSupportedException();
        }

        public ILifecycleService GetLifecycleService()
        {
            return lifecycleService;
        }

        public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
        {
            var clientProxy = proxyManager.GetOrCreateProxy<T>(serviceName, name);

            return (T) (clientProxy as IDistributedObject);
        }

        public ConcurrentDictionary<string, object> GetUserContext()
        {
            return userContext;
        }

        public void Shutdown()
        {
            GetLifecycleService().Shutdown();
        }

        #endregion

        #region HC

        private void Start()
        {
            lifecycleService.SetStarted();
            try
            {
                clusterService.Start();
            }
            catch (InvalidOperationException e)
            {
                //there was an authentication failure (todo: perhaps use an AuthenticationException
                // ??)
                lifecycleService.Shutdown();
                throw;
            }
            partitionService.Start();
        }

        internal ClientConfig GetClientConfig()
        {
            return config;
        }

        internal ISerializationService GetSerializationService()
        {
            return serializationService;
        }

        internal IClientConnectionManager GetConnectionManager()
        {
            return connectionManager;
        }

        internal IClientClusterService GetClientClusterService()
        {
            return clusterService;
        }

        internal IClientExecutionService GetClientExecutionService()
        {
            return executionService;
        }

        internal IClientPartitionService GetClientPartitionService()
        {
            return partitionService;
        }

        internal IClientInvocationService GetInvocationService()
        {
            return invocationService;
        }

        internal IRemotingService GetRemotingService()
        {
            return connectionManager;
        }

        internal void DoShutdown()
        {
            HazelcastClientProxy _out;
            Clients.TryRemove(id, out _out);
            executionService.Shutdown();
            partitionService.Stop();
            clusterService.Stop();
            connectionManager.Shutdown();
            proxyManager.Destroy();
        }

        #endregion

        #region statics

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
            Clients.TryAdd(client.id, proxy);
            return proxy;
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
        ///     Shutdowns all Hazelcast Clients .
        /// </summary>
        public static void ShutdownAll()
        {
            foreach (var proxy in Clients.Values)
            {
                try
                {
                    proxy.client.GetLifecycleService().Shutdown();
                }
                catch (Exception)
                {
                }
                proxy.client = null;
            }
            Clients.Clear();
        }

        #endregion
    }
}
