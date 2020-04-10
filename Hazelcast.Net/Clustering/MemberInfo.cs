using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Hazelcast.Logging;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    // FIXME rename and reconsider
    public class MemberInfo
    {
        private readonly ILogger _logger;

        public MemberInfo(NetworkAddress address, Guid uuid, IDictionary<string, string> attributes, bool isLiteMember, MemberVersion version)
        {
            _logger = Services.Get.LoggerFactory().CreateLogger(typeof(IMember) + ":" + address);
            Address = address;
            Uuid = uuid;
            IsLiteMember = isLiteMember;
            Version = version;
            Attributes = attributes;
        }

        public NetworkAddress Address { get; }
        public Guid Uuid { get; }
        public IDictionary<string, string> Attributes { get; }
        public bool IsLiteMember { get; }
        public MemberVersion Version { get; }


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

            return obj1.Uuid == obj2.Uuid;
        }

        public override int GetHashCode() => Uuid.GetHashCode();

        public override string ToString()
        {
            return $"Member [{Address.Host}]:{Address.Port} - {Uuid}{(IsLiteMember ? " lite" : "")}";
        }
    }
}
