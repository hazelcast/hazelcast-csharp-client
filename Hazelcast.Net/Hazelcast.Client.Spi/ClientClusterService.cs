using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Base;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;
using ICredentials = Hazelcast.Security.ICredentials;

namespace Hazelcast.Client.Spi
{
    public class ClientClusterService : IClientClusterService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientClusterService));
        private static int RetryCount = 20;
        private static int RetryWaitTime = 500;

        private readonly ConcurrentDictionary<string, IMembershipListener> _listeners =
            new ConcurrentDictionary<string, IMembershipListener>();

        private readonly HazelcastClient client;
        private readonly ClusterListener clusterListener;
        private readonly ICredentials credentials;
        private readonly bool redoOperation;
        private IDictionary<Address, IMember> _membersRef;

        private volatile bool active;
        private volatile ClientPrincipal principal;

        public ClientClusterService(HazelcastClient client)
        {
            this.client = client;
            clusterListener = new ClusterListener(this, client.GetName() + ".cluster-listener");
            ClientConfig clientConfig = client.GetClientConfig();
            redoOperation = clientConfig.IsRedoOperation();
            credentials = clientConfig.GetCredentials();
            IList<ListenerConfig> listenerConfigs = client.GetClientConfig().GetListenerConfigs();

            if (listenerConfigs != null && listenerConfigs.Count > 0)
            {
                foreach (ListenerConfig listenerConfig in listenerConfigs)
                {
                    IEventListener listener = listenerConfig.GetImplementation();
                    if (listener == null)
                    {
                        try
                        {
                            string className = listenerConfig.GetClassName();
                            Type type = Type.GetType(className);
                            if (type != null)
                            {
                                listener = Activator.CreateInstance(type) as IEventListener;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Severe(e);
                        }
                    }
                    var membershipListener = listener as IMembershipListener;
                    if (membershipListener != null)
                    {
                        AddMembershipListener(membershipListener);
                    }
                }
            }
        }

        public ConcurrentDictionary<string, IMembershipListener> Listeners
        {
            get { return _listeners; }
        }

        public IDictionary<Address, IMember> MembersRef
        {
            get { return _membersRef; }
            set { Interlocked.Exchange(ref _membersRef, value); }
        }

        public HazelcastClient Client
        {
            get { return client; }
        }

        public void Start()
        {
            clusterListener.Start();
            // TODO: replace with a better wait-notify
            while (_membersRef == null)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    throw new HazelcastException(e);
                }
            }
            active = true;
        }

        public void Stop()
        {
            active = false;
            clusterListener.Shutdown();
        }

        public IMember GetMember(Address address)
        {
            IDictionary<Address, IMember> members = _membersRef;
            IMember val = null;
            if (members != null)
            {
                members.TryGetValue(address, out val);
            }
            return val;
        }

        public IMember GetMember(string uuid)
        {
            ICollection<IMember> memberList = GetMemberList();
            foreach (IMember member in memberList)
            {
                if (uuid.Equals(member.GetUuid()))
                {
                    return member;
                }
            }
            return null;
        }

        public ICollection<IMember> GetMemberList()
        {
            IDictionary<Address, IMember> members = MembersRef;
            return members != null ? members.Values : new HashSet<IMember>();
        }

        public Address GetMasterAddress()
        {
            ICollection<IMember> memberList = GetMemberList();
            IEnumerator<IMember> enumerator = memberList.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return enumerator.Current.GetAddress();
            }
            return null;
        }

        public int GetSize()
        {
            return GetMemberList().Count;
        }

        public long GetClusterTime()
        {
            return Clock.CurrentTimeMillis();
        }

        public Client GetLocalClient()
        {
            string uuid = principal != null ? principal.GetUuid() : null;
            IConnection conn = clusterListener.Connection;
            IPEndPoint ipEndPoint = conn != null ? conn.GetLocalSocketAddress() : null;
            return new Client(uuid, ipEndPoint);
        }

        public T SendAndReceiveFixedConnection<T>(IConnection conn, object obj)
        {
            ISerializationService serializationService = client.GetSerializationService();
            Data request = serializationService.ToData(obj);
            conn.Write(request);
            Data response = conn.Read();
            object result = serializationService.ToObject(response);
            return ErrorHandler.ReturnResultOrThrowException<T>(result);
        }

        public Authenticator GetAuthenticator()
        {
            return ClusterAuthenticator;
        }

        public string MembersString()
        {
            var sb = new StringBuilder("\n\nMembers [");
            ICollection<IMember> members = GetMemberList();
            sb.Append(members != null ? members.Count : 0);
            sb.Append("] {");
            if (members != null)
            {
                foreach (IMember member in members)
                {
                    sb.Append("\n\t").Append(member);
                }
            }
            sb.Append("\n}\n");
            return sb.ToString();
        }

        public string AddMembershipListener(IMembershipListener listener)
        {
            string id = Guid.NewGuid().ToString();
            var membershipListener = listener as IInitialMembershipListener;
            if (membershipListener != null)
            {
                // TODO: needs sync with membership events...
                ICluster cluster = client.GetCluster();
                membershipListener.Init(new InitialMembershipEvent(cluster, cluster.GetMembers()));
            }
            _listeners.TryAdd(id, listener);
            return id;
        }

        public bool RemoveMembershipListener(string registrationId)
        {
            IMembershipListener removed;
            return _listeners.TryRemove(registrationId, out removed);
        }


        /// <exception cref="System.IO.IOException"></exception>
        internal T SendAndReceive<T>(Address address, object obj)
        {
            while (active)
            {
                IConnection conn = null;
                bool release = true;
                try
                {
                    conn = GetConnection(address);
                    ISerializationService serializationService = client.GetSerializationService();
                    Data request = serializationService.ToData(obj);
                    conn.Write(request);
                    Data response = conn.Read();
                    object result = serializationService.ToObject(response);
                    return ErrorHandler.ReturnResultOrThrowException<T>(result);
                }
                catch (Exception e)
                {
                    if (e is IOException)
                    {
                        if (Logger.IsFinestEnabled())
                        {
                            Logger.Finest("Error on connection... conn: " + conn + ", error: " + e);
                        }
                        IOUtil.CloseResource(conn);
                        release = false;
                    }
                    if (ErrorHandler.IsRetryable(e))
                    {
                        if (redoOperation || obj is IRetryableRequest)
                        {
                            if (Logger.IsFinestEnabled())
                            {
                                Logger.Finest("Retrying " + obj + ", last-conn: " + conn + ", last-error: " + e);
                            }
                            BeforeRetry();
                            continue;
                        }
                    }
                    throw ExceptionUtil.Rethrow<IOException>(e);
                }
                finally
                {
                    if (release && conn != null)
                    {
                        conn.Release();
                    }
                }
            }
            throw new HazelcastInstanceNotActiveException();
        }


        /// <exception cref="System.IO.IOException"></exception>
        internal void SendAndHandle(Address address, object obj, ResponseHandler handler)
        {
            IResponseStream stream = null;
            while (stream == null)
            {
                if (!active)
                {
                    throw new HazelcastInstanceNotActiveException();
                }
                IConnection conn = null;
                try
                {
                    conn = GetConnection(address);
                    ISerializationService serializationService = client.GetSerializationService();
                    Data request = serializationService.ToData(obj);
                    conn.Write(request);
                    stream = new ResponseStream(serializationService, conn);
                }
                catch (Exception e)
                {
                    if (e is IOException)
                    {
                        if (Logger.IsFinestEnabled())
                        {
                            Logger.Finest("Error on connection... conn: " + conn + ", error: " + e);
                        }
                    }
                    if (conn != null)
                    {
                        IOUtil.CloseResource(conn);
                    }
                    if (ErrorHandler.IsRetryable(e))
                    {
                        if (redoOperation || obj is IRetryableRequest)
                        {
                            if (Logger.IsFinestEnabled())
                            {
                                Logger.Finest("Retrying " + obj + ", last-conn: " + conn + ", last-error: " + e);
                            }
                            BeforeRetry();
                            continue;
                        }
                    }
                    throw ExceptionUtil.Rethrow<IOException>(e);
                }
            }
            try
            {
                handler(stream);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow<IOException>(e);
            }
            finally
            {
                stream.End();
            }
        }

        internal ICollection<IPEndPoint> GetConfigAddresses()
        {
            var socketAddresses = new List<IPEndPoint>();
            foreach (string address in client.GetClientConfig().GetAddresses())
            {
                ICollection<IPEndPoint> endPoints = AddressHelper.GetSocketAddresses(address);
                socketAddresses = socketAddresses.Union(endPoints).ToList();
            }

            var r = new Random();
            IOrderedEnumerable<IPEndPoint> shuffled = socketAddresses.OrderBy(x => r.Next());
            return new List<IPEndPoint>(shuffled);
        }

        internal IConnection ConnectToOne(ICollection<IPEndPoint> socketAddresses)
        {
            active = false;
            int connectionAttemptLimit = client.GetClientConfig().GetConnectionAttemptLimit();
            int attempt = 0;
            Exception lastError = null;
            while (true)
            {
                long nextTry = Clock.CurrentTimeMillis() + client.GetClientConfig().GetConnectionAttemptPeriod();
                foreach (IPEndPoint isa in socketAddresses)
                {
                    var address = new Address(isa);
                    try
                    {
                        IConnection connection = client.GetConnectionManager()
                            .FirstConnection(address, ManagerAuthenticator);
                        active = true;
                        clusterListener.FireConnectionEvent(false);
                        return connection;
                    }
                    catch (IOException e)
                    {
                        lastError = e;
                        Logger.Finest("IO error during initial connection...", e);
                    }
                    catch (AuthenticationException e)
                    {
                        lastError = e;
                        Logger.Warning("Authentication error on " + address, e);
                    }
                }
                if (attempt++ >= connectionAttemptLimit)
                {
                    break;
                }
                var remainingTime = (int) (nextTry - Clock.CurrentTimeMillis());
                Logger.Warning(
                    string.Format("Unable to get alive cluster connection," + " try in %d ms later, attempt %d of %d.",
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

        internal void FireConnectionEvent(bool disconnected)
        {
            var lifecycleService = (LifecycleService) client.GetLifecycleService();
            LifecycleEvent.LifecycleState state = disconnected
                ? LifecycleEvent.LifecycleState.ClientDisconnected
                : LifecycleEvent.LifecycleState.ClientConnected;
            lifecycleService.FireLifecycleEvent(state);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private IConnection GetConnection(Address address)
        {
            if (!client.GetLifecycleService().IsRunning())
            {
                throw new HazelcastInstanceNotActiveException();
            }
            IConnection connection = null;
            int retryCount = RetryCount;
            while (connection == null && retryCount > 0)
            {
                if (address != null)
                {
                    connection = client.GetConnectionManager().GetConnection(address);
                    address = null;
                }
                else
                {
                    connection = client.GetConnectionManager().GetRandomConnection();
                }
                if (connection == null)
                {
                    retryCount--;
                    BeforeRetry();
                }
            }
            if (connection == null)
            {
                throw new IOException("Unable to connect to " + address);
            }
            return connection;
        }

        private void BeforeRetry()
        {
            try
            {
                Thread.Sleep(RetryWaitTime);
                ((ClientPartitionService) client.GetClientPartitionService()).RefreshPartitions();
            }
            catch (Exception)
            {
            }
        }

        private void ClusterAuthenticator(IConnection connection)
        {
            _Authenticate<object>(connection, credentials, principal, false, false);
        }

        private void ManagerAuthenticator(IConnection connection)
        {
            principal = _Authenticate<ClientPrincipal>(connection, credentials, principal, true, true);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private T _Authenticate<T>(IConnection connection, ICredentials credentials, ClientPrincipal principal,
            bool reAuth, bool firstConnection)
        {
            var auth = new AuthenticationRequest(credentials, principal);
            auth.SetReAuth(reAuth);
            auth.SetFirstConnection(firstConnection);
            ISerializationService serializationService = client.GetSerializationService();
            Data data1 = serializationService.ToData(auth);
            connection.Write(data1);
            Data addressData = connection.Read();
            var address = ErrorHandler.ReturnResultOrThrowException<Address>(serializationService.ToObject(addressData));
            connection.SetRemoteEndpoint(address);
            Data data = connection.Read();
            return ErrorHandler.ReturnResultOrThrowException<T>(serializationService.ToObject(data));
        }
    }
}