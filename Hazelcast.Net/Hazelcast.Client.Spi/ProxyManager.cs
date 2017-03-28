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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal sealed class ProxyManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ProxyManager));
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<ObjectNamespace, ClientProxy> _proxies =
            new ConcurrentDictionary<ObjectNamespace, ClientProxy>();

        private readonly ConcurrentDictionary<string, ClientProxyFactory> _proxyFactories =
            new ConcurrentDictionary<string, ClientProxyFactory>();

        public ProxyManager(HazelcastClient client)
        {
            _client = client;
            var listenerConfigs = client.GetClientConfig().GetListenerConfigs();
            if (listenerConfigs != null && listenerConfigs.Count > 0)
            {
                foreach (
                    var listenerConfig in
                        listenerConfigs.Where(
                            listenerConfig => listenerConfig.GetImplementation() is IDistributedObjectListener))
                {
                    AddDistributedObjectListener((IDistributedObjectListener) listenerConfig.GetImplementation());
                }
            }
        }

        public string AddDistributedObjectListener(IDistributedObjectListener listener)
        {
            var request = ClientAddDistributedObjectListenerCodec.EncodeRequest(false);
            var context = new ClientContext(_client.GetSerializationService(), _client.GetClientClusterService(),
                _client.GetClientPartitionService(), _client.GetInvocationService(), _client.GetClientExecutionService(),
                _client.GetListenerService(),
                this, _client.GetClientConfig());
            //EventHandler<PortableDistributedObjectEvent> eventHandler = new _EventHandler_211(this, listener);

            DistributedEventHandler eventHandler = delegate(IClientMessage message)
            {
                ClientAddDistributedObjectListenerCodec.AbstractEventHandler.Handle(message,
                    (name, serviceName, type) =>
                    {
                        var ns = new ObjectNamespace(serviceName, name);
                        ClientProxy proxy;
                        _proxies.TryGetValue(ns, out proxy);
                        if (proxy == null)
                        {
                            proxy = GetProxy(serviceName, name);
                        }
                        var _event = new DistributedObjectEvent(type, serviceName, proxy);
                        if (DistributedObjectEvent.EventType.Created.Equals(type))
                        {
                            listener.DistributedObjectCreated(_event);
                        }
                        else
                        {
                            if (DistributedObjectEvent.EventType.Destroyed.Equals(type))
                            {
                                listener.DistributedObjectDestroyed(_event);
                            }
                        }
                    });
            };
            //PortableDistributedObjectEvent
            return context.GetListenerService()
                .StartListening(request, eventHandler,
                    m => ClientAddDistributedObjectListenerCodec.DecodeResponse(m).response);
        }

        public void Destroy()
        {
            _proxies.Clear();
        }

        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            return new ReadOnlyCollection<IDistributedObject>(_proxies.Values.ToList<IDistributedObject>());
        }

        public ClientProxy GetOrCreateProxy<T>(string service, string id) where T : IDistributedObject
        {
            var ns = new ObjectNamespace(service, id);
            ClientProxy proxy;
            _proxies.TryGetValue(ns, out proxy);
            var requestedInterface = typeof (T);
            if (proxy != null)
            {
                // only return the existing proxy, if the requested type args match
                var proxyInterface = proxy.GetType().GetInterface(requestedInterface.Name);
                var proxyArgs = proxyInterface.GetGenericArguments();
                var requestedArgs = requestedInterface.GetGenericArguments();
                if (proxyArgs.SequenceEqual(requestedArgs))
                {
                    // the proxy we found matches what we were looking for
                    return proxy;
                }
                
                // create a new proxy, which matches the interface requested
                proxy = makeProxy<T>(service, id, requestedInterface);
            }
            else
            {
                // create a new proxy, which needs initialization on server.
                proxy = makeProxy<T>(service, id, requestedInterface);
                InitializeWithRetry(proxy);
            }

            proxy.SetContext(new ClientContext(_client.GetSerializationService(),
                _client.GetClientClusterService(),
                _client.GetClientPartitionService(), _client.GetInvocationService(), _client.GetClientExecutionService(),
                _client.GetListenerService(),
                this, _client.GetClientConfig()));
            proxy.PostInit();

            _proxies.AddOrUpdate(ns, n => proxy, (n, oldProxy) => {
                Logger.Warning("Replacing old proxy for " + oldProxy.GetName() + " of type " + oldProxy.GetType() + " with " + proxy.GetType());
                return proxy;
            });
            return proxy;
        }

        private ClientProxy makeProxy<T>(string service, string id, Type requestedInterface)
        {
            ClientProxyFactory factory;
            _proxyFactories.TryGetValue(service, out factory);
            if (factory == null)
            {
                throw new ArgumentException("No factory registered for service: " + service);
            }
            var clientProxy = factory(requestedInterface, id);
            return clientProxy;
        }

        public ClientProxy GetProxy(string service, string id)
        {
            var ns = new ObjectNamespace(service, id);
            ClientProxy proxy;
            _proxies.TryGetValue(ns, out proxy);
            if (proxy != null)
            {
                return proxy;
            }
            return null;
        }

        public void Init(ClientConfig config)
        {
            // register defaults
            Register(ServiceNames.Map,
                (type, id) => ProxyFactory(typeof (ClientMapProxy<,>), type, ServiceNames.Map, id));
            Register(ServiceNames.Queue,
                (type, id) => ProxyFactory(typeof (ClientQueueProxy<>), type, ServiceNames.Queue, id));
            Register(ServiceNames.MultiMap,
                (type, id) => ProxyFactory(typeof (ClientMultiMapProxy<,>), type, ServiceNames.MultiMap, id));
            Register(ServiceNames.List,
                (type, id) => ProxyFactory(typeof (ClientListProxy<>), type, ServiceNames.List, id));
            Register(ServiceNames.Set, (type, id) => ProxyFactory(typeof (ClientSetProxy<>), type, ServiceNames.Set, id));
            Register(ServiceNames.Topic,
                (type, id) => ProxyFactory(typeof (ClientTopicProxy<>), type, ServiceNames.Topic, id));
            Register(ServiceNames.AtomicLong,
                (type, id) => ProxyFactory(typeof (ClientAtomicLongProxy), type, ServiceNames.AtomicLong, id));
            Register(ServiceNames.Lock,
                (type, id) => ProxyFactory(typeof (ClientLockProxy), type, ServiceNames.Lock, id));
            Register(ServiceNames.CountDownLatch,
                (type, id) => ProxyFactory(typeof (ClientCountDownLatchProxy), type, ServiceNames.CountDownLatch, id));
            Register(ServiceNames.Semaphore,
                (type, id) => ProxyFactory(typeof (ClientSemaphoreProxy), type, ServiceNames.Semaphore, id));
            Register(ServiceNames.Ringbuffer,
                (type, id) => ProxyFactory(typeof (ClientRingbufferProxy<>), type, ServiceNames.Ringbuffer, id));

            Register(ServiceNames.IdGenerator, delegate(Type type, string id)
            {
                var atomicLong = _client.GetAtomicLong("IdGeneratorService.ATOMIC_LONG_NAME" + id);
                return
                    Activator.CreateInstance(typeof (ClientIdGeneratorProxy), ServiceNames.IdGenerator, id, atomicLong)
                        as ClientProxy;
            });


            foreach (var proxyFactoryConfig in config.GetProxyFactoryConfigs())
            {
                try
                {
                    ClientProxyFactory clientProxyFactory = null;
                    var type = Type.GetType(proxyFactoryConfig.GetClassName());
                    if (type != null)
                    {
                        clientProxyFactory = (ClientProxyFactory) Activator.CreateInstance(type);
                    }
                    Register(proxyFactoryConfig.GetService(), clientProxyFactory);
                }
                catch (Exception e)
                {
                    Logger.Severe(e);
                }
            }
        }

        public void Register(string serviceName, ClientProxyFactory factory)
        {
            if (_proxyFactories.ContainsKey(serviceName))
            {
                throw new ArgumentException("Factory for service: " + serviceName + " is already registered!");
            }
            _proxyFactories.GetOrAdd(serviceName, factory);
        }

        public bool RemoveDistributedObjectListener(string id)
        {
            var context = new ClientContext(_client.GetSerializationService(), _client.GetClientClusterService(),
                _client.GetClientPartitionService(), _client.GetInvocationService(), _client.GetClientExecutionService(),
                _client.GetListenerService(), this, _client.GetClientConfig());
            return context.GetListenerService().StopListening(
                ClientRemoveDistributedObjectListenerCodec.EncodeRequest,
                m => ClientRemoveDistributedObjectListenerCodec.DecodeResponse(m).response, id);
        }

        public ClientProxy RemoveProxy(string service, string id)
        {
            var ns = new ObjectNamespace(service, id);
            ClientProxy removed;
            _proxies.TryRemove(ns, out removed);
            return removed;
        }

        internal static ClientProxy ProxyFactory(Type proxyType, Type interfaceType, string name, string id)
        {
            if (proxyType.ContainsGenericParameters)
            {
                var typeWithParams = GetTypeWithParameters(proxyType, interfaceType);
                return Activator.CreateInstance(typeWithParams, name, id) as ClientProxy;
            }
            return Activator.CreateInstance(proxyType, name, id) as ClientProxy;
        }

        public HazelcastClient GetHazelcastInstance()
        {
            return _client;
        }
        private Address FindNextAddressToCreateARequest()
        {
            var clusterSize = _client.GetClientClusterService().GetSize();
            IMember liteMember = null;

            var loadBalancer = _client.GetLoadBalancer();
            for (var i = 0; i < clusterSize; i++)
            {
                var member = loadBalancer.Next();
                if (member != null && !member.IsLiteMember)
                {
                    return member.GetAddress();
                }
                if (liteMember == null)
                {
                    liteMember = member;
                }
            }
            return liteMember != null ? liteMember.GetAddress() : null;
        }

        private static Type GetTypeWithParameters(Type proxyType, Type interfaceType)
        {
            var genericTypeArguments = interfaceType.GetGenericArguments();
            if (genericTypeArguments.Length == proxyType.GetGenericArguments().Length)
            {
                return proxyType.MakeGenericType(genericTypeArguments);
            }
            var types = new Type[proxyType.GetGenericArguments().Length];
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = typeof (object);
            }
            return proxyType.MakeGenericType(types);
        }

        private void InitializeOnServer(ClientProxy clientProxy)
        {
            var initializationTarget = FindNextAddressToCreateARequest();
            var invocationTarget = initializationTarget;
            if (initializationTarget != null &&
                _client.GetConnectionManager().GetConnection(initializationTarget) == null)
            {
                invocationTarget = _client.GetClientClusterService().GetOwnerConnectionAddress();
            }

            if (invocationTarget == null)
            {
                throw new IOException("Not able to setup owner connection!");
            }

            var request = ClientCreateProxyCodec.EncodeRequest(clientProxy.GetName(), clientProxy.GetServiceName(),
                initializationTarget);
            try
            {
                ThreadUtil.GetResult(_client.GetInvocationService().InvokeOnTarget(request, invocationTarget));
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        private void InitializeWithRetry(ClientProxy clientProxy)
        {
            var clientInvocationService = (ClientInvocationService) _client.GetInvocationService();
            long retryCountLimit = clientInvocationService.InvocationRetryCount;
            for (var retryCount = 0; retryCount < retryCountLimit; retryCount++)
            {
                try
                {
                    InitializeOnServer(clientProxy);
                    return;
                }
                catch (Exception e)
                {
                    Logger.Warning("Got error initializing proxy", e);
                    if (IsRetryable(e))
                    {
                        try
                        {
                            Thread.Sleep(clientInvocationService.InvocationRetryWaitTime);
                        }
                        catch (ThreadInterruptedException)
                        {
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private bool IsRetryable(Exception exception)
        {
            return exception is RetryableHazelcastException || exception is IOException
                   || exception is AuthenticationException || exception is HazelcastInstanceNotActiveException;
        }
    }
}