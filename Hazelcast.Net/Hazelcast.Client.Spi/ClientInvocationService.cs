using System.Collections.Concurrent;
using System.Collections.Generic;
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
    internal sealed class ClientInvocationService : IClientInvocationService
    {
        public const int DefaultEventThreadCount = 3;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientInvocationService));
        private readonly HazelcastClient _client;
        private readonly ClientConnectionManager _clientConnectionManager;

        private readonly ConcurrentDictionary<int, DistributedEventHandler> _eventHandlers =
            new ConcurrentDictionary<int, DistributedEventHandler>();

        private readonly ConcurrentDictionary<int, ClientInvocation> _invocationRequests =
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
        }

        public Task<IClientMessage> InvokeOnMember(IClientMessage request, IMember target,
            DistributedEventHandler handler = null)
        {
            var clientConnection = GetConnection(target.GetAddress());
            return Send(clientConnection, new ClientInvocation(request, memberUuid: target.GetUuid(), handler: handler));
        }

        public Task<IClientMessage> InvokeOnTarget(IClientMessage request, Address target,
            DistributedEventHandler handler = null)
        {
            var clientConnection = GetConnection(target);
            return Send(clientConnection, new ClientInvocation(request, handler: handler));
        }

        public Task<IClientMessage> InvokeOnKeyOwner(IClientMessage request, object key,
            DistributedEventHandler handler = null)
        {
            var partitionService = (ClientPartitionService) _client.GetClientPartitionService();
            var partitionId = partitionService.GetPartitionId(key);
            var owner = partitionService.GetPartitionOwner(partitionId);
            if (owner != null)
            {
                var clientConnection = GetConnection(owner);
                return Send(clientConnection, new ClientInvocation(request, partitionId: partitionId, handler: handler));
            }
            return InvokeOnRandomTarget(request);
        }

        public Task<IClientMessage> InvokeOnRandomTarget(IClientMessage request, DistributedEventHandler handler = null)
        {
            var clientConnection = GetConnection();
            return Send(clientConnection, new ClientInvocation(request, handler: handler));
        }

        public bool RemoveEventHandler(int correlationId)
        {
            DistributedEventHandler handler;
            return _eventHandlers.TryRemove(correlationId, out handler);
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

        public Task<IClientMessage> InvokeOnConnection(IClientMessage request, ClientConnection connection)
        {
            return Send(connection, new ClientInvocation(request));
        }

        public void Shutdown()
        {
            _taskScheduler.Dispose();
        }

        private void FailRequest(ClientConnection connection, TaskCompletionSource<IClientMessage> future)
        {
            if (_client.GetConnectionManager().Live)
            {
                future.SetException(
                    new TargetDisconnectedException("Target was disconnected: " + connection.GetAddress()));
            }
            else
            {
                future.SetException(new HazelcastException("Client is shutting down."));
            }
        }

        private ClientConnection GetConnection(Address address = null)
        {
            return _client.GetConnectionManager().GetOrConnectWithRetry(address);
        }

        private void HandleEventMessage(IClientMessage eventMessage)
        {
            var correlationId = eventMessage.GetCorrelationId();
            DistributedEventHandler handler;
            if (!_eventHandlers.TryGetValue(correlationId, out handler))
            {
                // no event handler found, could be that the event is already unregistered
                Logger.Warning("No eventHandler for correlationId: " + correlationId + ", event: " + eventMessage);
                return;
            }
            handler(eventMessage);
        }

        private void HandleResponseMessage(IClientMessage response)
        {
            var correlationId = response.GetCorrelationId();
            ClientInvocation invocation;
            if (_invocationRequests.TryRemove(correlationId, out invocation))
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
                                        invocation.Response.SetException(exception);
                                        return;
                                    }
                                }
                                Send(connection, invocation);
                            });
                        }

                    }
                    else
                    {
                        invocation.Response.SetException(exception);
                    }
                }
                else
                {
                    invocation.Response.SetResult(response);
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

        private TaskCompletionSource<IClientMessage> RegisterInvocation(int correlationId, ClientInvocation request)
        {
            _invocationRequests.TryAdd(correlationId, request);

            if (request.Handler != null)
            {
                _eventHandlers.TryAdd(correlationId, request.Handler);
            }

            return request.Response;
        }

        private void RemoveEventHandlers()
        {
            _eventHandlers.Clear();
        }

        public void CleanUpConnectionResources(ClientConnection connection)
        {
            foreach (var invocationRequest in _invocationRequests)
            {
                //TODO: delete invocations which belong a specific conneciton
            }

            foreach (var distributedEventHandler in _eventHandlers)
            {
                //TODO: remove event handlers which belong a to a connection
            }
        }

        private void RemoveInvocationRequests(int connectionId)
        {
//            Logger.Finest("RemoveInvocationRequests for connection id:" + connectionId + " COUNT:" + _invocationRequests.Count);
//            foreach (var entry in _invocationRequests)
//            {
//                InvocationData data;
//                if (_invocationRequests.TryRemove(entry.Key, out data))
//                {
//                    FailRequest(data.responseFuture);
//                }
//            }
        }

        private Task<IClientMessage> Send(ClientConnection connection, ClientInvocation clientInvocation)
        {
            var correlationId = NextCorrelationId();
            clientInvocation.Message.SetCorrelationId(correlationId);
            clientInvocation.Message.AddFlag(ClientMessage.BeginAndEndFlags);
            if (clientInvocation.PartitionId != -1)
            {
                clientInvocation.Message.SetPartitionId(clientInvocation.PartitionId);
            }
            if (clientInvocation.MemberUuid != null && clientInvocation.MemberUuid != connection.GetMember().GetUuid())
            {
                //TODO: If MemberUUID different fail the request
            }

            var future = RegisterInvocation(correlationId, clientInvocation);

            //enqueue to write queue
            if (!connection.WriteAsync((ISocketWritable) clientInvocation.Message))
            {
                UnregisterCall(correlationId);
                RemoveEventHandler(correlationId);

                FailRequest(connection, future);
            }
            return future.Task;
        }

        private void UnregisterCall(int correlationId)
        {
            ClientInvocation invocation;
            _invocationRequests.TryRemove(correlationId, out invocation);
        }
    }
}