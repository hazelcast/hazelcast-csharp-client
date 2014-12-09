using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    /// <summary></summary>
    internal class ClientClusterService : IClientClusterService
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (IClientClusterService));

        private readonly HazelcastClient _client;

        private readonly ConcurrentDictionary<string, IMembershipListener> _listeners = new ConcurrentDictionary<string, IMembershipListener>();

        private readonly bool _redoOperation;

        private volatile IDictionary<Address, IMember> _membersRef;

        private readonly Thread _thread;

        private volatile ManualResetEventSlim _manualReset = new ManualResetEventSlim(false);

        //private static int RetryWaitTime = 500;

        /// <summary></summary>
        /// <param name="client"></param>
        public ClientClusterService(HazelcastClient client)
        {
            _client = client;
            ClientConfig config = client.GetClientConfig();
            _redoOperation = config.GetNetworkConfig().IsRedoOperation();

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
                        _AddMembershipListener(membershipListener);
                    }
                }
            }

            _thread = new Thread(ClusterListenerLoop)
            {
                //IsBackground = true,
                Name = ("ClusterListener" + new Random().Next()).Substring(0, 20)
            };

        }

        #region property def

        internal ConcurrentDictionary<string, IMembershipListener> Listeners
        {
            get { return _listeners; }
        }

        internal IDictionary<Address, IMember> MembersRef
        {
            get { return _membersRef; }
            set { Interlocked.Exchange(ref _membersRef, value); }
        }

        internal HazelcastClient Client
        {
            get { return _client; }
        }

        public bool RedoOperation
        {
            get
            {
                return _redoOperation;
            }
        }

        #endregion

        #region IClientClusterService Impl

        public void Start()
        {
            _thread.Start();
            _manualReset.Wait();

            //while (_manualReset. && _thread.IsAlive)
            //{
            //    try
            //    {
            //        Thread.Sleep(100);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new HazelcastException(e);
            //    }
            //}
            InitMembershipListener();
        }

        //[SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void Stop()
        {
            _manualReset.Reset();
            var conMan = _client.GetConnectionManager();
            conMan.Shutdown();
            _thread.Interrupt();
            //_thread.Abort();
            _thread.Join();
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

        public IClient GetLocalClient()
        {
            var clientConnectionManager = _client.GetConnectionManager() as ClientConnectionManager;
            return clientConnectionManager != null? clientConnectionManager.GetLocalClient():null;
        }


        public string MembersString()
        {
            var ownerAddress = _client.GetConnectionManager().OwnerAddress();
            var sb = new StringBuilder("\n\nMembers [");
            ICollection<IMember> members = GetMemberList();
            sb.Append(members != null ? members.Count : 0);
            sb.Append("] {");
            if (members != null)
            {
                foreach (IMember member in members)
                {
                    sb.Append("\n\t").Append(member);
                    if (member.GetAddress().Equals(ownerAddress))
                    {
                        sb.Append(" [owner]");
                    }
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
                ICluster cluster = _client.GetCluster();
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
        #endregion

        //#region privates
        //#endregion

        #region cluster listener

        public void ClusterListenerLoop()
        {
            var conMan = (ClientConnectionManager) _client.GetConnectionManager();
            conMan.Start();
            while (_thread.IsAlive)
            {
                try
                {
                    if (!conMan.OwnerLive)
                    {
                        try
                        {
                            conMan.InitOwnerConnection();
                        }
                        catch (Exception e)
                        {
                            Logger.Severe("Error while connecting to cluster!", e);
                            _client.GetLifecycleService().Shutdown();
                            return;
                        }
                    }
                    LoadInitialMemberList();
                    //ready to goo
                    _manualReset.Set();
                    ListenMembershipEvents();
                }
                catch (Exception e)
                {
                    if (_client.GetLifecycleService().IsRunning())
                    {
                        if (Logger.IsFinestEnabled())
                        {
                            Logger.Warning("Error while listening cluster events! -> ", e);
                        }
                        else
                        {
                            Logger.Warning("Error while listening cluster events! ->Error: " + e);
                        }
                    }
                    //.Shutdown();
                    conMan.FireConnectionEvent(true);
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

        private string _AddMembershipListener(IMembershipListener listener)
        {
            string id = Guid.NewGuid().ToString();
            if (_listeners.TryAdd(id, listener))
            {
                return id;
            }
            return null;
        }

        private void InitMembershipListener()
        {
            foreach (IMembershipListener membershipListener in _listeners.Values)
            {
                var listener = membershipListener as IInitialMembershipListener;
                if (listener != null)
                {
                    // TODO: needs sync with membership events...
                    ICluster cluster = _client.GetCluster();
                    listener.Init(new InitialMembershipEvent(cluster, cluster.GetMembers()));
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void LoadInitialMemberList()
        {
            var serializationService = _client.GetSerializationService();
            var ccm = _client.GetConnectionManager();
            var request = new AddMembershipListenerRequest();
            var coll= (SerializableCollection)ccm.SendAndReceiveFromOwner(request);
            
            var members = new List<IMember>(GetMemberList());
            IDictionary<string, IMember> prevMembers = new Dictionary<string, IMember>();
            if (members.Count > 0)
            {
                prevMembers = new Dictionary<string, IMember>(members.Count);
                foreach (var member in members)
                {
                    prevMembers.Add(member.GetUuid(), member);
                }
                members.Clear();
            }
            foreach (IData d in coll.GetCollection())
            {
                members.Add(serializationService.ToObject<Member>(d));
            }
            UpdateMembersRef(members);
            Logger.Info(MembersString());
            var events = new List<MembershipEvent>();
            ICollection<IMember> eventMembers = new ReadOnlyCollection<IMember>(members);
            foreach (IMember member in members)
            {
                if (!prevMembers.Remove(member.GetUuid()))
                {
                    events.Add(new MembershipEvent(_client.GetCluster(), member, MembershipEvent.MemberAdded,eventMembers));
                }
            }
            foreach (IMember member in prevMembers.Values)
            {
                events.Add(new MembershipEvent(_client.GetCluster(), member,MembershipEvent.MemberRemoved, eventMembers));
            }
            foreach (MembershipEvent evnt in events)
            {
                FireMembershipEvent(evnt);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void ListenMembershipEvents()
        {
            var serializationService = _client.GetSerializationService();
            var ccm = _client.GetConnectionManager();
            while (_thread.IsAlive)
            {
                var members = new List<IMember>(GetMemberList());
                try
                {
                    var eventData = ccm.ReadFromOwner();
                    var clientResponse = serializationService.ToObject<ClientResponse>(eventData);
                    var eventObject = serializationService.ToObject<object>(clientResponse.Response);

                    var cmEvent = eventObject as ClientMembershipEvent;
                    var membersUpdated = false;
                    if (cmEvent != null)
                    {
                        var member = cmEvent.GetMember();
                        if (cmEvent.GetEventType() == MembershipEvent.MemberAdded)
                        {
                            members.Add(member);
                            membersUpdated = true;
                        }
                        else if (cmEvent.GetEventType() == MembershipEvent.MemberRemoved)
                        {
                            members.Remove(member);
                            membersUpdated = true;
                        }
                        else if (cmEvent.GetEventType() == MembershipEvent.MemberAttributeChanged)
                        {
                            var memberAttributeChange = cmEvent.GetMemberAttributeChange();
                            var memberMap = _membersRef;
                            if (memberMap != null) {
                                foreach (Member target in memberMap.Values) {
                                    if (target.GetUuid().Equals(memberAttributeChange.Uuid)) {
                                        var operationType = memberAttributeChange.OperationType;
                                        var key = memberAttributeChange.Key;
                                        var value = memberAttributeChange.Value;
                                        target.UpdateAttribute(operationType, key, value);
                                        var memberAttributeEvent = new MemberAttributeEvent(_client.GetCluster(), target, operationType, key, value);
                                        FireMemberAttributeEvent(memberAttributeEvent);
                                        break;
                                    }
                                }
                            }

                        }
                        if (membersUpdated)
                        {
                            ((ClientPartitionService)_client.GetClientPartitionService()).RefreshPartitions();
                            UpdateMembersRef(members);
                            Logger.Info(MembersString());
                            ICollection<IMember> eventMembers = new ReadOnlyCollection<IMember>(members);
                            var membershipEvent = new MembershipEvent(_client.GetCluster(), member, cmEvent.GetEventType(),eventMembers);
                            FireMembershipEvent(membershipEvent);
                        }
                    }
                }
                catch (Exception)
                {
                    Logger.Finest("Owner Connection error");
                    throw;
                }

            }
        }

        private void FireMembershipEvent(MembershipEvent membershipEvent)
        {
            _client.GetClientExecutionService().Submit(() => _FireMembershipEvent(membershipEvent));
        }
        private void FireMemberAttributeEvent(MemberAttributeEvent memberAttributeEvent)
        {
            _client.GetClientExecutionService().Submit(() => _FireMemberAttributeEvent(memberAttributeEvent));
        }

        private void _FireMembershipEvent(MembershipEvent membershipEvent)
        {
            foreach (IMembershipListener listener in Listeners.Values)
            {
                if (membershipEvent.GetEventType() == MembershipEvent.MemberAdded)
                {
                    listener.MemberAdded(membershipEvent);
                }
                else if (membershipEvent.GetEventType() == MembershipEvent.MemberRemoved)
                {
                    listener.MemberRemoved(membershipEvent);
                }
            }
            //delegate every event to connection manager
            //_client.GetConnectionManager().HandleMembershipEvent(membershipEvent);
        }
        private void _FireMemberAttributeEvent(MemberAttributeEvent memberAttributeEvent)
        {
            foreach (IMembershipListener listener in Listeners.Values)
            {
                if (memberAttributeEvent.GetEventType() == MembershipEvent.MemberAttributeChanged)
                {
                    listener.MemberAdded(memberAttributeEvent);
                }
            }
        }

        private void UpdateMembersRef(ICollection<IMember> members)
        {
            IDictionary<Address, IMember> map = new Dictionary<Address, IMember>(members.Count);
            foreach (IMember member in members)
            {
                map.Add(member.GetAddress(), member);
            }
            this.MembersRef = map;
        }

        #endregion
    }


}