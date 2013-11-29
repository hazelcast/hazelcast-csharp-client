using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    public class ClusterListener
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (ClusterListener));

        private readonly HazelcastClient _client;
        private readonly ClientClusterService _clientClusterService;
        private readonly Thread _thread;
        private readonly List<IMember> members = new List<IMember>();

        private volatile IConnection _connection;


        public ClusterListener(ClientClusterService clientClusterService, string name)
        {
            _clientClusterService = clientClusterService;
            _client = clientClusterService.Client;
            _thread = new Thread(Run)
            {
                IsBackground = true,
                Name = ("ClusterListener" + new Random().Next()).Substring(0, 20)
            };
        }

        public IConnection Connection
        {
            get { return _connection; }
        }

        public void Run()
        {
            while (_thread.IsAlive)
            {
                try
                {
                    if (_connection == null)
                    {
                        try
                        {
                            _connection = PickConnection();
                        }
                        catch (Exception e)
                        {
                            logger.Severe("Error while connecting to cluster!", e);
                            _clientClusterService.Client.GetLifecycleService().Shutdown();
                            return;
                        }
                    }
                    LoadInitialMemberList();
                    ListenMembershipEvents();
                }
                catch (Exception e)
                {
                    if (_clientClusterService.Client.GetLifecycleService().IsRunning())
                    {
                        if (logger.IsFinestEnabled())
                        {
                            logger.Warning("Error while listening cluster events! -> " + _connection, e);
                        }
                        else
                        {
                            logger.Warning("Error while listening cluster events! -> " + _connection + ", Error: " + e);
                        }
                    }
                    IOUtil.CloseResource(_connection);
                    _connection = null;
                    FireConnectionEvent(true);
                }
                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public void FireConnectionEvent(bool disconnected)
        {
            var lifecycleService = (LifecycleService) _client.GetLifecycleService();
            LifecycleEvent.LifecycleState state = disconnected
                ? LifecycleEvent.LifecycleState.ClientDisconnected
                : LifecycleEvent.LifecycleState.ClientConnected;
            lifecycleService.FireLifecycleEvent(state);
        }

        internal virtual void SetInitialConn(IConnection conn)
        {
            _connection = conn;
        }

        internal virtual void Start()
        {
            _thread.Start();
        }

        internal virtual void Shutdown()
        {
            _thread.Abort();
            IConnection c = _connection;
            if (c != null)
            {
                try
                {
                    c.Close();
                }
                catch (IOException e)
                {
                    logger.Warning("Error while closing connection!", e);
                }
            }
        }

        /// <exception cref="System.Exception"></exception>
        private IConnection PickConnection()
        {
            var addresses = new HashSet<IPEndPoint>();
            if (members.Count > 0)
            {
                addresses.UnionWith(GetClusterAddresses());
            }
            addresses.UnionWith(_clientClusterService.GetConfigAddresses());
            return _clientClusterService.ConnectToOne(addresses);
        }


        /// <exception cref="System.IO.IOException"></exception>
        private void LoadInitialMemberList()
        {
            ISerializationService serializationService = _client.GetSerializationService();
            Data request = serializationService.ToData(new AddMembershipListenerRequest());
            _connection.Write(request);
            Data response = _connection.Read();

            object result = serializationService.ToObject(response);

            var coll = ErrorHandler.ReturnResultOrThrowException<SerializableCollection>(result);
            IDictionary<string, IMember> prevMembers = new Dictionary<string, IMember>();
            if (members.Count > 0)
            {
                prevMembers = new Dictionary<string, IMember>(members.Count);
                foreach (IMember member in members)
                {
                    prevMembers.Add(member.GetUuid(), member);
                }
                members.Clear();
            }
            foreach (Data d in coll.GetCollection())
            {
                members.Add((IMember) serializationService.ToObject(d));
            }
            UpdateMembersRef();
            logger.Info(_clientClusterService.MembersString());
            var events = new List<MembershipEvent>();

            ICollection<IMember> eventMembers = new ReadOnlyCollection<IMember>(members);
            foreach (IMember member in members)
            {
                if (!prevMembers.Remove(member.GetUuid()))
                {
                    events.Add(new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberAdded,
                        eventMembers));
                }
            }
            foreach (IMember member in prevMembers.Values)
            {
                events.Add(new MembershipEvent(_client.GetCluster(), member,
                    MembershipEvent.MemberRemoved, eventMembers));
            }

            foreach (MembershipEvent evnt in events)
            {
                FireMembershipEvent(evnt);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void ListenMembershipEvents()
        {
            ISerializationService serializationService = _client.GetSerializationService();
            while (_thread.IsAlive)
            {
                Data eventData = _connection.Read();
                var evnt = (ClientMembershipEvent) serializationService.ToObject(eventData);
                IMember member = evnt.GetMember();
                if (evnt.GetEventType() == MembershipEvent.MemberAdded)
                {
                    members.Add(member);
                }
                else
                {
                    members.Remove(member);
                    _client.GetConnectionManager().RemoveConnectionPool(member.GetAddress());
                }
                UpdateMembersRef();
                logger.Info(_clientClusterService.MembersString());

                var membershipEvent = new MembershipEvent(_client.GetCluster(), member, evnt.GetEventType(),
                    new ReadOnlyCollection<IMember>(members));
                FireMembershipEvent(membershipEvent);
            }
        }

        private void FireMembershipEvent(MembershipEvent membershipEvent)
        {
            _client.GetClientExecutionService().Submit(() => _FireMembershipEvent(membershipEvent));
        }

        private void _FireMembershipEvent(MembershipEvent membershipEvent)
        {
            foreach (IMembershipListener listener in  _clientClusterService.Listeners.Values)
            {
                if (membershipEvent.GetEventType() == MembershipEvent.MemberAdded)
                {
                    listener.MemberAdded(membershipEvent);
                }
                else
                {
                    listener.MemberRemoved(membershipEvent);
                }
            }
        }

        private void UpdateMembersRef()
        {
            IDictionary<Address, IMember> map = new Dictionary<Address, IMember>(members.Count);
            foreach (IMember member in members)
            {
                map.Add(member.GetAddress(), member);
            }

            _clientClusterService.MembersRef = map;
        }

        private List<IPEndPoint> GetClusterAddresses()
        {
            List<IPEndPoint> socketAddresses = members.Select(member => member.GetSocketAddress()).ToList();
            var r = new Random();
            IOrderedEnumerable<IPEndPoint> shuffled = socketAddresses.OrderBy(x => r.Next());
            return new List<IPEndPoint>(shuffled);
        }
    }
}