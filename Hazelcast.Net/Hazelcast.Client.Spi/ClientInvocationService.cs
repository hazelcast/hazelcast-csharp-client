using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    internal sealed class ClientInvocationService : IClientInvocationService, IConnectionListener
    {
        public const int DefaultEventThreadCount = 3;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientInvocationService));
        private readonly HazelcastClient _client;
        private readonly ClientConnectionManager _clientConnectionManager;

        private readonly ConcurrentDictionary<int, ClientListenerInvocation> _listenerInvocations =
            new ConcurrentDictionary<int, ClientListenerInvocation>();

        private readonly ConcurrentDictionary<int, ClientInvocation> _invocations =
            new ConcurrentDictionary<int, ClientInvocation>();

        private readonly bool _redoOperations;
        private readonly int _retryCount = 20;
        private readonly int _retryWaitTime = 250;
        private readonly StripedTaskScheduler _taskScheduler;
        private int _correlationIdCounter = 1;

        public ClientInvocationService(HazelcastClient client)
        {
            _client = client;
            _redoOperations = client.GetClientConfig().GetNetworkConfig().IsRedoOperation();

            var eventTreadCount = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.event.thread.count");
            eventTreadCount = eventTreadCount > 0 ? eventTreadCount : DefaultEventThreadCount;
            _taskScheduler = new StripedTaskScheduler(eventTreadCount);

            var retryCount = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.request.retry.count");
            if (retryCount > 0)
            {
                _retryCount = retryCount;
            }
            var retryWaitTime = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.request.retry.wait.time");
            if (retryWaitTime > 0)
            {
                _retryWaitTime = retryWaitTime;
            }
            _clientConnectionManager = (ClientConnectionManager) client.GetConnectionManager();
            _clientConnectionManager.AddConnectionListener(this);
        }

        public IFuture<IClientMessage> InvokeOnMember(IClientMessage request, IMember target)
        {
            var clientConnection = GetConnection(target.GetAddress());
            return Send(clientConnection, new ClientInvocation(request, memberUuid: target.GetUuid()));
        }

        public IFuture<IClientMessage> InvokeOnTarget(IClientMessage request, Address target)
        {
            var clientConnection = GetConnection(target);
            return Send(clientConnection, new ClientInvocation(request));
        }

        public IFuture<IClientMessage> InvokeListenerOnTarget(IClientMessage request, Address target, DistributedEventHandler handler,
           DecodeStartListenerResponse responseDecoder)
        {
            var clientConnection = GetConnection(target);
            return Send(clientConnection, new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public IFuture<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key)
        {
            var partitionService = (ClientPartitionService)_client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            var connection = GetConnection(owner);

            return Send(connection, new ClientInvocation(request, partitionId));
        }

        public IFuture<IClientMessage> InvokeListenerOnKeyOwner(IClientMessage request, object key, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var partitionService = (ClientPartitionService)_client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            var connection = GetConnection(owner);

            return Send(connection, new ClientListenerInvocation(request, handler, responseDecoder, partitionId));
        }

        public IFuture<IClientMessage> InvokeOnRandomTarget(IClientMessage request)
        {
            var clientConnection = GetConnection();
            return Send(clientConnection, new ClientInvocation(request));
        }

        public IFuture<IClientMessage> InvokeListenerOnRandomTarget(IClientMessage request, DistributedEventHandler handler,
            DecodeStartListenerResponse responseDecoder)
        {
            var clientConnection = GetConnection();
            return Send(clientConnection, new ClientListenerInvocation(request, handler, responseDecoder));
        }

        public bool RemoveEventHandler(int correlationId)
        {
            ClientListenerInvocation invocationWithHandler;
            return _listenerInvocations.TryRemove(correlationId, out invocationWithHandler);
        }

        public void HandleClientMessage(IClientMessage message)
        {
            if (message.IsFlagSet(ClientMessage.ListenerEventFlag))
            {
                object state = message.GetPartitionId();
                var eventTask = new Task(o => HandleEventMessage(message), state);
                eventTask.Start(_taskScheduler);
            }
            else
            {
                Task.Factory.StartNew(() => HandleResponseMessage(message));
            }
        }

        public IFuture<IClientMessage> InvokeOnConnection(IClientMessage request, ClientConnection connection)
        {
            return Send(connection, new ClientInvocation(request));
        }

        public void Shutdown()
        {
            _taskScheduler.Dispose();
        }

        private void FailRequest(ClientConnection connection, ClientInvocation invocation)
        {
            if (_client.GetConnectionManager().Live)
            {
                invocation.Future.Exception =
                    new TargetDisconnectedException("Target was disconnected: " + connection.GetAddress());
            }
            else
            {
                invocation.Future.Exception = new HazelcastException("Client is shutting down.");
            }
        }

        private ClientConnection GetConnection(Address address = null)
        {
            return _client.GetConnectionManager().GetOrConnectWithRetry(address);
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
                    var exception = ExceptionUtil.ToException(error);

                    // retry only specific exceptions  
                    if (exception is RetryableHazelcastException )
                    {
                        if ((_redoOperations || invocation.Message.IsRetryable())
                            && invocation.IncrementAndGetRetryCount() < _retryCount)
                        {
                            // retry the task by sending it to a random node
                            Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(_retryWaitTime);
                                var connection = GetConnection();
                                if (invocation.MemberUuid != null)
                                {
                                    var member = connection.GetMember();
                                    if (member == null || member.GetUuid() != invocation.MemberUuid)
                                    {
                                        Logger.Finest(
                                            "The member UUID on the invocation doesn't match the member UUID on the connection.");
                                        invocation.Future.Exception = exception;
                                        return;
                                    }
                                }
                                Send(connection, invocation);
                            });
                        }

                    }
                    else
                    {
                        invocation.Future.Exception = exception;
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

        public void CleanUpConnectionResources(ClientConnection connection)
        {
            CleanupInvocations(connection);
            CleanupEventHandlers(connection);
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

        private void CleanupEventHandlers(ClientConnection connection)
        {
            var keys = new List<int>();
            foreach (var entry in _listenerInvocations)
            {
                if (entry.Value.SentConnection == connection)
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
                    ReregisterListener(key, invocation);
                }
            }
        }

        private void ReregisterListener(int correlationId, ClientListenerInvocation invocation)
        {
            Task.Factory.StartNew(() =>
            {
                var newConnection = GetConnection();
                var oldRegistrationId = invocation.ResponseDecoder(invocation.Future.Result);
                var resp = Send(newConnection,
                    new ClientListenerInvocation(invocation.Message, invocation.Handler, invocation.ResponseDecoder,
                        invocation.PartitionId, invocation.MemberUuid));
                var newRegistrationId = invocation.ResponseDecoder(ThreadUtil.GetResult(resp));
                _client.GetListenerService().ReregisterListener(oldRegistrationId, newRegistrationId, correlationId);
            });
        }

        private IFuture<IClientMessage> Send(ClientConnection connection, ClientInvocation clientInvocation)
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
                clientInvocation.Future.Exception = new InvalidOperationException(
                    "The member UUID on the invocation doesn't match the member UUID on the connection.");
                return clientInvocation.Future;
            }

            RegisterInvocation(correlationId, clientInvocation);

            //enqueue to write queue
            if (!connection.WriteAsync((ISocketWritable) clientInvocation.Message))
            {
                UnregisterCall(correlationId);
                RemoveEventHandler(correlationId);

                FailRequest(connection, clientInvocation);
            }
            return clientInvocation.Future;
        }

        private void UnregisterCall(int correlationId)
        {
            ClientInvocation invocation;
            _invocations.TryRemove(correlationId, out invocation);
        }

        public void ConnectionAdded(ClientConnection connection)
        {
            // noop
        }

        public void ConnectionRemoved(ClientConnection connection)
        {
            CleanUpConnectionResources(connection);
        }
    }
}