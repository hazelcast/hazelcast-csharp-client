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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    internal abstract class ClientInvocationService : IClientInvocationService, IConnectionListener
    {
        private const int DefaultInvocationTimeout = 120;
        private const int RetryWaitTimeMillis = 1000;
        static readonly TimeSpan RetryWaitTime = TimeSpan.FromMilliseconds(RetryWaitTimeMillis);

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(ClientInvocationService));
        private readonly HazelcastClient _client;
        private ClientConnectionManager _clientConnectionManager;
        private IClientListenerService _clientListenerService;


        private readonly ConcurrentDictionary<long, ClientInvocation> _invocations =
            new ConcurrentDictionary<long, ClientInvocation>();

        private readonly int _invocationTimeoutMillis;
        private readonly bool _redoOperations;
        private long _correlationIdCounter = 1;
        private volatile bool _isShutDown;

        protected ClientInvocationService(HazelcastClient client)
        {
            _client = client;
            _redoOperations = client.GetClientConfig().GetNetworkConfig().IsRedoOperation();
            _invocationTimeoutMillis =
            (EnvironmentUtil.ReadInt("hazelcast.client.invocation.timeout.seconds") ??
             DefaultInvocationTimeout) * 1000;
        }

        protected HazelcastClient Client => _client;

        internal int InvocationRetryCount => _invocationTimeoutMillis / RetryWaitTimeMillis;

        internal int InvocationRetryWaitTime => RetryWaitTimeMillis;

        public int InvocationTimeoutMillis => _invocationTimeoutMillis;

        public void Start()
        {
            _clientConnectionManager = _client.GetConnectionManager();
            _clientListenerService = _client.GetListenerService();
            _clientConnectionManager.AddConnectionListener(this);
        }

        public void Shutdown()
        {
            _isShutDown = true;
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
        }

        public void HandleClientMessage(ClientMessage message)
        {
            if (ClientMessage.IsFlagSet(message.HeaderFlags, ClientMessage.IsEventFlag))
            {
                _clientListenerService.HandleResponseMessage(message);
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

        public IFuture<ClientMessage> InvokeListenerOnConnection(ClientMessage request,
            DistributedEventHandler eventHandler, ClientConnection connection)
        {
            var clientInvocation = new ClientInvocation(request, connection, eventHandler);
            InvokeInternal(clientInvocation, null, connection);
            return clientInvocation.Future;
        }

        public IFuture<ClientMessage> InvokeOnConnection(ClientMessage request, ClientConnection connection)
        {
            var clientInvocation = new ClientInvocation(request, connection);
            InvokeInternal(clientInvocation, null, connection);
            return clientInvocation.Future;
        }

        protected IFuture<ClientMessage> Invoke(ClientInvocation invocation, Address address = null)
        {
            InvokeInternal(invocation, address);
            return invocation.Future;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void InvokeInternal(ClientInvocation invocation, Address address = null, ClientConnection connection = null)
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
                        if (address != null && _client.GetClientClusterService().GetMember(address) == null)
                        {
                            throw new TargetNotMemberException($"Target {address} is not a member.");
                        }

                        //Create an async connection and send the invocation afterward.
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
                ValidateInvocation(invocation, connection);

                if (!TrySend(invocation, connection))
                {
                    //Sending failed.
                    if (_client.GetConnectionManager().Live)
                    {
                        throw new TargetDisconnectedException(connection.GetAddress(), "Error writing to socket.");
                    }
                    throw new HazelcastException("Client is shut down.");
                }
                //Successfully sent.
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
                            }, RetryWaitTime, CancellationToken.None)
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
                try
                {
                    invocation.Future.Exception = exception;
                }
                catch (InvalidOperationException e)
                {
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Invocation already completed:", e);
                    }
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

            if (invocation.Address != null && exception is TargetNotMemberException &&
                _client.GetClientClusterService().GetMember(invocation.Address) == null)
            {
                //when invocation send over address
                //if exception is target not member and
                //address is not available in member list , don't retry
                return false;
            }

            //validate exception
            return exception is IOException
                   || exception is SocketException
                   || exception is HazelcastInstanceNotActiveException
                   || exception is AuthenticationException
            // above exceptions OR retryable exception case as below
                   || exception is RetryableHazelcastException && (_redoOperations || invocation.Message.IsRetryable);
        }

        private Address GetNewInvocationAddress(ClientInvocation invocation)
        {
            Address newAddress = null;
            if (invocation.Address != null)
            {
                newAddress = invocation.Address;
            }
            else if (invocation.MemberUuid != Guid.Empty)
            {
                var member = _client.GetClientClusterService().GetMember(invocation.MemberUuid);
                if (member == null)
                {
                    Logger.Finest("Could not find a member with UUID " + invocation.MemberUuid);
                    throw new InvalidOperationException("Could not find a member with UUID " + invocation.MemberUuid);
                }
                newAddress = member.Address;
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
            clientInvocation.Message.CorrelationId = correlationId;
            clientInvocation.SentConnection = connection;
            if (clientInvocation.PartitionId != -1)
            {
                clientInvocation.Message.PartitionId = clientInvocation.PartitionId;
            }
        }

        private void ValidateInvocation(ClientInvocation clientInvocation, ClientConnection connection)
        {
            if (clientInvocation.MemberUuid != Guid.Empty && clientInvocation.MemberUuid != connection.Member.Uuid)
            {
                throw new TargetNotMemberException(
                    "The member UUID on the invocation doesn't match the member UUID on the connection.");
            }
            if (_isShutDown)
            {
                throw new HazelcastException("Client is shut down.");
            }
        }


        private bool TrySend(ClientInvocation clientInvocation, ClientConnection connection)
        {
            var correlationId = clientInvocation.Message.CorrelationId;
            if (!TryRegisterInvocation(correlationId, clientInvocation)) return false;

            //enqueue to write queue
            if (connection.WriteAsync(clientInvocation.Message))   
            {
                return true;
            }

            //Rollback sending failed
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Unable to write request " + clientInvocation.Message + " to connection " + connection);
            }
            UnregisterInvocation(correlationId);
            _clientListenerService.RemoveEventHandler(correlationId);
            return false;
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

        protected virtual Address GetRandomAddress()
        {
            var member = _client.GetLoadBalancer().Next();
            if (member != null)
            {
                return member.Address;
            }
            throw new IOException("Could not find any available address");
        }

        private void HandleResponseMessage(ClientMessage response)
        {
            var correlationId = response.CorrelationId;
            if (_invocations.TryRemove(correlationId, out var invocation))
            {
                if (response.MessageType == ErrorsCodec.ExceptionMessageType)
                {
                    var error = ErrorsCodec.Decode(response);
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest("Error received from server: " + error);
                    }
                    var exception = ExceptionUtil.ToException(error);

                    // retry only specific exceptions
                    HandleInvocationException(invocation, exception);
                }
                else
                {
                    try
                    {
                        invocation.Future.Result = response;
                    }
                    catch (InvalidOperationException e)
                    {
                        if (Logger.IsFinestEnabled())
                        {
                            Logger.Finest("Invocation already completed:", e);
                        }
                    }
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

            if (request.EventHandler != null &&
                !_clientListenerService.AddEventHandler(correlationId, request.EventHandler))
            {
                _invocations.TryRemove(correlationId, out request);
                return false;
            }
            return true;
        }

        private void UnregisterInvocation(long correlationId)
        {
            ClientInvocation ignored;
            _invocations.TryRemove(correlationId, out ignored);
        }

        public abstract IFuture<ClientMessage> InvokeOnKeyOwner(ClientMessage request, object key);
        public abstract IFuture<ClientMessage> InvokeOnMember(ClientMessage request, IMember member);
        public abstract IFuture<ClientMessage> InvokeOnRandomTarget(ClientMessage request);
        public abstract IFuture<ClientMessage> InvokeOnTarget(ClientMessage request, Address target);
        public abstract IFuture<ClientMessage> InvokeOnPartition(ClientMessage request, int partitionId);
    }
}