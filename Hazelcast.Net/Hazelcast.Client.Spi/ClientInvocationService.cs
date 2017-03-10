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
using System.Diagnostics.CodeAnalysis;
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
    internal abstract class ClientInvocationService : IClientInvocationService, IConnectionListener
    {
        private const int DefaultEventThreadCount = 3;
        private const int DefaultInvocationTimeout = 120;
        private const int RetryWaitTime = 1000;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientInvocationService));
        private readonly HazelcastClient _client;
        private readonly ClientConnectionManager _clientConnectionManager;

        private readonly ConcurrentDictionary<long, ClientInvocation> _invocations =
            new ConcurrentDictionary<long, ClientInvocation>();

        private readonly int _invocationTimeoutMillis;

        private readonly ConcurrentDictionary<long, ClientListenerInvocation> _listenerInvocations =
            new ConcurrentDictionary<long, ClientListenerInvocation>();

        private readonly bool _redoOperations;

        private readonly StripedTaskScheduler _taskScheduler;
        private long _correlationIdCounter = 1;
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
             DefaultInvocationTimeout) * 1000;

            _clientConnectionManager = client.GetConnectionManager();
            _clientConnectionManager.AddConnectionListener(this);
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

        public bool RemoveEventHandler(long correlationId)
        {
            ClientListenerInvocation invocationWithHandler;
            return _listenerInvocations.TryRemove(correlationId, out invocationWithHandler);
        }

        public void Shutdown()
        {
            _isShutDown = true;
            _taskScheduler.Dispose();
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
            InvokeInternal(clientInvocation, null, connection);
            return clientInvocation.Future;
        }

        public IFuture<IClientMessage> InvokeOnConnection(IClientMessage request, ClientConnection connection,
            bool bypassHeartbeat = false)
        {
            var clientInvocation = new ClientInvocation(request, connection);
            InvokeInternal(clientInvocation, null, connection, bypassHeartbeat);
            return clientInvocation.Future;
        }

        protected IFuture<IClientMessage> Invoke(ClientInvocation invocation, Address address = null)
        {
            InvokeInternal(invocation, address);
            return invocation.Future;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void InvokeInternal(ClientInvocation invocation, Address address = null,
            ClientConnection connection = null, bool bypassHeartbeat = false)
        {
            try
            {
                if (connection == null)
                {
                    if (address == null)
                    {
                        address = GetRandomAddress();
                    }
                    connection = GetConnection(address);
                    if (connection == null)
                    {
                        //Create an async conneciion and send the invocation afterward.
                        _clientConnectionManager.GetOrConnectAsync(address).ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    HandleInvocationException(invocation, t.Exception.Flatten().InnerExceptions.First());
                                }
                                else
                                {
                                    InvokeInternal(invocation, address, t.Result);
                                }
                            })
                            .ContinueWith(t =>
                                {
                                    HandleInvocationException(invocation, t.Exception.Flatten().InnerExceptions.First());
                                }, TaskContinuationOptions.OnlyOnFaulted |
                                   TaskContinuationOptions.ExecuteSynchronously);
                        return;
                    }
                }
                //Sending Invocation via connection
                UpdateInvocation(invocation, connection);
                ValidateInvocation(invocation, connection, bypassHeartbeat);

                if (!TrySend(invocation, connection))
                {
                    //Sending failed.
                    if (_client.GetConnectionManager().Live)
                    {
                        throw new TargetDisconnectedException(connection.GetAddress(), "Error writing to socket.");
                    }
                    throw new HazelcastException("Client is shut down.");
                }
                //Successfully sended.
            }
            catch (Exception e)
            {
                HandleInvocationException(invocation, e);
            }
        }

        private void HandleInvocationException(ClientInvocation invocation, Exception exception)
        {
            try
            {
                //Should it retry?
                if (ShouldRetryInvocation(invocation, exception))
                {
                    try
                    {
                        _client.GetClientExecutionService()
                            .Schedule(() =>
                            {
                                var address = GetNewInvocationAddress(invocation);
                                InvokeInternal(invocation, address);
                            }, RetryWaitTime, TimeUnit.Milliseconds)
                            .ContinueWith(t =>
                                {
                                    HandleInvocationException(invocation, t.Exception.Flatten().InnerExceptions.First());
                                }, TaskContinuationOptions.OnlyOnFaulted |
                                   TaskContinuationOptions.ExecuteSynchronously);
                        return;
                    }
                    catch (Exception e)
                    {
                        HandleInvocationException(invocation, e);
                    }
                }
                //Fail with exception
                if (!invocation.Future.IsComplete)
                {
                    invocation.Future.Exception = exception;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("HandleInvocationException missed an exception:", ex);
                }
                throw ex;
            }
        }

        private bool ShouldRetryInvocation(ClientInvocation invocation, Exception exception)
        {
            if (invocation.BoundConnection != null)
            {
                return false;
            }
            if (Clock.CurrentTimeMillis() >= invocation.InvocationTimeMillis + _invocationTimeoutMillis)
            {
                return false;
            }

            //validate exception
            return exception is IOException
                   || exception is SocketException
                   || exception is HazelcastInstanceNotActiveException
                   || exception is AuthenticationException
            // above exceptions OR retryable excaption case as below
                   || exception is RetryableHazelcastException && (_redoOperations || invocation.Message.IsRetryable());
        }

        private Address GetNewInvocationAddress(ClientInvocation invocation)
        {
            Address newAddress = null;
            if (invocation.Address != null)
            {
                newAddress = invocation.Address;
            }
            else if (invocation.MemberUuid != null)
            {
                var member = _client.GetClientClusterService().GetMember(invocation.MemberUuid);
                if (member == null)
                {
                    Logger.Finest("Could not find a member with UUID " + invocation.MemberUuid);
                    throw new InvalidOperationException("Could not find a member with UUID " + invocation.MemberUuid);
                }
                newAddress = member.GetAddress();
            }
            else if (invocation.PartitionId != -1)
            {
                newAddress = _client.GetClientPartitionService().GetPartitionOwner(invocation.PartitionId);
            }
            return newAddress;
        }

        private void UpdateInvocation(ClientInvocation clientInvocation, ClientConnection connection)
        {
            var correlationId = NextCorrelationId();
            clientInvocation.Message.SetCorrelationId(correlationId);
            clientInvocation.Message.AddFlag(ClientMessage.BeginAndEndFlags);
            clientInvocation.SentConnection = connection;
            if (clientInvocation.PartitionId != -1)
            {
                clientInvocation.Message.SetPartitionId(clientInvocation.PartitionId);
            }
        }

        private void ValidateInvocation(ClientInvocation clientInvocation, ClientConnection connection,
            bool bypassHeartbeat)
        {
            if (clientInvocation.MemberUuid != null && clientInvocation.MemberUuid != connection.Member.GetUuid())
            {
                throw new TargetNotMemberException(
                    "The member UUID on the invocation doesn't match the member UUID on the connection.");
            }

            if (!connection.IsHeartBeating && !bypassHeartbeat)
            {
                throw new TargetDisconnectedException(connection.GetAddress() + " has stopped heartbeating.");
            }

            if (_isShutDown)
            {
                throw new HazelcastException("Client is shut down.");
            }
        }


        private bool TrySend(ClientInvocation clientInvocation, ClientConnection connection)
        {
            var correlationId = clientInvocation.Message.GetCorrelationId();
            if (!TryRegisterInvocation(correlationId, clientInvocation)) return false;

            //enqueue to write queue
            if (connection.WriteAsync((ISocketWritable) clientInvocation.Message))
            {
                return true;
            }

            //Rollback sending failed
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Unable to write request " + clientInvocation.Message + " to connection " + connection);
            }
            UnregisterInvocation(correlationId);
            RemoveEventHandler(correlationId);
            return false;
        }

        private void CleanupEventHandlers(ClientConnection connection)
        {
            var keys = new List<long>();
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
                    if (!_isShutDown)
                    {
                        _client.GetClientExecutionService().Schedule(() =>
                            {
                                ReregisterListener(invocation)
                                    .ToTask()
                                    .ContinueWith(t => { Logger.Warning("Cannot reregister listener.", t.Exception); },
                                        TaskContinuationOptions.OnlyOnFaulted |
                                        TaskContinuationOptions.ExecuteSynchronously);
                            }, RetryWaitTime, TimeUnit.Milliseconds);
                    }
                }
            }
        }

        private void CleanupInvocations(ClientConnection connection)
        {
            var keys = new List<long>();
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
                if (_invocations.TryRemove(key, out invocation))
                {
                    var ex = _client.GetConnectionManager().Live
                        ? new TargetDisconnectedException(connection.GetAddress(), "connection was closed.")
                        : new HazelcastException("Client is shut down.");
                    HandleInvocationException(invocation, ex);
                }
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

        private void HandleResponseMessage(IClientMessage response)
        {
            var correlationId = response.GetCorrelationId();
            ClientInvocation invocation;
            if (_invocations.TryRemove(correlationId, out invocation))
            {
                if (response.GetMessageType() == Error.Type)
                {
                    var error = Error.Decode(response);
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Error received from server: " + error);
                    }
                    var exception = ExceptionUtil.ToException(error);

                    // retry only specific exceptions
                    HandleInvocationException(invocation, exception);
                }
                // if this was a re-registration operation, then we will throw away the response and just store the alias
                else if ((invocation is ClientListenerInvocation) &&
                         (invocation.Future.IsComplete && invocation.Future.Result != null))
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

        private long NextCorrelationId()
        {
            return Interlocked.Increment(ref _correlationIdCounter);
        }

        private bool TryRegisterInvocation(long correlationId, ClientInvocation request)
        {
            if (!_invocations.TryAdd(correlationId, request)) return false;
            var listenerInvocation = request as ClientListenerInvocation;
            if (listenerInvocation != null)
            {
                if (!_listenerInvocations.TryAdd(correlationId, listenerInvocation))
                {
                    _invocations.TryRemove(correlationId, out request);
                }
            }
            return true;
        }

        private IFuture<IClientMessage> ReregisterListener(ClientListenerInvocation invocation)
        {
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Re-registering listener for " + invocation.Message);
            }
            return Invoke(invocation);
        }

        private void UnregisterInvocation(long correlationId)
        {
            ClientInvocation ignored;
            _invocations.TryRemove(correlationId, out ignored);
        }
    }
}