using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    /// <summary>Holds the clientSocket to one of the members of Hazelcast ICluster.</summary>
    /// <remarks>Holds the clientSocket to one of the members of Hazelcast ICluster.</remarks>
    internal sealed class ClientConnection
    {
        public const int BufferSize = 1 << 15; //32k
        public const int SocketReceiveBufferSize = 1 << 15; //32k
        public const int SocketSendBufferSize = 1 << 15; //32k
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (ClientConnection));
        private readonly ClientMessageBuilder _builder;
        private readonly ClientConnectionManager _clientConnectionManager;
        private readonly ClientListenerService _listenerService;
        private readonly int _id;

        private readonly ConcurrentDictionary<int, InvocationData> _invocationRequests =
            new ConcurrentDictionary<int, InvocationData>();
        private readonly ConcurrentDictionary<int, DistributedEventHandler> _eventHandlers =
            new ConcurrentDictionary<int, DistributedEventHandler>();

        private readonly ByteBuffer _receiveBuffer;
        private readonly ByteBuffer _sendBuffer;
        private readonly BlockingCollection<ISocketWritable> _writeQueue = new BlockingCollection<ISocketWritable>();
        private int _correlationIdCounter = 1;
        private volatile Socket _clientSocket;
        private volatile ISocketWritable _lastWritable;
        private volatile bool _live;
        private volatile IMember _member;
        private Thread _writeThread;

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection(ClientConnectionManager clientConnectionManager, ClientListenerService listenerService, int id, Address address,
            ClientNetworkConfig clientNetworkConfig)
        {
            _clientConnectionManager = clientConnectionManager;
            _listenerService = listenerService;
            _id = id;

            var isa = address.GetInetSocketAddress();
            var socketOptions = clientNetworkConfig.GetSocketOptions();
            var socketFactory = socketOptions.GetSocketFactory();

            if (socketFactory == null)
            {
                socketFactory = new DefaultSocketFactory();
            }
            _clientSocket = socketFactory.CreateSocket();
            try
            {
                _clientSocket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var lingerOption = new LingerOption(true, 5);
                if (socketOptions.GetLingerSeconds() > 0)
                {
                    lingerOption.LingerTime = socketOptions.GetLingerSeconds();
                }
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
                _clientSocket.NoDelay = socketOptions.IsTcpNoDelay();

                //TODO BURASI NOLCAK
                //clientSocket.ExclusiveAddressUse SetReuseAddress(options.IsReuseAddress());

                _clientSocket.ReceiveTimeout = socketOptions.GetTimeout() > 0 ? socketOptions.GetTimeout() : -1;

                var bufferSize = socketOptions.GetBufferSize()*1024;
                if (bufferSize < 0)
                {
                    bufferSize = BufferSize;
                }

                _clientSocket.SendBufferSize = bufferSize;
                _clientSocket.ReceiveBufferSize = bufferSize;

                _clientSocket.UseOnlyOverlappedIO = true;

                var connectionTimeout = clientNetworkConfig.GetConnectionTimeout() > -1
                    ? clientNetworkConfig.GetConnectionTimeout()
                    : -1;
                var socketResult = _clientSocket.BeginConnect(address.GetHost(), address.GetPort(), null, null);

                socketResult.AsyncWaitHandle.WaitOne(connectionTimeout, true);
                if (!_clientSocket.Connected)
                {
                    // NOTE, MUST CLOSE THE SOCKET
                    _clientSocket.Close();
                    throw new IOException("Failed to connect server.");
                }
                _sendBuffer = ByteBuffer.Allocate(BufferSize);
                _receiveBuffer = ByteBuffer.Allocate(BufferSize);

                _builder = new ClientMessageBuilder(HandleClientMessage);

                _live = true;
            }
            catch (Exception e)
            {
                _clientSocket.Close();
                throw new IOException("Cannot connect! Socket error:" + e.Message);
            }
        }

        public bool Live
        {
            get { return _live; }
        }

        internal int CurrentCorrelationId
        {
            get { return _correlationIdCounter; }
        }

        internal int Id
        {
            get { return _id; }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            Logger.Finest("Closing socket, id: " + _id);
            Release();
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            return _clientSocket != null ? (IPEndPoint) _clientSocket.LocalEndPoint : null;
        }

        public Address GetRemoteEndpoint()
        {
            return _member != null ? _member.GetAddress() : null;
        }

        private void RemoveInvocationRequests()
        {
            Logger.Finest("RemoveInvocationRequests for connection id:" + _id + " COUNT:" + _invocationRequests.Count);
            foreach (var entry in _invocationRequests)
            {
                InvocationData data;
                if (_invocationRequests.TryRemove(entry.Key, out data))
                {
                    FailRequest(data.responseFuture);
                }
            }
                   }

        private void RemoveEventHandlers()
        {
            foreach (var entry in _eventHandlers)
            {
                DistributedEventHandler handler;
                _eventHandlers.TryRemove(entry.Key, out handler);
            }
        }

        public Task<IClientMessage> Send(ClientInvocation clientInvocation)
        {
            var correlationId = NextCorrelationId();
            clientInvocation.Message.SetCorrelationId(correlationId);
            clientInvocation.Message.AddFlag(ClientMessage.BeginAndEndFlags);
            if (clientInvocation.PartitionId != -1)
            {
                clientInvocation.Message.SetPartitionId(clientInvocation.PartitionId);
            }

            var future = RegisterInvocation(correlationId, clientInvocation);

            //enqueue to write queue
            if (!WriteAsync((ISocketWritable) clientInvocation.Message))
            {
                UnregisterCall(correlationId);
                UnregisterEvent(correlationId);

                FailRequest(future);
            }
            return future.Task;
        }

        public void SetRemoteMember(IMember member)
        {
            _member = member;
        }

        public void SwitchToNonBlockingMode()
        {
            if (_clientSocket.Blocking)
            {
                _sendBuffer.Clear();
                _receiveBuffer.Clear();
                //clientSocket.Blocking = false;
                StartAsyncProcess();
            }
            //ignore other cases
        }

        public override string ToString()
        {
            var localSocketAddress = GetLocalSocketAddress();
            if (localSocketAddress != null)
            {
                return "Connection [" + _member + " -> " + localSocketAddress + "]";
            }
            return "Connection [" + _member + " -> CLOSED ]";
        }

        public bool UnregisterEvent(int correlationId)
        {
            DistributedEventHandler handler;
            return _eventHandlers.TryRemove(correlationId, out handler);
        }

        public bool WriteAsync(ISocketWritable packet)
        {
            if (!_live)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Connection is closed, won't write packet -> " + packet);
                }
                return false;
            }
            return _writeQueue.TryAdd(packet);
        }

        internal Socket GetSocket()
        {
            return _clientSocket;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void Init()
        {
            _clientSocket.Send(Encoding.UTF8.GetBytes(Protocols.ClientBinaryNew));
        }

        private void BeginRead()
        {
            if (!CheckLive())
            {
                Close();
                return;
            }
            try
            {
                try
                {
                    SocketError socketError;
                    _clientSocket.BeginReceive(
                        _receiveBuffer.Array(),
                        _receiveBuffer.Position,
                        _receiveBuffer.Remaining(),
                        SocketFlags.None, out socketError, EndReadCallback, null);
                }
                catch (Exception e)
                {
                    Logger.Severe("Exception at Socket.Read for endPoint: " + GetRemoteEndpoint(), e);
                    HandleSocketException(e);
                }
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at BeginRead : " + GetRemoteEndpoint(), e);
                HandleSocketException(e);
            }
        }

        private bool CheckLive()
        {
            if (!_live)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("We are being asked to read, but connection is not live so we won't");
                }
                return false;
            }
            if (!_clientSocket.Connected)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("We are being asked to read, but connection is not connected...");
                }
                Task.Factory.StartNew(Close);
                return false;
            }
            return true;
        }

        private void EndReadCallback(IAsyncResult asyncResult)
        {
            if (_clientSocket == null)
            {
                return;
            }
            try
            {
                //if (ThreadUtil.debug) { Console.Write(" ER"+id); }
                SocketError socketError;
                var receivedByteSize = _clientSocket.EndReceive(asyncResult, out socketError);

                if (receivedByteSize <= 0)
                {
                    //Socket Closed
                    Close();
                }

                if (socketError != SocketError.Success)
                {
                    Logger.Warning("Operation System Level Socket error code:" + socketError);
                    Close();
                    return;
                }
                _receiveBuffer.Position += receivedByteSize;
                _receiveBuffer.Flip();

                _builder.OnData(_receiveBuffer);

                if (_receiveBuffer.HasRemaining())
                {
                    _receiveBuffer.Compact();
                }
                else
                {
                    _receiveBuffer.Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Severe("Fatal Error at EndReadCallback : " + GetRemoteEndpoint(), e);
                HandleSocketException(e);
            }
            finally
            {
                BeginRead();
            }
        }

//        private void HandleRequestTask(Task task, IClientMessage response, Error error)
//        {
//            if (task == null)
//            {
//                Logger.Finest("Task does not exists");
//                return;
//            }
//            var taskData = task.AsyncState as TaskData;
//            if (taskData == null)
//            {
//                Logger.Severe("TaskData cannot be null");
//                return;
//            }
//            if (error != null)
//            {
//                if (error.ErrorCode == (int) ClientProtocolErrorCodes.TargetNotMember)
//                {
//                    if (ReSend(task)) return;
//                }
//                if (error.ErrorCode == (int) ClientProtocolErrorCodes.HazelcastInstanceNotActive ||
//                    error.ErrorCode == (int) ClientProtocolErrorCodes.TargetDisconnected)
//                {
//                    if (taskData.Request.IsRetryable() || _redoOperations)
//                    {
//                        if (ReSend(task)) return;
//                    }
//                }
//            }
//
//            // we have previously received a response 
//            if (taskData.Response != null && response != null)
//            {
//                if (taskData.Handler != null)
//                {
//                    // TODO: listener needs re-registering
////                    var registrationId = _serializationService.ToObject<string>(taskData.Response);
////                    var alias = _serializationService.ToObject<string>(response);
////                    _clientConnectionManager.ReRegisterListener(registrationId, alias, taskData.Request.GetCorrelationId());
//                }
//                if (taskData.Handler == null)
//                {
//                    Logger.Severe("Response can only be set once.");
//                }
//            }
//            if (taskData.Response != null && taskData.Handler == null)
//            {
//                Logger.Severe("Response can only be set once.");
//                return;
//            }
//
//            if (taskData.Response != null && response != null)
//            {
//                //TODO: Re-register listener
////                var registrationId = _serializationService.ToObject<string>(taskData.Response); 
////                var alias = _serializationService.ToObject<string>(response);
////                _clientConnectionManager.ReRegisterListener(registrationId, alias, taskData.Request.GetCorrelationId());
//                return;
//            }
//            ////////////////////////////////////////////
//
//            UpdateResponse(task, response, error);
//
//            //Response ready, lets run the task to return a Result
//            if (!task.IsCompleted)
//            {
//                task.Start();
//            }
//            else
//            {
//                Logger.Severe("Already started task error");
//            }
//        }

        private void FailRequest(TaskCompletionSource<IClientMessage> future)
        {
            if (_clientConnectionManager.Live)
            {
                future.SetException(new TargetDisconnectedException("Disconnected: " + GetRemoteEndpoint()));
            }
            else
            {
                future.SetException(new HazelcastException("Client is shutting down."));
            }
        }

        private void HandleClientMessage(IClientMessage message)
        {
            if (message.IsFlagSet(ClientMessage.ListenerEventFlag))
            {
                object state = message.GetPartitionId();
                var eventTask = new Task(o => HandleEventMessage(message), state);
                eventTask.Start(_listenerService.TaskScheduler);
            }
            else
            {
                Task.Factory.StartNew(() => HandleResponseMessage(message));
            }
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
            InvocationData data;
            if (_invocationRequests.TryRemove(correlationId, out data))
            {
                if (response.GetMessageType() == Error.Type)
                {
                    var error = Error.Decode(response);
                    var exception = ExceptionUtil.ToException(error);
                    data.responseFuture.SetException(exception);
                }
                else
                {
                    data.responseFuture.SetResult(response);
                }
            }
            else
            {
                Logger.Warning("No call for correlationId: " + correlationId + ", response: " + response);
            }
        }

        private void HandleSocketException(Exception e)
        {
            var se = e as SocketException;
            if (se != null)
            {
                var errorCode = se.ErrorCode;
                Logger.Warning("Operation System Level Socket error code:" + errorCode);
            }
            else
            {
                Logger.Warning(e.Message, e);
            }
            Close();
        }

        private int NextCorrelationId()
        {
            return Interlocked.Increment(ref _correlationIdCounter);
        }

        private TaskCompletionSource<IClientMessage> RegisterInvocation(int correlationId, ClientInvocation request)
        {
            var future = new TaskCompletionSource<IClientMessage>();
            var invocationData = new InvocationData(future, request);
            _invocationRequests.TryAdd(correlationId, invocationData);

            if (request.Handler != null)
            {
                _eventHandlers.TryAdd(correlationId, request.Handler);
            }

            return future;
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void Release()
        {
            if (!_live)
            {
                return;
            }
            _live = false;
            _writeQueue.CompleteAdding();

            if (_writeThread != null && _writeThread.IsAlive)
            {
                _writeThread.Interrupt();
                //writeThread.Join();
            }
            if (Logger.IsFinestEnabled())
            {
                Logger.Finest("Closing socket, id: " + _id);
            }

            //bool lockTaken = false;
            //spinLock.Enter(ref lockTaken);
            //try
            //{
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();

            _clientSocket = null;

            if (_id > -1)
            {
                _clientConnectionManager.DestroyConnection(this);
                RemoveInvocationRequests();
                RemoveEventHandlers();
            }
        }

//        private bool ReSend(Task task)
//        {
//            Logger.Finest("ReSending task:" + task.Id);
//            var taskData = task.AsyncState as TaskData;
//            if (taskData == null)
//            {
//                return false;
//            }
//            if (taskData.IncrementAndGetRetryCount() > ClientConnectionManager.RetryCount ||
//                (taskData.MemberUuid != null && taskData.MemberUuid != _member.GetUuid()))
//            {
//                return false;
//            }
//            Task.Factory.StartNew(() =>
//            {
//                Thread.Sleep(ClientConnectionManager.RetryWaitTime);
//                _clientConnectionManager.ReSend(task);
//            }).ContinueWith(taskResend =>
//            {
//                if (taskResend.IsFaulted && taskResend.Exception != null)
//                {
//                    FailRequest(task);
//                }
//            });
//            return true;
//        }

        private void StartAsyncProcess()
        {
            BeginRead();
            _writeThread = new Thread(WriteQueueLoop) {IsBackground = true, Priority = ThreadPriority.Highest};
            _writeThread.Start();
        }

        private void UnregisterCall(int correlationId)
        {
            InvocationData data;
            _invocationRequests.TryRemove(correlationId, out data);
        }

        private void WriteQueueLoop()
        {
            while (!_writeQueue.IsAddingCompleted)
            {
                try
                {
                    if (_lastWritable == null)
                    {
                        _lastWritable = _writeQueue.Take();
                    }
                }
                catch (InvalidOperationException)
                {
                    //BlockingCollection is empty
                    if (_writeQueue.IsAddingCompleted)
                    {
                        return;
                    }
                }

                while (_sendBuffer.HasRemaining() && _lastWritable != null)
                {
                    var complete = _lastWritable.WriteTo(_sendBuffer);
                    if (complete)
                    {
                        //grap one from queue
                        ISocketWritable tmp = null;
                        _writeQueue.TryTake(out tmp);
                        _lastWritable = tmp;
                    }
                    else
                    {
                        break;
                    }
                }
                if (_sendBuffer.Position > 0)
                {
                    _sendBuffer.Flip();
                    try
                    {
                        SocketError socketError;
                        var sendByteSize = _clientSocket.Send(
                            _sendBuffer.Array(),
                            _sendBuffer.Position,
                            _sendBuffer.Remaining(),
                            SocketFlags.None, out socketError);

                        if (sendByteSize <= 0)
                        {
                            Close();
                            return;
                        }

                        if (socketError != SocketError.Success)
                        {
                            Logger.Warning("Operation System Level Socket error code:" + socketError);
                            Close();
                            return;
                        }

                        _sendBuffer.Position += sendByteSize;
                        //logger.Info("SEND BUFFER CALLBACK: pos:" + sendBuffer.Position + " remaining:" + sendBuffer.Remaining());

                        //if success case
                        if (_sendBuffer.HasRemaining())
                        {
                            _sendBuffer.Compact();
                        }
                        else
                        {
                            _sendBuffer.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        _lastWritable = null;
                        HandleSocketException(e);
                    }
                }
            }
        }

        private class InvocationData
        {
            public readonly ClientInvocation clientInvocation;
            public readonly TaskCompletionSource<IClientMessage> responseFuture;

            public InvocationData(TaskCompletionSource<IClientMessage> responseFuture, ClientInvocation clientInvocation)
            {
                this.responseFuture = responseFuture;
                this.clientInvocation = clientInvocation;
            }
        }
    }
}