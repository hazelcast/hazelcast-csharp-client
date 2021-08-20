using System;
using System.Collections.Generic;
using Hazelcast.Clustering;

namespace Hazelcast.Testing.Accessors
{
    internal class ClusterMembersAccessor: AccessorBase<ClusterMembers>
    {
        internal ClusterMembersAccessor(ClusterMembers clusterMembers): base(clusterMembers)
        { }

        internal MemberTable Members
        {
            get => GetField<MemberTable>("_members");
            set => SetField("_members", value);
        }

        internal Dictionary<Guid, MemberConnection> Connections
        {
            get => GetField<Dictionary<Guid, MemberConnection>>("_connections");
            set => SetField("_connections", value);
        }
    }
}