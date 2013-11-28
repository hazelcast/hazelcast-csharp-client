using System;
using System.Collections.Generic;
using Hazelcast.Client;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Partition.Strategy;
using Hazelcast.Transaction;
using Hazelcast.Util;
using System.Collections.Concurrent;


namespace Hazelcast.Client
{
	/// <summary>
	/// Hazelcast IClient enables you to do all Hazelcast operations without
	/// being a member of the cluster.
	/// </summary>
	/// <remarks>
	/// Hazelcast IClient enables you to do all Hazelcast operations without
	/// being a member of the cluster. It connects to one of the
	/// cluster members and delegates all cluster wide operations to it.
	/// When the connected cluster member dies, client will
	/// automatically switch to another live member.
	/// </remarks>
	public sealed class HazelcastClient : IHazelcastInstance
	{
		private static readonly AtomicInteger ClientId = new AtomicInteger();

        private static readonly ConcurrentDictionary<int, HazelcastClientProxy> Clients = new ConcurrentDictionary<int, HazelcastClientProxy>();

		private readonly int id = ClientId.GetAndIncrement();

		private readonly string instanceName;

		private readonly ClientConfig config;

		//private readonly ThreadGroup threadGroup;

		private readonly LifecycleService lifecycleService;

		private readonly ISerializationService serializationService;

		private readonly IClientConnectionManager connectionManager;

		private readonly ClientClusterService clusterService;

		private readonly ClientPartitionService partitionService;

		private readonly ClientInvocationService invocationService;

		private readonly ClientExecutionService executionService;

		private readonly ProxyManager proxyManager;

        private readonly ConcurrentDictionary<string, object> userContext;

		public const string PropPartitioningStrategyClass = "hazelcast.partitioning.strategy.class";

		private HazelcastClient(ClientConfig config)
		{
			this.config = config;
			GroupConfig groupConfig = config.GetGroupConfig();
			instanceName = "hz.client_" + id + (groupConfig != null ? "_" + groupConfig.GetName() : string.Empty);
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
				serializationService = new SerializationServiceBuilder().SetManagedContext(new HazelcastClientManagedContext(this, config.GetManagedContext())).SetConfig(config.GetSerializationConfig()).SetPartitioningStrategy(partitioningStrategy).Build();
			}
			catch (Exception e)
			{
				throw ExceptionUtil.Rethrow(e);
			}
			proxyManager = new ProxyManager(this);

            //TODO EXECUTION SERVICE
            executionService =  new ClientExecutionService(instanceName, config.GetExecutorPoolSize());
			clusterService = new ClientClusterService(this);
			LoadBalancer loadBalancer = config.GetLoadBalancer();
			if (loadBalancer == null)
			{
				loadBalancer = new RoundRobinLB();
			}
			if (config.IsSmartRouting())
			{
				connectionManager = new SmartClientConnectionManager(this, clusterService.GetAuthenticator(), loadBalancer);
			}
			else
			{
				connectionManager = new DummyClientConnectionManager(this, clusterService.GetAuthenticator(), loadBalancer);
			}
			invocationService = new ClientInvocationService(this);
            userContext = new ConcurrentDictionary<string, object>();
			loadBalancer.Init(GetCluster(), config);
			proxyManager.Init(config);
			partitionService = new ClientPartitionService(this);
		}

		public static IHazelcastInstance NewHazelcastClient()
		{
			return NewHazelcastClient(new XmlClientConfigBuilder().Build());
		}

		public static IHazelcastInstance NewHazelcastClient(ClientConfig config)
		{
			if (config == null)
			{
				config = new XmlClientConfigBuilder().Build();
			}
			HazelcastClient client = new HazelcastClient(config);
			client.Start();
			HazelcastClientProxy proxy = new HazelcastClientProxy(client);
			Clients.TryAdd(client.id, proxy);
			return proxy;
		}

		public static System.Collections.Generic.ICollection<IHazelcastInstance> GetAllHazelcastClients()
		{
            return (System.Collections.Generic.ICollection<IHazelcastInstance>)Clients.Values;
		}

