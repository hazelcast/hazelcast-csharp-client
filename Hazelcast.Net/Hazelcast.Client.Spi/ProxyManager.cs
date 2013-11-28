using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Hazelcast.Client;
using Hazelcast.Client.Proxy;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Client.Spi
{
	
	public sealed class ProxyManager
	{
		private static readonly ILogger logger = Logger.GetLogger(typeof(ProxyManager));

		private readonly HazelcastClient client;

        private readonly ConcurrentDictionary<string, ClientProxyFactory> proxyFactories = new ConcurrentDictionary<string, ClientProxyFactory>();

        private readonly ConcurrentDictionary<ObjectNamespace, ClientProxy> proxies = new ConcurrentDictionary<ObjectNamespace, ClientProxy>();

        private readonly ConcurrentDictionary<string, ListenerSupport<EventData>> listeners = new ConcurrentDictionary<string, ListenerSupport<EventData>>();

		public ProxyManager(HazelcastClient client)
		{
			//import com.hazelcast.client.proxy.ClientExecutorServiceProxy;
			this.client = client;
			System.Collections.Generic.IList<ListenerConfig> listenerConfigs = client.GetClientConfig().GetListenerConfigs();
			if (listenerConfigs != null && listenerConfigs.Count > 0)
			{
				foreach (var listenerConfig in listenerConfigs.Where(listenerConfig => listenerConfig.GetImplementation() is IDistributedObjectListener))
				{
				    AddDistributedObjectListener((IDistributedObjectListener)listenerConfig.GetImplementation());
				}
			}
		}

		public void Init(ClientConfig config)
		{
			// register defaults
            Register(ServiceNames.Map,(type, id) => ProxyFactory(typeof(ClientMapProxy<,>),type,ServiceNames.Map,id)  );
            Register(ServiceNames.Queue, (type, id) => ProxyFactory(typeof(ClientQueueProxy<>), type, ServiceNames.Queue, id));
            Register(ServiceNames.MultiMap, (type, id) => ProxyFactory(typeof(ClientMultiMapProxy<,>), type, ServiceNames.MultiMap, id));
            Register(ServiceNames.List,(type, id) => ProxyFactory(typeof(ClientListProxy<>),type,ServiceNames.List,id)  );
            Register(ServiceNames.Set, (type, id) => ProxyFactory(typeof(ClientSetProxy<>), type, ServiceNames.Set, id));
            Register(ServiceNames.Topic, (type, id) => ProxyFactory(typeof(ClientTopicProxy<>), type, ServiceNames.Topic, id));
            Register(ServiceNames.AtomicLong, (type, id) => ProxyFactory(typeof(ClientAtomicLongProxy), type, ServiceNames.AtomicLong, id));
            Register(ServiceNames.Lock, (type, id) => ProxyFactory(typeof(ClientLockProxy), type, ServiceNames.Lock, id));
            Register(ServiceNames.CountDownLatch, (type, id) => ProxyFactory(typeof(ClientCountDownLatchProxy), type, ServiceNames.CountDownLatch, id));
            Register(ServiceNames.Semaphore, (type, id) => ProxyFactory(typeof(ClientSemaphoreProxy), type, ServiceNames.Semaphore, id));

            Register(ServiceNames.IdGenerator, delegate(Type type, string id)
            {
                IAtomicLong atomicLong = client.GetAtomicLong("IdGeneratorService.ATOMIC_LONG_NAME" + id);
                return Activator.CreateInstance(typeof(ClientIdGeneratorProxy), new object[] { ServiceNames.IdGenerator, id, atomicLong }) as ClientProxy;
            });


            //TODO EXECUTOR

			foreach (ProxyFactoryConfig proxyFactoryConfig in config.GetProxyFactoryConfigs())
			{
				try
				{
					ClientProxyFactory clientProxyFactory = null;
                    var type = Type.GetType(proxyFactoryConfig.GetClassName());
                    if (type != null )
				    {
                        clientProxyFactory = (ClientProxyFactory) Activator.CreateInstance(type);
				    }
					Register(proxyFactoryConfig.GetService(), clientProxyFactory);
				}
				catch (Exception e)
				{
					logger.Severe(e);
				}
			}
		}

	    internal static ClientProxy ProxyFactory(Type proxyType,Type interfaceType,string name, string id)
	    {
            if (proxyType.ContainsGenericParameters)
	        {
                //Type[] genericTypeArguments = interfaceType.GenericTypeArguments;
	            Type[] genericTypeArguments = interfaceType.GetGenericArguments();
	            var mgType = proxyType.MakeGenericType(genericTypeArguments);
                return Activator.CreateInstance(mgType,new object[]{name,id}) as ClientProxy;
	        }
	        return Activator.CreateInstance(proxyType, new object[] { name, id }) as ClientProxy;
	    }

	    public void Register(string serviceName, ClientProxyFactory factory)
		{
			if (proxyFactories.ContainsKey(serviceName) )
			{
				throw new ArgumentException("Factory for service: " + serviceName + " is already registered!");
			}
		    proxyFactories.GetOrAdd(serviceName, factory);
		}

        //public ClientProxy GetProxy(string service, string id)
        //{
        //    var ns = new ObjectNamespace(service, id);
        //    ClientProxy proxy = null;
        //    proxies.TryGetValue(ns,out proxy);
        //    if (proxy != null)
        //    {
        //        return proxy;
        //    }
        //    ClientProxyFactory factory = null;
            
        //    proxyFactories.TryGetValue(service,out factory);
        //    if (factory == null)
        //    {
        //        throw new ArgumentException("No factory registered for service: " + service);
        //    }
        //    ClientProxy clientProxy = factory(id);
        //    Initialize(clientProxy);
        //    return proxies.GetOrAdd(ns, clientProxy);
        //}
        public ClientProxy GetOrCreateProxy<T>(string service, string id)
        {
            var ns = new ObjectNamespace(service, id);
            ClientProxy proxy = null;
            proxies.TryGetValue(ns, out proxy);
            if (proxy != null)
            {
                return proxy;
            }
            ClientProxyFactory factory = null;

            proxyFactories.TryGetValue(service, out factory);
            if (factory == null)
            {
                throw new ArgumentException("No factory registered for service: " + service);
            }
            ClientProxy clientProxy = factory(typeof(T),id);
            Initialize(clientProxy);
            return proxies.GetOrAdd(ns, clientProxy);
        }
        public ClientProxy GetProxy(string service, string id)
        {
            var ns = new ObjectNamespace(service, id);
            ClientProxy proxy = null;
            proxies.TryGetValue(ns, out proxy);
            if (proxy != null)
            {
                return proxy;
            }
            return null;
        }

		public ClientProxy RemoveProxy(string service, string id)
		{
			var ns = new ObjectNamespace(service, id);
		    ClientProxy removed;
		    proxies.TryRemove(ns, out removed);
            return removed;
            
		}

		private void Initialize(ClientProxy clientProxy)
		{
			var request = new ClientCreateRequest(clientProxy.GetName(), clientProxy.GetServiceName());
			try
			{
				client.GetInvocationService().InvokeOnRandomTarget<object>(request);
			}
			catch (Exception e)
			{
				ExceptionUtil.Rethrow(e);
			}
			clientProxy.SetContext(new ClientContext(client.GetSerializationService(), client.GetClientClusterService(), client.GetClientPartitionService(), client.GetInvocationService(), client.GetClientExecutionService(), this, client.GetClientConfig()));
		}

		public System.Collections.Generic.ICollection<T> GetDistributedObjects<T>() where T:IDistributedObject
		{
		    var lst = new ReadOnlyCollection<ClientProxy>(proxies.Values.ToList());

            return lst as System.Collections.Generic.ICollection<T>;
		}

		public void Destroy()
		{
			proxies.Clear();
			listeners.Clear();
		}

		public string AddDistributedObjectListener(IDistributedObjectListener listener)
		{
			DistributedObjectListenerRequest request = new DistributedObjectListenerRequest();
			ClientContext context = new ClientContext(client.GetSerializationService(), client.GetClientClusterService(), client.GetClientPartitionService(), client.GetInvocationService(), client.GetClientExecutionService(), this, client.GetClientConfig());
            //EventHandler<PortableDistributedObjectEvent> eventHandler = new _EventHandler_211(this, listener);

            EventHandler<EventData> eventHandler = delegate(object eventSource, EventData eventArgs)
		    {
                if (eventArgs != null)
		        {
                    var _event = eventArgs.Data;
		            var e = (PortableDistributedObjectEvent) _event;
                    var ns = new ObjectNamespace(e.GetServiceName(), e.GetName());
		            ClientProxy proxy = null;
                    proxies.TryGetValue(ns,out proxy);
                    if (proxy == null)
                    {
                        proxy = GetProxy(e.GetServiceName(), e.GetName());
                    }
                    _event = new DistributedObjectEvent(e.GetEventType(), e.GetServiceName(), proxy);
                    if (DistributedObjectEvent.EventType.Created.Equals(e.GetEventType()))
                    {
                        listener.DistributedObjectCreated((DistributedObjectEvent) _event);
                    }
                    else
                    {
                        if (DistributedObjectEvent.EventType.Destroyed.Equals(e.GetEventType()))
                        {
                            listener.DistributedObjectDestroyed((DistributedObjectEvent) _event);
                        }
                    }   
		        }
		    };
            ListenerSupport<EventData> listenerSupport = new ListenerSupport<EventData>(context, request, eventHandler, null);
			string registrationId = listenerSupport.Listen();
			listeners.TryAdd(registrationId, listenerSupport);
			return registrationId;
		}



		public bool RemoveDistributedObjectListener(string id)
		{
            ListenerSupport<EventData> listenerSupport;
            listeners.TryRemove(id, out listenerSupport);
			if (listenerSupport != null)
			{
				listenerSupport.Stop();
				return true;
			}
			return false;
		}
	}
}
