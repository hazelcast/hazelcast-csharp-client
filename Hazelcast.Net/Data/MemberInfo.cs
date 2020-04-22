using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Data
{
    /// <summary>
    /// Represents a member of a cluster.
    /// </summary>
    public class MemberInfo // todo: consider renaming 'member'? no: codec!
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfo"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the member.</param>
        /// <param name="address">The network address of the member.</param>
        /// <param name="version">The version of the server running the member.</param>
        /// <param name="isLite">Whether the member is a "lite" member.</param>
        /// <param name="attributes">Attributes of the member.</param>
        public MemberInfo(Guid id, NetworkAddress address, MemberVersion version, bool isLite, IDictionary<string, string> attributes)
        {
            _logger = Services.Get.LoggerFactory().CreateLogger(typeof(MemberInfo) + ":" + address);

            Id = id;
            Address = address;
            Version = version;
            IsLite = isLite;
            Attributes = new ReadOnlyDictionary<string, string>(attributes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfo"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the member.</param>
        /// <param name="address">The network address of the member.</param>
        /// <param name="version">The version of the server running the member.</param>
        /// <param name="isLite">Whether the member is a "lite" member.</param>
        /// <param name="attributes">Attributes of the member.</param>
        /// <remarks>
        /// <para>That overload of the constructor is required by generated codecs.</para>
        /// </remarks>
        internal MemberInfo(NetworkAddress address, Guid id, IDictionary<string, string> attributes, bool isLite, MemberVersion version)
            : this(id, address, version, isLite, attributes)
        { }

        /// <summary>
        /// Gets the unique identifier of the member.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the unique identifier of the member (FOR INTERNAL USE ONLY).
        /// </summary>
        /// <remarks>
        /// <para>Generated codecs expect this naming of the property.</para>
        /// </remarks>
        internal Guid Uuid => Id;

        /// <summary>
        /// Gets the network address of the member.
        /// </summary>
        public NetworkAddress Address { get; }

        /// <summary>
        /// Gets the version of the server running the member.
        /// </summary>
        public MemberVersion Version { get; }

        /// <summary>
        /// Determines whether the member is a "lite" member.
        /// </summary>
        /// <remarks>
        /// <para>Lite members do not own partitions.</para>
        /// </remarks>
        public bool IsLite { get; }

        /// <summary>
        /// Determines whether the member is a "lite" member (FOR INTERNAL USE ONLY).
        /// </summary>
        /// <remarks>
        /// <para>Lite members do not own partitions.</para>
        /// <para>Generated codecs expect this naming of the property.</para>
        /// </remarks>
        internal bool IsLiteMember => IsLite;

        /// <summary>
        /// Gets the attributes of the member.
        /// </summary>
        public IReadOnlyDictionary<string, string> Attributes { get; }


        public IPEndPoint SocketAddress
        {
            get
            {
                try
                {
                    return Address.IPEndPoint;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to get socket address.");
                    return null;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is MemberInfo other && Equals(this, other);
        }

        private static bool Equals(MemberInfo obj1, MemberInfo obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1.Id == obj2.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString()
        {
            return $"Member [{Address.Host}]:{Address.Port} - {Id}{(IsLite ? " lite" : "")}";
        }
    }
}
