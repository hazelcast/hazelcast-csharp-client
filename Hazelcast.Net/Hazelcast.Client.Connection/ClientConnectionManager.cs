using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Cluster;
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

        public const int RetryCount = 20;
        public const int DefaultEventThreadCount = 3;
        private static readonly ILogger logger = Logger.GetLogger(typeof (IClientConnectionManager));

        private readonly ConcurrentDictionary<Address, LinkedListNode<ClientConnection>> _addresses = new ConcurrentDictionary<Address, LinkedListNode<ClientConnection>>();
        private readonly ConcurrentDictionary<string, int> _registrationMap = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, string> _registrationAliasMap = new ConcurrentDictionary<string, string>();

        private readonly LinkedList<ClientConnection> _clientConnections =new LinkedList<ClientConnection>();

        private readonly StripedTaskScheduler _taskScheduler;

        private readonly ICredentials _credentials;

        private readonly bool _redoOperation;
        private readonly bool _smartRouting;

        private readonly Authenticator authenticator;
        private readonly HazelcastClient client;
        private readonly Router router;

        private readonly ISocketInterceptor socketInterceptor;
        private readonly SocketOptions socketOptions;
        private volatile bool _live;

        private volatile int _nextConnectionId;
        LinkedListNode<ClientConnection> nextConnectionNode = null;
        //private volatile int _whoisnextId;

        private ClientConnection _ownerConnection;
        private volatile ClientPrincipal principal;

        private Thread _heartBeatThread;
        #endregion

        public ClientConnectionManager(HazelcastClient client, LoadBalancer loadBalancer, bool smartRouting = true)
        {
            this.client = client;
            authenticator = ClusterAuthenticator;
            router = new Router(loadBalancer);
            _smartRouting = smartRouting;

            ClientConfig config = client.GetClientConfig();
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

            //        int connectionTimeout = config.getConnectionTimeout(); //TODO
            socketOptions = config.GetNetworkConfig().GetSocketOptions();
            int eventTreadCount = 0;

            var param = Environment.GetEnvironmentVariable("hazelcast.client.event.thread.count");
            try
            {
                if (param != null)
                {
                    eventTreadCount = Convert.ToInt32(param, 10);
                }
            }
            catch (Exception)
            {
                
                logger.Warning("Provided event thread count is not a valid value : "+param);
            }
            eventTreadCount = eventTreadCount == 0 ? DefaultEventThreadCount : eventTreadCount;
            _taskScheduler = new StripedTaskScheduler(eventTreadCount);
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
                //IsBackground = true,
                Name = ("HearthBeat" + new Random().Next()).Substring(0, 15)
            };
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
                _heartBeatThread.Join();
            }
            try
            {
                var _itrNode = _clientConnections.Last;
                if (_itrNode != null)
                {
                    while (_itrNode != null)
                    {
                        try
                        {
                            var nxt=_itrNode.Previous;
                            _itrNode.Value.Close();
                            _itrNode = nxt;
                        }
                        catch (Exception) { }
                    }
                }

                _clientConnections.Clear();
                try
                {
                    _ownerConnection.Close();
                }
                catch (Exception) { }
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

        public object SendAndReceiveFromOwner(ClientRequest clientRequest)
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

        /// <summary>
        ///     used by in & out threads the process the next connection
        /// </summary>
        /// <returns>next ClientConnection</returns>
        internal ClientConnection NextProcessConnection()
        {
            try
            {
                for (;;)
                {
                    var current = nextConnectionNode;// ?? _clientConnections.First;
                    var newVal = (current == null) ? _clientConnections.First : current.Next ?? _clientConnections.First;
                    if (Interlocked.CompareExchange(ref nextConnectionNode, newVal, current) == current)
                    {
                        return current!=null?current.Value:null;
                    }
                }
            }
            catch (Exception)
            {
                logger.Finest("NextProcessConnection problem...");
            }
            return null;
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
                logger.Finest("ClientManager is Destroying Connection con:" + clientConnection);
                DestroyConnection(clientConnection.GetRemoteEndpoint());
            }
        }

        #endregion

        #region ClientConnectionManager Privates
        private void DestroyConnection(Address address)
        {
            if (address != null)
            {
                lock ((_addresses))
                {
                    LinkedListNode<ClientConnection> linkedListNode=null;
                    if (_addresses.TryRemove(address, out linkedListNode))
                    {
                        _clientConnections.Remove(linkedListNode);
                    }
                }

                //if (linkedListNode != null)
                //{
                //    DestroyConnection(linkedListNode.Value);
                //}

            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="HazelcastException"></exception>
        private ClientConnection TryGetNewConnection(Address address, Authenticator _authenticator, bool blocking)
        {
            CheckLive();
            return GetNewConnection(address, _authenticator, blocking);
        }

        private ClientConnection GetNewConnection(Address address, Authenticator _authenticator, bool blocking)
        {
            lock (this)
            {
                int id = blocking ? -1 : _nextConnectionId;
                var connection = new ClientConnection(this, id, address, socketOptions, client.GetSerializationService(),
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
        }

        /// <exception cref="System.IO.IOException"></exception>
        private ClientConnection _GetOrConnectWithRetry(Address target)
        {
            int count = 0;
            IOException lastError = null;
            while (count < RetryCount)
            {
                try
                {
                    var theTarget = target;
                    if (target == null || !isMember(target))
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
                target = null;
                count++;
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
            ClientConnection clientConnection = null;
            if (!_addresses.ContainsKey(address))
            {
                lock (_addresses)
                {
                    if (!_addresses.ContainsKey(address))
                    {
                        clientConnection = TryGetNewConnection(address, _authenticator, false);
                        var linkedListNode = _clientConnections.AddLast(clientConnection);
                        _addresses.TryAdd(address, linkedListNode);
                    }
                }
            }
            LinkedListNode<ClientConnection> clientConnectionNode = null;
            if (_addresses.TryGetValue(address, out clientConnectionNode))
            {
                clientConnection=clientConnectionNode.Value;
            }
            if (clientConnection == null)
            {
                logger.Severe("CONNECTION NULL");
            }
            return clientConnection;
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
                    try
                    {
                        ClientConnection connection = GetNewConnection(address, ManagerAuthenticator, true);
                        _live = true;
                        FireConnectionEvent(false);
                        return connection;
                    }
                    catch (IOException e)
                    {
                        lastError = e;
                        logger.Finest("IO error during initial connection...", e);
                    }
                    catch (AuthenticationException e)
                    {
                        lastError = e;
                        logger.Warning("Authentication error on " + address, e);
                    }
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
            return _clientConnections.Any(clientConnection => clientConnection.UnRegisterEvent(callId) != null);
        }

        private void HearthBeatLoop()
        {
            while (_heartBeatThread.IsAlive)
            {
                foreach (var clientConnection in _clientConnections)
                {
                    var request = new ClientPingRequest();
                    clientConnection.Send(request, -1);
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
            _Authenticate<object>(connection, _credentials, principal, false, false);
        }

        private void ManagerAuthenticator(ClientConnection connection)
        {
            principal = _Authenticate<ClientPrincipal>(connection, _credentials, principal, true, true);
        }


        private T _Authenticate<T>(ClientConnection connection, ICredentials credentials, ClientPrincipal principal,
            bool reAuth, bool firstConnection)
        {
            ISerializationService ss = client.GetSerializationService();
            var auth = new AuthenticationRequest(credentials, principal);
            connection.InitProtocalData();
            auth.SetReAuth(reAuth);
            auth.SetFirstConnection(firstConnection);
            SerializableCollection coll;
            try
            {
                coll = (SerializableCollection)connection.SendAndReceive(auth);
            }
            catch (Exception e)
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
            clientConnection.Send(task);
        }

        public Task<IData> Send(ClientRequest request)
        {
            return Send(request, null, -1);
        }

        public Task<IData> Send(ClientRequest request, Address target)
        {
            return Send(request, target, -1);
        }

        public Task<IData> Send(ClientRequest request, Address target, int partitionId)
        {
            ClientConnection clientConnection = _GetOrConnectWithRetry(target);
            return clientConnection.Send(request, partitionId);
        }

        public Task<IData> SendAndHandle(ClientRequest request, DistributedEventHandler handler)
        {
            return SendAndHandle(request, null, handler);
        }

        public Task<IData> SendAndHandle(ClientRequest request, Address target,
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