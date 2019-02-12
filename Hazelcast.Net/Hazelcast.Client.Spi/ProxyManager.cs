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

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    internal sealed class ProxyManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ProxyManager));
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<DistributedObjectInfo, ClientProxy> _proxies =
            new ConcurrentDictionary<DistributedObjectInfo, ClientProxy>();

        private readonly ConcurrentDictionary<string, ClientProxyFactory> _proxyFactories =
            new ConcurrentDictionary<string, ClientProxyFactory>();

        public ProxyManager(HazelcastClient client)
        {
            _client = client;
            var listenerConfigs = client.GetClientConfig().GetListenerConfigs();
            if (listenerConfigs != null && listenerConfigs.Count > 0)
            {
                foreach (var listenerConfig in listenerConfigs.Where(listenerConfig =>
                    listenerConfig.GetImplementation() is IDistributedObjectListener))
                {
                    AddDistributedObjectListener((IDistributedObjectListener) listenerConfig.GetImplementation());
                }
            }
        }

        public string AddDistributedObjectListener(IDistributedObjectListener listener)
        {
            var isSmart = _client.GetClientConfig().GetNetworkConfig().IsSmartRouting();
            var request = ClientAddDistributedObjectListenerCodec.EncodeRequest(isSmart);
            var context = new ClientContext(_client.GetSerializationService(), _client.GetClientClusterService(),
                _client.GetClientPartitionService(), _client.GetInvocationService(), _client.GetClientExecutionService(),
                _client.GetListenerService(), _client.GetNearCacheManager(), this, _client.GetClientConfig());
            DistributedEventHandler eventHandler = delegate(IClientMessage message)
            {
                ClientAddDistributedObjectListenerCodec.EventHandler.HandleEvent(message, (name, serviceName, eventType) =>
                {
                    var _event = new LazyDistributedObjectEvent(eventType, serviceName, name, this);
                    switch (eventType)
                    {
                        case DistributedObjectEvent.EventType.Created:
                            listener.DistributedObjectCreated(_event);
                            break;
                        case DistributedObjectEvent.EventType.Destroyed:
                            listener.DistributedObjectDestroyed(_event);
                            break;
                        default:
                            Logger.Warning(string.Format("Undefined DistributedObjectListener event type received: {0} !!!",
                                eventType));
                            break;
                    }
                });
            };
            return context.GetListenerService().RegisterListener(request,
                m => ClientAddDistributedObjectListenerCodec.DecodeResponse(m).response,
                ClientRemoveDistributedObjectListenerCodec.EncodeRequest, eventHandler);
        }

        public void Shutdown()
        {
            foreach (var proxy in _proxies)
            {
                proxy.Value.OnShutdown();
            }
            _proxies.Clear();
        }

        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            try
            {
                var request = ClientGetDistributedObjectsCodec.EncodeRequest();
                var task = _client.GetInvocationService().InvokeOnRandomTarget(request);
                var response = ThreadUtil.GetResult(task);
                var result = ClientGetDistributedObjectsCodec.DecodeResponse(response).response;
                foreach (var distributedObjectInfo in result)
                {
                    var proxy = InitProxyLocal(distributedObjectInfo.ServiceName, distributedObjectInfo.ObjectName,
                        typeof(IDistributedObject));
                    _proxies.TryAdd(distributedObjectInfo, proxy);
                }

                return GetLocalDistributedObjects();
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        private ICollection<IDistributedObject> GetLocalDistributedObjects()
        {
            return new ReadOnlyCollection<IDistributedObject>(_proxies.Values.ToList<IDistributedObject>());
        }

        private ClientProxy InitProxyLocal(string service, string id, Type requestedInterface)
        {
            var proxy = MakeProxy(service, id, requestedInterface);
            proxy.SetContext(new ClientContext(_client.GetSerializationService(), _client.GetClientClusterService(),
                _client.GetClientPartitionService(), _client.GetInvocationService(), _client.GetClientExecutionService(),
                _client.GetListenerService(), _client.GetNearCacheManager(), this, _client.GetClientConfig()));
            proxy.Init();
            return proxy;
        }

        public ClientProxy GetOrCreateProxy<T>(string service, string id) where T : IDistributedObject
        {
            var objectInfo = new DistributedObjectInfo(service, id);
            var requestedInterface = typeof(T);
            var clientProxy = _proxies.GetOrAdd(objectInfo, distributedObjectInfo =>
            {
                // create a new proxy, which needs initialization on server.
                var proxy = InitProxyLocal(service, id, requestedInterface);
                InitializeWithRetry(proxy);
                return proxy;
            });

            // only return the existing proxy, if the requested type args match
            var proxyInterface = clientProxy.GetType().GetInterface(requestedInterface.Name);
            var proxyArgs = proxyInterface.GetGenericArguments();
            var requestedArgs = requestedInterface.GetGenericArguments();
            if (proxyArgs.SequenceEqual(requestedArgs))
            {
                // the proxy we found matches what we were looking for
                return clientProxy;
            }

            var isAssignable = true;
            for (int i = 0; i < proxyArgs.Length; i++)
            {
                if (!proxyArgs[i].IsAssignableFrom(requestedArgs[i]))
                {
                    isAssignable = false;
                    break;
                }
            }

            if (isAssignable)
            {
                _proxies.TryRemove(objectInfo, out clientProxy);
                return GetOrCreateProxy<T>(service, id);
            }

            throw new InvalidCastException(string.Format("Distributed object is already created with incompatible types [{0}]",
                string.Join(", ", (object[]) proxyArgs)));
        }

        private ClientProxy MakeProxy(string service, string id, Type requestedInterface)
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

        public void Init(ClientConfig config)
        {
            // register defaults
            Register(ServiceNames.Map, (type, id) =>
            {
                var clientConfig = _client.GetClientConfig();
                var nearCacheConfig = clientConfig.GetNearCacheConfig(id);
                var proxyType = nearCacheConfig != null ? typeof(ClientMapNearCacheProxy<,>) : typeof(ClientMapProxy<,>);
                return ProxyFactory(proxyType, type, ServiceNames.Map, id);
            });
            Register(ServiceNames.Queue,
                (type, id) => ProxyFactory(typeof(ClientQueueProxy<>), type, ServiceNames.Queue, id));
            Register(ServiceNames.MultiMap,
                (type, id) => ProxyFactory(typeof(ClientMultiMapProxy<,>), type, ServiceNames.MultiMap, id));
            Register(ServiceNames.List,
                (type, id) => ProxyFactory(typeof(ClientListProxy<>), type, ServiceNames.List, id));
            Register(ServiceNames.Set,
                (type, id) => ProxyFactory(typeof(ClientSetProxy<>), type, ServiceNames.Set, id));
            Register(ServiceNames.Topic,
                (type, id) => ProxyFactory(typeof(ClientTopicProxy<>), type, ServiceNames.Topic, id));
            Register(ServiceNames.AtomicLong,
                (type, id) => ProxyFactory(typeof(ClientAtomicLongProxy), type, ServiceNames.AtomicLong, id));
            Register(ServiceNames.Lock, (type, id) => ProxyFactory(typeof(ClientLockProxy), type, ServiceNames.Lock, id));
            Register(ServiceNames.CountDownLatch,
                (type, id) => ProxyFactory(typeof(ClientCountDownLatchProxy), type, ServiceNames.CountDownLatch, id));
            Register(ServiceNames.PNCounter,
               (type, id) => ProxyFactory(typeof(ClientPNCounterProxy), type, ServiceNames.PNCounter, id));
            Register(ServiceNames.Semaphore,
                (type, id) => ProxyFactory(typeof(ClientSemaphoreProxy), type, ServiceNames.Semaphore, id));
            Register(ServiceNames.Ringbuffer,
                (type, id) => ProxyFactory(typeof(ClientRingbufferProxy<>), type, ServiceNames.Ringbuffer, id));
            Register(ServiceNames.ReplicatedMap,
                (type, id) => ProxyFactory(typeof(ClientReplicatedMapProxy<,>), type, ServiceNames.ReplicatedMap, id));

            Register(ServiceNames.IdGenerator, delegate(Type type, string id)
            {
                var atomicLong = _client.GetAtomicLong("IdGeneratorService.ATOMIC_LONG_NAME" + id);
                return Activator.CreateInstance(typeof(ClientIdGeneratorProxy), ServiceNames.IdGenerator, id, atomicLong) as
                    ClientProxy;
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
                _client.GetListenerService(), _client.GetNearCacheManager(), this, _client.GetClientConfig());
            return context.GetListenerService().DeregisterListener(id);
        }

        public ClientProxy RemoveProxy(string service, string id)
        {
            var ns = new DistributedObjectInfo(service, id);
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
                types[i] = typeof(object);
            }

            return proxyType.MakeGenericType(types);
        }

        private void InitializeOnServer(ClientProxy clientProxy)
        {
            var initializationTarget = FindNextAddressToCreateARequest();
            var invocationTarget = initializationTarget;
            if (initializationTarget != null && _client.GetConnectionManager().GetConnection(initializationTarget) == null)
            {
                invocationTarget = _client.GetClientClusterService().GetOwnerConnectionAddress();
            }

            if (invocationTarget == null)
            {
                throw new IOException("Not able to setup owner connection!");
            }

            var request =
                ClientCreateProxyCodec.EncodeRequest(clientProxy.GetName(), clientProxy.GetServiceName(), initializationTarget);
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
            return exception is RetryableHazelcastException || exception is IOException || exception is AuthenticationException ||
                   exception is HazelcastInstanceNotActiveException;
        }
    }
}