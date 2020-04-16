using System;
using Hazelcast.Networking;

namespace Hazelcast.Data
{
    public class MemberInfo2 // fixme? authResult?
    {
        public MemberInfo2(Guid clusterId, Guid memberId, NetworkAddress memberAddress, string serverVersion, bool failoverSupported, int partitionCount, byte serializationVersion)
        {
            ClusterId = clusterId;
            MemberId = memberId;
            MemberAddress = memberAddress;
            ServerVersion = serverVersion;
            FailoverSupported = failoverSupported;
            PartitionCount = partitionCount;
            SerializationVersion = serializationVersion;
        }

        public Guid ClusterId { get; }

        public Guid MemberId { get; }

        public NetworkAddress MemberAddress { get; }

        public string ServerVersion { get; } // fixme semver?

        public bool FailoverSupported { get; }

        public int PartitionCount { get; }

        public byte SerializationVersion { get; }
    }
}
