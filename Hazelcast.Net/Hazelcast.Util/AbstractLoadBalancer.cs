using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    public abstract class AbstractLoadBalancer : LoadBalancer, IMembershipListener
    {
        private volatile ICluster _clusterRef;
        private IMember[] _members;

        public IMember[] Members
        {
            get { return _members; }
            private set { Interlocked.Exchange(ref _members, value); }
        }

        public void MemberAdded(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void MemberRemoved(MembershipEvent membershipEvent)
        {
            SetMembersFromCluster();
        }

        public void Init(ICluster cluster, ClientConfig config)
        {
            _clusterRef = cluster;
            SetMembersFromCluster();
            cluster.AddMembershipListener(this);
        }

        public abstract IMember Next();

        protected internal void SetMembersFromCluster()
        {
            ICluster cluster = _clusterRef;
            if (cluster != null)
            {
                ISet<IMember> memberSet = cluster.GetMembers();
                Members = memberSet.ToArray();
            }
        }
    }
}