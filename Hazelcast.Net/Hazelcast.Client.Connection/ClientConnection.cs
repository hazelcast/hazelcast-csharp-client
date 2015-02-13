using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.Client.Spi;
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
        #region fields

        private static readonly ILogger logger = Logger.GetLogger(typeof (ClientConnection));

        public const int BufferSize = 1 << 15; //32k
        public const int socketReceiveBufferSize = 1 << 15; //32k
        public const int socketSendBufferSize = 1 << 15; //32k

        private readonly ObjectDataInputStream _in;
        private readonly ObjectDataOutputStream _out;

        private readonly int id;

        private volatile Address _endpoint;

        private volatile Socket clientSocket;
        //private long lastRead = Clock.CurrentTimeMillis();

        private readonly ISerializationService serializationService;

        private readonly ConcurrentDictionary<int, Task> requestTasks = new ConcurrentDictionary<int, Task>();

        private readonly ConcurrentDictionary<int, Task> eventTasks = new ConcurrentDictionary<int, Task>();

        private readonly BlockingCollection<ISocketWritable> writeQueue = new BlockingCollection<ISocketWritable>();

        private int _callIdCounter = 1;

        private readonly bool redoOperations;
        private readonly ClientConnectionManager _clientConnectionManager;

        private readonly ByteBuffer sendBuffer;
        private readonly ByteBuffer receiveBuffer;

        private volatile Packet packet;
        //private DataAdapter sendDataAdapter;

        private volatile ISocketWritable lastWritable;

        private volatile bool live = false;

        private AtomicBoolean sending = new AtomicBoolean(false);

        #endregion

        #region CONSTRUCTOR INIT ETC

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection(ClientConnectionManager clientConnectionManager, int id, Address address,
            ClientNetworkConfig clientNetworkConfig,
            ISerializationService serializationService, bool redoOperations)
        {
            _clientConnectionManager = clientConnectionManager;
            this.id = id;
            this.redoOperations = redoOperations;

            IPEndPoint isa = address.GetInetSocketAddress();
            var socketOptions = clientNetworkConfig.GetSocketOptions();
            ISocketFactory socketFactory = socketOptions.GetSocketFactory();

            this.serializationService = serializationService;
            if (socketFactory == null)
            {
                socketFactory = new DefaultSocketFactory();
            }
            clientSocket = socketFactory.CreateSocket();
            try
            {
                clientSocket = new Socket(isa.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                var lingerOption = new LingerOption(true, 5);
                if (socketOptions.GetLingerSeconds() > 0)
                {
                    lingerOption.LingerTime = socketOptions.GetLingerSeconds();
                }
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);


                clientSocket.NoDelay = socketOptions.IsTcpNoDelay();

                //TODO BURASI NOLCAK
                //clientSocket.ExclusiveAddressUse SetReuseAddress(options.IsReuseAddress());

                clientSocket.ReceiveTimeout = socketOptions.GetTimeout() > 0 ? socketOptions.GetTimeout() : -1;

                int bufferSize = socketOptions.GetBufferSize() * 1024;
                if (bufferSize < 0)
                {
                    bufferSize = BufferSize;
                }

                clientSocket.SendBufferSize = bufferSize;
                clientSocket.ReceiveBufferSize = bufferSize;

                clientSocket.UseOnlyOverlappedIO = true;

                //clientSocket.Connect(address.GetHost(), address.GetPort());

                var connectionTimeout = clientNetworkConfig.GetConnectionTimeout()>-1?clientNetworkConfig.GetConnectionTimeout():-1;
                var socketResult = clientSocket.BeginConnect(address.GetHost(), address.GetPort(),null,null);

                socketResult.AsyncWaitHandle.WaitOne(connectionTimeout,true);
                if (!clientSocket.Connected)
                {
                    // NOTE, MUST CLOSE THE SOCKET
                    clientSocket.Close();
                    throw new IOException("Failed to connect server.");
                }
                var netStream = new NetworkStream(clientSocket, false);
                var bufStream = new BufferedStream(netStream, bufferSize);

                var writer = new BinaryWriter(bufStream);
                var reader = new BinaryReader(bufStream);

                _out = serializationService.CreateObjectDataOutputStream(writer);
                _in = serializationService.CreateObjectDataInputStream(reader);

                sendBuffer = ByteBuffer.Allocate(BufferSize);
                receiveBuffer = ByteBuffer.Allocate(BufferSize);

                live = true;
            }
            catch (Exception e)
            {
                clientSocket.Close();
                throw new IOException("Cannot connect!. Socket error:" + e.Message);
            }
        }

        public void SwitchToNonBlockingMode()
        {
            if (clientSocket.Blocking)
            {
                sendBuffer.Clear();
                receiveBuffer.Clear();
                clientSocket.Blocking = false;
                StartAsyncProcess();
            }
            //ignore other cases
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal void InitProtocalData()
        {
            _out.Write(Encoding.UTF8.GetBytes(Protocols.ClientBinary));
            _out.Write(Encoding.UTF8.GetBytes(ClientTypes.Csharp));
            _out.Flush();
        }

        private void StartAsyncProcess()
        {
            BeginRead();
            new Thread(WriteQueueLoop).Start();
        }

        #endregion

        #region GETTER-SETTER

        public bool Live
        {
            get { return live; }
        }

        public Address GetRemoteEndpoint()
        {
            return _endpoint;
        }

        public void SetRemoteEndpoint(Address address)
        {
            _endpoint = address;
        }

        public IPEndPoint GetLocalSocketAddress()
        {
            return clientSocket != null ? (IPEndPoint) clientSocket.LocalEndPoint : null;
        }

        internal Socket GetSocket()
        {
            return clientSocket;
        }

        internal int CurrentCallId
        {
            get { return _callIdCounter;}
        }

        internal int Id
        {
            get { return id;}
        }
        #endregion

        #region BLOCKING IO

        private void WriteQueueLoop()
        {
            while (!writeQueue.IsAddingCompleted)
            {
                try
                {
                    lastWritable = writeQueue.Take();
                    
                }
                catch (InvalidOperationException)
                {
                    //BlockingCollection is empty
                }

                while (sendBuffer.HasRemaining() && lastWritable != null)
                {
                    bool complete = lastWritable.WriteTo(sendBuffer);
                    if (complete)
                    {
                        //grap one from queue
                        ISocketWritable tmp = null;
                        writeQueue.TryTake(out tmp);
                        lastWritable = tmp;
                    }
                    else
                    {
                        break;
                    }
                }
                if (sendBuffer.Position > 0)
                {
                    sendBuffer.Flip();
                    try
                    {
                        SocketError socketError;
                        int sendByteSize = clientSocket.Send(
                            sendBuffer.Array(),
                            sendBuffer.Position,
                            sendBuffer.Remaining(),
                            SocketFlags.None, out socketError);

                        if (sendByteSize <= 0)
                        {
                            Close();
                            return;
                        }

                        if (socketError != SocketError.Success)
                        {
                            logger.Warning("Operation System Level Socket error code:" + socketError);
                            Close();
                            return;
                        }

                        sendBuffer.Position += sendByteSize;
                        //logger.Info("SEND BUFFER CALLBACK: pos:" + sendBuffer.Position + " remaining:" + sendBuffer.Remaining());

                        //if success case
                        if (sendBuffer.HasRemaining())
                        {
                            sendBuffer.Compact();
                        }
                        else
                        {
                            sendBuffer.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        lastWritable = null;
                        HandleSocketException(e);
                    }
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void Write(IData data)
        {
            sendBuffer.Clear();
            var packet = new Packet(data, serializationService.GetPortableContext());
            packet.WriteTo(sendBuffer);
            var complete = false;
            while (!complete)
            {
                complete = packet.WriteTo(sendBuffer);
                sendBuffer.Flip();
                try
                {
                    SocketError socketError;
                    clientSocket.Send(
                        sendBuffer.Array(),
                        sendBuffer.Position,
                        sendBuffer.Remaining(),
                        SocketFlags.None, out socketError);
                }
                catch (Exception e)
                {
                    ExceptionUtil.Rethrow(e);
                }
                sendBuffer.Clear();
            }

        }

        /// <exception cref="System.IO.IOException"></exception>
        public IData Read()
        {
            receiveBuffer.Clear();
            var readFromSocket = true;
            var packet = new Packet(serializationService.GetPortableContext());
            while (true)
            {
                if (readFromSocket)
                {
                    try
                    {
                        SocketError socketError;
                        int receivedByteSize =
                            clientSocket.Receive(
                                receiveBuffer.Array(),
                                receiveBuffer.Position,
                                receiveBuffer.Remaining(),
                                SocketFlags.None, out socketError);
                        if (receivedByteSize <= 0)
                        {
                            throw new IOException("Remote socket closed!");
                        }
                        receiveBuffer.Position += receivedByteSize;
                        receiveBuffer.Flip();
                    }
                    catch (Exception)
                    {
                        throw new IOException("Remote socket closed!");
                    }
                }
                bool complete = packet.ReadFrom(receiveBuffer);
                if (complete)
                {
                    return packet.GetData();
                }

                if (receiveBuffer.HasRemaining())
                {
                    readFromSocket = false;
                    receiveBuffer.Compact();
                }
                else
                {
                    readFromSocket = true;
                    receiveBuffer.Clear();
                } 
            }

            //var data = new Data();
            //data.ReadData(_in);
            //lastRead = Clock.CurrentTimeMillis();
            //return data;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public object SendAndReceive(ClientRequest clientRequest)
        {
            IData request = serializationService.ToData(clientRequest);
            Write(request);
            IData responseData = Read();

            var clientResponse = serializationService.ToObject<ClientResponse>(responseData);
            if (clientResponse.IsError)
            {
                var resp = clientResponse.Response;
                var error = serializationService.ToObject<GenericError>(resp);
                ExceptionUtil.Rethrow(error);
            }
            var response = serializationService.ToObject<object>(clientResponse.Response);
            return response;
        }

        #endregion

        #region DISPOSING

        /// <exception cref="System.IO.IOException"></exception>
        private void Release()
        {
            if (!live)
            {
                return;
            }
            live = false;
            writeQueue.CompleteAdding();
            if (logger.IsFinestEnabled())
            {
                logger.Finest("Closing socket, id:" + id);
            }


            //bool lockTaken = false;
            //spinLock.Enter(ref lockTaken);
            //try
            //{
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            _out.Close();
            _in.Close();

            clientSocket = null;

            if (id > -1)
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

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
            logger.Finest("Closing socket" + id);
            Release();
        }

        public void RemoveConnectionCalls()
        {
            logger.Finest("RemoveConnectionCalls id:"+id+" ...START :"+ requestTasks.Count);
            foreach (var entry in requestTasks)
            {
                Task removed;
                if (requestTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
                if (eventTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
            }
            //requestTasks.Clear();

            foreach (var entry in eventTasks)
            {
                Task removed;
                if (eventTasks.TryRemove(entry.Key, out removed))
                {
                    HandleRequestTaskAsFailed(removed);
                }
            }
            //eventTasks.Clear();
        }

        #endregion

        #region ASYNC SEND RESEND REGISTER ETC

        public bool WriteAsync(ISocketWritable packet)
        {
            if (!live)
            {
                if (logger.IsFinestEnabled())
                {
                    logger.Finest("Connection is closed, won't write packet -> " + packet);
                }
                return false;
            }
            return writeQueue.TryAdd(packet);
            //if (sending.CompareAndSet(false, true))
            //{
            //    BeginWrite();
            //}
            //return true;
        }

        public Task<IData> Send(ClientRequest clientRequest, int partitionId)
        {
            return Send(clientRequest, null, partitionId);
        }

        public Task<IData> Send(ClientRequest clientRequest, DistributedEventHandler handler, int partitionId)
        {
            var taskData = new TaskData(clientRequest, null, handler, partitionId);
            //create task
            var task = new Task<IData>(taskObj => ResponseReady((TaskData) taskObj), taskData);
            Send(task);
            return task;
        }

        internal void Send(Task task)
        {
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                return;
            }
            var callId = RegisterCall(task);
            ClientRequest clientRequest = taskData.Request;
            IData data = serializationService.ToData(clientRequest);
            var packet = new Packet(data, taskData.PartitionId, serializationService.GetPortableContext());
            //enqueue to write queue
            //Console.WriteLine("SENDING:"+callId);
            if (!WriteAsync(packet))
            {
                UnRegisterCall(callId);
                UnRegisterEvent(callId);

                //var genericError = new GenericError("TargetDisconnectedException", "Disconnected:" + GetRemoteEndpoint(), "", 0);
                HandleRequestTaskAsFailed(task);
            }
        }

        /// <summary>
        ///     Request Task Execute Function. When response ready. This Func is called and result is return as Task.Result
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns>response result to Task.Result</returns>
        private IData ResponseReady(TaskData taskData)
        {
            if (taskData.Error != null)
            {
                ExceptionUtil.Rethrow(taskData.Error);
            }
            return taskData.Response;
        }

        private int RegisterCall(Task task)
        {
            //FIXME make queue size constriant
            //register task
            int nextCallId = NextCallId();
            while (requestTasks.ContainsKey(nextCallId) || eventTasks.ContainsKey(nextCallId))
            {
                nextCallId = NextCallId();
            }

            var taskData = task.AsyncState as TaskData;
            ClientRequest clientRequest = taskData != null ? taskData.Request : null;
            clientRequest.CallId = nextCallId;

            requestTasks.TryAdd(nextCallId, task);
            var td = task.AsyncState as TaskData;
            if (td != null && td.Handler != null)
            {
                eventTasks.TryAdd(nextCallId, task);
            }
            return nextCallId;
        }

        private Task UnRegisterCall(int callId)
        {
            Task _task;
            requestTasks.TryRemove(callId, out _task);
            return _task;
        }

        public Task UnRegisterEvent(int callId)
        {
            Task _task = null;
            eventTasks.TryRemove(callId, out _task);
            return _task;
        }

        private bool _ReSend(Task task)
        {
            logger.Finest("ReSending task:" + task.Id);
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                return false;
            }
            if (taskData.IncrementAndGetRetryCount() > ClientConnectionManager.RetryCount || taskData.Request.SingleConnection)
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
                if (taskResend.IsFaulted && taskResend.Exception !=null)
                {
                    HandleRequestTaskAsFailed(task);
                }
            });
            return true;
        }

        #endregion

        #region ASYNC PROCESS
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
                    clientSocket.BeginReceive(
                        receiveBuffer.Array(),
                        receiveBuffer.Position,
                        receiveBuffer.Remaining(),
                        SocketFlags.None, out socketError, EndReadCallback, null);
                }
                catch (Exception e)
                {
                    logger.Severe("Exception at Socket.Read for endPoint: " + GetRemoteEndpoint(), e);
                    HandleSocketException(e);
                }
            }
            catch (Exception e)
            {
                logger.Severe("Fatal Error at BeginRead : " + GetRemoteEndpoint(), e);
                HandleSocketException(e);
            }
        }

        private void EndReadCallback(IAsyncResult asyncResult)
        {
            if (clientSocket == null)
            {
                return;
            }
            try
            {
                //if (ThreadUtil.debug) { Console.Write(" ER"+id); }
                SocketError socketError;
                int receivedByteSize = clientSocket.EndReceive(asyncResult, out socketError);

                if (receivedByteSize <= 0)
                {
                    //Socket Closed
                    Close();
                }

                if (socketError != SocketError.Success)
                {
                    logger.Warning("Operation System Level Socket error code:" + socketError);
                    Close();
                    return;
                }
                receiveBuffer.Position += receivedByteSize;
                receiveBuffer.Flip();
                while (receiveBuffer.HasRemaining())
                {
                    if (packet == null)
                    {
                        packet = new Packet(serializationService.GetPortableContext());
                    }
                    bool complete = packet.ReadFrom(receiveBuffer);
                    if (complete)
                    {
                        //ASYNC HANDLE Received Packet
                        lock (packet)
                        {
                            Packet localPacket = packet;
                            if (packet.IsHeaderSet(Packet.HeaderEvent))
                            {
                                object state = packet.GetPartitionId();
                                var eventTask = new Task(o => HandleEventPacket(localPacket), state);
                                eventTask.Start(_clientConnectionManager.TaskScheduler);
                            }
                            else
                            {
                                Task.Factory.StartNew(() => HandleReceivedPacket(localPacket));
                            }
                            packet = null;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (receiveBuffer.HasRemaining())
                {
                    receiveBuffer.Compact();
                }
                else
                {
                    receiveBuffer.Clear();
                }
                //BeginRead();
            }
            catch (Exception e)
            {
                logger.Severe("Fatal Error at EndReadCallback : " + GetRemoteEndpoint(), e);
                HandleSocketException(e);
            }
            finally
            {
                BeginRead();
            }
        }

        private void HandleReceivedPacket(Packet localPacket)
        {
            var clientResponse = serializationService.ToObject<ClientResponse>(localPacket.GetData());
            int callId = clientResponse.CallId;
            //Console.WriteLine("HANDLING RECEIVED:"+callId);
            IData response = clientResponse.Response;
            GenericError error = clientResponse.IsError?serializationService.ToObject<GenericError>(response):null;
            Task task;
            if (requestTasks.TryRemove(callId, out task))
            {
                HandleRequestTask(task, response, error);
            }
            else
            {
                logger.Finest("No call for callId: " + callId + ", response: " + response);
            }
        }

        private void HandleEventPacket(Packet localPacket)
        {
            var clientResponse = serializationService.ToObject<ClientResponse>(localPacket.GetData());
            int callId = clientResponse.CallId;
            IData response = clientResponse.Response;
            GenericError error = clientResponse.IsError ? serializationService.ToObject<GenericError>(response) : null;
            if (error != null)
            {
                logger.Severe("Event Response cannot be an exception :" + error.Name);
            }
            //event handler
            Task task;
            eventTasks.TryGetValue(callId, out task);
            if (task == null)
            {
                //we already unregistered the handler so simply ignore event
                //logger.Warning("No eventHandler for callId: " + callId + ", event: " + response);
                return;
            }
            var td = task.AsyncState as TaskData;
            if (td != null && td.Handler != null)
            {
                td.Handler(response);
            }
        }

        private void HandleRequestTaskAsFailed(Task task)
        {
            GenericError responseGenericError;
            if (_clientConnectionManager.Live)
            {
                responseGenericError = new GenericError("TargetDisconnectedException", "Disconnected:" + GetRemoteEndpoint(), "", 0);
            }
            else
            {
                responseGenericError = new GenericError("HazelcastException", "Client is shutting down!!!", "", 0);
            }

            HandleRequestTask(task, null, responseGenericError);
        }

        private void HandleRequestTask(Task task, IData response, GenericError error)
        {
            if (task == null)
            {
                logger.Finest("Task not exist");
                return;
            }
            var taskData = task.AsyncState as TaskData;
            if (taskData == null)
            {
                logger.Severe("TaskData cannot be null");
                return;
            }
            if (error != null)
            {
                if (error.Name !=null && error.Name.Contains("TargetNotMemberException"))
                {
                    if (_ReSend(task)) return;
                }
                if (error.Name != null && (error.Name.Contains("HazelcastInstanceNotActiveException") ||
                    error.Name.Contains("TargetDisconnectedException")))
                {
                    if (taskData.Request is IRetryableRequest || redoOperations)
                    {
                        if (_ReSend(task)) return;
                    }
                }
            }
            if (taskData.Response != null && taskData.Handler == null)
            {
                logger.Severe("Response can only be set once!!!");
                return;
            }

            if (taskData.Response != null && response != null)
            {
                var registrationId = serializationService.ToObject<string>(taskData.Response);
                var alias = serializationService.ToObject<string>(response);
                _clientConnectionManager.ReRegisterListener(registrationId, alias, taskData.Request.CallId);
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
                logger.Severe("Already started task error");
            }
        }

        private void HandleSocketException(Exception e)
        {
            var se = e as SocketException;
            if (se != null)
            {
                int errorCode = se.ErrorCode;
                logger.Warning("Operation System Level Socket error code:" + errorCode);
            }
            else
            {
                logger.Warning(e.Message, e);
            }
            Close();
        }

        private static void UpdateResponse(Task task, IData response, GenericError error)
        {
            var taskData = task.AsyncState as TaskData;
            if (taskData != null)
            {
                taskData.Response = response;
                taskData.Error = error;
            }
        }

        #endregion

        private int NextCallId()
        {
            return Interlocked.Increment(ref _callIdCounter);
        }

        private bool CheckLive()
        {
            if (!live)
            {
                if (logger.IsFinestEnabled())
                {
                    logger.Finest("We are being asked to read, but connection is not live so we won't");
                }
                return false;
            }
            if (!clientSocket.Connected)
            {
                if (logger.IsFinestEnabled())
                {
                    logger.Finest("We are being asked to read, but connection is not connected...");
                }
                Task.Factory.StartNew(Close);
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            IPEndPoint localSocketAddress = GetLocalSocketAddress();
            if (localSocketAddress != null)
            {
                return "Connection [" + _endpoint + " -> " + localSocketAddress + "]";
            }
            return "Connection [" + _endpoint + " -> CLOSED ]";
        }
     }


    internal class TaskData
    {
        private volatile GenericError _error;
        private volatile IData _response;
        private volatile ClientRequest _request;
        private volatile DistributedEventHandler _handler;
        private int _retryCount;
        private volatile int _partitionId;

        public TaskData(ClientRequest request, IData response = null, DistributedEventHandler handler = null,
            int partitionId = -1)
        {
            _retryCount = 0;
            _request = request;
            _response = response;
            _handler = handler;
            _partitionId = partitionId;
        }

        internal GenericError Error
        {
            get { return _error; }
            set { _error = value; }
        }

        internal DistributedEventHandler Handler
        {
            get { return _handler; }
            set { _handler = value; }
        }

        internal ClientRequest Request
        {
            get { return _request; }
            set { _request = value; }
        }

        internal IData Response
        {
            get { return _response; }
            set { _response = value; }
        }

        internal int RetryCount
        {
            get { return _retryCount; }
        }

        internal int IncrementAndGetRetryCount()
        {
            return Interlocked.Increment(ref _retryCount);
        }

        public int PartitionId
        {
            get { return _partitionId; }
            set { _partitionId = value; }
        }
    }
}