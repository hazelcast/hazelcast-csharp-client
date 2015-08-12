using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;
using ICredentials = Hazelcast.Security.ICredentials;

namespace Hazelcast.Client.Connection
{
    internal class ClientConnectionManager : IClientConnectionManager
    {
        #region fields

        public static int RetryCount = 20;
        public static int RetryWaitTime = 250;
        public const int DefaultEventThreadCount = 3;
        private static readonly ILogger logger = Logger.GetLogger(typeof (IClientConnectionManager));

        private readonly ConcurrentDictionary<Address, ClientConnection> _addresses = new ConcurrentDictionary<Address, ClientConnection>();
        private readonly ConcurrentDictionary<string, int> _registrationMap = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, string> _registrationAliasMap = new ConcurrentDictionary<string, string>();

        //private readonly LinkedList<ClientConnection> _clientConnections =new LinkedList<ClientConnection>();

        private readonly StripedTaskScheduler _taskScheduler;

        private readonly ICredentials _credentials;

        private readonly bool _redoOperation;
        private readonly bool _smartRouting;

        private readonly Authenticator authenticator;
        private readonly HazelcastClient client;
        private readonly Router router;

        private readonly ISocketInterceptor socketInterceptor;
        private ClientNetworkConfig _networkConfig;
        private volatile bool _live;

        private volatile int _nextConnectionId;
        LinkedListNode<ClientConnection> nextConnectionNode = null;
        //private volatile int _whoisnextId;

        private volatile ClientConnection _ownerConnection;
        private volatile ClientPrincipal principal;

        private Thread _heartBeatThread;

        private object _connectionMutex = new object();

        #endregion

        public ClientConnectionManager(HazelcastClient client, LoadBalancer loadBalancer, bool smartRouting = true)
        {
            this.client = client;
            authenticator = ClusterAuthenticator;
            router = new Router(loadBalancer);
            _smartRouting = smartRouting;

            ClientConfig config = client.GetClientConfig();

            _networkConfig = config.GetNetworkConfig();

            _redoOperation = config.GetNetworkConfig().IsRedoOperation();
            _credentials = config.GetCredentials();

            //init socketInterceptor
            SocketInterceptorConfig sic = config.GetNetworkConfig().GetSocketInterceptorConfig();
            if (sic != null && sic.IsEnabled())
            {
                //TODO SOCKET INTERCEPTOR
                throw new NotImplementedException("Socket Interceptor not Implemented!!!");
            }
            socketInterceptor = null;

            var eventTreadCount = ReadEnvironmentVar("hazelcast.client.event.thread.count");
            eventTreadCount = eventTreadCount > 0 ? eventTreadCount : DefaultEventThreadCount;
            _taskScheduler = new StripedTaskScheduler(eventTreadCount);

            var timeout = ReadEnvironmentVar("hazelcast.client.request.timeout");
            if (timeout > 0)
            {
                ThreadUtil.TaskOperationTimeOutMilliseconds = timeout;
            }
            var retryCount = ReadEnvironmentVar("hazelcast.client.request.retry.count");
            if (retryCount > 0)
            {
                RetryCount = retryCount;
            }
            var retryWaitTime = ReadEnvironmentVar("hazelcast.client.request.retry.wait.time");
            if (retryWaitTime > 0)
            {
                RetryWaitTime = retryWaitTime;
            }
        }

        private int ReadEnvironmentVar(string var)
        {
            int p = 0;
            var param = Environment.GetEnvironmentVariable(var);
            try
            {
                if (param != null)
                {
                    p = Convert.ToInt32(param, 10);
                }
            }
            catch (Exception)
            {
                logger.Warning("Provided value is not a valid value : " + param);
            }
            return p;
        }

        #region IConnectionManager

        public void Start()
        {
            if (_live)
            {
                return;
            }
            _live = true;
            InitOwnerConnection();
            //start HeartBeat
            _heartBeatThread = new Thread(HearthBeatLoop)
            {
                IsBackground = true,
                Name = ("HearthBeat" + new Random().Next()).Substring(0, 15)
            };
            _heartBeatThread.Start();
        }

        public bool Shutdown()
        {
            if (!_live)
            {
                return _live;
            }
            _live = false;

            //Stop heartBeat
            if (_heartBeatThread.IsAlive)
            {
                _heartBeatThread.Interrupt();
                //_heartBeatThread.Join();
            }
            try
            {
                foreach (var kvPair in _addresses)
                {
                    try
                    {
                        kvPair.Value.Close();
                    }
                    catch (Exception)
                    {
                        logger.Finest("Exception during closing connection on shutdown");
                    }
                }
                try
                {
                    _ownerConnection.Close();
                }
                catch (Exception)
                {
                    logger.Finest("Exception during closing owner connection on shutdown");
                }
                //_ownerConnection = null;

                _taskScheduler.Dispose();
            }
            catch (Exception e)
            {
                logger.Warning(e.Message);
            }
            _live = _ownerConnection != null && _ownerConnection.Live;
            return _live;
        }

