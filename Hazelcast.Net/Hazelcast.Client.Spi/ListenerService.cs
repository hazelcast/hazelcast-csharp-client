// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class ListenerService : IConnectionListener, IDisposable
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ListenerService));

        private const int DefaultEventThreadCount = 3;
        private const int DefaultEventQueueCapacity = 1000000;

        private readonly HazelcastClient _client;

        private readonly StripedTaskScheduler _registrationScheduler;
        private readonly StripedTaskScheduler _eventExecutor;
        private readonly ConnectionManager _connectionManager;

        private readonly ConcurrentDictionary<Guid, ListenerRegistration> _registrations;
        private readonly ConcurrentDictionary<long, DistributedEventHandler> _eventHandlers;

        public bool RegisterLocalOnly { get; }

        public ListenerService(HazelcastClient client)
        {
            _client = client;
            _connectionManager = client.ConnectionManager;
            var eventTreadCount = EnvironmentUtil.ReadInt("hazelcast.client.event.thread.count") ?? DefaultEventThreadCount;
            var eventQueueCapacity =
                EnvironmentUtil.ReadInt("hazelcast.client.event.queue.capacity") ?? DefaultEventQueueCapacity;
            _eventExecutor = new StripedTaskScheduler(eventTreadCount, eventQueueCapacity, client.Name + ".event");
            _registrationScheduler = new StripedTaskScheduler(1, eventQueueCapacity, client.Name + ".eventRegistration");
            _registrations = new ConcurrentDictionary<Guid, ListenerRegistration>();
            _eventHandlers = new ConcurrentDictionary<long, DistributedEventHandler>();
            RegisterLocalOnly = client.ClientConfig.GetNetworkConfig().IsSmartRouting();
        }

        public void Start()
        {
            _connectionManager.AddConnectionListener(this);
        }
        
        public static void RegisterConfigListeners<T>(IEnumerable<IEventListener> configuredListeners, Func<T, Guid> registrationAction) where T : IEventListener
        {
            foreach (var configuredListener in configuredListeners)
            {
                if (configuredListener is T listener)
                {
                    registrationAction(listener);
                }
            }
        }

        public bool AddEventHandler(long correlationId, DistributedEventHandler eventHandler)
        {
            return _eventHandlers.TryAdd(correlationId, eventHandler);
        }

        public bool RemoveEventHandler(long correlationId)
        {
            return _eventHandlers.TryRemove(correlationId, out _);
        }

        public Guid RegisterListener(ClientMessage registrationMessage, DecodeRegisterResponse responseDecoder,
            EncodeDeregisterRequest encodeDeregisterRequest, DistributedEventHandler eventHandler)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));

            var registrationTask = new Task<Guid>(() =>
            {
                var userRegistrationId = Guid.NewGuid();

                var listenerRegistration = new ListenerRegistration(userRegistrationId, registrationMessage, responseDecoder,
                    encodeDeregisterRequest, eventHandler);

                _registrations.TryAdd(userRegistrationId, listenerRegistration);

                var connections = _connectionManager.ActiveConnections;
                foreach (var connection in connections)
                {
                    try
                    {
                        RegisterListenerOnConnection(listenerRegistration, connection);
                    }
                    catch (Exception e)
                    {
                        if (connection.IsAlive)
                        {
                            DeregisterListenerInternal(userRegistrationId);
                            throw new HazelcastException("Listener cannot be added ", e);
                        }
                    }
                }
                return userRegistrationId;
            });
            try
            {
                registrationTask.Start(_registrationScheduler);
                return registrationTask.Result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public bool DeregisterListener(Guid userRegistrationId)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));
            try
            {
                return Task<bool>.Factory.StartNew(() => DeregisterListenerInternal(userRegistrationId),
                    Task<bool>.Factory.CancellationToken, Task<bool>.Factory.CreationOptions, _registrationScheduler).Result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public void HandleEventMessage(ClientMessage message)
        {
            var partitionId = message.PartitionId;
            Task.Factory.StartNew(o =>
            {
                var correlationId = message.CorrelationId;
                if (!_eventHandlers.TryGetValue(correlationId, out var eventHandler))
                {
                    Logger.Warning($"No eventHandler for correlationId: {correlationId} , event: {message} .");
                    return;
                }
                eventHandler(message);
            }, partitionId, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _eventExecutor);
        }

        public void ConnectionAdded(Connection connection)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                foreach (var listenerRegistration in _registrations.Values)
                {
                    RegisterListenerFromInternal(listenerRegistration, connection);
                }
            });
        }

        public void ConnectionRemoved(Connection connection)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                foreach (var listenerRegistration in _registrations.Values)
                {
                    if (listenerRegistration.ConnectionRegistrations.TryRemove(connection, out var registration))
                    {
                        RemoveEventHandler(registration.CorrelationId);
                    }
                }
            });
        }

        public void Dispose()
        {
            _registrationScheduler.Dispose();
            _eventExecutor.Dispose();
        }

        private void RegisterListenerFromInternal(ListenerRegistration listenerRegistration, Connection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));
            try
            {
                RegisterListenerOnConnection(listenerRegistration, connection);
            }
            catch (Exception e)
            {
                Logger.Warning(
                    $"Listener {listenerRegistration} can not be added to a new connection: {connection}, reason: {e.Message}");
            }
        }

        private void RegisterListenerOnConnection(ListenerRegistration listenerRegistration, Connection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));

            if (listenerRegistration.ConnectionRegistrations.ContainsKey(connection))
            {
                return;
            }
            var future = _client.InvocationService.InvokeListenerOnConnection(listenerRegistration.RegistrationRequest,
                listenerRegistration.EventHandler, connection);

            ClientMessage clientMessage;
            try
            {
                clientMessage = ThreadUtil.GetResult(future);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }

            var serverRegistrationId = listenerRegistration.DecodeRegisterResponse(clientMessage);
            var correlationId = listenerRegistration.RegistrationRequest.CorrelationId;
            var registration = new EventRegistration(serverRegistrationId, correlationId, connection);

            Debug.Assert(listenerRegistration.ConnectionRegistrations != null, "registrationMap should be created!");
            listenerRegistration.ConnectionRegistrations[connection] = registration;
        }

        private bool DeregisterListenerInternal(Guid userRegistrationId)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));
            if (!_registrations.TryGetValue(userRegistrationId, out var listenerRegistration))
            {
                return false;
            }

            var successful = true;
            foreach (var connectionRegistration in listenerRegistration.ConnectionRegistrations.Values)
            {
                var connection = connectionRegistration.Connection;
                try
                {
                    var serverRegistrationId = connectionRegistration.ServerRegistrationId;
                    var request = listenerRegistration.EncodeDeregisterRequest(serverRegistrationId);

                    var future = _client.InvocationService.InvokeOnConnection(request, connection);
                    ThreadUtil.GetResult(future);
                    _eventHandlers.TryRemove(connectionRegistration.CorrelationId, out _);
                    listenerRegistration.ConnectionRegistrations.TryRemove(connection, out _);
                }
                catch (Exception e)
                {
                    if (connection.IsAlive)
                    {
                        successful = false;
                        Logger.Warning(
                            $"Deregistration of listener with ID  {userRegistrationId} has failed to address {connection.RemoteAddress}",
                            e);
                    }
                }
            }
            if (successful)
            {
                _registrations.TryRemove(userRegistrationId, out listenerRegistration);
            }
            return successful;
        }

        private void SubmitToRegistrationScheduler(Action action)
        {
            Task.Factory.StartNew(action, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _registrationScheduler);
        }
    }
}