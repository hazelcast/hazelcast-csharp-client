using System;
using Hazelcast.Networking;

namespace Hazelcast.Data
{
    /// <summary>
    /// Represents the result of the client authentication.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
        /// </summary>
        /// <param name="clusterId">The unique identifier of the cluster.</param>
        /// <param name="memberId">The unique identifier of the member.</param>
        /// <param name="memberAddress">The network address of the member.</param>
        /// <param name="serverVersion">The version of the server running the member.</param>
        /// <param name="failoverSupported">Whether fail-over is supported.</param>
        /// <param name="partitionCount">The partition count.</param>
        /// <param name="serializationVersion">The serialization version.</param>
        public AuthenticationResult(Guid clusterId, Guid memberId, NetworkAddress memberAddress, string serverVersion, bool failoverSupported, int partitionCount, byte serializationVersion)
        {
            ClusterId = clusterId;
            MemberId = memberId;
            MemberAddress = memberAddress;
            ServerVersion = serverVersion;
            FailoverSupported = failoverSupported;
            PartitionCount = partitionCount;
            SerializationVersion = serializationVersion;
        }

        /// <summary>
        /// Gets the unique identifier of the cluster.
        /// </summary>
        public Guid ClusterId { get; }

        /// <summary>
        /// Gets the unique identifier of the member.
        /// </summary>
        public Guid MemberId { get; }

        /// <summary>
        /// Gets the network address of the member.
        /// </summary>
        public NetworkAddress MemberAddress { get; }

        /// <summary>
        /// Gets the version of the server running the member.
        /// </summary>
        public string ServerVersion { get; }

        /// <summary>
        /// Determines whether fail-over is supported.
        /// </summary>
        public bool FailoverSupported { get; }

        /// <summary>
        /// Gets the partition count.
        /// </summary>
        public int PartitionCount { get; }

        /// <summary>
        /// Gets the serialization version.
        /// </summary>
        public byte SerializationVersion { get; }
    }
}