        public bool Live
        {
            get { return _live; }
        }
        public bool OwnerLive
        {
            get { return _ownerConnection != null && _ownerConnection.Live; }
        }

        public Address BindToRandomAddress()
        {
            CheckLive();
            Address address = router.Next();
            return _GetOrConnectWithRetry(address).GetRemoteEndpoint();
        }

        public object SendAndReceiveFromOwner(IClientMessage clientRequest)
        {
            if (_ownerConnection != null)
            {
                return _ownerConnection.SendAndReceive(clientRequest);
            }
            throw new HazelcastException("Cannot connect to Cluster");
        }

        public IData ReadFromOwner()
        {
            if (_ownerConnection != null)
            {
                return _ownerConnection.Read();
            }
            throw new HazelcastException("Cannot connect to Cluster");
        }

        public void FireConnectionEvent(bool disconnected)
        {
            var lifecycleService = (LifecycleService) client.GetLifecycleService();
            LifecycleEvent.LifecycleState state = disconnected
                ? LifecycleEvent.LifecycleState.ClientDisconnected
                : LifecycleEvent.LifecycleState.ClientConnected;
            lifecycleService.FireLifecycleEvent(state);

            if (disconnected && _ownerConnection != null)
            {
                _ownerConnection.Close();
            }
        }

        public Address OwnerAddress()
        {
           return _ownerConnection != null ? _ownerConnection.GetRemoteEndpoint():null;
        }

        public Client GetLocalClient()
        {
            ClientPrincipal cp = principal;
            IPEndPoint socketAddress = _ownerConnection != null ? _ownerConnection.GetLocalSocketAddress() : null;
            string uuid = cp != null ? cp.GetUuid() : null;
            return new Client(uuid, socketAddress);
        }

        internal void DestroyConnection(ClientConnection clientConnection)
        {
            if (clientConnection != null && !clientConnection.Live)
            {
                 DestroyConnection(clientConnection.GetRemoteEndpoint());
            }
        }

        #endregion

