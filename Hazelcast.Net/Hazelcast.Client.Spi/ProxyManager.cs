// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Util;
using static Hazelcast.Core.ServiceNames;
using static Hazelcast.Util.ValidationUtil;

namespace Hazelcast.Client.Spi
{
    internal sealed class ProxyManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ProxyManager));
        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<DistributedObjectInfo, Lazy<ClientProxy>> _proxies =
            new ConcurrentDictionary<DistributedObjectInfo, Lazy<ClientProxy>>();

        private readonly ConcurrentDictionary<string, Func<string, Type, ClientProxy>> _proxyFactories =
            new ConcurrentDictionary<string, Func<string, Type, ClientProxy>>();


        public ProxyManager(HazelcastClient client)
        {
            _client = client;
        }

        public void Init(ClientConfig config)
        {
            // register defaults
            Register(ServiceNames.Map, CreateClientMapProxyFactory);
            Register(Queue, typeof(ClientQueueProxy<>));
            Register(MultiMap, typeof(ClientMultiMapProxy<,>));
            Register(List, typeof(ClientListProxy<>));
            Register(Set, typeof(ClientSetProxy<>));
            Register(Topic, typeof(ClientTopicProxy<>));
//            Register(ServiceNames.PNCounter, typeof(ClientPNCounterProxy));
            Register(Ringbuffer, typeof(ClientRingbufferProxy<>));
            Register(ReplicatedMap, typeof(ClientReplicatedMapProxy<,>));
        }

        private ClientProxy CreateClientMapProxyFactory(string id, Type type)
        {
            var clientConfig = _client.ClientConfig;
            var nearCacheConfig = clientConfig.GetNearCacheConfig(id);
            var proxyType = nearCacheConfig != null ? typeof(ClientMapNearCacheProxy<,>) : typeof(ClientMapProxy<,>);
            return InstantiateClientProxy(proxyType, type, ServiceNames.Map, id, _client);
        }

        public Guid AddDistributedObjectListener(IDistributedObjectListener listener)
        {
            var listenerService = _client.ListenerService;
            var request = ClientAddDistributedObjectListenerCodec.EncodeRequest(listenerService.RegisterLocalOnly);

            void HandleDistributedObjectEvent(string name, string serviceName, string eventTypeName, Guid source)
            {
                var ns = new DistributedObjectInfo(serviceName, name);
                _proxies.TryGetValue(ns, out var lazyProxy);
                // ClientProxy proxy = future == null ? null : future.get();
                var eventType =
                    (DistributedObjectEvent.DistributedEventType) Enum.Parse(typeof(DistributedObjectEvent.DistributedEventType),
                        eventTypeName);
                var _event = new DistributedObjectEvent(eventType, serviceName, name, lazyProxy.Value, source, this);
                switch (eventType)
                {
                    case DistributedObjectEvent.DistributedEventType.CREATED:
                        listener.DistributedObjectCreated(_event);
                        break;
                    case DistributedObjectEvent.DistributedEventType.DESTROYED:
                        listener.DistributedObjectDestroyed(_event);
                        break;
                    default:
                        Logger.Warning($"Undefined DistributedObjectListener event type received: {eventType} !!!");
                        break;
                }
            }

            void EventHandler(ClientMessage message) =>
                ClientAddDistributedObjectListenerCodec.EventHandler.HandleEvent(message, HandleDistributedObjectEvent);

            return listenerService.RegisterListener(request,
                m => ClientAddDistributedObjectListenerCodec.DecodeResponse(m).Response,
                ClientRemoveDistributedObjectListenerCodec.EncodeRequest, EventHandler);
        }

        public bool RemoveDistributedObjectListener(Guid id)
        {
            return _client.ListenerService.DeregisterListener(id);
        }

        private void Register(string serviceName, Type proxyType)
        {
            try
            {
                Register(serviceName, (id, type) => InstantiateClientProxy(proxyType, type, Queue, id, _client));
            }
            catch (Exception e)
            {
                throw new HazelcastException($"Factory for service: {serviceName} could not be created for {proxyType}", e);
            }
        }

        private void Register(string serviceName, Func<string, Type, ClientProxy> factory)
        {
            if (!_proxyFactories.TryAdd(serviceName, factory))
            {
                throw new ArgumentException($"Factory for service: {serviceName} is already registered!");
            }
        }

        private static ClientProxy InstantiateClientProxy(Type proxyType, Type interfaceType, string name, string id,
            HazelcastClient client)
        {
            if (proxyType.ContainsGenericParameters)
            {
                var typeWithParams = GetTypeWithParameters(proxyType, interfaceType);
                return Activator.CreateInstance(typeWithParams, name, id, client) as ClientProxy;
            }

            return Activator.CreateInstance(proxyType, name, id) as ClientProxy;
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

        public ICollection<IDistributedObject> GetDistributedObjects()
        {
            try
            {
                var request = ClientGetDistributedObjectsCodec.EncodeRequest();
                var task = _client.InvocationService.InvokeOnRandomTarget(request);

                var localDistributedObjects = new HashSet<DistributedObjectInfo>();
                var distributedObjects = GetLocalDistributedObjects();
                foreach (var localInfo in distributedObjects)
                {
                    localDistributedObjects.Add(new DistributedObjectInfo(localInfo.ServiceName, localInfo.Name));
                }

                var response = ThreadUtil.GetResult(task);
                var newDistributedObjectInfo = ClientGetDistributedObjectsCodec.DecodeResponse(response).Response;
                foreach (var distributedObjectInfo in newDistributedObjectInfo)
                {
                    localDistributedObjects.Remove(distributedObjectInfo);
                    GetOrCreateLocalProxy<IDistributedObject>(distributedObjectInfo.ServiceName, distributedObjectInfo.Name);
                }

                foreach (var distributedObjectInfo in localDistributedObjects)
                {
                    DestroyProxyLocally(distributedObjectInfo.ServiceName, distributedObjectInfo.Name);
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
            return new ReadOnlyCollection<IDistributedObject>(_proxies.Values.Select(lazy => lazy.Value)
                .ToList<IDistributedObject>());
        }

        public ClientProxy GetOrCreateProxy<T>(string service, string id)
        {
            return GetOrCreateProxyInternal<T>(service, id, true);
        }

        public ClientProxy GetOrCreateLocalProxy<T>(string service, string id)
        {
            return GetOrCreateProxyInternal<T>(service, id, false);
        }

        private ClientProxy GetOrCreateProxyInternal<T>(string service, string id, bool remote)
        {
            CheckNotNull(service, "Service name is required!");
            CheckNotNull(id, "Object name is required!");
            var requestedInterface = typeof(T);
            var ns = new DistributedObjectInfo(service, id);
            var lazy = _proxies.GetOrAdd(ns, new Lazy<ClientProxy>(() =>
            {
                if (!_proxyFactories.TryGetValue(service, out var factory))
                {
                    new ArgumentException($"No factory registered for service: {service}");
                }
                try
                {
                    var _clientProxy = factory(id, requestedInterface);
                    if (remote)
                    {
                        Initialize(_clientProxy);
                    }
                    _clientProxy.OnInitialize();
                    return _clientProxy;
                }
                catch (Exception e)
                {
                    throw ExceptionUtil.Rethrow(e);
                }
            }, LazyThreadSafetyMode.ExecutionAndPublication));

            ClientProxy clientProxy;
            try
            {
                clientProxy = lazy.Value;
            }
            catch (Exception e)
            {
                _proxies.TryRemove(ns, out _);
                throw ExceptionUtil.Rethrow(e);
            }

            // only return the existing proxy, if the requested type args match
            var proxyInterface = clientProxy.GetType().GetInterface(requestedInterface.Name);
            var proxyArgs = proxyInterface.GetGenericArguments();
            var requestedArgs = requestedInterface.GetGenericArguments();
            if (proxyArgs.SequenceEqual(requestedArgs))
            {
                // the proxy we found matches what we were looking for
                return clientProxy;
            }
            //TODO implement support for multiple generic parameters            
            throw new InvalidCastException(
                $"Distributed object is already created with incompatible types [{string.Join(", ", (object[]) proxyArgs)}]");
        }

        private void Initialize(ClientProxy clientProxy)
        {
            var request = ClientCreateProxyCodec.EncodeRequest(clientProxy.Name, clientProxy.ServiceName);
            ThreadUtil.GetResult(_client.InvocationService.InvokeOnRandomTarget(request));
        }

        public void DestroyProxy(ClientProxy clientProxy)
        {
            var ns = new DistributedObjectInfo(clientProxy.Name, clientProxy.ServiceName);
            if (_proxies.TryRemove(ns, out var registeredProxyLazy))
            {
                ClientProxy registeredProxy = null;
                try
                {
                    registeredProxy = registeredProxyLazy.Value;
                    if (registeredProxy != null)
                    {
                        try
                        {
                            registeredProxy.DestroyLocally();
                        }
                        finally
                        {
                            registeredProxy.DestroyRemotely();
                        }
                    }
                }
                finally
                {
                    if (clientProxy != registeredProxy)
                    {
                        // The given proxy is stale and was already destroyed, but the caller
                        // may have allocated local resources in the context of this stale proxy
                        // instance after it was destroyed, so we have to cleanup it locally one
                        // more time to make sure there are no leaking local resources.
                        clientProxy.DestroyLocally();
                    }
                }
            }
        }

        public void DestroyProxyLocally(string service, string id)
        {
            var objectNamespace = new DistributedObjectInfo(service, id);
            if (_proxies.TryRemove(objectNamespace, out var lazy))
            {
                var clientProxy = lazy.Value;
                clientProxy.DestroyLocally();
            }
        }

        public void Destroy()
        {
            foreach (var lazy in _proxies.Values)
            {
                try
                {
                    lazy.Value.OnShutdown();
                }
                catch (Exception e)
                {
                    Logger.Finest("Proxy destroy error!", e);
                }
            }
            _proxies.Clear();
        }

        public void CreateDistributedObjectsOnCluster()
        {
            ICollection<KeyValuePair<string, string>> proxyEntries =
                _proxies.Keys.Select(ns => new KeyValuePair<string, string>(ns.Name, ns.ServiceName)).ToList();
            if (proxyEntries.Count == 0)
            {
                return;
            }
            var request = ClientCreateProxiesCodec.EncodeRequest(proxyEntries);
            _client.InvocationService.InvokeOnRandomTarget(request);
        }
    }
}