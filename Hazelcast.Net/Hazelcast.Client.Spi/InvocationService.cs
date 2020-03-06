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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class InvocationService
    {
        private const int DefaultInvocationTimeout = 120;
        private const int DefaultCleanResourceMillis = 100;
        private const int DefaultInvocationRetryPauseMillis = 1000;
        private const int MaxFastInvocationCount = 5;

        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(InvocationService));

        private readonly HazelcastClient _client;
        private ConnectionManager _connectionManager;
        private ListenerService _listenerService;
        private PartitionService _partitionService;

        private readonly ConcurrentDictionary<long, Invocation> _invocations = new ConcurrentDictionary<long, Invocation>();
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        private readonly bool _redoOperations;
        public int InvocationTimeoutMillis { get; }
        private readonly int _invocationRetryPauseMillis;
        private readonly TimeSpan _cleanResourceInterval;
        private long _correlationIdCounter = 1;
        private volatile bool _isShutDown;

        public InvocationService(HazelcastClient client)
        {
            _client = client;
            _redoOperations = client.ClientConfig.GetNetworkConfig().IsRedoOperation();
            InvocationTimeoutMillis =
                (EnvironmentUtil.ReadInt("hazelcast.client.invocation.timeout.seconds") ?? DefaultInvocationTimeout) * 1000;

            _invocationRetryPauseMillis = EnvironmentUtil.ReadInt("hazelcast.client.invocation.retry.pause.millis") ??
                                          DefaultInvocationRetryPauseMillis;
            var cleanResourceMillis = EnvironmentUtil.ReadInt("hazelcast.client.internal.clean.resources.millis") ??
                                      DefaultCleanResourceMillis;

            _cleanResourceInterval = TimeSpan.FromMilliseconds(cleanResourceMillis);
        }

        public void Start()
        {
            _connectionManager = _client.ConnectionManager;
            _partitionService = _client.PartitionService;
            _listenerService = _client.ListenerService;

            _client.ExecutionService.ScheduleWithFixedDelay(CleanResources, _cleanResourceInterval, _cleanResourceInterval,
                _cancelToken.Token);
        }

        private void CleanResources()
        {
            foreach (var invocation in _invocations.Values)
            {
                var connection = invocation.SentConnection;
                if (connection == null)
                {
                    continue;
                }
                if (!connection.IsAlive)
                {
                    Exception ex = new TargetDisconnectedException(connection.CloseReason, connection.CloseException);
                    HandleInvocationException(invocation, ex);
                    return;
                }
            }
        }

        public void Shutdown()
        {
            _isShutDown = true;
            foreach (var invocation in _invocations.Values)
            {
                HandleInvocationException(invocation, new HazelcastClientNotActiveException());
            }
        }

        internal void HandleClientMessage(ClientMessage message)
        {
            if (message.IsEvent)
            {
                _listenerService.HandleEventMessage(message);
            }
            else
            {
                _client.ExecutionService.Submit(() =>
                {
                    if (!_isShutDown)
                    {
                        HandleResponseMessage(message);
                    }
                });
            }
        }

        private void HandleResponseMessage(ClientMessage response)
        {
            if (response.IsBackupEvent)
            {
                //TODO backup event not implemented yet
                throw new NotImplementedException();
            }
            var correlationId = response.CorrelationId;
            if (_invocations.TryRemove(correlationId, out var invocation))
            {
                if (response.IsExceptionType)
                {
                    var exception = response.ToException();
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
                        if (Logger.IsFinestEnabled)
                        {
                            Logger.Finest("Invocation already completed:", e);
                        }
                    }
                }
            }
            else
            {
                Logger.Warning($"No call for correlationId: {correlationId}, response: {response}");
            }
        }

        public IFuture<ClientMessage> InvokeOnKeyOwner(ClientMessage request, object key)
        {
            var partitionId = _partitionService.GetPartitionId(key);
            return InvokeOnPartitionOwner(request, partitionId);
        }

        public IFuture<ClientMessage> InvokeOnRandomTarget(ClientMessage request)
        {
            return Invoke(new Invocation(request));
        }

        public IFuture<ClientMessage> InvokeOnTarget(ClientMessage request, Guid target)
        {
            return Invoke(new Invocation(request, targetUuid: target));
        }

        public IFuture<ClientMessage> InvokeOnPartitionOwner(ClientMessage request, int partitionId)
        {
            return Invoke(new Invocation(request, partitionId: partitionId));
        }

        public IFuture<ClientMessage> InvokeOnConnection(ClientMessage request, Connection connection)
        {
            return Invoke(new Invocation(request, boundConnection: connection));
        }

        public IFuture<ClientMessage> InvokeListenerOnConnection(ClientMessage request, DistributedEventHandler eventHandler,
            Connection connection)
        {
            var invocation = new Invocation(request, boundConnection: connection, eventHandler: eventHandler);
            return Invoke(invocation);
        }

        private IFuture<ClientMessage> Invoke(Invocation invocation)
        {
            invocation.Request.CorrelationId = NextCorrelationId();
            InvokeOnSelection(invocation);
            return invocation.Future;
        }

        private void InvokeOnSelection(Invocation invocation)
        {
            try
            {
                invocation.IncrementCounter();
                if (!_client.LifecycleService.IsRunning())
                {
                    throw new HazelcastClientNotActiveException("Client is shut down.");
                }
                var connection = GetInvocationConnection(invocation);
                
                if (connection == null) {
                    throw new IOException("No connection found.");
                }

                if (!TrySend(invocation, connection))
                {
                    //Sending failed.
                    if (_connectionManager.IsALive)
                    {
                        throw new IOException($"Error writing to socket. {connection.RemoteAddress}");
                    }
                    throw new HazelcastClientNotActiveException("Client is shut down.");
                }
                //Successfully sent.
            }
            catch (Exception e)
            {
                HandleInvocationException(invocation, e);
            }
        }

        private bool TrySend(Invocation invocation, Connection connection)
        {
            var correlationId = invocation.Request.CorrelationId;
            if (!TryRegisterInvocation(correlationId, invocation)) return false;

            //enqueue to write queue
            if (connection.TryQueue(invocation.Request))
            {
                invocation.SentConnection = connection;
                return true;
            }

            //Rollback sending failed
            if (Logger.IsFinestEnabled)
            {
                Logger.Finest($"Unable to write request  {invocation.Request} to connection {connection}");
            }

            UnregisterInvocation(correlationId);
            return false;
        }

        private Connection GetInvocationConnection(Invocation invocation)
        {
            if (invocation.IsBindToSingleConnection)
            {
                return invocation.BoundConnection;
            }

            var target = invocation.PartitionId != -1
                ? _partitionService.GetPartitionOwner(invocation.PartitionId)
                : invocation.TargetUuid;

            return _connectionManager.GetTargetOrRandomConnection(target);
        }

        private long NextCorrelationId()
        {
            return Interlocked.Increment(ref _correlationIdCounter);
        }

        private bool TryRegisterInvocation(long correlationId, Invocation invocation)
        {
            if (!_invocations.TryAdd(invocation.Request.CorrelationId, invocation)) return false;

            if (invocation.EventHandler != null && !_listenerService.AddEventHandler(correlationId, invocation.EventHandler))
            {
                _invocations.TryRemove(correlationId, out _);
                return false;
            }
            return true;
        }

        private void UnregisterInvocation(long correlationId)
        {
            _invocations.TryRemove(correlationId, out _);
            _listenerService.RemoveEventHandler(correlationId);
        }

        private void HandleInvocationException(Invocation invocation, Exception exception)
        {
            if (Logger.IsFinestEnabled)
            {
                Logger.Finest($"Invocation got an exception! {invocation}, ", exception);
            }

            if (!_client.LifecycleService.IsRunning())
            {
                CompleteInvocationWithException(invocation,
                    new HazelcastClientNotActiveException("Client is shutting down", exception));
                return;
            }
            if (!ShouldRetry(invocation, exception))
            {
                CompleteInvocationWithException(invocation, exception);
                return;
            }

            if (Clock.CurrentTimeMillis() >= invocation.StartTimeMillis + InvocationTimeoutMillis)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Exception will not be retried because invocation timed out", exception);
                }
                var msg =
                    $"{invocation} timed out because exception occurred after client invocation timeout {InvocationTimeoutMillis} ms." +
                    $"Total elapsed time: {Clock.CurrentTimeMillis() - invocation.StartTimeMillis} ms.";
                CompleteInvocationWithException(invocation, new TimeoutException(msg, exception));
                return;
            }

            try
            {
                Retry(invocation);
            }
            catch (Exception e)
            {
                CompleteInvocationWithException(invocation,
                    new HazelcastClientNotActiveException("Client is shutting down", exception));
            }
        }


        private void Retry(Invocation invocation)
        {
            if (invocation.InvokeCount < MaxFastInvocationCount)
            {
                // fast retry for the first few invocations
                _client.ExecutionService.Submit(() => { RetryInternal(invocation); });
            }
            else
            {
                var delayMillis = Math.Min(1 << (invocation.InvokeCount - MaxFastInvocationCount), _invocationRetryPauseMillis);
                TimeSpan.FromMilliseconds(delayMillis);
                _client.ExecutionService.Schedule(() => { RetryInternal(invocation); }, TimeSpan.FromMilliseconds(delayMillis),
                    CancellationToken.None);
            }
        }

        private void RetryInternal(Invocation invocation)
        {
            var correlationId = NextCorrelationId();
            invocation = invocation.CopyWithNewCorrelationId(correlationId);
            InvokeOnSelection(invocation);
        }

        private bool ShouldRetry(Invocation invocation, Exception exception)
        {
            if (invocation.IsBindToSingleConnection && (exception is IOException || exception is TargetDisconnectedException))
            {
                return false;
            }
            if (exception is IOException || exception is SocketException || exception is HazelcastInstanceNotActiveException ||
                exception is IRetryableException)
            {
                return true;
            }
            if (exception is TargetDisconnectedException)
            {
                return invocation.Request.IsRetryable || _redoOperations;
            }
            return false;
        }

        private void CompleteInvocationWithException(Invocation invocation, Exception exception)
        {
            try
            {
                invocation.Future.Exception = exception;
            }
            catch (InvalidOperationException e)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Invocation already completed:", e);
                }
            }
        }
    }
}