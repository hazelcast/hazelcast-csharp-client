using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
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
        private readonly ConcurrentDictionary<int, Task> _eventTasks = new ConcurrentDictionary<int, Task>();
        private readonly int _id;
        private readonly ObjectDataInputStream _in;
        private readonly ObjectDataOutputStream _out;
        private readonly ByteBuffer _receiveBuffer;
        private readonly bool _redoOperations;
        private readonly ConcurrentDictionary<int, Task> _requestTasks = new ConcurrentDictionary<int, Task>();
        private readonly ByteBuffer _sendBuffer;
        private readonly BlockingCollection<ISocketWritable> _writeQueue = new BlockingCollection<ISocketWritable>();
        private int _callIdCounter = 1;
        private volatile Socket _clientSocket;
        private volatile Address _endpoint;
        private volatile ISocketWritable _lastWritable;
        private volatile bool _live;
        private Thread _writeThread;

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection(ClientConnectionManager clientConnectionManager, int id, Address address,
            ClientNetworkConfig clientNetworkConfig,
            ISerializationService serializationService, bool redoOperations)
        {
            _clientConnectionManager = clientConnectionManager;
            _id = id;
            _redoOperations = redoOperations;

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

                //clientSocket.Connect(address.GetHost(), address.GetPort());

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
                var netStream = new NetworkStream(_clientSocket, false);
                var bufStream = new BufferedStream(netStream, bufferSize);

                var writer = new BinaryWriter(bufStream);
                var reader = new BinaryReader(bufStream);

                _out = serializationService.CreateObjectDataOutputStream(writer);
                _in = serializationService.CreateObjectDataInputStream(reader);

                _sendBuffer = ByteBuffer.Allocate(BufferSize);
                _receiveBuffer = ByteBuffer.Allocate(BufferSize);

                _builder = new ClientMessageBuilder(HandleClientMessage);

                _live = true;
            }
            catch (Exception e)
            {
                _clientSocket.Close();
                throw new IOException("Cannot connect!. Socket error:" + e.Message);
            }
        }

        public bool Live
        {
            get { return _live; }
        }

        internal int CurrentCallId
        {
            get { return _callIdCounter; }
        }

        internal int Id
        {
            get { return _id; }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            Logger.Finest("Closing socket" + _id);
            Release();
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            return _clientSocket != null ? (IPEndPoint) _clientSocket.LocalEndPoint : null;
        }

        public Address GetRemoteEndpoint()
        {
            return _endpoint;
        }

        public void RemoveConnectionCalls()
        {
            Logger.Finest("RemoveConnectionCalls id:" + _id + " ...START :" + _requestTasks.Count);
            foreach (var entry in _requestTasks)
            {
                Task removed;
                if (_requestTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
                if (_eventTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
            }
            //requestTasks.Clear();

            foreach (var entry in _eventTasks)
            {
                Task removed;
                if (_eventTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
            }
            //eventTasks.Clear();
        }

        public Task<IClientMessage> Send(IClientMessage clientRequest, int partitionId)
        {
            return Send(clientRequest, null, partitionId);
        }

        public Task<IClientMessage> Send(IClientMessage clientRequest, DistributedEventHandler handler, int partitionId)
        {
            clientRequest.AddFlag(ClientMessage.BeginAndEndFlags);
            var taskData = new TaskData(clientRequest, null, handler, partitionId);
            //create task
            var task = new Task<IClientMessage>(d => taskData.ResponseReady(), taskData);
            Send(task);
            return task;
        }

        public void SetRemoteEndpoint(Address address)
        {
            _endpoint = address;
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
                return "Connection [" + _endpoint + " -> " + localSocketAddress + "]";
            }
            return "Connection [" + _endpoint + " -> CLOSED ]";
        }

        public Task UnRegisterEvent(int callId)
        {
            Task _task = null;
            _eventTasks.TryRemove(callId, out _task);
            return _task;
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
            //if (sending.CompareAndSet(false, true))
            //{
            //    BeginWrite();
            //}
            //return true;
        }

        internal Socket GetSocket()
        {
            return _clientSocket;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void Init()
        {
            _out.Write(Encoding.UTF8.GetBytes(Protocols.ClientBinaryNew));
            _out.Flush();
        }

        internal void Send(Task task)
        {
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                return;
            }
            var callId = RegisterCall(task);
            var clientRequest = taskData.Request;
            clientRequest.AddFlag(ClientMessage.BeginAndEndFlags);
            if (taskData.PartitionId != -1)
            {
                clientRequest.SetPartitionId(taskData.PartitionId);
            }

            //enqueue to write queue
            //Console.clientRequest("SENDING:"+callId);
            if (!WriteAsync((ISocketWritable)clientRequest))
            {
                UnRegisterCall(callId);
                UnRegisterEvent(callId);

                //var genericError = new GenericError("TargetDisconnectedException", "Disconnected:" + GetRemoteEndpoint(), "", 0);
                HandleRequestTaskAsFailed(task);
            }
        }

        private bool _ReSend(Task task)
        {
            Logger.Finest("ReSending task:" + task.Id);
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                return false;
            }
            if (taskData.IncrementAndGetRetryCount() > ClientConnectionManager.RetryCount)
//                taskData.Request.SingleConnection) //TODO: a way to ensure transactional requests always get invoked on same connection 
            {
                return false;
            }
            Task.Factory.StartNew(() =>
            {
                //while (!_clientConnectionManager.OwnerLive)
                //{
                //    Console.WriteLine("WAITING FOR OWNER COME BACK TO RESEND");
                //    Thread.Sleep(ClientConnectionManager.RetryWaitTime);
                //}
                Thread.Sleep(ClientConnectionManager.RetryWaitTime);
                _clientConnectionManager.ReSend(task);
            }).ContinueWith(taskResend =>
            {
                if (taskResend.IsFaulted && taskResend.Exception != null)
                {
                    HandleRequestTaskAsFailed(task);
                }
            });
            return true;
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
                    //if (ThreadUtil.debug) { Console.Write(" BR:"+id); }
                    var socketError = SocketError.Success;
                    //Console.Write(id);
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

        private void HandleClientMessage(IClientMessage message)
        {
            if (message.IsFlagSet(ClientMessage.ListenerEventFlag))
            {
                object state = message.GetPartitionId();
                var eventTask = new Task(o => HandleEventPacket(message), state);
                eventTask.Start(_clientConnectionManager.TaskScheduler);
            }
            else
            {
                Task.Factory.StartNew(() => HandleReceivedPacket(message));
            }
        }

        private void HandleEventPacket(IClientMessage localPacket)
        {
            var clientResponse = localPacket; //serializationService.ToObject<ClientResponse>(response.GetData());
            var callId = clientResponse.GetCorrelationId();
            //IData response = clientResponse.GetData();
            //GenericError error = clientResponse.IsError ? serializationService.ToObject<GenericError>(response) : null;
            //if (error != null)
            //{
            //    logger.Severe("Event Response cannot be an exception :" + error.Name);
            //}
            //event handler
            Task task;
            _eventTasks.TryGetValue(callId, out task);
            if (task == null)
            {
                //we already unregistered the handler so simply ignore event
                //logger.Warning("No eventHandler for callId: " + callId + ", event: " + response);
                return;
            }
            var td = task.AsyncState as TaskData;
            if (td != null && td.Handler != null)
            {
                td.Handler(clientResponse);
            }
        }

        private void HandleReceivedPacket(IClientMessage response)
        {
            var callId = response.GetCorrelationId();
            Error error = null;

            if (response.GetMessageType() == Error.Type)
            {
                error = Error.Decode(response);
            }
            Task task;
            if (_requestTasks.TryRemove(callId, out task))
            {
                HandleRequestTask(task, response, error);
            }
            else
            {
                Logger.Finest("No call for callId: " + callId + ", response: " + response);
            }
        }

        private void HandleRequestTask(Task task, IClientMessage response, Error error)
        {
            if (task == null)
            {
                Logger.Finest("Task does not exists");
                return;
            }
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                Logger.Severe("TaskData cannot be null");
                return;
            }
            if (error != null)
            {
                if (error.ErrorCode == (int)ClientProtocolErrorCodes.TargetNotMember)
                {
                    if (_ReSend(task)) return;
                }
                if (error.ErrorCode == (int)ClientProtocolErrorCodes.HazelcastInstanceNotActive ||
                                           error.ErrorCode == (int)ClientProtocolErrorCodes.TargetDisconnected)
                {
                    if (taskData.Request.IsRetryable() || _redoOperations)
                    {
                        if (_ReSend(task)) return;
                    }
                }
            }
            if (taskData.Response != null && taskData.Handler == null)
            {
                Logger.Severe("Response can only be set once!!!");
                return;
            }

            if (taskData.Response != null && response != null)
            {
                //TODO: Re-register listener
//                var registrationId = _serializationService.ToObject<string>(taskData.Response); 
//                var alias = _serializationService.ToObject<string>(response);
//                _clientConnectionManager.ReRegisterListener(registrationId, alias, taskData.Request.GetCorrelationId());
                return;
            }
            ////////////////////////////////////////////

            UpdateResponse(task, response, error);

            //Response ready, lets run the task to return a Result
            if (!task.IsCompleted)
            {
                task.Start();
            }
            else
            {
                Logger.Severe("Already started task error");
            }
        }

        private void HandleRequestTaskAsFailed(Task task)
        {
            Error responseError;
            if (_clientConnectionManager.Live)
            {
                responseError = new Error((int)ClientProtocolErrorCodes.TargetDisconnected, "", "Disconnected:" + GetRemoteEndpoint(), null, null, null);
            }
            else
            {
                responseError = new Error((int)ClientProtocolErrorCodes.Hazelcast, "", "Client is shutting down.", "", null, null);
            }

            HandleRequestTask(task, null, responseError);
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

        private int NextCallId()
        {
            return Interlocked.Increment(ref _callIdCounter);
        }

        private int RegisterCall(Task task)
        {
            //FIXME make queue size constriant
            //register task
            var nextCallId = NextCallId();
            while (_requestTasks.ContainsKey(nextCallId) || _eventTasks.ContainsKey(nextCallId))
            {
                nextCallId = NextCallId();
            }

            var taskData = task.AsyncState as TaskData;
            var clientRequest = taskData != null ? taskData.Request : null;
            clientRequest.SetCorrelationId(nextCallId);

            _requestTasks.TryAdd(nextCallId, task);
            var td = task.AsyncState as TaskData;
            if (td != null && td.Handler != null)
            {
                _eventTasks.TryAdd(nextCallId, task);
            }
            return nextCallId;
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
                Logger.Finest("Closing socket, id:" + _id);
            }


            //bool lockTaken = false;
            //spinLock.Enter(ref lockTaken);
            //try
            //{
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();

            _out.Close();
            _in.Close();

            _clientSocket = null;

            if (_id > -1)
            {
                _clientConnectionManager.DestroyConnection(this);
                RemoveConnectionCalls();
            }
            //}
            //finally
            //{
            //    if (lockTaken) spinLock.Exit();
            //}
            //GetSocket().Close();
        }

        private void StartAsyncProcess()
        {
            BeginRead();
            _writeThread = new Thread(WriteQueueLoop) {IsBackground = true, Priority = ThreadPriority.Highest};
            _writeThread.Start();
        }

        private Task UnRegisterCall(int callId)
        {
            Task _task;
            _requestTasks.TryRemove(callId, out _task);
            return _task;
        }

        private static void UpdateResponse(Task task, IClientMessage response, Error error)
        {
            var taskData = task.AsyncState as TaskData;
            if (taskData != null)
            {
                taskData.Response = response;
                taskData.Error = error;
            }
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
    }


    internal class TaskData
    {
        private readonly object _mutex = new object();
        private volatile Error _error;
        private volatile DistributedEventHandler _handler;
        private volatile int _partitionId;
        private volatile IClientMessage _request;
        private volatile IClientMessage _response;
        private int _retryCount;

        public TaskData(IClientMessage request, IClientMessage response = null, DistributedEventHandler handler = null,
            int partitionId = -1)
        {
            _retryCount = 0;
            _request = request;
            _response = response;
            _handler = handler;
            _partitionId = partitionId;
        }

        internal Error Error
        {
            get { return _error; }
            set { _error = value; }
        }

        internal DistributedEventHandler Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }

        internal IClientMessage Request
        {
            get { return _request; }
            set { _request = value; }
        }

        internal IClientMessage Response
        {
            get { return _response; }
            set { _response = value; }
        }

        internal int RetryCount
        {
            get { return _retryCount; }
        }

        public int PartitionId
        {
            get { return _partitionId; }
            set { _partitionId = value; }
        }

        public IClientMessage ResponseReady()
        {
            Monitor.Enter(_mutex);
            Monitor.PulseAll(_mutex);
            Monitor.Exit(_mutex);
            return Response;
        }

        public bool Wait()
        {
            var result = true;
            Monitor.Enter(_mutex);
            if (_response == null)
            {
                result = Monitor.Wait(_mutex, ThreadUtil.TaskOperationTimeOutMilliseconds);
            }
            Monitor.Exit(_mutex);
            return result;
        }

        internal int IncrementAndGetRetryCount()
        {
            return Interlocked.Increment(ref _retryCount);
        }
    }
}