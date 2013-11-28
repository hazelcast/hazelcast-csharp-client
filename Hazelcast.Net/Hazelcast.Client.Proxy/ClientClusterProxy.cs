using System;
using System.Collections.Generic;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Net.Ext;


namespace Hazelcast.Client.Proxy
{
	
	public class ClientClusterProxy : ICluster
	{
		private readonly ClientClusterService clusterService;

		public ClientClusterProxy(ClientClusterService clusterService)
		{
			this.clusterService = clusterService;
		}

		public virtual string AddMembershipListener(IMembershipListener listener)
		{
			return clusterService.AddMembershipListener(listener);
		}

		public virtual bool RemoveMembershipListener(string registrationId)
		{
			return clusterService.RemoveMembershipListener(registrationId);
		}

		public virtual ISet<IMember> GetMembers()
		{
			var members = clusterService.GetMemberList();

            return members != null ? new HashSet<IMember>(members) : new HashSet<IMember>(new IMember[0]);
		}

		public virtual IMember GetLocalMember()
		{
			throw new NotSupportedException("IClient has no local member!");
		}

		public virtual long GetClusterTime()
		{
			return clusterService.GetClusterTime();
		}
	}
}
