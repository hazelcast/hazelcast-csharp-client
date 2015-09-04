using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Security;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    internal class ClientConnectionManager : IClientConnectionManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientConnectionManager));
        private readonly int _retryCount = 20;
        private readonly int _retryWaitTime = 250;
        private readonly int _heartBeatTimeout = 60000;
        private readonly int _heartBeatInterval = 5000;

        private readonly ConcurrentDictionary<Address, ClientConnection> _addresses =
            new ConcurrentDictionary<Address, ClientConnection>();

        private readonly HazelcastClient _client;

        private readonly ConcurrentBag<IConnectionListener> _connectionListeners =
            new ConcurrentBag<IConnectionListener>();

        private readonly object _connectionMutex = new object();
        private readonly ICredentials _credentials;

        private readonly ConcurrentBag<IConnectionHeartbeatListener> _heatHeartbeatListeners =
            new ConcurrentBag<IConnectionHeartbeatListener>();

        private readonly ClientNetworkConfig _networkConfig;
        private readonly Router _router;
        private readonly ISocketInterceptor _socketInterceptor;
        private Thread _heartBeatThread;
        private volatile bool _live;
        private volatile int _nextConnectionId;

        public ClientConnectionManager(HazelcastClient client, ILoadBalancer loadBalancer)
        {
            _client = client;
            _router = new Router(loadBalancer);

            var config = client.GetClientConfig();

            _networkConfig = config.GetNetworkConfig();

            config.GetNetworkConfig().IsRedoOperation();
            _credentials = config.GetCredentials();

            //init socketInterceptor
            var sic = config.GetNetworkConfig().GetSocketInterceptorConfig();
            if (sic != null && sic.IsEnabled())
            {
                //TODO SOCKET INTERCEPTOR
                throw new NotImplementedException("Socket Interceptor not Implemented!!!");
            }
            _socketInterceptor = null;

            var timeout = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.request.timeout");
            if (timeout > 0)
            {
                ThreadUtil.TaskOperationTimeOutMilliseconds = timeout.Value;
            }

            _retryCount = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.request.retry.count") ?? _retryCount;
            _retryWaitTime = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.request.retry.wait.time") ?? _retryWaitTime;
            _heartBeatTimeout = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.heartbeat.timeout") ?? _heartBeatTimeout;
            _heartBeatInterval = EnvironmentUtil.ReadEnvironmentVar("hazelcast.client.heartbeat.interval") ?? _heartBeatInterval;
        }

        public void Start()
        {
            if (_live)
            {
                return;
            }
            _live = true;
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
                        Logger.Finest("Exception during closing connection on shutdown");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning(e.Message);
            }
            return _live;
        }

        public bool Live
        {
            get { return _live; }
        }

        public Address BindToRandomAddress()
        {
            CheckLive();
            var address = _router.Next();
            return GetOrConnectWithRetry(address).GetAddress();
        }

        public void AddConnectionListener(IConnectionListener connectionListener)
        {
            _connectionListeners.Add(connectionListener);
        }

        public void AddConnectionHeartBeatListener(IConnectionHeartbeatListener connectonHeartbeatListener)
        {
            _heatHeartbeatListeners.Add(connectonHeartbeatListener);
        }

        public void DestroyConnection(ClientConnection clientConnection)
        {
            if (clientConnection != null && !clientConnection.Live)
            {
                DestroyConnection(clientConnection.GetAddress());
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection GetOrConnect(Address address, Authenticator authenticator)
        {
            if (address == null)
            {
                throw new ArgumentException("address");
            }
            lock (_connectionMutex)
            {
                var clientConnection = _addresses.GetOrAdd(address,
                    address1 =>
                    {
                        var connection = TryGetNewConnection(address, authenticator);
                        FireConnectionListenerEvent(f => f.ConnectionAdded(connection));
                        return connection;
                    });
                if (clientConnection == null)
                {
                    Logger.Severe("CONNECTION Cannot be NULL here");
                }
                return clientConnection;
            }
        }

        public ClientConnection GetConnection(Address address)
        {
            ClientConnection connection;
            return _addresses.TryGetValue(address, out connection) ? connection : null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection GetOrConnectWithRetry(Address target)
        {
            var count = 0;
            Exception lastError = null;
            var theTarget = target;
            while (count < _retryCount)
            {
                try
                {
                    if (theTarget == null || !IsMember(theTarget))
                    {
                        theTarget = _router.Next();
                    }
                    if (theTarget == null)
                    {
                        Logger.Severe("Address cannot be null here...");
                    }
                    return GetOrConnect(theTarget, ClusterAuthenticator);
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
                    Thread.Sleep(_retryWaitTime);
                }
                catch (Exception)
                {
                }
            }
            throw lastError;
        }

        /// <exception cref="HazelcastException"></exception>
        private void CheckLive()
        {
            if (!_live)
            {
                throw new HazelcastException("ConnectionManager is not active");
            }
        }

        private void ClusterAuthenticator(ClientConnection connection)
        {
            var ss = _client.GetSerializationService();
            var clusterService = (ClientClusterService) _client.GetClientClusterService();
            var principal = clusterService.GetPrincipal();

            var uuid = principal.GetUuid();
            var ownerUuid = principal.GetOwnerUuid();
            ClientMessage request;

            if (_credentials is UsernamePasswordCredentials)
            {
                var usernamePasswordCr = (UsernamePasswordCredentials) _credentials;
                request = ClientAuthenticationCodec.EncodeRequest(usernamePasswordCr.GetUsername(),
                    usernamePasswordCr.GetPassword(), uuid, ownerUuid, false,
                    ClientTypes.Csharp);
            }
            else
            {
                var data = ss.ToData(_credentials);
                request = ClientAuthenticationCustomCodec.EncodeRequest(data, uuid, ownerUuid, false,
                    ClientTypes.Csharp);
            }

            connection.Init();
            IClientMessage response;
            try
            {
                var future = ((ClientInvocationService)_client.GetInvocationService()).InvokeOnConnection(request, connection);
                response = ThreadUtil.GetResult(future);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            var rp = ClientAuthenticationCodec.DecodeResponse(response);
            var member = _client.GetClientClusterService().GetMember(rp.address);
            if (member == null)
            {
                throw new HazelcastException("Node with address '" + rp.address + "' was not found in the member list");
            }
            connection.SetRemoteMember(member);
        }

        private void DestroyConnection(Address address)
        {
            lock (_connectionMutex)
            {
                if (address != null)
                {
                    ClientConnection connection = null;
                    if (_addresses.TryRemove(address, out connection))
                    {
                        FireConnectionListenerEvent(f => f.ConnectionRemoved(connection));
                    }
                }
            }
        }

        private void FireConnectionListenerEvent(Action<IConnectionListener> listenerAction)
        {
            foreach (var listener in _connectionListeners)
            {
                listenerAction(listener);
            }
        }

        private void FireHeartBeatEvent(Action<IConnectionHeartbeatListener> listenerAction)
        {
            foreach (var listener in _heatHeartbeatListeners)
            {
                listenerAction(listener);
            }
        }

        private ClientConnection GetNewConnection(Address address, Authenticator authenticator)
        {
            ClientConnection connection = null;
            lock (_connectionMutex)
            {
                try
                {
                    var id = _nextConnectionId;
                    connection = new ClientConnection(this, (ClientInvocationService)_client.GetInvocationService(),
                        id,
                        address, _networkConfig);
                    if (_socketInterceptor != null)
                    {
                        _socketInterceptor.OnConnect(connection.GetSocket());
                    }
                    connection.SwitchToNonBlockingMode();
                    authenticator(connection);
                    Interlocked.Increment(ref _nextConnectionId);
                    return connection;
                }
                catch (Exception e)
                {
                    if (connection != null)
                    {
                        connection.Close();
                    }
                    throw ExceptionUtil.Rethrow(e, typeof (IOException));
                }
            }
        }

        private void HearthBeatLoop()
        {
            while (_heartBeatThread.IsAlive)
            {
                foreach (var clientConnection in _addresses.Values)
                {
                    var request = ClientPingCodec.EncodeRequest();
                    var task = ((ClientInvocationService)_client.GetInvocationService()).InvokeOnConnection(request, clientConnection);
                    var remoteEndPoint = clientConnection.GetSocket() != null
                        ? clientConnection.GetSocket().RemoteEndPoint.ToString()
                        : "CLOSED";
                    
                    Logger.Finest("Sending heartbeat request to " + remoteEndPoint);
                    try
                    {
                        var response = ThreadUtil.GetResult(task, _heartBeatTimeout);
                        var result = ClientPingCodec.DecodeResponse(response);
                        Logger.Finest("Got heartbeat response from " + remoteEndPoint);
                    }
                    catch (Exception e)
                    {
                        Logger.Warning(string.Format("Error getting heartbeat from {0}: {1}", remoteEndPoint, e));
                        var connection = clientConnection;
                        FireHeartBeatEvent((listener) => listener.HeartBeatStopped(connection));
                    }
                }
                try
                {
                    Thread.Sleep(_heartBeatInterval);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private bool IsMember(Address target)
        {
            var clientClusterService = _client.GetClientClusterService();
            return clientClusterService.GetMember(target) != null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="HazelcastException"></exception>
        private ClientConnection TryGetNewConnection(Address address, Authenticator authenticator)
        {
            CheckLive();
            return GetNewConnection(address, authenticator);
        }
    }
}