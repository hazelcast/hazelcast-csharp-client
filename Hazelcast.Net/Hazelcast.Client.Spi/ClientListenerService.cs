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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class
        ClientListenerService : IClientListenerService, IConnectionListener, IConnectionHeartbeatListener, IDisposable
    {
        private const int DefaultEventThreadCount = 3;
        private const int DefaultEventQueueCapacity = 1000000;

        private readonly HazelcastClient _client;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientSmartInvocationService));

        private readonly StripedTaskScheduler _registrationScheduler;
        private readonly StripedTaskScheduler _eventExecutor;
        private readonly ClientConnectionManager _connectionManager;

        private Timer _connectionReopener;

        private readonly ConcurrentDictionary<ClientConnection, ICollection<ListenerRegistrationKey>>
            _failedRegistrations;

        private readonly
            ConcurrentDictionary<ListenerRegistrationKey, ConcurrentDictionary<ClientConnection, EventRegistration>>
            _registrations;

        private readonly ConcurrentDictionary<long, DistributedEventHandler> _eventHandlers;

        public bool IsSmart { get; private set; }


        public ClientListenerService(HazelcastClient client)
        {
            _client = client;
            _connectionManager = client.GetConnectionManager();

            var eventTreadCount = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.event.thread.count") ??
                                  DefaultEventThreadCount;

            var eventQueueCapacity = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.event.queue.capacity") ??
                                     DefaultEventQueueCapacity;

            _eventExecutor = new StripedTaskScheduler(eventTreadCount, eventQueueCapacity, client.GetName() + ".event");
            _registrationScheduler =
                new StripedTaskScheduler(1, eventQueueCapacity, client.GetName() + ".eventRegistration");

            _registrations =
                new ConcurrentDictionary<ListenerRegistrationKey,
                    ConcurrentDictionary<ClientConnection, EventRegistration>>();
            _eventHandlers = new ConcurrentDictionary<long, DistributedEventHandler>();

            _failedRegistrations = new ConcurrentDictionary<ClientConnection, ICollection<ListenerRegistrationKey>>();

            IsSmart = client.GetClientConfig().GetNetworkConfig().IsSmartRouting();
        }

        public string RegisterListener(IClientMessage registrationMessage, DecodeRegistrationResponse responseDecoder,
            EncodeDeregisterListenerRequest encodeDeregisterListenerRequest, DistributedEventHandler eventHandler)
        {
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            TrySyncConnectToAllConnections();
            var registrationTask = new Task<string>(() =>
            {
                var userRegistrationId = Guid.NewGuid().ToString();

                var registrationKey =
                    new ListenerRegistrationKey(userRegistrationId, registrationMessage, responseDecoder, eventHandler);

                _registrations.TryAdd(registrationKey, new ConcurrentDictionary<ClientConnection, EventRegistration>());
                var connections = _connectionManager.ActiveConnections;
                foreach (var connection in connections)
                {
                    try
                    {
                        RegisterListenerOnConnection(registrationKey, connection);
                    }
                    catch (Exception e)
                    {
                        if (connection.Live)
                        {
                            DeregisterListenerInternal(userRegistrationId, encodeDeregisterListenerRequest);
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

        public bool DeregisterListener(string userRegistrationId,
            EncodeDeregisterListenerRequest encodeDeregisterListenerRequest)
        {
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));
            try
            {
                return Task<bool>.Factory.StartNew(
                    () => DeregisterListenerInternal(userRegistrationId, encodeDeregisterListenerRequest),
                    Task<bool>.Factory.CancellationToken,
                    Task<bool>.Factory.CreationOptions, _registrationScheduler).Result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }

        private bool DeregisterListenerInternal(string userRegistrationId,
            EncodeDeregisterListenerRequest encodeDeregisterListenerRequest)
        {
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            var key = new ListenerRegistrationKey(userRegistrationId);
            ConcurrentDictionary<ClientConnection, EventRegistration> registrationMap;
            if (!_registrations.TryGetValue(key, out registrationMap))
            {
                return false;
            }
            var successful = true;
            foreach (var registration in registrationMap.Values)
            {
                var connection = registration.ClientConnection;
                try
                {
                    var serverRegistrationId = registration.ServerRegistrationId;
                    var request = encodeDeregisterListenerRequest(serverRegistrationId);

                    var future = ((ClientInvocationService) _client.GetInvocationService())
                        .InvokeOnConnection(request, connection);
                    ThreadUtil.GetResult(future);
                    DistributedEventHandler removed;
                    _eventHandlers.TryRemove(registration.CorrelationId, out removed);
                    EventRegistration reg;
                    registrationMap.TryRemove(connection, out reg);
                }
                catch (Exception e)
                {
                    if (connection.Live)
                    {
                        successful = false;
                        Logger.Warning(
                            string.Format("Deregistration of listener with ID  {0} has failed to address {1}",
                                userRegistrationId, connection.GetLocalSocketAddress()), e);
                    }
                }
            }
            if (successful)
            {
                _registrations.TryRemove(key, out registrationMap);
            }
            return successful;
        }

        private void RegisterListenerOnConnection(ListenerRegistrationKey registrationKey, ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            ConcurrentDictionary<ClientConnection, EventRegistration> registrationMap;
            if (_registrations.TryGetValue(registrationKey, out registrationMap) &&
                registrationMap.ContainsKey(connection))
            {
                return;
            }
            var future = ((ClientInvocationService) _client.GetInvocationService())
                .InvokeListenerOnConnection(registrationKey.RegistrationRequest, registrationKey.EventHandler,
                    connection);

            IClientMessage clientMessage;
            try
            {
                clientMessage = ThreadUtil.GetResult(future);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }

            var serverRegistrationId = registrationKey.ResponseDecoder(clientMessage);
            var correlationId = registrationKey.RegistrationRequest.GetCorrelationId();
            var registration = new EventRegistration(serverRegistrationId, correlationId, connection);

            Debug.Assert(registrationMap != null, "registrationMap should be created!");
            registrationMap[connection] = registration;
        }

        public bool AddEventHandler(long correlationId, DistributedEventHandler eventHandler)
        {
            return _eventHandlers.TryAdd(correlationId, eventHandler);
        }

        public bool RemoveEventHandler(long correlationId)
        {
            DistributedEventHandler removed;
            return _eventHandlers.TryRemove(correlationId, out removed);
        }

        internal void TrySyncConnectToAllConnections()
        {
            if (!IsSmart) return;
            long timeLeftMillis = ((ClientInvocationService) _client.GetInvocationService()).InvocationTimeoutMillis;
            do
            {
                // Define the cancellation token.
                using (var source = new CancellationTokenSource())
                {
                    var token = source.Token;
                    var clientClusterService = _client.GetClientClusterService();
                    var tasks = clientClusterService.GetMemberList().Select(member => Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            _connectionManager.GetOrConnectAsync(member.GetAddress()).Wait(token);
                        }
                        catch (Exception)
                        {
                            // if an exception occur cancel the process
                            source.Cancel();
                        }
                    }, token)).ToArray();

                    var start = Clock.CurrentTimeMillis();
                    try
                    {
                        if (Task.WaitAll(tasks, (int)timeLeftMillis, token))
                        {
                            //All succeed
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        //waitAll did not completed
                    }
                    timeLeftMillis -= Clock.CurrentTimeMillis() - start;
                }
            } while (_client.GetLifecycleService().IsRunning() && timeLeftMillis > 0);
            throw new TimeoutException("Registering listeners is timed out.");
        }

        private void RegisterListenerFromInternal(ListenerRegistrationKey registrationKey, ClientConnection connection)
        {
            try
            {
                RegisterListenerOnConnection(registrationKey, connection);
            }
            catch (IOException)
            {
                ICollection<ListenerRegistrationKey> failedRegsToConnection;
                if (!_failedRegistrations.TryGetValue(connection, out failedRegsToConnection))
                {
                    failedRegsToConnection = new HashSet<ListenerRegistrationKey>();
                    _failedRegistrations[connection] = failedRegsToConnection;
                }
                failedRegsToConnection.Add(registrationKey);
            }
            catch (Exception e)
            {
                Logger.Warning(string.Format("Listener {0} can not be added to a new connection: {1}, reason: {2}",
                    registrationKey, connection, e.Message));
            }
        }

        public void HandleResponseMessage(IClientMessage message)
        {
            var partitionId = message.GetPartitionId();
            Task.Factory.StartNew(o =>
            {
                var correlationId = message.GetCorrelationId();
                DistributedEventHandler eventHandler;
                if (!_eventHandlers.TryGetValue(correlationId, out eventHandler))
                {
                    Logger.Warning(string.Format("No eventHandler for correlationId: {0} , event: {1} .",
                        correlationId, message));
                    return;
                }
                eventHandler(message);
            }, partitionId, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _eventExecutor);
        }

        public void ConnectionAdded(ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                foreach (var registrationKey in _registrations.Keys)
                {
                    RegisterListenerFromInternal(registrationKey, connection);
                }
            });
        }

        public void ConnectionRemoved(ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                ICollection<ListenerRegistrationKey> removed;
                _failedRegistrations.TryRemove(connection, out removed);
                foreach (var registrationMap in _registrations.Values)
                {
                    EventRegistration registration;
                    if (registrationMap.TryRemove(connection, out registration))
                    {
                        RemoveEventHandler(registration.CorrelationId);
                    }
                }
            });
        }

        public void HeartBeatResumed(ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(!Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                ICollection<ListenerRegistrationKey> registrationKeys;
                if (_failedRegistrations.TryGetValue(connection, out registrationKeys))
                {
                    foreach (var registrationKey in registrationKeys)
                    {
                        RegisterListenerFromInternal(registrationKey, connection);
                    }
                }
            });
        }

        public void HeartBeatStopped(ClientConnection connection)
        {
            //no op
        }

        public void Start()
        {
            _connectionManager.AddConnectionListener(this);
            if (IsSmart)
            {
                _connectionManager.AddConnectionHeartBeatListener(this);
                _connectionReopener = new Timer(ReOpenAllConnectionsIfNotOpen, null, 1000, 1000);
            }
        }

        public void Dispose()
        {
            if (_connectionReopener != null)
            {
                _connectionReopener.Dispose();
            }
            _registrationScheduler.Dispose();
            _eventExecutor.Dispose();
        }

        private void SubmitToRegistrationScheduler(Action action)
        {
            Task.Factory.StartNew(action, Task.Factory.CancellationToken, Task.Factory.CreationOptions,
                _registrationScheduler);
        }

        private void ReOpenAllConnectionsIfNotOpen(object state)
        {
            var clientClusterService = _client.GetClientClusterService();
            var memberList = clientClusterService.GetMemberList();
            foreach (var member in memberList)
            {
                try
                {
                    _connectionManager.GetOrConnectAsync(member.GetAddress());
                }
                catch (IOException)
                {
                    return;
                }
            }
        }
    }
}