        #region ClientConnectionManager Privates
        private void DestroyConnection(Address address)
        {
            lock (_connectionMutex)
            {
                if (address != null)
                {
                    ClientConnection connection = null;
                    _addresses.TryRemove(address, out connection);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="HazelcastException"></exception>
        private ClientConnection TryGetNewConnection(Address address, Authenticator _authenticator)
        {
            CheckLive();
            return GetNewConnection(address, _authenticator, false);
        }

        private ClientConnection GetNewConnection(Address address, Authenticator _authenticator, bool blocking)
        {
            ClientConnection connection = null;
            lock (_connectionMutex)
            {
                try
                {
                    int id = blocking ? -1 : _nextConnectionId;
                    connection = new ClientConnection(this, id, address, _networkConfig, client.GetSerializationService(),
                        _redoOperation);
                    if (socketInterceptor != null)
                    {
                        socketInterceptor.OnConnect(connection.GetSocket());
                    }
                    _authenticator(connection);
                    if (!blocking)
                    {
                        connection.SwitchToNonBlockingMode();
                        Interlocked.Increment(ref _nextConnectionId);
                    }
                    return connection;
                }
                catch (Exception e)
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                    ExceptionUtil.Rethrow(e, typeof(IOException));
                }
            }
            return null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        private ClientConnection _GetOrConnectWithRetry(Address target)
        {
            int count = 0;
            Exception lastError = null;
            var theTarget = target;
            while (count < RetryCount)
            {
                try
                {
                    if (theTarget == null || !isMember(theTarget))
                    {
                        theTarget = router.Next();
                    }
                    if (theTarget == null)
                    {
                        logger.Severe("Address cannot be null here...");
                    }
                    return _GetOrConnect(theTarget, authenticator);
                }
                catch (IOException e)
                {
                    lastError = e;
                }
                catch (HazelcastInstanceNotActiveException e)
                {
                    lastError = e;
                }
                theTarget = null;
                count++;
                try
                {
                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                }
            }
            throw lastError;
        }

            
        private bool isMember(Address target) 
        {
            var clientClusterService = client.GetClientClusterService();
            return clientClusterService.GetMember(target) != null;
        }


        /// <exception cref="System.IO.IOException"></exception>
        private ClientConnection _GetOrConnect(Address address, Authenticator _authenticator)
        {
            if (address == null)
            {
                throw new ArgumentException("address");
            }
            if (!_smartRouting)
            {
                address = _ownerConnection.GetRemoteEndpoint();
            }
            lock (_connectionMutex)
            {

                ClientConnection clientConnection = _addresses.GetOrAdd(address, address1 =>
                {
                    return clientConnection = TryGetNewConnection(address, _authenticator);
                });
                if (clientConnection == null)
                {
                    logger.Severe("CONNECTION Cannot be NULL here");
                }
                return clientConnection;
            }
        }

        public bool InitOwnerConnection()
        {
            if (_live)
            {
                ICollection<IPEndPoint> ipEndPoints = GetEndPoints();
                _ownerConnection = ConnectToOwner(ipEndPoints);
                logger.Finest("Owner connection established: " + _ownerConnection);
            }
            return _ownerConnection != null;
        }

        private ClientConnection ConnectToOwner(ICollection<IPEndPoint> socketAddresses)
        {
            int connectionAttemptLimit = client.GetClientConfig().GetNetworkConfig().GetConnectionAttemptLimit();
            int attempt = 0;
            Exception lastError = null;
            while (true)
            {
                long nextTry = Clock.CurrentTimeMillis() + client.GetClientConfig().GetNetworkConfig().GetConnectionAttemptPeriod();
                foreach (IPEndPoint isa in socketAddresses)
                {
                    var address = new Address(isa);
                    ClientConnection connection=null;
                    try
                    {
                        connection = GetNewConnection(address, ManagerAuthenticator, true);
                        _live = true;
                        FireConnectionEvent(false);
                        return connection;
                    }
                    catch (AuthenticationException e)
                    {
                        lastError = e;
                        logger.Warning("Authentication error during initial connection to " + address, e);
                    }
                    catch (Exception e)
                    {
                        lastError = e;
                        logger.Finest("Exception during initial connection to" + address, e);
                    }
                    //if (connection != null)
                    //{
                    //    connection.Close();
                    //}
                }
                if (attempt++ >= connectionAttemptLimit)
                {
                    break;
                }
                var remainingTime = (int) (nextTry - Clock.CurrentTimeMillis());
                logger.Warning(
                    string.Format("Unable to get alive cluster connection, try in {0} ms later, attempt {1} of{2}.",
                        Math.Max(0, remainingTime), attempt, connectionAttemptLimit));
                if (remainingTime > 0)
                {
                    try
                    {
                        Thread.Sleep(remainingTime);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            throw new InvalidOperationException("Unable to connect to any address in the config!", lastError);
        }


        /// <exception cref="System.Exception"></exception>
        private ICollection<IPEndPoint> GetEndPoints()
        {
            ICollection<IMember> memberList = client.GetClientClusterService().GetMemberList();

            List<IPEndPoint> socketAddresses = memberList.Select(member => member.GetSocketAddress()).ToList();
            var r = new Random();
            IOrderedEnumerable<IPEndPoint> shuffled = socketAddresses.OrderBy(x => r.Next());

            var ipEndPoints = new List<IPEndPoint>(shuffled);

            var addresses = new HashSet<IPEndPoint>();
            if (memberList.Count > 0)
            {
                addresses.UnionWith(ipEndPoints);
            }
            addresses.UnionWith(GetConfigAddresses());

            return addresses;
        }

        private ICollection<IPEndPoint> GetConfigAddresses()
        {
            var socketAddresses = new List<IPEndPoint>();
            foreach (string address in client.GetClientConfig().GetNetworkConfig().GetAddresses())
            {
                ICollection<IPEndPoint> endPoints = AddressHelper.GetSocketAddresses(address);
                socketAddresses = socketAddresses.Union(endPoints).ToList();
            }

            var r = new Random();
            IOrderedEnumerable<IPEndPoint> shuffled = socketAddresses.OrderBy(x => r.Next());
            return new List<IPEndPoint>(shuffled);
        }

        /// <exception cref="HazelcastException"></exception>
        private void CheckLive()
        {
            if (!_live)
            {
                throw new HazelcastException("ConnectionManager is not active!!!");
            }
        }

        private bool RemoveEventHandler(int callId)
        {
            return _addresses.Values.Any(clientConnection => clientConnection.UnRegisterEvent(callId) != null);
            //return _clientConnections.Any(clientConnection => clientConnection.UnRegisterEvent(callId) != null);
        }

        private void HearthBeatLoop()
        {
            while (_heartBeatThread.IsAlive)
            {
                foreach (var clientConnection in _addresses.Values)
                {
                    var request = ClientPingCodec.EncodeRequest();
                    var task = clientConnection.Send(request, -1);
                    var remoteEndPoint = clientConnection.GetSocket() != null ? clientConnection.GetSocket().RemoteEndPoint.ToString() : "CLOSED";
                    try
                    {
                        var result = ThreadUtil.GetResult(task);
                        //Console.WriteLine("PING:" + remoteEndPoint);
                    }
                    catch (Exception)
                    {

                        //Console.WriteLine("PING ERROR:" + remoteEndPoint);
                    }
                }
                try
                {
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        #endregion

        #region Authenticators
        private void ClusterAuthenticator(ClientConnection connection)
        {
            _Authenticate<object>(connection, _credentials, principal, false);
        }

        private void ManagerAuthenticator(ClientConnection connection)
        {
            principal = _Authenticate<ClientPrincipal>(connection, _credentials, principal, true);
        }


        private T _Authenticate<T>(ClientConnection connection, ICredentials credentials, ClientPrincipal principal, bool firstConnection)
        {
            ISerializationService ss = client.GetSerializationService();
            var auth = ClientAuthenticationCodec.EncodeRequest(??, ??, principal.GetUuid(), principal.GetOwnerUuid(), ??);
            connection.InitProtocalData();
            auth.SetFirstConnection(firstConnection);
            SerializableCollection coll = null;
            try
            {
                coll = (SerializableCollection)connection.SendAndReceive(auth);
            }
            catch (Exception e)
            {
                //ignore the exception, it will be handled on next line if coll is null
            }
            if (coll == null)
            {
                throw new IOException("Retry this");
            }
            IEnumerator<IData> enumerator = coll.GetEnumerator();
            enumerator.MoveNext();
            if (enumerator.Current != null)
            {
                IData addressData = enumerator.Current;
                var address = ss.ToObject<Address>(addressData);
                connection.SetRemoteEndpoint(address);
                enumerator.MoveNext();
                if (enumerator.Current != null)
                {
                    IData principalData = enumerator.Current;
                    return ss.ToObject<T>(principalData);
                }
            }
            throw new AuthenticationException(); //TODO
        }

        #endregion

        #region IRemotingService

        internal void ReSend(Task task)
        {
            ClientConnection clientConnection = _GetOrConnectWithRetry(null);
            //Console.WriteLine("RESEND TO:"+clientConnection.GetSocket().RemoteEndPoint);
            clientConnection.Send(task);
        }

        public Task<IClientMessage> Send(IClientMessage request)
        {
            return Send(request, null, -1);
        }

        public Task<IClientMessage> Send(IClientMessage request, Address target)
        {
            return Send(request, target, -1);
        }

        public Task<IClientMessage> Send(IClientMessage request, Address target, int partitionId)
        {
            ClientConnection clientConnection = _GetOrConnectWithRetry(target);
            return clientConnection.Send(request, partitionId);
        }

        public Task<IClientMessage> SendAndHandle(IClientMessage request, DistributedEventHandler handler)
        {
            return SendAndHandle(request, null, handler);
        }

        public Task<IClientMessage> SendAndHandle(IClientMessage request, Address target,
            DistributedEventHandler handler)
        {
            ClientConnection clientConnection = _GetOrConnectWithRetry(target);
            return clientConnection.Send(request, handler, -1);
        }

        public void RegisterListener(string registrationId, int callId)
        {
            _registrationAliasMap.TryAdd(registrationId, registrationId);
            _registrationMap.TryAdd(registrationId, callId);
        }

        public bool UnregisterListener(string registrationId)
        {
            string uuid;
            if (_registrationAliasMap.TryRemove(registrationId, out uuid))
            {
                int callId ;
                if (_registrationMap.TryRemove(registrationId, out callId))
                {
                    return RemoveEventHandler(callId);
                }
            }
            return false;
        }

        public void ReRegisterListener(string uuidregistrationId, string alias, int callId)
        {
            string oldAlias = null;
            if (_registrationAliasMap.TryRemove(uuidregistrationId, out oldAlias))
            {
                int removed;
                _registrationMap.TryRemove(oldAlias,out removed);
                _registrationMap.TryAdd(alias, callId);
            }
            _registrationAliasMap.TryAdd(uuidregistrationId, alias);
        }

        #endregion

        public StripedTaskScheduler TaskScheduler
        {
            get { return _taskScheduler; }
        }
    }
}