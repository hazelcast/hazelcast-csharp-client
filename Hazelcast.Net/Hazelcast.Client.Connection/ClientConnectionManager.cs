using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public const int DefaultEventThreadCount = 3;
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientConnectionManager));
        public static int RetryCount = 20;
        public static int RetryWaitTime = 250;

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
        private readonly bool _redoOperation;

        private readonly ConcurrentDictionary<string, string> _registrationAliasMap =
            new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, int> _registrationMap = new ConcurrentDictionary<string, int>();
        private readonly Router _router;
        private readonly bool _smartRouting;
        private readonly ISocketInterceptor _socketInterceptor;
        private readonly StripedTaskScheduler _taskScheduler;
        private Thread _heartBeatThread;
        private volatile bool _live;
        private volatile int _nextConnectionId;
        private LinkedListNode<ClientConnection> _nextConnectionNode = null;
        //private volatile int _whoisnextId;

        private volatile ClientPrincipal _principal;

        public ClientConnectionManager(HazelcastClient client, LoadBalancer loadBalancer, bool smartRouting = true)
        {
            _client = client;
            _router = new Router(loadBalancer);
            _smartRouting = smartRouting;

            var config = client.GetClientConfig();

            _networkConfig = config.GetNetworkConfig();

            _redoOperation = config.GetNetworkConfig().IsRedoOperation();
            _credentials = config.GetCredentials();

            //init socketInterceptor
            var sic = config.GetNetworkConfig().GetSocketInterceptorConfig();
            if (sic != null && sic.IsEnabled())
            {
                //TODO SOCKET INTERCEPTOR
                throw new NotImplementedException("Socket Interceptor not Implemented!!!");
            }
            _socketInterceptor = null;

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

        public StripedTaskScheduler TaskScheduler
        {
            get { return _taskScheduler; }
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

                _taskScheduler.Dispose();
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
            return GetOrConnectWithRetry(address).GetRemoteEndpoint();
        }

        public void AddConnectionListener(IConnectionListener connectionListener)
        {
            _connectionListeners.Add(connectionListener);
        }

        public void AddConnectionHeartBeatListener(IConnectionHeartbeatListener connectonHeartbeatListener)
        {
            _heatHeartbeatListeners.Add(connectonHeartbeatListener);
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
            var clientConnection = GetOrConnectWithRetry(target);
            return clientConnection.Send(request, partitionId);
        }

        public Task<IClientMessage> SendAndHandle(IClientMessage request, DistributedEventHandler handler)
        {
            return SendAndHandle(request, null, handler);
        }

        public Task<IClientMessage> SendAndHandle(IClientMessage request, Address target,
            DistributedEventHandler handler)
        {
            var clientConnection = GetOrConnectWithRetry(target);
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
                int callId;
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
                _registrationMap.TryRemove(oldAlias, out removed);
                _registrationMap.TryAdd(alias, callId);
            }
            _registrationAliasMap.TryAdd(uuidregistrationId, alias);
        }

        public void DestroyConnection(ClientConnection clientConnection)
        {
            if (clientConnection != null && !clientConnection.Live)
            {
                DestroyConnection(clientConnection.GetRemoteEndpoint());
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public ClientConnection GetOrConnect(Address address, Authenticator authenticator)
        {
            if (address == null)
            {
                throw new ArgumentException("address");
            }
            if (!_smartRouting)
            {
                //TODO: What to do here if smart routing is turned off?
                //address = _ownerConnection.GetRemoteEndpoint();
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
        /// TODO: move logic to ClientInvocationService, single retry mechanism in invocation
        public ClientConnection GetOrConnectWithRetry(Address target)
        {
            var count = 0;
            Exception lastError = null;
            var theTarget = target;
            while (count < RetryCount)
            {
                try
                {
                    if (theTarget == null || !isMember(theTarget))
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
                    Thread.Sleep(100);
                }
                catch (Exception)
                {
                }
            }
            throw lastError;
        }

        public void ReSend(Task task)
        {
            var clientConnection = GetOrConnectWithRetry(null);
            //Console.WriteLine("RESEND TO:"+clientConnection.GetSocket().RemoteEndPoint);
            clientConnection.Send(task);
        }

        /// <exception cref="HazelcastException"></exception>
        private void CheckLive()
        {
            if (!_live)
            {
                throw new HazelcastException("ConnectionManager is not active!!!");
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
                response = connection.Send(request, -1).Result;
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
            var rp = ClientAuthenticationCodec.DecodeResponse(response);
            connection.SetRemoteEndpoint(rp.address);
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

        private ClientConnection GetNewConnection(Address address, Authenticator authenticator)
        {
            ClientConnection connection = null;
            lock (_connectionMutex)
            {
                try
                {
                    var id = _nextConnectionId;
                    connection = new ClientConnection(this, id, address, _networkConfig,
                        _client.GetSerializationService(),
                        _redoOperation);
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
                    ExceptionUtil.Rethrow(e, typeof (IOException));
                }
            }
            return null;
        }

        private void HearthBeatLoop()
        {
            while (_heartBeatThread.IsAlive)
            {
                foreach (var clientConnection in _addresses.Values)
                {
                    var request = ClientPingCodec.EncodeRequest();
                    var task = clientConnection.Send(request, -1);
                    var remoteEndPoint = clientConnection.GetSocket() != null
                        ? clientConnection.GetSocket().RemoteEndPoint.ToString()
                        : "CLOSED";

                    //TODO: fire heartbeat stopped event if heartbeat times out
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

        private void FireHeartBeatEvent(Action<IConnectionHeartbeatListener> listenerAction) {
            foreach (var listener in _heatHeartbeatListeners)
            {
                listenerAction(listener);
            }
        }

        private void FireConnectionListenerEvent(Action<IConnectionListener> listenerAction)
        {
            foreach (var listener in _connectionListeners)
            {
                listenerAction(listener);
            }
        }

        private bool isMember(Address target)
        {
            var clientClusterService = _client.GetClientClusterService();
            return clientClusterService.GetMember(target) != null;
        }

        private int ReadEnvironmentVar(string var)
        {
            var p = 0;
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
                Logger.Warning("Provided value is not a valid value : " + param);
            }
            return p;
        }

        private bool RemoveEventHandler(int callId)
        {
            return _addresses.Values.Any(clientConnection => clientConnection.UnRegisterEvent(callId) != null);
            //return _clientConnections.Any(clientConnection => clientConnection.UnRegisterEvent(callId) != null);
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