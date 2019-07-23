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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Util;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    internal class ClientListenerService : IClientListenerService, IConnectionListener, IDisposable
    {
        private const int DefaultEventThreadCount = 3;
        private const int DefaultEventQueueCapacity = 1000000;

        private readonly HazelcastClient _client;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientSmartInvocationService));

        private readonly StripedTaskScheduler _registrationScheduler;
        private readonly StripedTaskScheduler _eventExecutor;
        private readonly ClientConnectionManager _connectionManager;

        private Timer _connectionReopener;

        private readonly ConcurrentDictionary<ClientConnection, ICollection<ListenerRegistration>> _failedRegistrations;
        private readonly ConcurrentDictionary<string, ListenerRegistration> _registrations;
        private readonly ConcurrentDictionary<long, DistributedEventHandler> _eventHandlers;

        public bool IsSmart { get; private set; }


        public ClientListenerService(HazelcastClient client)
        {
            _client = client;
            _connectionManager = client.GetConnectionManager();
            var eventTreadCount = EnvironmentUtil.ReadInt("hazelcast.client.event.thread.count") ?? DefaultEventThreadCount;
            var eventQueueCapacity =
                EnvironmentUtil.ReadInt("hazelcast.client.event.queue.capacity") ?? DefaultEventQueueCapacity;
            _eventExecutor = new StripedTaskScheduler(eventTreadCount, eventQueueCapacity, client.GetName() + ".event");
            _registrationScheduler = new StripedTaskScheduler(1, eventQueueCapacity, client.GetName() + ".eventRegistration");
            _registrations = new ConcurrentDictionary<string, ListenerRegistration>();
            _eventHandlers = new ConcurrentDictionary<long, DistributedEventHandler>();
            _failedRegistrations = new ConcurrentDictionary<ClientConnection, ICollection<ListenerRegistration>>();
            IsSmart = client.GetClientConfig().GetNetworkConfig().IsSmartRouting();
        }

        public string RegisterListener(IClientMessage registrationMessage, DecodeRegisterResponse responseDecoder,
            EncodeDeregisterRequest encodeDeregisterRequest, DistributedEventHandler eventHandler)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));

            TrySyncConnectToAllConnections();
            var registrationTask = new Task<string>(() =>
            {
                var userRegistrationId = Guid.NewGuid().ToString();

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
                        if (connection.Live)
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


        public bool DeregisterListener(string userRegistrationId)
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

        private bool DeregisterListenerInternal(string userRegistrationId)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));
            ListenerRegistration listenerRegistration;
            if (!_registrations.TryGetValue(userRegistrationId, out listenerRegistration))
            {
                return false;
            }

            var successful = true;
            foreach (var connectionRegistration in listenerRegistration.ConnectionRegistrations.Values)
            {
                var connection = connectionRegistration.ClientConnection;
                try
                {
                    var serverRegistrationId = connectionRegistration.ServerRegistrationId;
                    var request = listenerRegistration.EncodeDeregisterRequest(serverRegistrationId);

                    var future =
                        ((ClientInvocationService) _client.GetInvocationService()).InvokeOnConnection(request, connection);
                    ThreadUtil.GetResult(future);
                    DistributedEventHandler removed;
                    _eventHandlers.TryRemove(connectionRegistration.CorrelationId, out removed);
                    EventRegistration reg;
                    listenerRegistration.ConnectionRegistrations.TryRemove(connection, out reg);
                }
                catch (Exception e)
                {
                    if (connection.Live)
                    {
                        successful = false;
                        Logger.Warning(
                            string.Format("Deregistration of listener with ID  {0} has failed to address {1}", userRegistrationId,
                                connection.GetLocalSocketAddress()), e);
                    }
                }
            }
            if (successful)
            {
                _registrations.TryRemove(userRegistrationId, out listenerRegistration);
            }
            return successful;
        }

        private void RegisterListenerOnConnection(ListenerRegistration listenerRegistration, ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));

            if (listenerRegistration.ConnectionRegistrations.ContainsKey(connection))
            {
                return;
            }
            var future = ((ClientInvocationService) _client.GetInvocationService()).InvokeListenerOnConnection(
                listenerRegistration.RegistrationRequest, listenerRegistration.EventHandler, connection);

            IClientMessage clientMessage;
            try
            {
                clientMessage = ThreadUtil.GetResult(future);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }

            var serverRegistrationId = listenerRegistration.DecodeRegisterResponse(clientMessage);
            var correlationId = listenerRegistration.RegistrationRequest.GetCorrelationId();
            var registration = new EventRegistration(serverRegistrationId, correlationId, connection);

            Debug.Assert(listenerRegistration.ConnectionRegistrations != null, "registrationMap should be created!");
            listenerRegistration.ConnectionRegistrations[connection] = registration;
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
                        if (Task.WaitAll(tasks, (int) timeLeftMillis, token))
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

        private void RegisterListenerFromInternal(ListenerRegistration listenerRegistration, ClientConnection connection)
        {
            //This method should only be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name != null && Thread.CurrentThread.Name.Contains("eventRegistration"));
            try
            {
                RegisterListenerOnConnection(listenerRegistration, connection);
            }
            catch (IOException)
            {
                ICollection<ListenerRegistration> failedRegsToConnection;
                if (!_failedRegistrations.TryGetValue(connection, out failedRegsToConnection))
                {
                    failedRegsToConnection = new HashSet<ListenerRegistration>();
                    _failedRegistrations[connection] = failedRegsToConnection;
                }
                failedRegsToConnection.Add(listenerRegistration);
            }
            catch (Exception e)
            {
                Logger.Warning(string.Format("Listener {0} can not be added to a new connection: {1}, reason: {2}",
                    listenerRegistration, connection, e.Message));
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
                    Logger.Warning(string.Format("No eventHandler for correlationId: {0} , event: {1} .", correlationId,
                        message));
                    return;
                }
                eventHandler(message);
            }, partitionId, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _eventExecutor);
        }

        public void ConnectionAdded(ClientConnection connection)
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

        public void ConnectionRemoved(ClientConnection connection)
        {
            //This method should not be called from registrationExecutor
            Debug.Assert(Thread.CurrentThread.Name == null || !Thread.CurrentThread.Name.Contains("eventRegistration"));

            SubmitToRegistrationScheduler(() =>
            {
                ICollection<ListenerRegistration> removed;
                _failedRegistrations.TryRemove(connection, out removed);
                foreach (var listenerRegistration in _registrations.Values)
                {
                    EventRegistration registration;
                    if (listenerRegistration.ConnectionRegistrations.TryRemove(connection, out registration))
                    {
                        RemoveEventHandler(registration.CorrelationId);
                    }
                }
            });
        }

        public void Start()
        {
            _connectionManager.AddConnectionListener(this);
            if (IsSmart)
            {
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
            Task.Factory.StartNew(action, Task.Factory.CancellationToken, Task.Factory.CreationOptions, _registrationScheduler);
        }

        private void ReOpenAllConnectionsIfNotOpen(object state)
        {
            var clientClusterService = _client.GetClientClusterService();
            var memberList = clientClusterService.GetMemberList();
            foreach (var member in memberList)
            {
                try
                {
                    _connectionManager.GetOrConnectAsync(member.GetAddress()).IgnoreExceptions();
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}