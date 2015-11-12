// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal abstract class ClientInvocationService : IClientInvocationService, IConnectionListener,
        IConnectionHeartbeatListener
    {
        private const int DefaultEventThreadCount = 3;
        private const int DefaultInvocationTimeout = 120;
        private const int RetryWaitTime = 1000;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientInvocationService));
        private readonly HazelcastClient _client;
        private readonly ClientConnectionManager _clientConnectionManager;

        private readonly ConcurrentDictionary<int, ClientInvocation> _invocations =
            new ConcurrentDictionary<int, ClientInvocation>();

        private readonly int _invocationTimeoutMillis;

        private readonly ConcurrentDictionary<int, ClientListenerInvocation> _listenerInvocations =
            new ConcurrentDictionary<int, ClientListenerInvocation>();

        private readonly bool _redoOperations;
        
        private readonly StripedTaskScheduler _taskScheduler;
        private int _correlationIdCounter = 1;
        private volatile bool _isShutDown;

        protected ClientInvocationService(HazelcastClient client)
        {
            _client = client;
            _redoOperations = client.GetClientConfig().GetNetworkConfig().IsRedoOperation();

            var eventTreadCount = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.event.thread.count") ??
                                  DefaultEventThreadCount;
            _taskScheduler = new StripedTaskScheduler(eventTreadCount);

            _invocationTimeoutMillis =
                (EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.invocation.timeout.seconds") ??
                 DefaultInvocationTimeout)*1000;

            _clientConnectionManager = (ClientConnectionManager) client.GetConnectionManager();
            _clientConnectionManager.AddConnectionListener(this);
            _clientConnectionManager.AddConnectionHeartBeatListener(this);
        }

        protected HazelcastClient Client
        {
            get { return _client; }
        }

        internal int InvocationRetryCount
        {
            get { return _invocationTimeoutMillis / RetryWaitTime; }
        }

        internal int InvocationRetryWaitTime
        {
            get { return RetryWaitTime; }
        }

        public abstract IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder);

        public abstract IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder);

        public abstract IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder);

        public abstract IFuture<IClientMessage> InvokeListenerOnPartition(IClientMessage request, int partitionId,
            DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder);

        public abstract IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key);
        public abstract IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember member);
        public abstract IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request);
        public abstract IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target);
        public abstract IFuture<IClientMessage> InvokeOnPartition(IClientMessage request, int partitionId);

        public bool RemoveEventHandler(int correlationId)
        {
            ClientListenerInvocation invocationWithHandler;
            return _listenerInvocations.TryRemove(correlationId, out invocationWithHandler);
        }

        public void Shutdown()
        {
            _isShutDown = true;
            _taskScheduler.Dispose();
        }

        public void HeartBeatStarted(ClientConnection connection)
        {
            // noop
        }

        public void HeartBeatStopped(ClientConnection connection)
        {
            CleanupInvocations(connection);
            CleanupEventHandlers(connection);
        }

        public void ConnectionAdded(ClientConnection connection)
        {
            // noop
        }

        public void ConnectionRemoved(ClientConnection connection)
        {
            CleanUpConnectionResources(connection);
        }

        public void CleanUpConnectionResources(ClientConnection connection)
        {
            Logger.Finest("Cleaning up connection resources for " + connection.Id);
            CleanupInvocations(connection);
            CleanupEventHandlers(connection);
        }

        public void HandleClientMessage(IClientMessage message)
        {
            if (message.IsFlagSet(ClientMessage.ListenerEventFlag))
            {
                object state = message.GetPartitionId();
                var eventTask = new Task(o =>
                {
                    if (!_isShutDown)
                    {
                        HandleEventMessage(message);
                    }
                }, state);
                eventTask.Start(_taskScheduler);
            }
            else
            {
                _client.GetClientExecutionService().Submit(() =>
                {
                    if (!_isShutDown)
                    {
                        HandleResponseMessage(message);
                    }
                });
            }
        }

        public IFuture<IClientMessage> InvokeListenerOnConnection(IClientMessage request,
            DistributedEventHandler handler, DecodeStartListenerResponse responseDecoder, ClientConnection connection)
        {
            var clientInvocation = new ClientListenerInvocation(request, handler, responseDecoder, connection);
            Send(connection, clientInvocation);
            return clientInvocation.Future;
        }

        public IFuture<IClientMessage> InvokeOnConnection(IClientMessage request, ClientConnection connection)
        {
            var clientInvocation = new ClientInvocation(request, connection);
            Send(connection, clientInvocation);
            return clientInvocation.Future;
        }

        protected IFuture<IClientMessage> Invoke(ClientInvocation invocation, Address address = null)
        {
            try
            {
                if (address == null)
                {
                    address = GetRandomAddress();
                }

                // try to get an existing connection, if not establish a new connection asyncronously
                var connection = GetConnection(address);
                if (connection != null)
                {
                    Send(connection, invocation);
                }
                else
                {
                    _clientConnectionManager.GetOrConnectAsync(address).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            var innerException = t.Exception.InnerExceptions.First();
                            HandleException(invocation, innerException);
                        }
                        else
                        {
                            try
                            {
                                Send(t.Result, invocation);
                            }
                            catch (Exception e)
                            {
                                HandleException(invocation, e);
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                HandleException(invocation, e);
            }
            return invocation.Future;
        }

        protected virtual void Send(ClientConnection connection, ClientInvocation clientInvocation)
        {
            var correlationId = NextCorrelationId();
            clientInvocation.Message.SetCorrelationId(correlationId);
            clientInvocation.Message.AddFlag(ClientMessage.BeginAndEndFlags);
            clientInvocation.SentConnection = connection;
            if (clientInvocation.PartitionId != -1)
            {
                clientInvocation.Message.SetPartitionId(clientInvocation.PartitionId);
            }
            if (clientInvocation.MemberUuid != null && clientInvocation.MemberUuid != connection.GetMember().GetUuid())
            {
                HandleException(clientInvocation,
                    new TargetNotMemberException(
                        "The member UUID on the invocation doesn't match the member UUID on the connection."));
            }

            if (_isShutDown)
            {
                FailRequestDueToShutdown(clientInvocation);
            }
            else
            {
                RegisterInvocation(correlationId, clientInvocation);

                //enqueue to write queue
                if (!connection.WriteAsync((ISocketWritable) clientInvocation.Message))
                {
                    UnregisterCall(correlationId);
                    RemoveEventHandler(correlationId);
                    FailRequest(connection, clientInvocation);
                }
            }
        }

        private void CleanupEventHandlers(ClientConnection connection)
        {
            var keys = new List<int>();
            foreach (var entry in _listenerInvocations)
            {
                if (entry.Value.SentConnection == connection && entry.Value.BoundConnection == null)
                {
                    keys.Add(entry.Key);
                }
            }

            foreach (var key in keys)
            {
                ClientListenerInvocation invocation;
                _listenerInvocations.TryRemove(key, out invocation);

                if (invocation.Future.IsComplete && invocation.Future.Result != null)
                {
                    //re-register listener on a new node
                    _client.GetClientExecutionService().Submit(() =>
                    {
                        if (!_isShutDown)
                        {
                            ReregisterListener(invocation);
                        }
                    });
                }
            }
        }

        private void CleanupInvocations(ClientConnection connection)
        {
            var keys = new List<int>();
            foreach (var entry in _invocations)
            {
                if (entry.Value.SentConnection == connection)
                {
                    keys.Add(entry.Key);
                }
            }
            foreach (var key in keys)
            {
                ClientInvocation invocation;
                _invocations.TryRemove(key, out invocation);
                FailRequest(connection, invocation);
            }
        }

        private void EnsureOwnerConnectionAvailable()
        {
            var clientClusterService = _client.GetClientClusterService();
            var ownerConnectionAddress = clientClusterService.GetOwnerConnectionAddress();

            var isOwnerConnectionAvailable = ownerConnectionAddress != null
                                             && _clientConnectionManager.GetConnection(ownerConnectionAddress) != null;

            if (!isOwnerConnectionAvailable)
            {
                if (_isShutDown)
                {
                    throw new HazelcastException("Client is shut down.");
                }
                throw new IOException("Owner connection was not live.");
            }
        }

        private void FailRequest(ClientConnection connection, ClientInvocation invocation)
        {
            if (_client.GetConnectionManager().Live)
            {
                HandleException(invocation,
                    new TargetDisconnectedException(connection.GetAddress()));
            }
            else
            {
                FailRequestDueToShutdown(invocation);
            }
        }

        private void FailRequestDueToShutdown(ClientInvocation invocation)
        {
            if (Logger.IsFinestEnabled())
            {
                var connectionId = invocation.SentConnection == null
                    ? "unknown"
                    : invocation.SentConnection.Id.ToString();
                Logger.Finest("Aborting request on connection " + connectionId + ": " + invocation.Message +
                              " due to shutdown.");
            }
            if (!invocation.Future.IsComplete)
            {
                invocation.Future.Exception = new HazelcastException("Client is shutting down.");
            }
        }

        private ClientConnection GetConnection(Address address)
        {
            EnsureOwnerConnectionAvailable();
            return _client.GetConnectionManager().GetConnection(address);
        }

        private Address GetRandomAddress()
        {
            var member = _client.GetLoadBalancer().Next();
            if (member != null)
            {
                return member.GetAddress();
            }
            throw new IOException("Could not find any available address");
        }

        private static string GetRegistrationIdFromResponse(ClientListenerInvocation invocation)
        {
            var originalResponse = (ClientMessage) invocation.Future.Result;
            originalResponse.Index(originalResponse.GetDataOffset());
            return invocation.ResponseDecoder(originalResponse);
        }

        private void HandleEventMessage(IClientMessage eventMessage)
        {
            var correlationId = eventMessage.GetCorrelationId();
            ClientListenerInvocation invocationWithHandler;
            if (!_listenerInvocations.TryGetValue(correlationId, out invocationWithHandler))
            {
                // no event handler found, could be that the event is already unregistered
                Logger.Warning("No eventHandler for correlationId: " + correlationId + ", event: " + eventMessage);
                return;
            }
            invocationWithHandler.Handler(eventMessage);
        }

        private void HandleException(ClientInvocation invocation, Exception exception)
        {
            Logger.Finest("Got exception for request " + invocation.Message + ":" + exception);

            if (exception is IOException
                || exception is SocketException
                || exception is HazelcastInstanceNotActiveException
                || exception is AuthenticationException)
            {
                if (RetryRequest(invocation)) return;
            }

            if (exception is RetryableHazelcastException)
            {
                if (_redoOperations || invocation.Message.IsRetryable())
                {
                    if (RetryRequest(invocation))
                        return;
                }
            }
            invocation.Future.Exception = exception;
        }

        private void HandleResponseMessage(IClientMessage response)
        {
            var correlationId = response.GetCorrelationId();
            ClientInvocation invocation;
            if (_invocations.TryRemove(correlationId, out invocation))
            {
                if (response.GetMessageType() == Error.Type)
                {
                    var error = Error.Decode(response);
                    var exception = ExceptionUtil.ToException(error);

                    // retry only specific exceptions
                    HandleException(invocation, exception);
                }
                // if this was a re-registration operation, then we will throw away the response and just store the alias
                else if (invocation is ClientListenerInvocation && invocation.Future.IsComplete &&
                         invocation.Future.Result != null)
                {
                    var listenerInvocation = (ClientListenerInvocation) invocation;
                    var originalRegistrationId = GetRegistrationIdFromResponse(listenerInvocation);
                    var newRegistrationId = listenerInvocation.ResponseDecoder(response);
                    _client.GetListenerService()
                        .ReregisterListener(originalRegistrationId, newRegistrationId,
                            invocation.Message.GetCorrelationId());
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest(string.Format("Re-registered listener for {0} of type {1:X}",
                            originalRegistrationId,
                            listenerInvocation.Message.GetMessageType()));
                    }
                }
                else
                {
                    invocation.Future.Result = response;
                }
            }
            else
            {
                Logger.Warning("No call for correlationId: " + correlationId + ", response: " + response);
            }
        }

        private int NextCorrelationId()
        {
            return Interlocked.Increment(ref _correlationIdCounter);
        }

        private void RegisterInvocation(int correlationId, ClientInvocation request)
        {
            _invocations.TryAdd(correlationId, request);

            var listenerInvocation = request as ClientListenerInvocation;
            if (listenerInvocation != null)
            {
                _listenerInvocations.TryAdd(correlationId, listenerInvocation);
            }
        }

        private void ReinvokeInvocation(ClientInvocation invocation)
        {
            Address address = null;
            if (invocation.Address != null)
            {
                address = invocation.Address;
            }
            else if (invocation.MemberUuid != null)
            {
                var member = _client.GetClientClusterService().GetMember(invocation.MemberUuid);
                if (member == null)
                {
                    throw new InvalidOperationException("Could not find a member with UUID " + invocation.MemberUuid);
                }
                address = member.GetAddress();
            }
            else if (invocation.PartitionId != -1)
            {
                address = _client.GetClientPartitionService().GetPartitionOwner(invocation.PartitionId);
            }
            Invoke(invocation, address);
        }

        private void ReregisterListener(ClientListenerInvocation invocation)
        {
            Logger.Finest("Re-registering listener for " + invocation.Message);
            Invoke(invocation, GetRandomAddress());
        }

        private bool RetryRequest(ClientInvocation invocation)
        {
            if (invocation.BoundConnection != null)
            {
                return false;
            }

            if (Clock.CurrentTimeMillis() >= (invocation.InvocationTimeMillis + _invocationTimeoutMillis))
            {
                return false;
            }

            Logger.Finest("Retry for request " + invocation.Message);

            _client.GetClientExecutionService().Schedule(() =>
            {
                if (_isShutDown) FailRequestDueToShutdown(invocation);
                else
                {
                    // get the appropriate connection for retry
                    ReinvokeInvocation(invocation);
                }
            }, RetryWaitTime, TimeUnit.Milliseconds).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var innerException = t.Exception.InnerExceptions.First();
                    Logger.Finest("Retry of request " + invocation.Message + " failed with ", innerException);
                    HandleException(invocation, innerException);
                }
            });
            return true;
        }

        private void UnregisterCall(int correlationId)
        {
            ClientInvocation invocation;
            _invocations.TryRemove(correlationId, out invocation);
        }
    }
}