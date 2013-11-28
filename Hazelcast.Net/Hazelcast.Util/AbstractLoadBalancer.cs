using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;


namespace Hazelcast.Util
{
	public abstract class AbstractLoadBalancer : LoadBalancer, IMembershipListener
	{
        private IMember[] _members;
	    private volatile ICluster _clusterRef;

		public void Init(ICluster cluster, ClientConfig config)
		{
			this._clusterRef = cluster;
            SetMembersFromCluster();
            cluster.AddMembershipListener(this);
		}

	    public IMember[] Members
	    {
	        get
	        {
                return _members; 
	        }
	        private set
	        {
                Interlocked.Exchange(ref _members, value);
	        }

	    }

	    protected internal void SetMembersFromCluster()
	    {
            ICluster cluster = _clusterRef;
            if (cluster != null)
            {
                var memberSet = cluster.GetMembers();
                Members = memberSet.ToArray();
            }
	    }

		public void MemberAdded(MembershipEvent membershipEvent)
		{
            SetMembersFromCluster();
		}

		public void MemberRemoved(MembershipEvent membershipEvent)
		{
            SetMembersFromCluster();
		}

		public abstract IMember Next();
	}
}