		public static void ShutdownAll()
		{
			foreach (HazelcastClientProxy proxy in Clients.Values)
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

		public IHazelcastSet<E> GetSet<E>(string name)
        {
            return GetDistributedObject<IHazelcastSet<E>>(ServiceNames.Set, name);
		}

		public IHazelcastList<E> GetList<E>(string name)
        {
            return GetDistributedObject<IHazelcastList<E>>(ServiceNames.List, name);
		}

        public IHazelcastMap<K, V> GetMap<K, V>(string name)
		{
            return GetDistributedObject<IHazelcastMap<K, V>>(ServiceNames.Map, name);
		}

		public IMultiMap<K, V> GetMultiMap<K, V>(string name)
        {
            return GetDistributedObject<IMultiMap<K, V>>(ServiceNames.MultiMap, name);
		}

		public ILock GetLock(string key)
        {
			return GetDistributedObject<ILock>(ServiceNames.Lock, key);
		}

        //[Obsolete]
        //public ILock GetLock(object key)
        //{
        //    IPartitioningStrategy IPartitioningStrategy = new StringPartitioningStrategy();
        //    // will be removed when IHazelcastInstance.getLock(Object key) is removed from API
        //    string lockName = (key is string) ? key.ToString() : Arrays.ToString(serializationService.ToData(key, IPartitioningStrategy).GetBuffer());
        //    //
        //    return GetDistributedObject<ClientLockProxy>(ServiceNames.Lock, lockName);
        //}

		public ICluster GetCluster()
		{
			return new ClientClusterProxy(clusterService);
		}

		public IClient GetLocalEndpoint()
		{
			return clusterService.GetLocalClient();
		}

		public IExecutorService GetExecutorService(string name)
		{
            //TODO FIXME
            throw new NotSupportedException("Not implemented yet");
            //return GetDistributedObject<ClientE>(ServiceNames.DistributedExecutor, name);
		}

		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		public T ExecuteTransaction<T>(ITransactionalTask<T> task)
		{
			return ExecuteTransaction(TransactionOptions.GetDefault(), task);
		}

		/// <exception cref="Hazelcast.Transaction.TransactionException"></exception>
		public T ExecuteTransaction<T>(TransactionOptions options, ITransactionalTask<T> task)
		{

			ITransactionContext context = NewTransactionContext(options);
			context.BeginTransaction();
			try
			{
				T value = task.Execute(context);
				context.CommitTransaction();
				return value;
			}
			catch (Exception e)
			{
				context.RollbackTransaction();
                //TODO FIX EXEPTION
                if (e is TransactionException)
                {
                    throw (TransactionException)e;
                }
                if (e.InnerException is TransactionException)
                {
                    throw (TransactionException)e.InnerException;
                }
                if (e is SystemException)
                {
                    throw (SystemException)e;
                }
                throw new TransactionException(e);
			}
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
            return GetDistributedObject<IIdGenerator>(ServiceNames.IdGenerator, name) as IIdGenerator;
		}

		public IAtomicLong GetAtomicLong(string name)
		{
            return GetDistributedObject<IAtomicLong>(ServiceNames.AtomicLong, name) as IAtomicLong;
		}

		public ICountDownLatch GetCountDownLatch(string name)
		{
			return GetDistributedObject<ICountDownLatch>(ServiceNames.CountDownLatch, name) as ICountDownLatch;
		}

		public ISemaphore GetSemaphore(string name)
		{
            return GetDistributedObject<ISemaphore>(ServiceNames.Semaphore, name) as ISemaphore;
		}

		public ICollection<IDistributedObject> GetDistributedObjects()
		{
			try
			{
				GetDistributedObjectsRequest request = new GetDistributedObjectsRequest();
                SerializableCollection serializableCollection = invocationService.InvokeOnRandomTarget<SerializableCollection>(request);
				foreach (Data data in serializableCollection)
				{
					DistributedObjectInfo o = (DistributedObjectInfo)serializationService.ToObject(data);
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
		//    public PartitionService getPartitionService() {
		//        return new PartitionServiceProxy(partitionService);
		//    }
		public IClientService GetClientService()
		{
			throw new NotSupportedException();
		}

		//    @Override
		//    public LoggingService getLoggingService() {
		//        throw new UnsupportedOperationException();
		//    }
		public ILifecycleService GetLifecycleService()
		{
			return lifecycleService;
		}

        //[Obsolete]
        //public T GetDistributedObject<T>(string serviceName, object id) where T : IDistributedObject
        //{
        //    if (id is string)
        //    {
        //        return (T)proxyManager.GetProxy(serviceName, (string)id);
        //    }
        //    throw new ArgumentException("'id' must be type of String!");
        //}

	    public T GetDistributedObject<T>(string serviceName, string name) where T : IDistributedObject
	    {
	        var clientProxy = proxyManager.GetOrCreateProxy<T>(serviceName, name);

	        return (T) (clientProxy as IDistributedObject);
	    }

	    //public IDistributedObject GetDistributedObject(string serviceName, string name)
        //{
        //    return proxyManager.GetProxy(serviceName, name);
        //}

		public ConcurrentDictionary<string, object> GetUserContext()
		{
			return userContext;
		}

		public ClientConfig GetClientConfig()
		{
			return config;
		}

		public ISerializationService GetSerializationService()
		{
			return serializationService;
		}

		public IClientConnectionManager GetConnectionManager()
		{
			return connectionManager;
		}

		public IClientClusterService GetClientClusterService()
		{
			return clusterService;
		}

		public IClientExecutionService GetClientExecutionService()
		{
			return executionService;
		}

		public IClientPartitionService GetClientPartitionService()
		{
			return partitionService;
		}

		public IClientInvocationService GetInvocationService()
		{
			return invocationService;
		}

        //public ThreadGroup GetThreadGroup()
        //{
        //    return threadGroup;
        //}

		public void Shutdown()
		{
			GetLifecycleService().Shutdown();
		}

		internal void DoShutdown()
		{
            HazelcastClientProxy _out;
            Clients.TryRemove(id,out _out);
			executionService.Shutdown();
			partitionService.Stop();
			clusterService.Stop();
			connectionManager.Shutdown();
			proxyManager.Destroy();
		}
	}
}
